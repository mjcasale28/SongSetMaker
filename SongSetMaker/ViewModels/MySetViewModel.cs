using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SongSetMaker.Models;
using SongSetMaker.Services;
using SongSetMaker.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SongSetMaker.ViewModels
{
    public partial class MySetViewModel : BaseViewModel
    {
        public ObservableCollection<SongSet> MySongs { get; } = new();

        private readonly IDatabaseService _db;
        private readonly RESTApiService _apiServ; // Changed to readonly and lowercase for convention
        

        private string _songDate;
        public string SongDate
        {
            get => _songDate;
            set => SetProperty(ref _songDate, value);
        }
        public string SongDateFormatted
        {
            get
            {
                if (DateTime.TryParse(SongDate, out var dt))
                    return dt.ToString("yyyy-MM-dd");

                return SongDate; // fallback
            }
        }

        private int _songCount;
        public int SongCount
        {
            get => _songCount;
            set => SetProperty(ref _songCount, value);
        }

        /// <summary>
        /// Constructor 
        /// Load Database and REST_API Service. Load Song Set from Database
        /// </summary>
        /// <param name="databaseService"></param>
        public MySetViewModel(IDatabaseService databaseService)
        {
            _db = databaseService;
            _apiServ = new RESTApiService(); // Assigned to readonly field
            Title = "My Song Set";
            LoadMySetCommand.Execute(null);
        }

        [RelayCommand]
        async Task LoadMySet()
        {
            if (IsLoading) // Prevent multiple simultaneous loads
                return;
            try
            {
                IsLoading = true;
                MySongs.Clear();
                var list = await _db.GetMySongSetAsync();
                foreach (var s in list)
                    MySongs.Add(s);
                SongDate = MySongs.FirstOrDefault()?.SongDate;
                OnPropertyChanged(nameof(SongDateFormatted));
                SongCount = MySongs.Count;
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
        async Task ClearSongSet()
        {
            await _db.ClearMySetAsync();
            MySongs.Clear();
            SongCount = MySongs.Count;
        }

        [RelayCommand]
        async Task RemoveSong(SongSet song)
        {
            await _db.RemoveFromMySetAsync(song.SongId);
            MySongs.Remove(song);
            SongCount = MySongs.Count;
        }

        [RelayCommand]
        private async Task ShowMenu(object songItem)  // songItem is the CommandParameter {Binding .}
        {
            if (songItem is not SongSetMaker.Models.SongSet song)
                return;

            var popup = new SongMenuPopup(song, _db);

            await Application.Current.MainPage.ShowPopupAsync(popup);
        }
        [RelayCommand]
        private async Task PlayYouTube(SongSet song)
        {
            if (song != null && !string.IsNullOrWhiteSpace(song.YouTubeUrl))
            {
                try
                {
                    await Browser.Default.OpenAsync(song.YouTubeUrl, BrowserLaunchMode.SystemPreferred);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Could not open YouTube: {ex.Message}");
                    // Optional: Show a toast if the link is broken
                }
            }
            else
            {
                var toast = MyMakeToast("No YouTube link available for this song.");
                if (toast != null) toast.Show();
            }
        }

        [RelayCommand]
        private async Task OpenChordSheet(SongSet song)
        {
            if (song != null && !string.IsNullOrWhiteSpace(song.ChordSheetUrl))
            {
                try
                {
                    await Browser.Default.OpenAsync(song.ChordSheetUrl, BrowserLaunchMode.SystemPreferred);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Could not open Chord Sheet: {ex.Message}");
                    // Optional: Show a toast if the link is broken
                }
            }
            else
            {
                var toast = MyMakeToast("No Chord Sheet link available for this song.");
                if (toast != null) toast.Show();
            }
        }

        [RelayCommand]
        private async Task OpenScripture(SongSet song)
        {
            if (song != null && !string.IsNullOrWhiteSpace(song.ScriptureUrl))
            {
                try
                {
                    await Browser.Default.OpenAsync(song.ScriptureUrl, BrowserLaunchMode.SystemPreferred);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Could not open Scripture: {ex.Message}");
                    // Optional: Show a toast if the link is broken
                }
            }
            else
            {
                var toast = MyMakeToast("No Scripture link available for this song.");
                if (toast != null) toast.Show();
            }
        }


        [RelayCommand]
        private async Task OpenHistory(SongSet song)
        {
            // 1. Validation: Ensure we have a song and a valid CCLI number
            if (song == null || song.CcliNum <= 0)
            {
                var toast = MyMakeToast("No CCLI number found for this song.");
                if (toast != null) toast.Show();
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
                ShowToast("Downloading...");
                //var toast = MyMakeToast("Downloading..");
                // show toast
                //toast.Show();
                // 1. Fetch the songs from the external API
                var songs = await _apiServ.GetSongSet();

                // 2. Add the songs to the local database.
                // The issue here was likely that setSongsApi was not awaited,
                // meaning the list refresh would happen before songs were actually saved.
                await setSongsApi(songs);

                // 3. Reuse the LoadMySet logic to refresh the ObservableCollection
                // This is the clean, correct way to refresh the list after an action.
                await LoadMySet();
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
        }

        async Task setSongsApi(List<SongSet> _songs)
        {
            if (_songs != null && _songs.Count > 0)
            {
                // IMPORTANT: You MUST await the database operation here
                // to ensure the songs are saved before LoadMySet() attempts to read them.
                await _db.AddSetToMySetAsync(_songs);
            }
        }
        public async Task SaveNewOrderAsync()
        {
            await _db.AddSetToMySetAsync(MySongs.ToList());
        }

        [RelayCommand]
        async Task UploadSongSet()
        {
            var popup = new DatePickerPopup();
            // 1. Ask user for a date
            await Application.Current.MainPage.ShowPopupAsync(popup);

            if (popup.SelectedDate is not DateTime selectedDate)
                return; // user cancelled or popup closed without OK

            // 2. Update every song's date
            foreach (var song in MySongs)
                song.SongDate = selectedDate.ToString("MM/dd/yyyy");
            //.ToString("yyyy-MM-dd");

            var songs = MySongs.ToList();  // ObservableCollection → List
            var success = await _apiServ.UploadSongSetAsync(songs);
            if (success)
                await Shell.Current.DisplayAlert("Success", "Song set uploaded!", "OK");
            else
                await Shell.Current.DisplayAlert("Error", "Upload failed.", "OK");
        }
        /// <summary>
        ///   Send Email of the Song Set
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task EmailSongSet()
        {
            // 1. Ask user for a date (reuse your popup)
            var popup = new DatePickerPopup();
            await Application.Current.MainPage.ShowPopupAsync(popup);

            if (popup.SelectedDate is not DateTime selectedDate)
                return;

            // 2. Update each song with the selected date
            foreach (var song in MySongs)
                song.SongDate = selectedDate.ToString("MM/dd/yyyy");

            // 4. Build the email body
            var emailBody = BuildEmailBody(MySongs, selectedDate.ToString("MM/dd/yyyy"));

            // 5. Launch the email client
            var message = new EmailMessage
            {
                Subject = $"Praise and Worship Song Set for {selectedDate:MMMM dd, yyyy}",
                Body = emailBody,
                // BodyFormat = DeviceInfo.Current.Platform == DevicePlatform.WinUI
                //  ? EmailBodyFormat.PlainText
                //  : EmailBodyFormat.Html,
                BodyFormat = EmailBodyFormat.PlainText,
                To = new List<string> { "" } // blank but valid
            };
            // Send Email Message using OS Email Client
            try
            {
                await Email.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException)
            {
                await Shell.Current.DisplayAlert("Error", "Email is not supported on this device.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Email failed: {ex.Message}", "OK");
            }

            // 3. Upload the songset first
            //  var success = await _apiServ.UploadSongSetAsync(MySongs.ToList());
            /*
              if (success)
              {
                  await Shell.Current.DisplayAlert("Success", "Song set uploaded!", "OK");
                  // 4. Build the email body
                  var emailBody = BuildEmailBody(MySongs, selectedDate.ToString("MM/dd/yyyy"));

                  // 5. Launch the email client
                  var message = new EmailMessage
                  {
                      Subject = $"HCF Praise and Worship Song Set for {selectedDate:MMMM dd, yyyy}",
                      Body = emailBody,
                      // BodyFormat = DeviceInfo.Current.Platform == DevicePlatform.WinUI
                      //  ? EmailBodyFormat.PlainText
                      //  : EmailBodyFormat.Html,
                      BodyFormat = EmailBodyFormat.PlainText,
                      To = new List<string> { "worshipteam@henriettacf.org" } // blank but valid
                  };
                  // Send Email Message using OS Email Client
                  try
                  {
                      await Email.ComposeAsync(message);
                  }
                  catch (FeatureNotSupportedException)
                  {
                      await Shell.Current.DisplayAlert("Error", "Email is not supported on this device.", "OK");
                  }
                  catch (Exception ex)
                  {
                      await Shell.Current.DisplayAlert("Error", $"Email failed: {ex.Message}", "OK");
                  }
              }
              else
              {
                  await Shell.Current.DisplayAlert("Error", "Upload failed.", "OK");
              }
            */
        }

        private string BuildEmailBody(IEnumerable<SongSet> songs, string mySongDate)
        {
            var sb = new StringBuilder();

            // 2. Build the message content
            sb.AppendLine("Hi everyone,");
            sb.AppendLine(); // Adds the blank line
            sb.AppendLine($"Song Set for " + mySongDate + " is: ");
            sb.AppendLine(); // Adds the blank line
            sb.AppendLine("----------------------------");
            int songCtr = 1;
            foreach (var s in songs)
            {

                sb.Append($"Song {songCtr} - {s.Title}");
                sb.AppendLine();
                sb.Append($"-Lead: {s.LeadSinger}");
                sb.AppendLine();
                sb.Append($"-Key: {s.Key}");
                sb.AppendLine();
                sb.Append($"-Tempo: {s.Tempo}");
                sb.AppendLine();
                sb.Append($"-Artist: {s.Artist}");
                sb.AppendLine();
                sb.Append($"{s.YouTubeUrl}");
                sb.AppendLine();
                sb.AppendLine();
                songCtr++;
            }
            /*
                        foreach (var s in songs)
                        {
                            sb.Append("<tr>");
                            sb.Append($"<td style='padding:8px; border:1px solid #ccc;'>{s.Title}</td>");
                            sb.Append($"<td style='padding:8px; border:1px solid #ccc;'>{s.Key}</td>");
                            sb.Append($"<td style='padding:8px; border:1px solid #ccc;'>{s.Tempo}</td>");
                            sb.Append($"<td style='padding:8px; border:1px solid #ccc;'>{s.Timing}</td>");
                            sb.Append($"<td style='padding:8px; border:1px solid #ccc;'>{s.LeadSinger}</td>");
                            sb.Append("</tr>");
                        }
                        sb.Append("</table>");
            */
            sb.AppendLine("----------------------------");

            return sb.ToString();
        }


    }
}
