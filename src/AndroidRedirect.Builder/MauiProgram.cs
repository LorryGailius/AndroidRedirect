using Microsoft.Extensions.Logging;
using AndroidRedirect.Builder.Services;

namespace AndroidRedirect.Builder
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
                    fonts.AddFont("Inter-Regular.ttf", "InterRegular");
                    fonts.AddFont("Inter-Light.ttf", "InterLight");
                    fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold");
                });

            // Register services
            builder.Services.AddSingleton<IApplicationBuilderService, ApplicationBuilderService>();
            builder.Services.AddTransient<MainPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
