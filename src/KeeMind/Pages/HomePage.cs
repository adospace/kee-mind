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

    private MauiControls.NavigationPage? _navigationPage;


    #region Render
    public override VisualNode Render()
    {
        return new NavigationPage(navigationPage => _navigationPage = navigationPage)
        {
            new CardsPage()
                .OnOpenFlyout(_onOpenFlyout)
                .OnAddOrEditCard(OnAddOrEditCard)
        }
        
        .BackgroundColor(Theme.Current.WhiteColor);
    }
    #endregion

    #region Events
    private async Task OnAddOrEditCard(Action<EditEntryPageProps> actionToGetProps)
    {
        if (_navigationPage == null)
        {
            return;
        }

        await _navigationPage.Navigation.PushAsync<EditCardPage, EditEntryPageProps>(props =>
        {
            actionToGetProps(props);
        });
    }
    #endregion
}