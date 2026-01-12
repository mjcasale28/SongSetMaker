using SQLite;


namespace SongSetMaker.Models
{
    [Table("SongHistory")]
    public class SongHistory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int CcliNum { get; set; }
        public string SongDate { get; set; } = string.Empty;
    }
}