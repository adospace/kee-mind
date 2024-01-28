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
using System.Collections.ObjectModel;
using ReactorData;

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
    
    public EditEntryPageProps? CurrentEditCardPros { get; set; }


}

class MainParameters
{
    public ObservableCollection<IndexedModel<Card>> Cards { get; set; } = new();

    public ObservableCollection<IndexedModel<Card>> SortedAndFilteredCards { get; set; } = new();

    public SortedDictionary<int, Tag> FilterTags { get; set; } = new();

    public SortedDictionary<int, Tag> AllTags { get; set; } = new();

    public bool ShowFavoritesOnly { get; set; }

    public void Refresh()
    {
        for (int i = 0; i < Cards.Count; i++)
        {
            Cards[i].Index = i;
        }

        while (SortedAndFilteredCards.Count > 0)
        {
            SortedAndFilteredCards.RemoveAt(0);
        }

        var cards = FilterTags.Count > 0 ?
            Cards.Where(_ => _.Model.Tags.Any(x => FilterTags.ContainsKey(x.Tag.Id)))
            :
            Cards;

        if (ShowFavoritesOnly)
        {
            cards = cards.Where(_ => _.Model.IsFavorite);
        }

        foreach (var cardToInsert in cards)
        {
            SortedAndFilteredCards.Add(cardToInsert);
        }

        AllTags = new SortedDictionary<int, Tag>(
            Cards
            .SelectMany(_ => _.Model.Tags)
            .Select(_ => _.Tag)
            .GroupBy(_ => _.Id)
            .ToDictionary(_ => _.Key, _ => _.First()));
    }
}

partial class MainPage : Component<MainPageState>
{
    [Inject]
    IModelContext _modelContext;

    [Inject]
    IRepository _repository;

    [Param]
    IParameter<MainParameters> _cardsViewParameter;

    #region Initialization
    //private readonly IParameter<MainParameters> _cardsViewParameter;

    //public MainPage()
    //{
    //    _cardsViewParameter = CreateParameter<MainParameters>();
    //}

    protected override void OnMounted()
    {
        if (!_repository.ArchiveExists())
        {
            State.CurrentPage = PageEnum.CreateLocalArchive;
        }
        else
        {
            State.CurrentPage = PageEnum.Login;
        }

        base.OnMounted();
    }

    private void UpdateStatusBarAppearance()
    {
#if ANDROID
        MainActivity.SetWindowTheme(State.CurrentPage == PageEnum.Login || State.CurrentPage == PageEnum.CreateLocalArchive ? false : true);
#endif
    }
    #endregion

    #region Render
    public override VisualNode Render()
    {
        if (Microsoft.Maui.Devices.DeviceInfo.Idiom == Microsoft.Maui.Devices.DeviceIdiom.Phone)
        {
            return RenderMobileLayout();
        }
        else
        {
            return RenderBody();
        }
    }

    VisualNode RenderMobileLayout()
    {
        return new FlyoutPage
        {
            RenderBody()
        }
        .IsGestureEnabled(State.CurrentPage == PageEnum.Home)
        .Flyout(new ContentPage("KeeMind")
        {
            RenderFlyoutBody()
        })
        .IsPresented(() => State.IsFlyoutOpen)
        .OnIsPresentedChanged(isPresented => State.IsFlyoutOpen = isPresented)
        .Set(MauiControls.NavigationPage.HasNavigationBarProperty, false)
        .OnAppearing(UpdateStatusBarAppearance)
        ;
    }

    VisualNode RenderFlyoutBody()
    {
        return new Grid("64 128 * 62", "*")
        {
            Theme.Current.ImageButton("close_white.png")
                .Aspect(Aspect.Center)
                .HStart()
                .WidthRequest(64)
                .BackgroundColor(Colors.Transparent)
                .OnClicked(()=>SetState(s => s.IsFlyoutOpen = false, invalidateComponent: false))
                .IsVisible(Microsoft.Maui.Devices.DeviceInfo.Idiom == Microsoft.Maui.Devices.DeviceIdiom.Phone),

            new Image("logo.png")
                .Aspect(Aspect.Center)
                .IsVisible(Microsoft.Maui.Devices.DeviceInfo.Idiom == Microsoft.Maui.Devices.DeviceIdiom.Desktop),

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
                        p.Refresh();
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
                    _cardsViewParameter.Set(p =>
                    {
                        p.ShowFavoritesOnly = true;
                        p.Refresh();
                    });
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
                    _cardsViewParameter.Value.AllTags.Select(RenderFlyoutTagItem)
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
        .BackgroundColor(Theme.Current.BlackColor);
    }

    VisualNode RenderFlyoutTagItem(KeyValuePair<int, Tag> tagItem)
    {
        return Theme.Current.Button(tagItem.Value.Name.ToUpper())
            .BackgroundColor(Theme.Current.AccentColor)
            .TextColor(Theme.Current.BlackColor)
            .Padding(12, 0)
            .HeightRequest(30)
            .Margin(0, 0, 10, 20)
            .OnClicked(() =>
            {
                _cardsViewParameter.Set(p =>
                {
                    p.FilterTags.Add(tagItem.Key, tagItem.Value);
                    p.Refresh();
                });
                SetState(s => s.IsFlyoutOpen = false);
            });
    }

    VisualNode RenderBody()
    {
        switch (State.CurrentPage)
        {
            case PageEnum.Login:
                return RenderLoginPage();
            case PageEnum.Home:
                return RenderHomePage();
            case PageEnum.CreateLocalArchive:
                return RenderCreateLocalArchive();
        }

        throw new InvalidOperationException();
    }

    VisualNode RenderCreateLocalArchive()
    {
        return new CreateArchivePage()
            .IsLocal(true)
            .OnArchiveCreated(() =>
            {
                SetState(s => s.CurrentPage = PageEnum.Home);
                UpdateStatusBarAppearance();
            });
        
    }

    VisualNode RenderHomePage()
    {
        if (Microsoft.Maui.Devices.DeviceInfo.Idiom == Microsoft.Maui.Devices.DeviceIdiom.Phone)
        {
            return new HomePage()
                .OnOpenFlyout(() => SetState(s => s.IsFlyoutOpen = true, invalidateComponent: false));
        }
        else
        {
            return new ContentPage("KeeMind")
            {
                new Grid("*", "300, 400, *")
                {
                    RenderFlyoutBody(),

                    new CardsPage()
                        .OnAddOrEditCard(OnAddOrEditCard)
                        .GridColumn(1),

                    //State.CurrentEditCardPros != null ?
                    //new EditCardPage(State.CurrentEditCardPros)
                    //    .GridColumn(2)
                    //:null,
                }
            }
            .WindowTitle("KeeMind");
        }
    }

    VisualNode RenderLoginPage()
    {
        return new LoginPage()
            .OnLoggedIn(cardList => SetState(s =>
            {
                s.CurrentPage = PageEnum.Home;
                _cardsViewParameter.Set(p =>
                {
                    p.Cards = new ObservableCollection<IndexedModel<Card>>(cardList);
                    //p.AllTags = new SortedDictionary<int, Tag>(
                    //    cardList
                    //    .SelectMany(_ => _.Model.Tags)
                    //    .Select(_ => _.Tag)
                    //    .GroupBy(_ => _.Id)
                    //    .ToDictionary(_ => _.Key, _ => _.First()));

                    p.Refresh();
                });

                UpdateStatusBarAppearance();
            }));
    }
    #endregion

    #region Events
    Task OnAddOrEditCard(Action<EditEntryPageProps> actionToGetProps)
    {
        var currentEditCardProps = new EditEntryPageProps();

        actionToGetProps(currentEditCardProps);

        currentEditCardProps.OnEditCanceled = (card) =>
        {
            if (card.EditMode == EditMode.New ||
                card.EditMode == EditMode.Deleted)
            {
                SetState(s =>
                {
                    s.CurrentEditCardPros = null;
                });
            }
        };

        SetState(s =>
        {
            s.CurrentEditCardPros = currentEditCardProps;
        });

        return Task.CompletedTask;
    }
    #endregion
}
