using CommunityToolkit.Maui.Views;
using SongSetMaker.ViewModels;

namespace SongSetMaker.Views;

public partial class SongHistoryPopup : Popup
{
    public SongHistoryPopup(string title, List<string> dates)
    {
        InitializeComponent();
        BindingContext = new SongHistoryPopupViewModel(title, dates, this);
    }
}
