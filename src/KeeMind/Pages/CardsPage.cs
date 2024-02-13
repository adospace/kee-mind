using KeeMind.Services.Data;
using KeeMind.Resources;
using KeeMind.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MauiReactor;
using MauiReactor.Internals;
using System.Collections.ObjectModel;
using ReactorData;
using MauiReactor.Parameters;
using KeeMind.Controls;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Devices;

namespace KeeMind.Pages;

class CardsPageState
{
    public bool IsLoading { get; set; }

    public IQuery<Card> Cards { get; set; } = default!;

}

partial class CardsPage : Component<CardsPageState>
{
    [Prop]
    Action? _onOpenFlyout;

    [Prop]
    Action<int>? _onEditCard;

    [Prop]
    Action? _onCreateCard;

    [Inject]
    IModelContext _modelContext;

    [Param]
    IParameter<MainParameters> _mainParameters;

    [Prop]
    int? _selectedCardId;

    protected override void OnMounted()
    {
        _modelContext.Load<Card>(_ => _.Include(x => x.Tags).ThenInclude(x => x.Tag));

        base.OnMounted();
    }

    protected override void OnMountedOrPropsChanged()
    {
        if (_mainParameters.Value.FilterTags.Count > 0)
        {
            State.Cards = _modelContext.Query<Card>(query =>
                query
                    .When(_mainParameters.Value.ShowFavoritesOnly, _ => _.Where(_ => _.IsFavorite))
                    .Where(_ => _.Tags.Any(x => _mainParameters.Value.FilterTags.ContainsKey(x.Tag.Id)))
                    .OrderBy(_ => _.Name));
        }
        else
        {
            State.Cards = _modelContext.Query<Card>(query =>
                query
                    .When(_mainParameters.Value.ShowFavoritesOnly, _ => _.Where(_ => _.IsFavorite))
                    .OrderBy(_ => _.Name));
        }

        base.OnMountedOrPropsChanged();
    }

    #region Render
    public override VisualNode Render()
        => DeviceInfo.Idiom == DeviceIdiom.Phone ?
            //Mobile layout
            new ContentPage
            {
                new StatusBarBehavior()
                    .StatusBarColor(Theme.Current.WhiteColor)
                    .StatusBarStyle(StatusBarStyle.DarkContent),

                RenderBody()
            }
            .Set(MauiControls.NavigationPage.HasNavigationBarProperty, false)
            :
            //Desktop layout
            RenderBody();


    VisualNode RenderBody()
        => Grid("64 Auto Auto *", "*",
            RenderTop(),

            RenderTagFilters(),

            RenderFavoriteOnlySwitch(),

            RenderEntryList(),

            ActivityIndicator()
                .GridRowSpan(3)
                .IsRunning(State.IsLoading)
                .HCenter()
                .VCenter()
                .BackgroundColor(Colors.Transparent)
                .Color(Theme.Current.BlackColor)
        )
        .BackgroundColor(Theme.Current.WhiteColor);

    CollectionView RenderEntryList()
        => CollectionView()
            .ItemsSource(State.Cards, RenderCardItem)
            .ItemSizingStrategy(MauiControls.ItemSizingStrategy.MeasureFirstItem)
            .GridRow(3);

    VisualNode RenderCardItem(Card cardModel)
        => Grid("64", "* Auto 42",
                Theme.Current.Label(cardModel.Name ?? string.Empty)
                    .VCenter()
                    .Margin(16,0),

                HStack(spacing: 5,
                    [.. cardModel.Tags.OrderBy(_=>_.Tag.Name).Select(RenderTag)]
                )
                .HeightRequest(24)
                .GridColumn(1),

                Image("right_black.png")
                    .GridColumn(2)
                    .Aspect(Aspect.Center)
                    .VCenter()
                    .HCenter(),

                DeviceInfo.Current.Idiom == DeviceIdiom.Desktop ? 
                Border()
                    .HeightRequest(2)
                    .VEnd()
                    .BackgroundColor(Theme.Current.DarkGrayColor)
                    .GridColumnSpan(3)
                    : null
            )            
            .When((DeviceInfo.Idiom == DeviceIdiom.Phone && State.Cards.IndexOf(cardModel) % 2 == 1) || _selectedCardId == cardModel.Id, grid => grid.BackgroundColor(Theme.Current.GrayColor))
            .OnTapped(()=>_onEditCard?.Invoke(cardModel.Id))
            ;
    

    VisualNode RenderTag(TagEntry tag)
        => Theme.Current.Button(tag.Tag.Name.ToUpper())
            .BackgroundColor(Theme.Current.BlackColor)
            .TextColor(Theme.Current.WhiteColor)
            .FontSize(12)
            .Padding(12, 0);
    

    VisualNode? RenderFavoriteOnlySwitch()
        => _mainParameters.Value.ShowFavoritesOnly ?
            Theme.ClosableButton(
                text: "SHOW FAVORITES ONLY",
                closeBeforeText: true,
                closeAction: () => _mainParameters.Set(p => p.ShowFavoritesOnly = false))
            .Margin(0, 0, 16, 16)
            .HEnd()
            .GridRow(2)
        : null;

    VisualNode RenderTagFilters() 
        => HScrollView(
            HStack(spacing: 5,
                [.. _mainParameters.Value.FilterTags
                    .OrderBy(_=>_.Value.Name)
                    .Select(RenderFilteredTagItem)]
            )
        )
        .Margin(16, 0, 0, 16)
        .GridRow(1);

    VisualNode RenderFilteredTagItem(KeyValuePair<int, Tag> tag)
        => Theme.ClosableButton(
            text: tag.Value.Name.ToUpper(),
            closeBeforeText: true,
            closeAction: () => _mainParameters.Set(p => p.FilterTags.Remove(tag.Key)));

    VisualNode RenderTop()
    {
        return Grid("64", "64 * 64",
            DeviceInfo.Idiom == DeviceIdiom.Phone ?
            Theme.Current.ImageButton("menu_black.png")
                .Aspect(Aspect.Center)
                .OnClicked(_onOpenFlyout)
                : null,

            Theme.Current.H1("Cards")
                .TextColor(Theme.Current.BlackColor)
                .GridColumn(1).VCenter().HCenter(),

            Theme.Current.ImageButton("plus_black.png")
                .Aspect(Aspect.Center)
                .OnClicked(_onCreateCard)
                .GridColumn(2)
        );
    }
#endregion
}

