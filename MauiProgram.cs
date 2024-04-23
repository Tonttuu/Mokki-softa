using Microsoft.Extensions.Logging;

namespace Mokki_softa
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            var appSettings = ConfigurationProvider.GetAppSettings();
            builder.Services.AddSingleton(new DatabaseConnector(appSettings));

            return builder.Build();
        }
    }
}
