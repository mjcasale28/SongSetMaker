using CommunityToolkit.Maui.Views;
using SongSetMaker.Models;
using SongSetMaker.Services;
using SongSetMaker.ViewModels;

namespace SongSetMaker.Views;

public partial class AddSongPopup : Popup
{
    private readonly IDatabaseService _db;
    public AddSongPopup(IDatabaseService db)
	{
		InitializeComponent();
        _db = db;
        BindingContext = new AddSongViewModel(_db,ClosePopup);
    }
    private async void ClosePopup() => await CloseAsync();
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await CloseAsync(/* optional result, e.g. true */);
    }
}