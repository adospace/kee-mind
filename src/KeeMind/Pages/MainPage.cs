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
using Microsoft.Maui.Devices;

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

    public int? SelectedCardId { get; set; }

    public bool ShowCardEditor { get; set; }
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
        => DeviceInfo.Idiom == DeviceIdiom.Phone ?
            RenderMobileLayout()
            :
            RenderBody();

    VisualNode RenderMobileLayout()
        => FlyoutPage(
                RenderBody()
            )
            .IsGestureEnabled(State.CurrentPage == PageEnum.Home)
            .Flyout(ContentPage("KeeMind",
                RenderFlyoutBody()
                ))
            .IsPresented(() => State.IsFlyoutOpen)
            .OnIsPresentedChanged(isPresented => State.IsFlyoutOpen = isPresented)
            .Set(MauiControls.NavigationPage.HasNavigationBarProperty, false)
            .OnAppearing(UpdateStatusBarAppearance);
    

    VisualNode RenderFlyoutBody()
        => Grid("64 128 * 62", "*", 
            Theme.Current.ImageButton("close_white.png")
                .Aspect(Aspect.Center)
                .HStart()
                .WidthRequest(64)
                .BackgroundColor(Colors.Transparent)
                .OnClicked(()=>SetState(s => s.IsFlyoutOpen = false, invalidateComponent: false))
                .IsVisible(DeviceInfo.Idiom == DeviceIdiom.Phone),

            Image("logo.png")
                .Aspect(Aspect.Center)
                .IsVisible(DeviceInfo.Idiom == DeviceIdiom.Desktop),

            VStack(spacing:24,
                Grid("*", "20,*",
                    Image("home_white.png")
                        .HCenter(),

                    Theme.Current.H3("All Cards")

                        .GridColumn(1)
                        .VCenter()
                        .Margin(12,0,0,0)
                        .TextColor(Theme.Current.WhiteColor)
                )
                .OnTapped(()=> SetState(s =>
                {
                    _mainParameters.Set(p =>
                    {
                        p.ShowFavoritesOnly = false;
                        p.FilterTags.Clear();
                    });

                    s.IsFlyoutOpen = false;
                })),

                Grid("*", "20,*",
                    Image("favorites_white.png")
                        .HCenter(),

                    Theme.Current.H3("Favorites")
                        .GridColumn(1)
                        .VCenter()
                        .Margin(12,0,0,0)
                        .TextColor(Theme.Current.WhiteColor)
                )
                .OnTapped(()=> SetState(s =>
                {
                    _mainParameters.Set(p =>
                    {
                        p.ShowFavoritesOnly = true;
                    });
                    s.IsFlyoutOpen = false;
                }))
            )
            .Padding(16,0)
            .GridRow(1)
            .VEnd(),

            VStack(spacing: 15,
                Theme.Current.Label("TAGS")
                    .GridColumn(1)
                    .VCenter()
                    .TextColor(Theme.Current.WhiteColor),

                FlexLayout([.. State.Tags.Select(RenderFlyoutTagItem)])
                .Wrap(Microsoft.Maui.Layouts.FlexWrap.Wrap)
            )
            .GridRow(2)
            .Margin(16,40,16,0),

            Grid("*", "20,*",
                Image("gear_white.png")
                    .HCenter(),

                Theme.Current.H3("Settings")
                    .GridColumn(1)
                    .VCenter()
                    .Margin(12,0,0,0)
                    .TextColor(Theme.Current.WhiteColor)
            )
            .Padding(16,0)
            .BackgroundColor(Theme.Current.DarkBlackColor)
            .GridRow(3)
        )
        .BackgroundColor(Theme.Current.BlackColor);
    

    VisualNode RenderFlyoutTagItem(Tag tag)
        =>Theme.Current.Button(tag.Name.ToUpper())
            .BackgroundColor(Theme.Current.AccentColor)
            .TextColor(Theme.Current.BlackColor)
            .Padding(12, 0)
            .HeightRequest(30)
            .Margin(0, 0, 10, 20)
            .OnClicked(() =>
            {
                _mainParameters.Set(p => p.FilterTags.Add(tag.Id, tag));
                SetState(s => s.IsFlyoutOpen = false);
            });

    VisualNode RenderBody() 
        => State.CurrentPage switch
        {
            PageEnum.Login => RenderLoginPage(),
            PageEnum.Home => RenderHomePage(),
            PageEnum.CreateLocalArchive => RenderCreateLocalArchive(),
            _ => throw new InvalidOperationException(),
        };

    VisualNode RenderCreateLocalArchive() 
        => new CreateArchivePage()
            .IsLocal(true)
            .OnArchiveCreated(() =>
            {
                SetState(s => s.CurrentPage = PageEnum.Home);
                UpdateStatusBarAppearance();
            });

    VisualNode RenderHomePage() 
        => DeviceInfo.Idiom == DeviceIdiom.Phone ?
            new HomePage()
                .OnOpenFlyout(() => SetState(s => s.IsFlyoutOpen = true, invalidateComponent: false))
            :
            ContentPage("KeeMind",
                Grid("*", "300, 400, *",

                    RenderFlyoutBody(),

                    new CardsPage()
                        .OnEditCard(cardId => SetState(s =>
                        {
                            s.SelectedCardId = cardId;
                            s.ShowCardEditor = true;
                        }))
                        .OnCreateCard(()=> SetState(s =>
                        {
                            s.SelectedCardId = null;
                            s.ShowCardEditor = true;
                        }))
                        .GridColumn(1),

                State.ShowCardEditor ?
                new EditCardPage()
                    .CardId(State.SelectedCardId)
                    .GridColumn(2)
                :null
                )
            )
            .WindowTitle("KeeMind");

    VisualNode RenderLoginPage()
        => new LoginPage()
            .OnLoggedIn(() => SetState(s =>
            {
                s.CurrentPage = PageEnum.Home;

                _modelContext.Load<Tag>(_ => _.Where(x => x.Entries!.Any()));
            }));
    #endregion

}
