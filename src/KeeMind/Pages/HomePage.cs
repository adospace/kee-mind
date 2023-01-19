using KeeMind.Resources;
using KeeMind.Services;
using KeeMind.Services.Data;
using MauiReactor.Internals;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KeeMind.Pages;

class HomePageState
{
}


class HomePage : Component<HomePageState>
{
    Action? _openFlyoutAction;
    private MauiControls.NavigationPage? _navigationPage;

    public HomePage OnOpenFlyout(Action openFlyoutAction)
    {
        _openFlyoutAction = openFlyoutAction;
        return this;
    }


    public override VisualNode Render()
    {
        return new NavigationPage(navigationPage => _navigationPage = navigationPage)
        {
            new CardsPage()
                .OnOpenFlyout(_openFlyoutAction)
                .OnAddOrEditCard(OnAddOrEditCard)
        }
        
        .BackgroundColor(Theme.Current.WhiteColor);
    }

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
}