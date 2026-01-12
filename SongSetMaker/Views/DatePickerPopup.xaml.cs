using CommunityToolkit.Maui.Views;

namespace SongSetMaker.Views;

public partial class DatePickerPopup : Popup
{
    public DateTime? SelectedDate { get; private set; }
    public DatePickerPopup()
    {
        InitializeComponent();
    }

    private async void OnOkClicked(object sender, EventArgs e)
    {
        // await CloseAsync(Picker.Date);
        SelectedDate = Picker.Date;
        CloseAsync();

    }
}