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

class MainParameters
{
    public Dictionary<int, Tag> FilterTags { get; set; } = [];

    public bool ShowFavoritesOnly { get; set; }
}

class MainPageState
{
    public PageEnum CurrentPage { get; set; }

    public bool IsFlyoutOpen { get; set; }

    public IQuery<Tag> Tags { get; set; } = default!;
}

partial class MainPage : Component<MainPageState>
{
    [Inject]
    IModelContext _modelContext;

    [Inject]
    IRepository _repository;

    [Param]
    IParameter<MainParameters> _mainParameters;

    #region Initialization

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


        State.Tags = _modelContext.Query<Tag>(_ => _.OrderBy(x => x.Name));

        State.Tags.CollectionChanged += (s, e) => Invalidate();


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
                _mainParameters.Set(p =>
                {
                    p.ShowFavoritesOnly = false;
                    p.FilterTags.Clear();
                    //p.Refresh();
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
                _mainParameters.Set(p =>
                {
                    p.ShowFavoritesOnly = true;
                    //p.Refresh();
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
                State.Tags.Select(RenderFlyoutTagItem)
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

    VisualNode RenderFlyoutTagItem(Tag tag)
    {
        return Theme.Current.Button(tag.Name.ToUpper())
            .BackgroundColor(Theme.Current.AccentColor)
            .TextColor(Theme.Current.BlackColor)
            .Padding(12, 0)
            .HeightRequest(30)
            .Margin(0, 0, 10, 20)
            .OnClicked(() =>
            {
                _mainParameters.Set(p =>
                {
                    p.FilterTags.Add(tag.Id, tag);
                    //p.Refresh();
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
                    //.OnAddOrEditCard(OnAddOrEditCard)
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
            .OnLoggedIn(() => SetState(s =>
            {
                s.CurrentPage = PageEnum.Home;


                _modelContext.Load<Tag>(_ => _.Where(x => x.Entries!.Any()));

                //_cardsViewParameter.Set(p =>
                //{
                //    p.Cards = new ObservableCollection<IndexedModel<Card>>(cardList);
                //    //p.AllTags = new SortedDictionary<int, Tag>(
                //    //    cardList
                //    //    .SelectMany(_ => _.Model.Tags)
                //    //    .Select(_ => _.Tag)
                //    //    .GroupBy(_ => _.Id)
                //    //    .ToDictionary(_ => _.Key, _ => _.First()));

                //    p.Refresh();
                //});

                //UpdateStatusBarAppearance();
            }));
    }
    #endregion

    //#region Events
    //Task OnAddOrEditCard(Action<EditEntryPageProps> actionToGetProps)
    //{
    //    var currentEditCardProps = new EditEntryPageProps();

    //    actionToGetProps(currentEditCardProps);

    //    //currentEditCardProps.OnEditCanceled = (card) =>
    //    //{
    //    //    if (card.EditMode == EditMode.New ||
    //    //        card.EditMode == EditMode.Deleted)
    //    //    {
    //    //        SetState(s =>
    //    //        {
    //    //            s.CurrentEditCardPros = null;
    //    //        });
    //    //    }
    //    //};

    //    SetState(s =>
    //    {
    //        s.CurrentEditCardPros = currentEditCardProps;
    //    });

    //    return Task.CompletedTask;
    //}
    //#endregion
}
