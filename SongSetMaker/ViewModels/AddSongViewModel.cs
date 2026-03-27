using __XamlGeneratedCode__;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SongSetMaker.Models;
using SongSetMaker.Services;
using SQLite;

namespace SongSetMaker.ViewModels;

public partial class AddSongViewModel : ObservableObject
{
    private readonly Song _existingSong; // Keep track of the song being edited
    public bool IsEditing => _existingSong != null;

    [ObservableProperty]
    private string pageTitle = "Add New Song";

    private readonly IDatabaseService _db;
    private readonly Action _closePopup;
    public AddSongViewModel(IDatabaseService databaseService, Action closePopup)
    {
        _db = databaseService;
        _closePopup = closePopup;
    }

    // Constructor for EDIT
    public AddSongViewModel(IDatabaseService databaseService, Action closePopup, Song songToEdit)
        : this(databaseService, closePopup)
    {
        _existingSong = songToEdit;
        PageTitle = "Edit Song";
        // Map existing data to properties
        Title = songToEdit.Title;
        Artist = songToEdit.Artist;
        LeadSinger = songToEdit.LeadSinger;
        Key = songToEdit.Key;
        Tempo = songToEdit.Tempo;
        CcliNum = songToEdit.CcliNum;
        Timing = songToEdit.Timing;
        Theme = songToEdit.Theme;
        YouTubeUrl = songToEdit.YouTubeUrl;
        ChordSheetUrl = songToEdit.ChordSheetUrl;
        Scripture = songToEdit.Scripture;
        ScriptureUrl = songToEdit.ScriptureUrl;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))] 
    private string title = string.Empty;
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

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            await Shell.Current.DisplayAlert("Missing Info", "Title is required.", "OK");
            return;
        }
        if (IsEditing)
        {
            // 1. Update the existing song object's properties with the entry field values
            _existingSong.Title = Title;
            _existingSong.Artist = Artist;
            _existingSong.LeadSinger = LeadSinger;
            _existingSong.Key = Key;
            _existingSong.Tempo = Tempo;
            _existingSong.CcliNum = CcliNum;
            _existingSong.Timing = Timing;
            _existingSong.Theme = Theme;
            _existingSong.YouTubeUrl = YouTubeUrl;
            _existingSong.ChordSheetUrl = ChordSheetUrl;
            _existingSong.Scripture = Scripture;
            _existingSong.ScriptureUrl = ScriptureUrl;
            // Note: We usually DON'T change DateItemAdded during an update

            // 2. Pass that object back to the database
            _ = await _db.UpdateSongToMasterAsync(_existingSong);
        } else { 

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
        }

        _closePopup.Invoke();

    }
    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(Title);
    }
}
