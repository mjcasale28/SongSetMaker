// Views/MySetPage.xaml.cs
using CommunityToolkit.Maui.Extensions;
using SongSetMaker.ViewModels;
using SongSetMaker.Models;
using SongSetMaker.Services;
using CommunityToolkit.Maui.Alerts;

namespace SongSetMaker.Views
{
    public partial class MySetPage : ContentPage
    {
        private readonly IDatabaseService _db;
        private MySetViewModel ViewModel => BindingContext as MySetViewModel;
        public MySetPage(MySetViewModel viewModel, IDatabaseService db)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _db = db;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is MySetViewModel vm)
                await vm.LoadMySetCommand.ExecuteAsync(null);
        }
        bool IsKindle() =>
           DeviceInfo.Platform == DevicePlatform.Android &&
           DeviceInfo.Manufacturer?.ToLower().Contains("amazon") == true;

        private void DownloadCommand()
        {
            if (BindingContext is MySetViewModel viewModel)
            {
                viewModel.DownloadSongsCommand.Execute(null);
            }
        }

        private void UploadCommand()
        {
            if (BindingContext is MySetViewModel viewModel)
            {
                viewModel.UploadSongSetCommand.Execute(null);
            }
        }
        private void EmailCommand()
        {
            if (BindingContext is MySetViewModel viewModel)
            {
                viewModel.EmailSongSetCommand.Execute(null);
            }
        }

        private async void OnMenuButtonClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton btn && btn.BindingContext is SongSet songset)
            {
                await this.ShowPopupAsync(new SongMenuPopup(songset, _db));
            }
        }

        private void OnDragStarting(object sender, DragStartingEventArgs e)
        {
            var songset = (sender as Element).BindingContext as SongSet;
            e.Data.Properties.Add("DraggedItem", songset);
        }

        private async void OnDrop(object sender, DropEventArgs e)
        {
            var dragged = e.Data.Properties["DraggedItem"] as SongSet;
            var target = (sender as Element).BindingContext as SongSet;

            if (dragged == null || target == null)
                return;

            var list = ViewModel.MySongs;   // <-- THIS IS WHAT YOU NEEDED

            int oldIndex = list.IndexOf(dragged);
            int newIndex = list.IndexOf(target);

            if (oldIndex < 0 || newIndex < 0)
                return;

            list.Move(oldIndex, newIndex);
            await ViewModel.SaveNewOrderAsync();
        }

    }
}