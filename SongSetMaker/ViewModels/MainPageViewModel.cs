using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SongSetMaker.Messages;
using SongSetMaker.Models;
using SongSetMaker.Services;
using SongSetMaker.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using static SQLite.SQLite3;



namespace SongSetMaker.ViewModels
{
    public partial class MainPageViewModel : BaseViewModel
    {
        public ObservableCollection<Song> Songs { get; } = new();
        public ObservableCollection<Song> FilteredSongs { get; } = new();
        public ObservableCollection<string> Keys { get; } = new();

        private readonly IDatabaseService _db;
        private RESTApiService apiServ;

        public ObservableCollection<string> SortOptions { get; } =
                new ObservableCollection<string>
                {
                    "Title (A–Z)",
                    "Title (Z–A)",
                    "Tempo (Lo–Hi)",
                    "Tempo (Hi–Lo)"
                };
        /// <summary>
        ///   Constructor class
        /// </summary>
        /// <param name="databaseService"></param>
        public MainPageViewModel(IDatabaseService databaseService)
        {
            _db = databaseService;
            apiServ = new RESTApiService();
            SelectedSortOption = "Title (A–Z)";
            WeakReferenceMessenger.Default.Register<SongAddedToSetMessage>(this, async (r, m) =>
            {
                await LoadSongs();
            });
        }
        //--------------- Functions -------------------------------

        private int _songCount;
        public int SongCount
        {
            get => _songCount;
            set => SetProperty(ref _songCount, value);
        }


        private string _selectedSortOption;
        public string SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                if (SetProperty(ref _selectedSortOption, value))
                    ApplyFilters(); // re-sort when changed
            }
        }

        private void ApplyFilters()
        {
            if (Songs == null)
                return;

            IEnumerable<Song> query = Songs;

            // ✅ Search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var lower = SearchText.ToLower();
                query = query.Where(s =>
                    (s.Title?.ToLower().Contains(lower) ?? false) ||
                    (s.Artist?.ToLower().Contains(lower) ?? false) ||
                    (s.Theme?.ToLower().Contains(lower) ?? false));
            }

            //   if (!string.IsNullOrWhiteSpace(SearchText))
            //       query = query.Where(s => s.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            // ✅ Key filter
            if (!string.IsNullOrWhiteSpace(SelectedKey) && SelectedKey != "Key")
                query = query.Where(s => s.Key == SelectedKey);

            // ✅ Sorting
            query = SelectedSortOption switch
            {
                "Title (A–Z)" => query.OrderBy(s => s.Title),
                "Title (Z–A)" => query.OrderByDescending(s => s.Title),
                "Tempo (Lo–Hi)" => query.OrderBy(s => s.Tempo),
                "Tempo (Hi–Lo)" => query.OrderByDescending(s => s.Tempo),
                _ => query.OrderBy(s => s.Title) // default
            };

            // ✅ Update the existing collection
            FilteredSongs.Clear();
            foreach (var song in query)
                FilteredSongs.Add(song);
            SongCount = FilteredSongs.Count;
        }


        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
            /*   Use this code instead to search while typing
            set
            {
                if (SetProperty(ref _searchText, value))
                   FilterSongs();
            }
            */
        }

        private string _selectedKey;
        public string SelectedKey
        {
            get => _selectedKey;
            set
            {
                if (SetProperty(ref _selectedKey, value))
                    FilterSongs();   // re-filter when user picks a key
            }
        }
        private void FilterSongs()
        {
            IEnumerable<Song> results = Songs;

            // ✅ Search filter (only if text exists)
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var lower = SearchText.ToLower();
                results = results.Where(s =>
                    (s.Title?.ToLower().Contains(lower) ?? false) ||
                    (s.Artist?.ToLower().Contains(lower) ?? false) ||
                    (s.Theme?.ToLower().Contains(lower) ?? false));
            }

            // ✅ Key filter (always allowed)
            if (!string.IsNullOrWhiteSpace(SelectedKey) && SelectedKey != "Key")
            {
                results = results.Where(s => s.Key?.Trim() == SelectedKey.Trim());
            }

            // ✅ Update UI list
            FilteredSongs.Clear();
            foreach (var song in results)
                FilteredSongs.Add(song);
            SongCount = FilteredSongs.Count;
        }

        private void BuildKeyFilter()
        {
            Keys.Clear();
            Keys.Add("Key");

            foreach (var key in Songs
                .Select(s => NormalizeKey(s.Key))
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Distinct()
                .OrderBy(k => k))
            {
                Keys.Add(key);
            }

            SelectedKey = "Key";
        }

        private string NormalizeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            key = key.Trim();

            // If key is 2 characters like Bb, Ab, Eb
            if (key.Length == 2)
                return $"{char.ToUpper(key[0])}{char.ToLower(key[1])}";

            // If key is 1 character like C, D, E
            return key.ToUpper();
        }

        private bool _isHistoryLoading;
        public bool IsHistoryLoading
        {
            get => _isHistoryLoading;
            set => SetProperty(ref _isHistoryLoading, value);
        }

        //------------ Relay Commands -------------------------

        [RelayCommand]
        private void PerformSearch(string query)
        {
            SearchText = query;
            FilterSongs();
        }

        [RelayCommand]
        async Task RemoveSong(Song song)
        {
            await _db.RemoveFromMasterAsync(song.SongId);
            Songs.Remove(song);
            SongCount = Songs.Count;
            await LoadSongs();
        }

        [RelayCommand]
        public async Task RefreshAsync()
        {
            await LoadSongs();
        }

        [RelayCommand]
        async Task LoadSongs()
        {
            if (IsLoading) // Prevent multiple simultaneous loads
                return;
            try
            {
                IsLoading = true;
                Songs.Clear();
                var songs = await _db.GetMasterSongsAsync();

                foreach (var song in songs.OrderBy(s => s.Title))
                    Songs.Add(song);
                BuildKeyFilter();
                FilterSongs();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load set: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ShowMenu(object songItem)  // songItem is the CommandParameter {Binding .}
        {
            if (songItem is not SongSetMaker.Models.Song song)
                return;

            var popup = new SongMenuPopup(song, _db);

            await Application.Current.MainPage.ShowPopupAsync(popup);
        }

        [RelayCommand]
        private async Task PlayYouTube(Song song)
        {
            if (song != null && !string.IsNullOrWhiteSpace(song.YouTubeUrl))
            {
                try
                {
                    await Browser.Default.OpenAsync(song.YouTubeUrl, BrowserLaunchMode.SystemPreferred);
                }
                catch (Exception ex)
                {
                    //Debug.WriteLine($"Could not open YouTube: {ex.Message}");
                    // Optional: Show a toast if the link is broken
                    ShowToast("Could not open YouTube");
                }
            }
            else
            {
                //  var toast = MyMakeToast("No YouTube link available for this song.");
                //  if (toast != null) 
                ShowToast("No YouTube link available for this song.");
            }
        }

        [RelayCommand]
        private async Task OpenChordSheet(Song song)
        {
            if (song != null && !string.IsNullOrWhiteSpace(song.ChordSheetUrl))
            {
                try
                {
                    await Browser.Default.OpenAsync(song.ChordSheetUrl, BrowserLaunchMode.SystemPreferred);
                }
                catch (Exception ex)
                {
                    // Debug.WriteLine($"Could not open Chord Sheet: {ex.Message}");
                    // Optional: Show a toast if the link is broken
                    ShowToast("Could not open Chord Sheet");
                }
            }
            else
            {
                // var toast = MyMakeToast("No Chord Sheet link available for this song.");
                // if (toast != null) toast.Show();
                ShowToast("No Chord Sheet link available for this song");
            }
        }

        [RelayCommand]
        private async Task OpenScripture(Song song)
        {
            if (song != null && !string.IsNullOrWhiteSpace(song.ScriptureUrl))
            {
                try
                {
                    await Browser.Default.OpenAsync(song.ScriptureUrl, BrowserLaunchMode.SystemPreferred);
                }
                catch (Exception ex)
                {
                    // Debug.WriteLine($"Could not open Scripture: {ex.Message}");
                    // Optional: Show a toast if the link is broken
                    ShowToast("Could not open Scripture");
                }
            }
            else
            {
                // var toast = MyMakeToast("No Scripture link available for this song.");
                // if (toast != null) toast.Show();
                ShowToast("No Scripture link available for this song.");
            }
        }


        [RelayCommand]
        private async Task OpenHistory(Song song)
        {
            // 1. Validation: Ensure we have a song and a valid CCLI number
            if (song == null || song.CcliNum <= 0)
            {
                //var toast = MyMakeToast("No CCLI number found for this song.");
                // if (toast != null) toast.Show();
                ShowToast("No CCLI number found for this song");
                return;
            }

            try
            {
                // 2. Fetch history using the CcliNum from the specific song
                // Note: Ensure your _db method handles string or int based on your DB schema
                List<string> dates = await _db.GetSongHistoryAsync(song.CcliNum);

                // 3. Create and show the popup
                var popup = new SongHistoryPopup(song.Title, dates);
                await Application.Current.MainPage.ShowPopupAsync(popup);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening history: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task EditTheme(object songItem)
        {
            // Navigate to edit page, e.g.:
            // await Shell.Current.GoToAsync($"{nameof(EditThemePage)}?SongId={((Song)songItem).Id}");
        }


        [RelayCommand]
        private async Task AddToMySet(Song song)
        {
            var popup = new AddToSetPopup(song, _db);

            await Application.Current.MainPage.ShowPopupAsync(popup, new PopupOptions
            {
                CanBeDismissedByTappingOutsideOfPopup = false  // Critical for Picker to work reliably
            });
            // result is true if user pressed Add
        }

        //async Task AddToMySet(Song song) => await _db.AddSongToMySetAsync(song);

        private async Task AddSampleSongs()
        {
            var samples = new List<Song>
            {
                new() { Title = "Amazing Grace (My Chains Are Gone)", Artist = "Chris Tomlin", Key = "G", Tempo = 72, YouTubeUrl = "https://youtu.be/..." },
                new() { Title = "10,000 Reasons", Artist = "Matt Redman", Key = "G", Tempo = 73 },
                new() { Title = "How Great Is Our God", Artist = "Chris Tomlin", Key = "A", Tempo = 78 },
            };

            foreach (var s in samples)
                await _db.SaveSongAsync(s);
        }

        private Toast MyMakeToast(string mytext)
        {
            try
            {

                ToastDuration duration = ToastDuration.Short;
                double fontSize = 14;
                var toast = Toast.Make(mytext, duration, fontSize);
                return (Toast)toast;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private void ShowToast(string message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Toast.Make(message, ToastDuration.Short, 14).Show();
            });
        }

        [RelayCommand]
        async Task DownloadSongs()
        {
            try
            {
                // var toast = MyMakeToast("Downloading..");
                // toast.Show();
                ShowToast("Downloading..");
                // 1. Fetch the songs from the external API
                var songs = await apiServ.GetMasterSongs();
                // 2. Add the songs to the local database.
                await setSongsApi(songs);
                // 3. Reuse the LoadSongs() logic to refresh the ObservableCollection
                await LoadSongs();
                //toast = MyMakeToast("Download Complete");
                //toast.Show();
                ShowToast("Download Complete");
            }
            catch (Exception ex)
            {
                // Handle any exceptions during the download or saving process
                // Debug.WriteLine($"Download failed: {ex.Message}");
                // You might want to notify the user here
                ShowToast("Download failed");
            }
        }
        async Task setSongsApi(List<Song> _songs)
        {
            if (_songs != null && _songs.Count > 0)
            {
                await _db.AddMasterToMySetAsync(_songs);
            }
        }

        [RelayCommand]
        async Task DownloadHistory()
        {
            if (IsHistoryLoading)
                return;

            try
            {
                //var toast = MyMakeToast("Downloading..");
                //toast.Show();
                ShowToast("Downloading..");
                // 1. Fetch the songs from the external API
                var songhist = await apiServ.GetSongHistory();
                // 2. Add the songs to the local database.
                await setSongHistoryApi(songhist);

                // Optional: show a toast or popup
                /*
                await Application.Current.MainPage.DisplayAlert(
                    "History Updated",
                    "Song history download completed.",
                    "OK");
                */
                //toast = MyMakeToast("Download Complete");
                //toast.Show();
                ShowToast("Download Complete");
            }
            catch (Exception ex)
            {
                // Handle any exceptions during the download or saving process
                Debug.WriteLine($"Download failed: {ex.Message}");
                // You might want to notify the user here
            }
            finally
            {
                IsHistoryLoading = false;
            }
        }

        async Task setSongHistoryApi(List<SongHistory> _songs)
        {
            if (_songs != null && _songs.Count > 0)
            {
                await _db.AddHistoryAsync(_songs);
            }
        }
    }
}