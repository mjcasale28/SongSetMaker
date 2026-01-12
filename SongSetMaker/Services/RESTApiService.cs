using SongSetMaker.Models;
using System.Buffers.Text;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SongSetMaker.Services
{
    /// <summary>
    ///     This class supports interactions with the HCF Google Sheet
    ///     Main actions
    ///     GET
    ///     1. action=getlist    (Master Song List)
    ///     2. action=getset     (Song Set List)
    ///     3. action=gethistory (Song History Information)
    ///     POST
    ///     4. action=addsong    (Add Song to Song Set List)
    ///     5. action=addmaster  (Add Song to Master Song list)
    ///     6. action=deletesong (Delete Song from Song Set List)
    ///     7. action=deleteset  (Delete Song Set)
    ///     8. action=updatetheme (Update Song Theme in Master List)
    /// </summary>
    public class RESTApiService
    {
        // Prod - Android folder
        private readonly string baseUrl = "https://script.google.com/macros/s/AKfycbwo5kdALs20XJtuCwMuzV6WKh_YX_sn9ogZzjGQDuPzDWz7H_bJybVOU_jAwqJy8ZEZ/exec";
        // Dev - Android Dev
        // private readonly string baseUrl = "https://script.google.com/macros/s/AKfycbz_C9boRjenjaOG86gii8-_NlEtSVFmkviSLlb9cRf1hZmPXwhWMSPexodyfjaE0zo4Jw/exec";
        public string StatusMessage;
        public string songList;
        public List<Song> masterSongList;
        public List<SongSet> songSetList;
        public List<SongHistory> songHistoryList;
        private JsonSerializerOptions _serializerOptions;
        //private string songs;
        HttpClient client;

        public RESTApiService()
        {
            // songs = "";
            client = new HttpClient();
            masterSongList = [];
            songSetList = [];
            songHistoryList = [];
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
            };
        }
        public async Task<List<Song>> GetMasterSongs()
        {
            masterSongList.Clear();
            var res = await LoadMasterSongs();
            return masterSongList;
        }
        public async Task<string> LoadMasterSongs()
        {
            //masterSongList.Clear();

            await getSongsAPI(1);
            try
            {
                using (JsonDocument document = JsonDocument.Parse(songList))
                {
                    JsonElement root = document.RootElement;
                    JsonElement songsElement = root.GetProperty("songs");
                    if (songsElement.ValueKind == JsonValueKind.Array)
                    {
                        JsonArray jsonArray = [];
                        // Iterate through the elements of the JsonElement and add them to the JsonArray
                        foreach (JsonElement childElement in songsElement.EnumerateArray())
                        {
                            Song mysong = new Song();
                            mysong = parseSong(childElement);
                            masterSongList.Add(mysong);
                        }
                    }
                    StatusMessage = "Successfully retrieved data";
                }
            }
            catch (Exception ex)
            {
                masterSongList = [];
                StatusMessage = "Failed to retrieve data";
                return StatusMessage;
            }
            return StatusMessage;
        }
        /// <summary>
        ///   Get the song set by calling clearing out the list of songs
        ///   loading the songs from the API and returning the song set
        /// </summary>
        /// <returns></returns>
        public async Task<List<SongSet>> GetSongSet()
        {
            songSetList.Clear();
            var res = await LoadSongSet();
            return songSetList;
        }

        /// <summary>
        ///   REST API call to get the Song Set
        /// </summary>
        /// <returns></returns>
        public async Task<string> LoadSongSet()
        {
            await getSongsAPI(2);
            try
            {
                using (JsonDocument document = JsonDocument.Parse(songList))
                {
                    JsonElement root = document.RootElement;
                    JsonElement songsElement = root.GetProperty("songs");
                    if (songsElement.ValueKind == JsonValueKind.Array)
                    {
                        JsonArray jsonArray = new JsonArray();
                        // Iterate through the elements of the JsonElement and add them to the JsonArray
                        foreach (JsonElement childElement in songsElement.EnumerateArray())
                        {
                            SongSet mysong = new SongSet();
                            mysong = parseSongSet(childElement);
                            songSetList.Add(mysong);
                        }
                    }
                    StatusMessage = "Successfully retrieved data";
                }
            }
            catch (Exception ex)
            {
                songSetList = [];
                StatusMessage = "Failed to retrieve data";
                return StatusMessage;
            }
            return StatusMessage;
        }

        public async Task<List<SongHistory>> GetSongHistory()
        {
            songHistoryList.Clear();
            var res = await LoadSongHistory();
            return songHistoryList;
        }
        public async Task<string> LoadSongHistory()
        {
            await getSongsAPI(3);
            try
            {
                using (JsonDocument document = JsonDocument.Parse(songList))
                {
                    JsonElement root = document.RootElement;
                    JsonElement songsElement = root.GetProperty("songs");
                    if (songsElement.ValueKind == JsonValueKind.Array)
                    {
                        JsonArray jsonArray = new JsonArray();
                        // Iterate through the elements of the JsonElement and add them to the JsonArray
                        foreach (JsonElement childElement in songsElement.EnumerateArray())
                        {
                            SongHistory mysong = new SongHistory();
                            mysong = parseSongHistory(childElement);
                            songHistoryList.Add(mysong);
                        }
                    }
                    StatusMessage = "Successfully retrieved data";
                }
            }
            catch (Exception ex)
            {
                songHistoryList = [];
                StatusMessage = "Failed to retrieve data";
                return StatusMessage;
            }
            return StatusMessage;
        }

        public async Task<bool> UploadSongSetAsync(List<SongSet> songs)
        {
            if (songs == null || songs.Count == 0) return false;
            var url = $"{baseUrl}?action=addsong";

            // Build JSON object expected by Google Script

            var payload = new
            {
                songs = songs.Select(s => new
                {
                    title = s.Title,
                    songkey = s.Key,
                    songlead = s.LeadSinger,
                    tempo = s.Tempo,
                    timing = s.Timing,
                    artist = s.Artist,
                    cclinum = s.CcliNum,
                    youtube = s.YouTubeUrl,
                    chordsheet = s.ChordSheetUrl,
                    theme = s.Theme,
                    songdate = s.SongDate,
                    scripture = s.Scripture,
                    scriptureurl = s.ScriptureUrl
                }).ToList()
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                var response = await client.PostAsync(url, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Upload failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        ///   Helper method to parse Json content from Api for Master List of Songs
        /// </summary>
        /// <param name="songElement"></param>
        /// <returns></returns>
        private static Song parseSong(JsonElement songElement)
        {
            Song song = new Song();
            string? title = songElement.GetProperty("title").GetString();
            string? key = songElement.GetProperty("key").GetString();
            string? leadsinger = songElement.GetProperty("lead").GetString();
            int tempo = songElement.GetProperty("tempo").GetInt32();
            string? timing = songElement.GetProperty("timing").GetString();
            string? artist = songElement.GetProperty("artist").GetString();
            int cclinum = songElement.GetProperty("ccli_num").GetInt32();
            string? youtube = songElement.GetProperty("youtube").GetString();
            string? chordsheet = songElement.GetProperty("chordsheet").GetString();
            string? theme = songElement.GetProperty("theme").GetString();
            string? songdate = songElement.GetProperty("songdate").GetString();
            string? scripture = songElement.GetProperty("scripture").GetString();
            string? scriptureurl = songElement.GetProperty("scriptureurl").GetString();


            song.Artist = artist;
            song.Key = key;
            song.LeadSinger = leadsinger;
            song.CcliNum = cclinum;
            song.ChordSheetUrl = chordsheet;
            song.SongDate = songdate;
            song.Tempo = tempo;
            song.Theme = theme;
            song.Timing = timing;
            song.Title = title;
            song.YouTubeUrl = youtube;
            song.Scripture = scripture;
            song.ScriptureUrl = scriptureurl;

            return song;
        }
        /// <summary>
        ///    Helper method to parse Json content returned
        ///    from REST API of Songs from Song Set
        /// </summary>
        /// <param name="songElement"></param>
        /// <returns></returns>
        private SongSet parseSongSet(JsonElement songElement)
        {
            SongSet song = new SongSet();
            string? title = songElement.GetProperty("title").GetString();
            string? key = songElement.GetProperty("key").GetString();
            string? leadsinger = songElement.GetProperty("lead").GetString();
            int tempo = songElement.GetProperty("tempo").GetInt32();
            string? timing = songElement.GetProperty("timing").GetString();
            string? artist = songElement.GetProperty("artist").GetString();
            int cclinum = songElement.GetProperty("ccli_num").GetInt32();
            string? youtube = songElement.GetProperty("youtube").GetString();
            string? chordsheet = songElement.GetProperty("chordsheet").GetString();
            string? theme = songElement.GetProperty("theme").GetString();
            string? songdate = songElement.GetProperty("songdate").GetString();
            string? scripture = songElement.GetProperty("scripture").GetString();
            string? scriptureurl = songElement.GetProperty("scriptureurl").GetString();

            song.Artist = artist;
            song.Key = key;
            song.LeadSinger = leadsinger;
            song.CcliNum = cclinum;
            song.ChordSheetUrl = chordsheet;
            song.SongDate = songdate;
            song.Tempo = tempo;
            song.Theme = theme;
            song.Timing = timing;
            song.Title = title;
            song.YouTubeUrl = youtube;
            song.Scripture = scripture;
            song.ScriptureUrl = scriptureurl;

            return song;
        }
        /// <summary>
        ///   Helper method to parse Json content returned from 
        ///   Song History API
        /// </summary>
        /// <param name="songElement"></param>
        /// <returns></returns>
        private SongHistory parseSongHistory(JsonElement songElement)
        {
            SongHistory songhistory = new SongHistory();
            int cclinum = songElement.GetProperty("ccli_num").GetInt32();
            string? songdate = songElement.GetProperty("songdate").GetString();

            songhistory.CcliNum = cclinum;
            songhistory.SongDate = songdate;

            return songhistory;
        }


        // Method to convert JsonElement to Dictionary<string, JsonElement>
        static Dictionary<string, JsonElement> ConvertJsonElementToDictionary(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                // Create a Dictionary to store the key-value pairs
                Dictionary<string, JsonElement> jsonObject = new Dictionary<string, JsonElement>();

                // Iterate through the properties of the JsonElement
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    jsonObject[property.Name] = property.Value;
                }

                return jsonObject;
            }
            else
            {
                throw new InvalidOperationException("The provided JsonElement is not an object.");
            }
        }

        public async Task getSongsAPI(int i_action)
        {
            string action = "";
            switch (i_action)
            {
                case 1:
                    action = "?action=getlist";
                    break;
                case 2:
                    action = "?action=getset";
                    break;
                case 3:
                    action = "?action=gethistory";
                    break;
            }

            string apiUrl = baseUrl + action;
            // Create an instance of HttpClient
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Make a GET request to the API
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    // Check if the request was successful (status code 200)
                    if (response.IsSuccessStatusCode)
                    {
                        // Read and display the response content as a string
                        string content = await response.Content.ReadAsStringAsync();
                        SetSongList(content);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }
        } // End GetSongs from REST API

        public async Task getMasterSongsAPI()
        {
            string action = "?action=getlist";
            string apiUrl = baseUrl + action;
            // Create an instance of HttpClient
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Make a GET request to the API
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    // Check if the request was successful (status code 200)
                    if (response.IsSuccessStatusCode)
                    {
                        // Read and display the response content as a string
                        string content = await response.Content.ReadAsStringAsync();
                        SetSongList(content);
                        //   Console.WriteLine("Response from the server:");
                        //   Console.WriteLine(content);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }
        } // End GetSongs from REST API
        private void SetSongList(string _songList)
        {
            songList = _songList;
        }
        public string getSongList()
        {
            return songList;
        }
    }
}













