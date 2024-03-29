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
    Action _onCreateCard;

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
                    .StatusBarColor(AppTheme.Current.WhiteColor)
                    .StatusBarStyle(StatusBarStyle.DarkContent),

                RenderBody()
            }
            .Set(MauiControls.NavigationPage.HasNavigationBarProperty, false)
            :
            //Desktop layout
            RenderBody();

    VisualNode RenderBody()
        => Grid("64 Auto *", "*",
            RenderTop(),

            RenderTagFilters(),

            RenderEntryList(),

            ActivityIndicator()
                .GridRowSpan(3)
                .IsRunning(State.IsLoading)
                .HCenter()
                .VCenter()
                .BackgroundColor(Colors.Transparent)
                .Color(AppTheme.Current.BlackColor)
        )
        .BackgroundColor(AppTheme.Current.WhiteColor);

    CollectionView RenderEntryList()
        => CollectionView()
            .ItemsSource(State.Cards, RenderCardItem)
            .ItemSizingStrategy(MauiControls.ItemSizingStrategy.MeasureFirstItem)
            .GridRow(2);

    VisualNode RenderCardItem(Card cardModel)
        => Grid("64", "* Auto 42",
                AppTheme.Current.Label(cardModel.Name ?? string.Empty)
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
                    .BackgroundColor(AppTheme.Current.DarkGrayColor)
                    .GridColumnSpan(3)
                    : null
            )            
            .When((DeviceInfo.Idiom == DeviceIdiom.Phone && State.Cards.IndexOf(cardModel) % 2 == 1) || _selectedCardId == cardModel.Id, grid => grid.BackgroundColor(AppTheme.Current.GrayColor))
            .OnTapped(()=>_onEditCard?.Invoke(cardModel.Id))
            ;
    

    VisualNode RenderTag(TagEntry tag)
        => AppTheme.Current.Button(tag.Tag.Name.ToUpper())
            .BackgroundColor(AppTheme.Current.BlackColor)
            .TextColor(AppTheme.Current.WhiteColor)
            .FontSize(12)
            .Padding(12, 0);
    

    VisualNode RenderFavoriteOnlySwitch()
    {
        return new ScrollView
        {
            new HStack(spacing: 5)
            {
                _mainParameters.Value.FilterTags
                    .OrderBy(_=>_.Value.Name)
                    .Select(RenderFilteredTagItem)
            }
        }
        .Orientation(ScrollOrientation.Horizontal)
        .Margin(16, 0, 0, 16)
        .GridRow(1);
    }

    VisualNode RenderTagFilters()
    {
        return new ScrollView
        {
            new HStack(spacing: 5)
            {
                _mainParameters.Value.FilterTags
                    .OrderBy(_=>_.Value.Name)
                    .Select(RenderFilteredTagItem)
            }
        }
        .Orientation(ScrollOrientation.Horizontal)
        .Margin(16, 0, 0, 16)
        .GridRow(1);
    }

    VisualNode RenderFilteredTagItem(KeyValuePair<int, Tag> tag)
    {
        void RemoveTag() => _mainParameters.Set(p => p.FilterTags.Remove(tag.Key));

        return new Grid("Auto", "Auto, Auto, *")
        {
            AppTheme.Current.Button(string.Empty)
                .HFill()
                .VFill()
                .FontSize(12)
                .Padding(2)
                .OnClicked(RemoveTag)
                .BackgroundColor(AppTheme.Current.DarkGrayColor)
                .GridColumnSpan(3),

            new Image("close_white.png")
                .VCenter()
                .HCenter()
                .Margin(5,0,0,0)
                .OnTapped(RemoveTag)
                .WidthRequest(10),

            AppTheme.Current.Label(tag.Value.Name.ToUpper())
                .VCenter()
                .FontSize(12)
                .Margin(5,0)
                .TextColor(AppTheme.Current.WhiteColor)
                .OnTapped(RemoveTag)
                .GridColumn(1),
        };
    }

    VisualNode RenderTop()
    {
        return Grid("64", "64 * 64",
            DeviceInfo.Idiom == DeviceIdiom.Phone ?
            AppTheme.Current.ImageButton("menu_black.png")
                .Aspect(Aspect.Center)
                .OnClicked(_onOpenFlyout)
                : null,

            AppTheme.Current.H1("Cards")
                .TextColor(AppTheme.Current.BlackColor)
                .GridColumn(1).VCenter().HCenter(),

            AppTheme.Current.ImageButton("plus_black.png")
                .Aspect(Aspect.Center)
                .OnClicked(_onCreateCard)
                .GridColumn(2)
        );
    }
#endregion
}

