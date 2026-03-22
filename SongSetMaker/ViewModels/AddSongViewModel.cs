using __XamlGeneratedCode__;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SongSetMaker.Models;
using SongSetMaker.Services;
using SQLite;

namespace SongSetMaker.ViewModels;

public partial class AddSongViewModel : ObservableObject
{
    private readonly IDatabaseService _db;
    private readonly Action _closePopup;
    public AddSongViewModel(IDatabaseService databaseService, Action closePopup)
    {
        _db = databaseService;
        _closePopup = closePopup;
    }

    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string artist = string.Empty;
    [ObservableProperty] private string leadSinger = string.Empty;
    [ObservableProperty] private string key = string.Empty;
    [ObservableProperty] private int tempo;
    [ObservableProperty] private int ccliNum;
    [ObservableProperty] private string timing = "4/4";
    [ObservableProperty] private string theme = string.Empty;
    [ObservableProperty] private string youTubeUrl = string.Empty;
    [ObservableProperty] private string chordSheetUrl = string.Empty;
    [ObservableProperty] private string songDate = DateTime.Today.ToString("yyyy-MM-dd");
    [ObservableProperty] private string scripture = string.Empty;
    [ObservableProperty] private string scriptureUrl = string.Empty;

    [RelayCommand]
    private async Task SaveAsync()
    {
        var song = new Song
        {
            Title = Title,
            Artist = Artist,
            LeadSinger = LeadSinger,
            Key = Key,
            Tempo = Tempo,
            CcliNum = CcliNum,
            Timing = Timing,
            Theme = Theme,
            YouTubeUrl = YouTubeUrl,
            ChordSheetUrl = ChordSheetUrl,
            SongDate = SongDate,
            Scripture = Scripture,
            ScriptureUrl = ScriptureUrl,
            DateItemAdded = DateTime.Today.ToString("yyyy-MM-dd")
        };

        await _db.AddSongToMasterAsync(song);

        await Shell.Current.DisplayAlert("Success", "Song added!", "OK");

    }
}
