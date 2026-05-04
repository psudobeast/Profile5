using Microsoft.Extensions.Logging;
using Profile_assignment_5.Services;
using Profile_assignment_5.View;

namespace Profile_assignment_5
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

            // Register DatabaseService as Singleton
            builder.Services.AddSingleton<DatabaseService>();

            // Register Pages with Dependency Injection
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<ShoppingListPage>();
            builder.Services.AddTransient<ShoppingCartPage>();

            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
#if ANDROID
        handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#elif IOS || MACCATALYST
        handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
            });
#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
         }
    }
}
