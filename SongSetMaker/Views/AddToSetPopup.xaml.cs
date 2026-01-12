using CommunityToolkit.Maui.Views;
using SongSetMaker.Models;
using SongSetMaker.Services;
using SongSetMaker.ViewModels;

namespace SongSetMaker.Views
{
    public partial class AddToSetPopup : Popup
    {
        private AddToSetPopupViewModel Vm => (AddToSetPopupViewModel)BindingContext;
        public AddToSetPopup(Song song, IDatabaseService db)
        {
            InitializeComponent();

            BindingContext = new AddToSetPopupViewModel(song, db);

        }

        private async void OnAddClicked(object sender, EventArgs e)
        {
            await Vm.AddCommand.ExecuteAsync(null);
            await CloseAsync(/* optional result, e.g. true */);

        }


        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Vm.CancelCommand.ExecuteAsync(null);
            await CloseAsync(/* optional result, e.g. true */);

        }
    }
}
