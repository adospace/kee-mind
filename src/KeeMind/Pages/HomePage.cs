using KeeMind.Resources;
using KeeMind.Services;
using KeeMind.Services.Data;
using System;
using System.Collections.Generic;

namespace KeeMind.Pages;

class HomePageState
{
}


class HomePage : Component<HomePageState>
{
    Action? _openFlyoutAction;

    public HomePage OnOpenFlyout(Action openFlyoutAction)
    {
        _openFlyoutAction = openFlyoutAction;
        return this;
    }


    public override VisualNode Render()
    {
        return new NavigationPage()
        {
            new CardsPage()
                .OnOpenFlyout(_openFlyoutAction)
        }
        
        .BackgroundColor(Theme.Current.WhiteColor);
    }

}