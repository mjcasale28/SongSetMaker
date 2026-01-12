using CommunityToolkit.Mvvm.ComponentModel;

namespace SongSetMaker.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        bool isLoading;

        [ObservableProperty]
        string title = "Worship Song Set Maker";
    }
}