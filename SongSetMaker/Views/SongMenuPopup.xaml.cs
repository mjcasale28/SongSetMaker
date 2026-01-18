using CommunityToolkit.Maui.Views;
using SongSetMaker.Models;
using SongSetMaker.Services;
using SongSetMaker.ViewModels;

namespace SongSetMaker.Views
{
    public partial class SongMenuPopup : Popup
    {
        private readonly IDatabaseService _db;
        public SongMenuPopup(Song song, IDatabaseService db)
        {
            InitializeComponent();
            BindingContext = new SongMenuViewModel(song, db);
            //  Console.WriteLine(GetType().BaseType);
        }
        public SongMenuPopup(SongSet songset, IDatabaseService db)
        {
            InitializeComponent();
            _db = db;
            BindingContext = new SongMenuViewModel(songset, db);
            //  Console.WriteLine(GetType().BaseType);
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await CloseAsync(/* optional result, e.g. true */);
        }
    }
}
