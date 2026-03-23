using SongSetMaker.Models;
using SQLite;

namespace SongSetMaker.Services
{
    public class DatabaseService : IDatabaseService, IDisposable
    {
        private SQLiteAsyncConnection _db;
        private readonly string databasePath = Constants.DatabasePath;
        private async Task Init()
        {
            if (_db is not null) return;

            //databasePath = Path.Combine(FileSystem.AppDataDirectory, "WorshipSongs.db3");

            _db = new SQLiteAsyncConnection(databasePath);
            await _db.CreateTableAsync<Song>();
            await _db.CreateTableAsync<SongSet>();
            await _db.CreateTableAsync<SongHistory>();
        }
        // Helper to get/lazy-init connection
        private SQLiteAsyncConnection GetConnection()
        {
            if (_db == null)
            {
                _db = new SQLiteAsyncConnection(databasePath);
            }
            return _db;
        }

        // ────────────────────────────────────────────────
        // IDisposable implementation (synchronous dispose)
        // ────────────────────────────────────────────────
        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);  // Prevent finalizer from running if Dispose was called
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Dispose managed resources
                if (_db != null)
                {
                    _db.CloseAsync().GetAwaiter().GetResult();  // Sync wrapper — avoid if possible
                    _db = null;
                }
            }

            // If you had unmanaged resources (rare in MAUI/SQLite), release them here

            _disposed = true;
        }

        // Optional: Finalizer (only needed if you have true unmanaged resources)
        ~DatabaseService()
        {
            Dispose(false);
        }


        /// <summary>
        ///     GetSongHistory - pulls the song history dates based on the ccli number
        /// </summary>
        /// <param name="ccliNum"></param>
        /// <returns></returns>
        public async Task<List<string>> GetSongHistoryAsync(int ccliNum)
        {
            await Init().ConfigureAwait(false);

            var oneYearAgo = DateTime.Now.AddYears(-1);

            var records = await _db.Table<SongHistory>()
                                   .Where(h => h.CcliNum == ccliNum)
                                   .ToListAsync()
                                   .ConfigureAwait(false);

            return records
                .Select(r => DateTime.Parse(r.SongDate))
                .Where(d => d >= oneYearAgo)
                .OrderByDescending(d => d)
                .Select(d => d.ToString("MMM dd, yyyy"))
                .ToList();
        }


        public async Task<List<string>> GetDistinctLeadSingersAsync()
        {
            await Init().ConfigureAwait(false);


            var records = await _db.Table<Song>()
                                   .ToListAsync()
                                   .ConfigureAwait(false);

            return records
                .Select(r => r.LeadSinger)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();
        }
        /*
        public async Task<List<string>> GetDistinctLeadSingersAsync()
        {
            await Init();
            try
            {
                var songs = await _db.Table<Song>()
                    .Where(s => !string.IsNullOrEmpty(s.LeadSinger))                  
                    .ToListAsync();

                var distinct = songs
                    .Select(s => s.LeadSinger!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(s => s)
                    .ToList();

                return distinct;
            }
            catch (Exception ex)
            {
                return new List<string> { "Psalmist" };
            }
        }
        */

        /// <summary>
        ///   Pulls the full list of all songs
        /// </summary>
        /// <returns></returns>
        public async Task<List<Song>> GetMasterSongsAsync()
        {
            await Init().ConfigureAwait(false);
            return await _db.Table<Song>().ToListAsync().ConfigureAwait(false);
        }


        public async Task<int> SaveSongAsync(Song song)
        {
            await Init();
            if (song.SongId != 0)
                return await _db.UpdateAsync(song);
            return await _db.InsertAsync(song);
        }
        public async Task DeleteSongAsync(int id)
        {
            await Init();
            await _db.Table<Song>().DeleteAsync(s => s.SongId == id);
        }

        public async Task<List<SongSet>> GetMySongSetAsync()
        {
            await Init();
            return await _db.Table<SongSet>().ToListAsync().ConfigureAwait(false);
        }

        /// <summary>
        ///    Add a single song to the SongSet table
        /// </summary>
        /// <param name="song"></param>
        /// <returns></returns>
        public async Task AddToMySetAsync(Song song)
        {
            await Init();
            var currDate = DateTime.Today.ToString("yyyy-MM-dd");
            var exists = await _db.Table<SongSet>()
                .FirstOrDefaultAsync(s => s.Title == song.Title && s.Artist == song.Artist);

            if (exists == null)
            {
                await _db.InsertAsync(new SongSet
                {
                    Title = song.Title,
                    Artist = song.Artist,
                    LeadSinger = song.LeadSinger,
                    Key = song.Key,
                    Tempo = song.Tempo,
                    CcliNum = song.CcliNum,
                    Timing = song.Timing,
                    Theme = song.Theme,
                    YouTubeUrl = song.YouTubeUrl,
                    ChordSheetUrl = song.ChordSheetUrl,
                    DateItemAdded = currDate,
                    Scripture = song.Scripture,
                    ScriptureUrl = song.ScriptureUrl
                });
            }
        }
        /// <summary>
        ///   Load the Master Song list into the Database
        /// </summary>
        /// <param name="songs"></param>
        /// <returns></returns>
        public async Task AddMasterToMySetAsync(List<Song> songs)
        {

            await Init().ConfigureAwait(false);

            var currDate = DateTime.Today.ToString("yyyy-MM-dd");


            await _db.DeleteAllAsync<Song>().ConfigureAwait(false);
            // 2. Reset the auto-increment counter
            await _db.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name='Song'").ConfigureAwait(false);

            // 3. Optional: Vacuum the database to reclaim space immediately
            await _db.ExecuteAsync("VACUUM").ConfigureAwait(false);
            foreach (var song in songs)
            {

                await _db.InsertAsync(new Song
                {
                    Title = song.Title,
                    Artist = song.Artist,
                    LeadSinger = song.LeadSinger,
                    Key = song.Key,
                    Tempo = song.Tempo,
                    CcliNum = song.CcliNum,
                    Timing = song.Timing,
                    Theme = song.Theme,
                    YouTubeUrl = song.YouTubeUrl,
                    ChordSheetUrl = song.ChordSheetUrl,
                    DateItemAdded = currDate,
                    Scripture = song.Scripture,
                    ScriptureUrl = song.ScriptureUrl
                }).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///   Add Song Set to the Song Set table
        /// </summary>
        /// <param name="songs"></param>
        /// <returns></returns>
        public async Task AddSetToMySetAsync(List<SongSet> songs)
        {
            await Init();
            var currDate = DateTime.Today.ToString("yyyy-MM-dd");
            await _db.DeleteAllAsync<SongSet>().ConfigureAwait(false);
            // 2. Reset the auto-increment counter
            await _db.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name='SongSet'").ConfigureAwait(false);

            // 3. Optional: Vacuum the database to reclaim space immediately
            await _db.ExecuteAsync("VACUUM").ConfigureAwait(false);

            foreach (var song in songs)
            {
                var exists = await _db.Table<SongSet>()
                    .FirstOrDefaultAsync(s => s.Title == song.Title && s.Artist == song.Artist);

                if (exists == null)
                {
                    await _db.InsertAsync(new SongSet
                    {
                        Title = song.Title,
                        Artist = song.Artist,
                        LeadSinger = song.LeadSinger,
                        Key = song.Key,
                        Tempo = song.Tempo,
                        CcliNum = song.CcliNum,
                        Timing = song.Timing,
                        Theme = song.Theme,
                        YouTubeUrl = song.YouTubeUrl,
                        ChordSheetUrl = song.ChordSheetUrl,
                        DateItemAdded = currDate,
                        SongDate = song.SongDate,
                        Scripture = song.Scripture,
                        ScriptureUrl = song.ScriptureUrl
                    });
                }
            }
        }
        public async Task AddSongToMySetAsync(Song song)
        {
            await Init();
            var currDate = DateTime.Today.ToString("yyyy-MM-dd");

            var exists = await _db.Table<SongSet>()
                .FirstOrDefaultAsync(s => s.Title == song.Title && s.Artist == song.Artist);

            if (exists == null)
            {
                await _db.InsertAsync(new SongSet
                {
                    Title = song.Title,
                    Artist = song.Artist,
                    LeadSinger = song.LeadSinger,
                    Key = song.Key,
                    Tempo = song.Tempo,
                    CcliNum = song.CcliNum,
                    Timing = song.Timing,
                    Theme = song.Theme,
                    YouTubeUrl = song.YouTubeUrl,
                    ChordSheetUrl = song.ChordSheetUrl,
                    DateItemAdded = currDate,
                    Scripture = song.Scripture,
                    ScriptureUrl = song.ScriptureUrl
                });
            }
        }

        public async Task AddSongToMasterAsync(Song song)
        {
            await Init();
            var currDate = DateTime.Today.ToString("yyyy-MM-dd");

            var exists = await _db.Table<Song>()
                .FirstOrDefaultAsync(s => s.Title == song.Title && s.Artist == song.Artist);

            if (exists == null)
            {
                await _db.InsertAsync(new Song
                {
                    Title = song.Title,
                    Artist = song.Artist,
                    LeadSinger = song.LeadSinger,
                    Key = song.Key,
                    Tempo = song.Tempo,
                    CcliNum = song.CcliNum,
                    Timing = song.Timing,
                    Theme = song.Theme,
                    YouTubeUrl = song.YouTubeUrl,
                    ChordSheetUrl = song.ChordSheetUrl,
                    DateItemAdded = currDate,
                    Scripture = song.Scripture,
                    ScriptureUrl = song.ScriptureUrl
                });
            }
        }

        public async Task AddHistoryAsync(List<SongHistory> songhist)
        {
            await Init();
            var currDate = DateTime.Today.ToString("yyyy-MM-dd");
            await _db.DeleteAllAsync<SongHistory>().ConfigureAwait(false);
            // 2. Reset the auto-increment counter
            await _db.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name='SongHistory'").ConfigureAwait(false);

            // 3. Optional: Vacuum the database to reclaim space immediately
            await _db.ExecuteAsync("VACUUM").ConfigureAwait(false);

            foreach (var mysonghist in songhist)
            {
                // Guard: skip null entries
                if (mysonghist == null)
                    continue;

                // Guard: skip invalid CCLI numbers
                if (mysonghist.CcliNum <= 0)
                    continue;

                // Convert incoming date safely
                string convertedDate = ConvertToShortDate(mysonghist.SongDate);

                // Guard: skip if conversion failed (optional)
                if (string.IsNullOrWhiteSpace(convertedDate))
                    continue;

                await _db.InsertAsync(new SongHistory
                {
                    CcliNum = mysonghist.CcliNum,
                    SongDate = convertedDate
                });
            }
        }

        private string ConvertToShortDate(string rawDate)
        {
            if (string.IsNullOrWhiteSpace(rawDate))
                return DateTime.Today.ToString("yyyy-MM-dd");

            if (DateTime.TryParse(rawDate, out var parsed))
                return parsed.ToString("yyyy-MM-dd");

            // Fallback if parsing fails
            return DateTime.Today.ToString("yyyy-MM-dd");
        }

        public async Task RemoveFromMySetAsync(int songId)
        {
            await Init();
            await _db.Table<SongSet>().DeleteAsync(s => s.SongId == songId);
        }
        public async Task RemoveFromMasterAsync(int songId)
        {
            await Init();
            await _db.Table<Song>().DeleteAsync(s => s.SongId == songId);
        }

        public async Task ClearMySetAsync()
        {
            await Init();
            await _db.DeleteAllAsync<SongSet>().ConfigureAwait(false);
            // 2. Reset the auto-increment counter
            await _db.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name='SongSet'").ConfigureAwait(false);

            // 3. Optional: Vacuum the database to reclaim space immediately
            await _db.ExecuteAsync("VACUUM").ConfigureAwait(false);
        }

        public async Task ClearMasterListAsync()
        {
            await Init();
            await _db.DeleteAllAsync<Song>().ConfigureAwait(false);
        }


    }
}