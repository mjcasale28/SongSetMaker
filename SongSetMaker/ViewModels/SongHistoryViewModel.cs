using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Views;

namespace SongSetMaker.ViewModels;

public partial class SongHistoryPopupViewModel : ObservableObject
{
    public string SongTitle { get; }
    public List<string> Dates { get; }
    public int TotalCount => Dates?.Count ?? 0;

    private readonly Popup _popup;

    public SongHistoryPopupViewModel(string title, List<string> dates, Popup popup)
    {
        SongTitle = title;
        Dates = dates;
        _popup = popup;
    }

    [RelayCommand]
    private void Close() => _popup.CloseAsync();
}


