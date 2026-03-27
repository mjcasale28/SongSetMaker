// Views/MainPage.xaml.cs
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using SongSetMaker.Models;
using SongSetMaker.ViewModels;
using SongSetMaker.Services;

namespace SongSetMaker.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly IDatabaseService _db;
        public MainPage(MainPageViewModel viewModel, IDatabaseService db)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _db = db;

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            if (BindingContext is MainPageViewModel vm)
                await vm.LoadSongsCommand.ExecuteAsync(null);
        }
        bool IsKindle() =>
                   DeviceInfo.Platform == DevicePlatform.Android &&
                   DeviceInfo.Manufacturer?.ToLower().Contains("amazon") == true;
        private void DownloadCommand()
        {
            if (BindingContext is MainPageViewModel viewModel)
            {
                viewModel.DownloadSongsCommand.Execute(null);
            }
        }

        private void DownloadHistoryCommand()
        {
            if (BindingContext is MainPageViewModel viewModel)
            {
                viewModel.DownloadHistoryCommand.Execute(null);
            }
        }
        private async void OnMenuButtonClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton btn && btn.BindingContext is Song song)
            {
                await this.ShowPopupAsync(new SongMenuPopup(song, _db, true));
            }
        }

        /*
        private void OnAddSongClicked(object sender, EventArgs e)
        {
            this.ShowPopup(new AddSongPopup(_db));
        }
        */

        private async void OnAddSongClicked(object sender, EventArgs e)
        {
            var result = await Application.Current.MainPage.ShowPopupAsync(new AddSongPopup(_db));
            /*
            if (result != null)
            {
                await Shell.Current.DisplayAlert("Success", "Song added!", "OK");
            }
            */
        }

    }
}