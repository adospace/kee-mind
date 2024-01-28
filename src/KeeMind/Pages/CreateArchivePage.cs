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

class CreateArchivePage : Component<CreateArchivePageState>
{
    #region Initialization
    bool _isLocal;
    Action? _archiveCreatedAction;

    public CreateArchivePage IsLocal(bool isLocal)
    {
        _isLocal = isLocal;
        return this;
    }

    public CreateArchivePage OnArchiveCreated(Action action)
    {
        _archiveCreatedAction = action;
        return this;
    }
    #endregion

    #region Render
    public override VisualNode Render()
    {
        return new ContentPage
        {
            RenderBody()
        }
        .WindowTitle("KeeMind")
        .BackgroundColor(Theme.Current.BlackColor);
    }

    VisualNode RenderBody()
    {
        switch (State.CurrentStep)
        {
            case CreateArchiveStep.Intro:
                return RenderIntro();
            case CreateArchiveStep.Pin:
            case CreateArchiveStep.PinConfirm:
                return RenderPinBoard();
            case CreateArchiveStep.Busy:
                return RenderIsBusy();
        }

        throw new InvalidOperationException();
    }

    VisualNode RenderIntro()
    {
        return new VerticalStackLayout
        {
            new Image("logox.png")
                .Margin(0,0,0,50)
                .Aspect(Aspect.Center),

            Theme.Current.Label("Save your secretes, no account, no internet, no ads, export anytime!")
                .TextColor(Theme.Current.WhiteColor)
                .HCenter()
                .HorizontalTextAlignment(TextAlignment.Center),

            Theme.Current.TransparentButton("Get started >")
                .HCenter()
                .FontSize(24)
                .OnClicked(()=>SetState(s => s.CurrentStep = CreateArchiveStep.Pin))
        }
        .VCenter()
        .Spacing(30);
    }

    VisualNode RenderPinBoard()
    {
        return new Grid("* Auto", "*")
        {
            new VerticalStackLayout
            {
                new Image("logo.png")
                    .Aspect(Aspect.Center)
                    .Margin(0,30,0,0),

                Theme.Current.H1("Local Archive")
                    .HCenter(),
            }
            .Spacing(60),


            new PinBoard()
                .Label(State.CurrentStep == CreateArchiveStep.Pin ? "Please create a PIN" : "Please confirm your PIN")
                .OnPinEntered(pin => Task.Run(async ()=> await OnPinEntered(pin)))
                .GridRow(1)
        };
    }

    VisualNode RenderIsBusy()
    {
        return new Grid("*", "*")
        {
            new VStack(spacing:15)
            {
                new Image("logox.png")
                    .Aspect(Aspect.Center),

                Theme.Current.Label("your password manager")
                    .TextColor(Theme.Current.WhiteColor)
                    .HCenter()
            }
            .Margin(0,60,0,0),

            new BusyIndicator()
        };
    }
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

        _archiveCreatedAction?.Invoke();
    }
    #endregion
}
