namespace SongSetMaker.Views;
using CommunityToolkit.Maui.Views;
public partial class AboutPopup : Popup
{
	public AboutPopup()
	{
		InitializeComponent();
	}
    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await CloseAsync(/* optional result, e.g. true */);
    }


    // === Correct handlers for the TapGestureRecognizers ===
    private void OnEmailTapped(object sender, TappedEventArgs e)
    {
        try
        {
            Launcher.Default.OpenAsync("mailto:lionheartpraise@gmail.com");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email tap failed: {ex.Message}");
            // You can show an alert here if you want
        }
    }

    private void OnYouTubeTapped(object sender, TappedEventArgs e)
    {
        try
        {
            Launcher.Default.OpenAsync("https://www.youtube.com/@lionheartpraise3697");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"YouTube tap failed: {ex.Message}");
        }
    }

    private void OnSoundCloudTapped(object sender, TappedEventArgs e)
    {
        try
        {
            Launcher.Default.OpenAsync("https://soundcloud.com/lionheartpraise"); 
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SoundCloud tap failed: {ex.Message}");
        }
    }
}