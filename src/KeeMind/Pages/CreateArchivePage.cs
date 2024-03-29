using KeeMind.Pages.Components;
using KeeMind.Resources;
using KeeMind.Services;
using MauiReactor;
using MauiReactor.Shapes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeeMind.Pages;

enum CreateArchiveStep
{
    Intro,

    Pin,

    PinConfirm,

    Busy
}

class CreateArchivePageState
{
    public CreateArchiveStep CurrentStep { get; set; }

    public string PIN { get; set; } = string.Empty;

    public string ConfirmedPIN { get; set; } = string.Empty;
}

partial class CreateArchivePage : Component<CreateArchivePageState>
{
    [Prop]
    bool _isLocal;

    [Prop]
    Action? _onArchiveCreated;

    #region Render
    public override VisualNode Render() 
        => ContentPage(
            RenderBody()
        )
        .WindowTitle("KeeMind")
        .BackgroundColor(AppTheme.Current.BlackColor);

    VisualNode RenderBody()
        => State.CurrentStep switch
        {
            CreateArchiveStep.Intro => RenderIntro(),
            CreateArchiveStep.Pin or CreateArchiveStep.PinConfirm => RenderPinBoard(),
            CreateArchiveStep.Busy => CreateArchivePage.RenderIsBusy(),
            _ => throw new InvalidOperationException(),
        };

    VStack RenderIntro() 
        => VStack(
            Image("logox.png")
                .Margin(0,0,0,50)
                .Aspect(Aspect.Center),

            AppTheme.Current.Label("Save your secretes, no account, no internet, no ads, export anytime!")
                .TextColor(AppTheme.Current.WhiteColor)
                .HCenter()
                .HorizontalTextAlignment(TextAlignment.Center),

            AppTheme.Current.TransparentButton("Get started >")
                .HCenter()
                .FontSize(24)
                .OnClicked(()=>SetState(s => s.CurrentStep = CreateArchiveStep.Pin))
        )
        .VCenter()
        .Spacing(30);

    Grid RenderPinBoard() 
        => Grid("* Auto", "*",
            VStack(
                new Image("logo.png")
                    .Aspect(Aspect.Center)
                    .Margin(0,30,0,0),

                AppTheme.Current.H1("Local Archive")
                    .HCenter()
            )
            .Spacing(60),


            new PinBoard()
                .Label(State.CurrentStep == CreateArchiveStep.Pin ? "Please create a PIN" : "Please confirm your PIN")
                .OnPinEntered(pin => Task.Run(async ()=> await OnPinEntered(pin)))
                .GridRow(1)
        );

    static Grid RenderIsBusy()
        => Grid("*", "*",
            VStack(spacing:15,
                Image("logox.png")
                    .Aspect(Aspect.Center),

                AppTheme.Current.Label("your password manager")
                    .TextColor(AppTheme.Current.WhiteColor)
                    .HCenter()
            )
            .Margin(0,60,0,0),

            new BusyIndicator()
        );
    #endregion

    #region Events
    async Task OnPinEntered(string pin)
    {
        if (State.CurrentStep == CreateArchiveStep.Pin)
        {
            State.PIN = pin;
            SetState(s => s.CurrentStep = CreateArchiveStep.PinConfirm);
        }
        else
        {
            State.ConfirmedPIN = pin;

            if (State.PIN == State.ConfirmedPIN)
            {
                SetState(s => s.CurrentStep = CreateArchiveStep.Busy);
                await CreateArchive();
            }
            else if (ContainerPage != null)
            {
                await ContainerPage.DisplayAlert("PIN mismatch", "You have entered a different PIN from previous page", "OK, restart");

                SetState(s =>
                {
                    s.PIN = s.ConfirmedPIN = string.Empty;
                    s.CurrentStep = CreateArchiveStep.Pin;
                });
            }
        }
    }

    async Task CreateArchive()
    {
        var repository = Services.GetRequiredService<IRepository>();

        await repository.CreateArchive(State.PIN);

        //await repository.TryOpenArchive(State.PIN);

        _onArchiveCreated?.Invoke();
    }
    #endregion
}
