
using KeeMind.Pages.Components;
using KeeMind.Resources;
using KeeMind.Services;
using KeeMind.Services.Data;
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

class LoginPage : Component<LoginPageState>
{
    #region Initialization
    private Action<List<IndexedModel<Card>>>? _loggedInAction;

    public LoginPage OnLoggedIn(Action<List<IndexedModel<Card>>> loggedInAction)
    {
        _loggedInAction = loggedInAction;
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
        return new Grid("* Auto", "*")
        {
            new VStack(spacing:15)
            {
                new Image("logox.png")
                    .Aspect(Aspect.Center),

                State.LoggingIn ?
                Theme.Current.Label("your password manager")
                    .TextColor(Theme.Current.WhiteColor)
                    .HCenter() : null,
            }
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
        };
    }
    #endregion

    #region Render
    async Task OnPinEntered(string pin)
    {
        var repository = Services.GetRequiredService<IRepository>();
        var db = await repository.TryOpenArchive(pin);
        if (db != null)
        {
            List<IndexedModel<Card>> cardList = new();
            int cardIndex = 0;

            await foreach (var card in db.Cards
                .Include(_ => _.Tags)
                .ThenInclude(_ => _.Tag)
                .OrderBy(_ => _.Name)
                .AsAsyncEnumerable())
            {
                cardList.Add(new IndexedModel<Card>(card, cardIndex));
                cardIndex++;
            }

            State.LoggingIn = false;
            _loggedInAction?.Invoke(cardList);
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