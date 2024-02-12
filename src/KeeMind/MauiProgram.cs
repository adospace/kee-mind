using CommunityToolkit.Maui;
using KeeMind.Controls;
using KeeMind.Pages;
using KeeMind.Services;
using KeeMind.Services.Data;
using MauiReactor;
using Microsoft.Maui.Platform;
using ReactorData.EFCore;

namespace KeeMind;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiReactorApp<MainPage>()
            .UseMauiCommunityToolkit()
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

        //Controls.Native.BorderlessEntry.Configure();

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
                    else
                    {
                        action();
                    }
                };
            });

        RemoveBordersFromEntry();

        return builder.Build();
    }

    static void RemoveBordersFromEntry()
    {
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("Borderless", (handler, view) =>
        {
#if ANDROID
            handler.PlatformView.SetSelectAllOnFocus(true);
            handler.PlatformView.Background = null;
            handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
            handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
            handler.PlatformView.FocusChange += (s, e) =>
            {
                if (!e.HasFocus &&
                    Microsoft.Maui.ApplicationModel.Platform.CurrentActivity != null)
                    Microsoft.Maui.ApplicationModel.Platform.CurrentActivity.HideKeyboard(handler.PlatformView);
            };
#elif IOS || MACCATALYST                   
            handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
            handler.PlatformView.Layer.BorderWidth = 0;
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
            handler.PlatformView.EditingDidBegin += (s, e) =>
            {
                handler.PlatformView.PerformSelector(new ObjCRuntime.Selector("selectAll"), null, 0.0f);
            };
#elif WINDOWS        
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.Background = null;

            handler.PlatformView.GotFocus += (s, e) =>
            {
                handler.PlatformView.SelectAll();
            };
#endif

        });
    }
}


