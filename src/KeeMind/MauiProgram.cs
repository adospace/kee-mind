using KeeMind.Controls;
using KeeMind.Pages;
using KeeMind.Services;
using KeeMind.Services.Data;
using MauiReactor;
using ReactorData.EFCore;

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
            .OnMauiReactorUnhandledException(ex => 
            {
                System.Diagnostics.Debug.WriteLine(ex);
            })
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

        //ReactorData
        builder.Services.AddReactorDataWithEfCore<DatabaseContext>(
            modelContextConfigure: options =>
            {
                options.Dispatcher = action =>
                {
                    if (MauiControls.Application.Current?.Dispatcher.IsDispatchRequired == true)
                    {
                        MauiControls.Application.Current?.Dispatcher.Dispatch(action);
                    }
                };

                //options.ConfigureContext = context =>
                //{
                //    context.Load<Card>();
                //    context.Load<Tag>();
                //};
            });


        return builder.Build();
    }
}


