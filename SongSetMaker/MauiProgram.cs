using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SongSetMaker.Services;
using SongSetMaker.ViewModels;
using SongSetMaker.Views;

namespace SongSetMaker
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit(options =>
                {
                    options.SetShouldEnableSnackbarOnWindows(true);
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // Services
            builder.Services.AddSingleton<IDatabaseService, DatabaseService>();

            // ViewModels
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<MySetViewModel>();

            // Pages are auto-registered via Shell, but you can also do:
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MySetPage>();


            return builder.Build();
        }
    }
}