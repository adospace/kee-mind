using KeeMind.Resources;
using KeeMind.Services;
using KeeMind.Services.Data;
using MauiReactor;
using MauiReactor.Internals;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KeeMind.Pages;

partial class HomePage : Component
{
    [Prop]
    Action? _onOpenFlyout;
    private MauiControls.Shell? _shell;

    protected override void OnMounted()
    {
        Routing.RegisterRoute<EditCardPage>();
        
        base.OnMounted();
    }


    #region Render
    public override VisualNode Render()
        => Shell(shell => _shell = shell,
            new CardsPage()
                .OnOpenFlyout(_onOpenFlyout)
                .OnCreateCard(OnAddCard)
                .OnEditCard(cardId => OnEditCard(cardId))
            );
    
    #endregion

    #region Events
    private async void OnAddCard()
    {
        await _shell!.GoToAsync<EditCardPage>();
    }

    private async void OnEditCard(int cardId)
    {
        await _shell!.GoToAsync<EditCardPage, EditEntryPageProps>(props => props.CardId = cardId);
    }
    #endregion
}