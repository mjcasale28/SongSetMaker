namespace SongSetMaker; 

public static class Constants
{
    public const string DatabaseFilename = "WorshipSongs.db3";

    // This is a computed property (not const, because FileSystem.AppDataDirectory is runtime)
    // It's the most common and clean pattern in MAUI apps
    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

    // Optional: add other app-wide constants here
    public const string AppName = "SongSetMaker";
    public const string BackupFileExtension = ".db";
    public const string DateFormat = "yyyy-MM-dd";
}