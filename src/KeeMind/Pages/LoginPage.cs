using CommunityToolkit.Maui.Core;
using KeeMind.Controls;
using KeeMind.Pages.Components;
using KeeMind.Resources;
using KeeMind.Services;
using KeeMind.Services.Data;
using MauiReactor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KeeMind.Pages;

class LoginPageState
{
    public bool LoggingIn { get; set; }
}

partial class LoginPage : Component<LoginPageState>
{
    [Prop]
    private Action? _onLoggedIn;


    #region Render
    public override VisualNode Render() 
        =>  ContentPage(
            new StatusBarBehavior()
                .StatusBarColor(Theme.Current.BlackColor)
                .StatusBarStyle(StatusBarStyle.LightContent),

            RenderBody()
        )
        .WindowTitle("KeeMind")
        .BackgroundColor(Theme.Current.BlackColor);

    Grid RenderBody() 
        => Grid("* Auto", "*",
            VStack(spacing:15,
                Image("logox.png")
                    .Aspect(Aspect.Center),

                State.LoggingIn ?
                Theme.Current.Label("your password manager")
                    .TextColor(Theme.Current.WhiteColor)
                    .HCenter() : null
            )
            .Margin(0,60,0,0),

            !State.LoggingIn ?
            new PinBoard()
                .Label("Enter your PIN")
                .OnPinEntered(pin =>
                {
                    if (!State.LoggingIn)
                    {
                        SetState(s => s.LoggingIn = true);
                        Task.Run(async () => await OnPinEntered(pin));
                    }
                })
                .GridRow(1)
                :
            new BusyIndicator()
                .GridRowSpan(2)
        );
    #endregion

    #region Events
    async Task OnPinEntered(string pin)
    {
        var repository = Services.GetRequiredService<IRepository>();
        var db = await repository.TryOpenArchive(pin);
        if (db != null)
        {
            _onLoggedIn?.Invoke();
        }
        else if (MauiControls.Application.Current != null)
        {
            MauiControls.Application.Current.Dispatcher.Dispatch(async () =>
            {
                if (ContainerPage != null)
                {
                    await ContainerPage.DisplayAlert("KeeMind", "You entered a wrong PIN, please try again", "OK");
                }                
            });                

            SetState(s => s.LoggingIn = false);
        }
    }
    #endregion
}