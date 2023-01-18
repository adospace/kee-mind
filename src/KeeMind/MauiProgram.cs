using KeeMind.Controls;
using KeeMind.Pages;
using KeeMind.Services;

namespace KeeMind;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiReactorApp<MainPage>()
#if DEBUG
            .EnableMauiReactorHotReload()
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Inter-Regular.ttf", "InterRegular");
                fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold");
            })
            ;

        builder.Services.AddKeeMindServices();
        builder.Services.AddKeeMindMauiServices();

        KeeMind.Controls.Native.BorderlessEntry.Configure();

        return builder.Build();
    }
}


