using KeeMind.Services.Data;
using KeeMind.Resources;
using KeeMind.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiReactor;
using MauiReactor.Parameters;

namespace KeeMind.Pages;

enum PageEnum
{
    CreateLocalArchive,

    Login,

    Home,
}

class MainPageState
{
    public PageEnum CurrentPage { get; set; }

    public bool IsFlyoutOpen { get; set; }
    
}

class CardsViewParameters
{
    public List<IndexedModel<Card>> Cards { get; set; } = new();

    public SortedDictionary<int, Tag> FilterTags { get; set; } = new();

    public SortedDictionary<int, Tag> AllTags { get; set; } = new();

    public bool ShowFavoritesOnly { get; set; }
}

class MainPage : Component<MainPageState>
{
    private readonly IParameter<CardsViewParameters> _cardsViewParameter;

    public MainPage()
    {
        _cardsViewParameter = CreateParameter<CardsViewParameters>();
    }

    protected override void OnMounted()
    {
        var repository = Services.GetRequiredService<IRepository>();

        if (!repository.ArchiveExists())
        {
            State.CurrentPage = PageEnum.CreateLocalArchive;
        }
        else
        {
            State.CurrentPage = PageEnum.Login;
        }

        base.OnMounted();
    }

    private void UpdateStatusBarAppereance()
    {
#if ANDROID
        MainActivity.SetWindowTheme(State.CurrentPage == PageEnum.Login || State.CurrentPage == PageEnum.CreateLocalArchive ? false : true);
#endif
    }

    public override VisualNode Render()
    {
        return new FlyoutPage
        {
            RenderBody()
        }
        .IsGestureEnabled(State.CurrentPage == PageEnum.Home)
        .Flyout(RenderFlyout())
        .IsPresented(() => State.IsFlyoutOpen)
        .OnIsPresentedChanged(isPresented => State.IsFlyoutOpen = isPresented)
        .Set(MauiControls.NavigationPage.HasNavigationBarProperty, false)
        .OnAppearing(UpdateStatusBarAppereance)
        ;
    }


    private VisualNode RenderFlyout()
    {
        return new ContentPage("KeeMind")
        {
            new Grid("64 128 * 62", "*")
            {
                Theme.Current.ImageButton("close_white.png")
                    .Aspect(Aspect.Center)
                    .HStart()
                    .WidthRequest(64)
                    .BackgroundColor(Colors.Transparent)
                    .OnClicked(()=>SetState(s => s.IsFlyoutOpen = false, invalidateComponent: false)),

                new VStack(spacing:24)
                {
                    new Grid("*", "20,*")
                    {
                        new Image("home_white.png")
                            .HCenter(),

                        Theme.Current.H3("All Cards")
                            .GridColumn(1)
                            .VCenter()
                            .Margin(12,0,0,0)
                            .TextColor(Theme.Current.WhiteColor)
                    }
                    .OnTapped(()=> SetState(s =>
                    {
                        _cardsViewParameter.Set(p =>
                        {
                            p.ShowFavoritesOnly = false;
                            p.FilterTags.Clear();
                        });

                        s.IsFlyoutOpen = false;
                    })),

                    new Grid("*", "20,*")
                    {
                        new Image("favorites_white.png")
                            .HCenter(),

                        Theme.Current.H3("Favorites")
                            .GridColumn(1)
                            .VCenter()
                            .Margin(12,0,0,0)
                            .TextColor(Theme.Current.WhiteColor)
                    }
                    .OnTapped(()=> SetState(s => 
                    {
                        _cardsViewParameter.Set(p => p.ShowFavoritesOnly = true);
                        s.IsFlyoutOpen = false;
                    })),
                }
                .Padding(16,0)
                .GridRow(1)
                .VEnd(),
                
                new VStack(spacing: 15)
                {
                    Theme.Current.Label("TAGS")
                        .GridColumn(1)
                        .VCenter()
                        .TextColor(Theme.Current.WhiteColor),
                    
                    new FlexLayout
                    {
                        _cardsViewParameter.Value.AllTags.Select(RenderFlayoutTagItem)
                    }
                    .Wrap(Microsoft.Maui.Layouts.FlexWrap.Wrap)
                }
                .GridRow(2)
                .Margin(16,40,16,0),

                new Grid("*", "20,*")
                {
                    new Image("gear_white.png")
                        .HCenter(),

                    Theme.Current.H3("Settings")
                        .GridColumn(1)
                        .VCenter()
                        .Margin(12,0,0,0)
                        .TextColor(Theme.Current.WhiteColor)
                }
                .Padding(16,0)
                .BackgroundColor(Theme.Current.DarkBlackColor)
                .GridRow(3)
            }
        }
        .BackgroundColor(Theme.Current.BlackColor);
    }

    private VisualNode RenderFlayoutTagItem(KeyValuePair<int, Tag> tagItem)
    {
        return Theme.Current.Button(tagItem.Value.Name.ToUpper())
            .BackgroundColor(Theme.Current.AccentColor)
            .TextColor(Theme.Current.BlackColor)
            .Padding(12, 0)
            .HeightRequest(30)
            .Margin(0, 0, 10, 20)
            .OnClicked(() =>
            {
                _cardsViewParameter.Set(p => p.FilterTags.Add(tagItem.Key, tagItem.Value));
                SetState(s => s.IsFlyoutOpen = false);
            });
    }

    private VisualNode RenderBody()
    {
        switch (State.CurrentPage)
        {
            //case PageEnum.Startup:
            //    return RenderStartupPage();
            //case PageEnum.Banner:
            //    return RenderBannerPage();
            case PageEnum.Login:
                return RenderLoginPage();
            case PageEnum.Home:
                return RenderHomePage();
            case PageEnum.CreateLocalArchive:
                return RenderCreateLocalArchive();
        }


        throw new InvalidOperationException();
    }

    private VisualNode RenderCreateLocalArchive()
    {
        return new CreateArchivePage()
            .IsLocal(true)
            .OnArchiveCreated(() =>
            {
                SetState(s => s.CurrentPage = PageEnum.Home);
                UpdateStatusBarAppereance();
            });
        
    }

    private VisualNode RenderHomePage()
    {
        return new HomePage()
            .OnOpenFlyout(() => SetState(s => s.IsFlyoutOpen = true, invalidateComponent: false))
            ;
    }

    private VisualNode RenderLoginPage()
    {
        return new LoginPage()
            .OnLoggedIn(cardList => SetState(s =>
            {
                s.CurrentPage = PageEnum.Home;
                _cardsViewParameter.Set(p =>
                {
                    p.Cards = cardList;
                    p.AllTags = new SortedDictionary<int, Tag>(
                        cardList
                        .SelectMany(_ => _.Model.Tags)
                        .Select(_ => _.Tag)
                        .GroupBy(_ => _.Id)
                        .ToDictionary(_ => _.Key, _ => _.First()));
                });

                UpdateStatusBarAppereance();
            }));
    }

    //private VisualNode RenderBannerPage()
    //{
    //    return new BannerPage();
    //}

    //private VisualNode RenderStartupPage()
    //{
    //    return new ContentPage
    //    {
    //        new ActivityIndicator()
    //            .VCenter()
    //            .HCenter()
    //    };
    //}

}
