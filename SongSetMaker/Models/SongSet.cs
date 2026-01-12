using SQLite;


namespace SongSetMaker.Models
{
    [Table("MySongSet")]
    public class SongSet
    {
        [PrimaryKey, AutoIncrement]
        public int SongId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string LeadSinger { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public int Tempo { get; set; }
        public int CcliNum { get; set; }
        public string Timing { get; set; } = "4/4";
        public string Theme { get; set; } = string.Empty;
        public string YouTubeUrl { get; set; } = string.Empty;
        public string ChordSheetUrl { get; set; } = string.Empty;
        public string DateItemAdded { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");
        public string SongDate { get; set; } = string.Empty;
        public string Scripture { get; set; } = string.Empty;
        public string ScriptureUrl { get; set; } = string.Empty;
    }
}
