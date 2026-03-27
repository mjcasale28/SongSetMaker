using SongSetMaker.Models;

namespace SongSetMaker.Services
{
    public interface IDatabaseService
    {
        Task<List<string>> GetSongHistoryAsync(int _cclinum);
        Task<List<Song>> GetMasterSongsAsync();
        Task<int> SaveSongAsync(Song song);
        Task DeleteSongAsync(int id);
        Task<List<SongSet>> GetMySongSetAsync();
        Task AddToMySetAsync(Song song);
        Task AddMasterToMySetAsync(List<Song> songs);
        Task AddSetToMySetAsync(List<SongSet> songs);
        Task AddSongToMySetAsync(Song song);
        Task AddSongToMasterAsync(Song song);
        Task<int> UpdateSongToMasterAsync(Song song);
        Task AddHistoryAsync(List<SongHistory> songhist);
        Task RemoveFromMySetAsync(int songId);
        Task RemoveFromMasterAsync(int songId);
        Task<List<string>> GetDistinctLeadSingersAsync();
        Task ClearMySetAsync();

    }
}