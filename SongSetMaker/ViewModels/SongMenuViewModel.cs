using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using SongSetMaker.Models;
using SongSetMaker.Services;
using SongSetMaker.Views;
using System.Windows.Input;

namespace SongSetMaker.ViewModels
{
    public class SongMenuViewModel
    {
        public string Title { get; }
        public string YouTubeUrl { get; }
        public string ChordSheetUrl { get; }
        public string ScriptureUrl { get; }
        public int CcliNum { get; }

        private readonly IDatabaseService _db;
        public ICommand OpenYouTubeCommand { get; }
        public ICommand OpenChordSheetCommand { get; }
        public ICommand OpenHistoryCommand { get; }
        public ICommand OpenScriptureCommand { get; }

        public SongMenuViewModel(Song song, IDatabaseService db)
        {
            Title = song.Title;
            YouTubeUrl = song.YouTubeUrl;
            ChordSheetUrl = song.ChordSheetUrl;
            ScriptureUrl = song.ScriptureUrl;
            CcliNum = song.CcliNum;
            _db = db;

            OpenYouTubeCommand = new Command(() =>
            {
                if (!string.IsNullOrWhiteSpace(YouTubeUrl))
                    Launcher.OpenAsync(YouTubeUrl);
            });

            OpenChordSheetCommand = new Command(() =>
            {
                if (!string.IsNullOrWhiteSpace(ChordSheetUrl))
                    Launcher.OpenAsync(ChordSheetUrl);
            });

            OpenScriptureCommand = new Command(() =>
            {
                if (!string.IsNullOrWhiteSpace(ScriptureUrl))
                    Launcher.OpenAsync(ScriptureUrl);
            });

            OpenHistoryCommand = new Command(async () =>
            {
                if (CcliNum <= 0)
                    return;
                List<string> dates;
                dates = await _db.GetSongHistoryAsync(CcliNum);

                var popup = new SongHistoryPopup(Title, dates);

                await Application.Current.MainPage.ShowPopupAsync(popup);
            });
        } // End of Song Method
        public SongMenuViewModel(SongSet song, IDatabaseService db)
        {
            Title = song.Title;
            YouTubeUrl = song.YouTubeUrl;
            ChordSheetUrl = song.ChordSheetUrl;
            ScriptureUrl = song.ScriptureUrl;
            CcliNum = song.CcliNum;
            _db = db;

            OpenYouTubeCommand = new Command(() =>
            {
                if (!string.IsNullOrWhiteSpace(YouTubeUrl))
                    Launcher.OpenAsync(YouTubeUrl);
            });

            OpenScriptureCommand = new Command(() =>
            {
                if (!string.IsNullOrWhiteSpace(ScriptureUrl))
                    Launcher.OpenAsync(ScriptureUrl);
            });

            OpenChordSheetCommand = new Command(() =>
            {
                if (!string.IsNullOrWhiteSpace(ChordSheetUrl))
                    Launcher.OpenAsync(ChordSheetUrl);
            });

            OpenHistoryCommand = new Command(async () =>
            {
                if (CcliNum <= 0)
                    return;
                if (_db == null)
                {
                    System.Diagnostics.Debug.WriteLine("DB is null in SongMenuViewModel");
                    return;
                }

                List<string> dates;
                dates = await _db.GetSongHistoryAsync(CcliNum);

                var popup = new SongHistoryPopup(Title, dates);

                await Application.Current.MainPage.ShowPopupAsync(popup);
            });
        } // End of SongSet Method

    } // End of Class
}
