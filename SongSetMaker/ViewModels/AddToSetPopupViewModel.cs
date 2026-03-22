using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SongSetMaker.Messages;
using SongSetMaker.Models;
using SongSetMaker.Services;
using System.Collections.ObjectModel;

namespace SongSetMaker.ViewModels
{
    public partial class AddToSetPopupViewModel : ObservableObject
    {
        private readonly Song _song;
        private readonly IDatabaseService _db;
        public AddToSetPopupViewModel(Song song, IDatabaseService db)
        {
            _song = song;
            _db = db;

            SongTitle = song.Title;
            songKey = song.Key;

            _ = LoadLeadSingersAsync();
           // SelectedLeadSinger = LeadSingers[0];
            /*
            LeadSingers.Add("Barbie");
            LeadSingers.Add("Doug");
            LeadSingers.Add("Marc");
            LeadSingers.Add("Marc & Barbie");
            */
        }


        private async Task LoadLeadSingersAsync()
        {
            try
            {
                List<string> singers = await _db.GetDistinctLeadSingersAsync();

        //        var singers = await _db.GetDistinctLeadSingersAsync();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    LeadSingers.Clear();
                    foreach (var s in singers)
                    {
                        LeadSingers.Add(s);
                    }
                });
            }
            catch {
                LeadSingers.Add("Psalmist");
            } finally
            {
                SelectedLeadSinger = LeadSingers[0];
            }
        }

        [ObservableProperty]
        private string songTitle;

        [ObservableProperty]
        private ObservableCollection<string> leadSingers = new();

        [ObservableProperty]
        private string selectedLeadSinger;

        [ObservableProperty]
        private string songKey;

        [RelayCommand]
        public async Task<bool> Add()
        {
            if (string.IsNullOrWhiteSpace(SelectedLeadSinger) ||
                string.IsNullOrWhiteSpace(SongKey))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please fill all fields", "OK");
                return false;
            }
            var setEntry = new Song
            {
                Title = _song.Title,
                Artist = _song.Artist,
                LeadSinger = SelectedLeadSinger,
                Key = SongKey,
                Tempo = _song.Tempo,
                CcliNum = _song.CcliNum,
                Timing = _song.Timing,
                Theme = _song.Theme,
                YouTubeUrl = _song.YouTubeUrl,
                ChordSheetUrl = _song.ChordSheetUrl,
                SongDate = DateTime.Today.ToString("yyyy-MM-dd"),
                Scripture = _song.Scripture,
                ScriptureUrl = _song.ScriptureUrl
            };

            await _db.AddSongToMySetAsync(setEntry);

            WeakReferenceMessenger.Default.Send(new SongAddedToSetMessage(setEntry));
            return true;
        }

        [RelayCommand]
        public Task<bool> Cancel() => Task.FromResult(false);


    }
}

