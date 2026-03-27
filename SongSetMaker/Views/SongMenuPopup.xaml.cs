using CommunityToolkit.Maui.Views;
using SongSetMaker.Models;
using SongSetMaker.Services;
using SongSetMaker.ViewModels;
using CommunityToolkit.Maui.Extensions;

namespace SongSetMaker.Views
{
    public partial class SongMenuPopup : Popup
    {
        private readonly IDatabaseService _db;
        private readonly Song _song;
        public SongMenuPopup(Song song, IDatabaseService db, bool canEdit = true)
        {
            InitializeComponent();
            _song = song;
            _db = db;
            BindingContext = new SongMenuViewModel(song, db, canEdit);
            //  Console.WriteLine(GetType().BaseType);
        }
        public SongMenuPopup(SongSet songset, IDatabaseService db)
        {
            InitializeComponent();
            _db = db;
            BindingContext = new SongMenuViewModel(songset, db);
            //  Console.WriteLine(GetType().BaseType);
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await CloseAsync(/* optional result, e.g. true */);
        }

        private void OnEditSongClicked(object sender, EventArgs e)
        {
            var currentPage = Application.Current?.MainPage;

            if (currentPage != null)
            {
                // Show the new popup on the current page
                currentPage.ShowPopup(new AddSongPopup(_db, _song));
            }
        }
    }
}
