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

namespace KeeMind.Pages;

class CardsPageState
{
    public bool IsLoading { get; set; }
}

class CardsPage : Component<CardsPageState>
{
    #region Initialization
    Action? _openFlyoutAction;
    Func<Action<EditEntryPageProps>, Task>? _addOrEditCardAction;

    public CardsPage OnOpenFlyout(Action? openFlyoutAction)
    {
        _openFlyoutAction = openFlyoutAction;
        return this;

    }

    public CardsPage OnAddOrEditCard(Func<Action<EditEntryPageProps>, Task>? addOrEditCardAction)
    {
        _addOrEditCardAction = addOrEditCardAction;
        return this;
    }
    #endregion

    #region Render
    public override VisualNode Render()
    {
        if (Microsoft.Maui.Devices.DeviceInfo.Idiom == Microsoft.Maui.Devices.DeviceIdiom.Phone)
        {
            return new ContentPage
            {
                RenderBody()
            }
            .Set(MauiControls.NavigationPage.HasNavigationBarProperty, false);
        }
        else
        {
            return RenderBody();
        }
    }

    VisualNode RenderBody()
    {
        return new Grid("64 Auto *", "*")
        {
            RenderTop(),

            RenderTagFilters(),

            RenderEntryList(),

            new ActivityIndicator()
                .GridRowSpan(3)
                .IsRunning(State.IsLoading)
                .HCenter()
                .VCenter()
                .BackgroundColor(Colors.Transparent)
                .Color(Theme.Current.BlackColor)
        };
    }

    VisualNode RenderEntryList()
    {
        var cardsViewParameters = GetParameter<MainParameters>();

        var cards = cardsViewParameters.Value.FilterTags.Count > 0 ?
            cardsViewParameters.Value.Cards.Where(_ => _.Model.Tags.Any(x => cardsViewParameters.Value.FilterTags.ContainsKey(x.Tag.Id)))
            :
            cardsViewParameters.Value.Cards;

        if (cardsViewParameters.Value.ShowFavoritesOnly)
        {
            cards = cards.Where(_ => _.Model.IsFavorite);
        }

        System.Diagnostics.Debug.WriteLine($"RenderEntryList()");

        return new CollectionView()
            .ItemsSource(cards.ToList(), RenderCardItem)
            .ItemSizingStrategy(MauiControls.ItemSizingStrategy.MeasureFirstItem)
            .GridRow(2);
    }

    VisualNode RenderCardItem(IndexedModel<Card> cardModel)
    {
        System.Diagnostics.Debug.WriteLine($"RenderCardItem({cardModel.Index})");
        return new Grid("64", "* Auto 42")
        {
            Theme.Current.Label(cardModel.Model.Name ?? string.Empty)
                .VCenter()
                .Margin(16,0),

            new HStack(spacing: 5)
            {
                cardModel.Model.Tags.OrderBy(_=>_.Tag.Name).Select(RenderTag)
            }
            .HeightRequest(24)
            .GridColumn(1),

            new Image("right_black.png")
                .GridColumn(2)
                .Aspect(Aspect.Center)
                .VCenter()
                .HCenter()
        }
        .When(cardModel.Index % 2 == 1, grid => grid.BackgroundColor(Theme.Current.GrayColor))
        .OnTapped(()=>OnEditCard(cardModel));
    }

    VisualNode RenderTag(TagEntry tag)
    {
        return Theme.Current.Button(tag.Tag.Name.ToUpper())
            .BackgroundColor(Theme.Current.BlackColor)
            .TextColor(Theme.Current.WhiteColor)
            .FontSize(12)
            .Padding(12, 0);
    }

    VisualNode RenderTagFilters()
    {
        var cardsViewParameters = GetParameter<MainParameters>();
        return new ScrollView
        {
            new HStack(spacing: 5)
            {
                cardsViewParameters.Value.FilterTags.Select(RenderFilteredTagItem)
            }
        }
        .Orientation(ScrollOrientation.Horizontal)
        .Margin(16, 0, 0, 16)
        .GridRow(1);
    }

    VisualNode RenderFilteredTagItem(KeyValuePair<int, Tag> tag)
    {
        void RemoveTag()
        {
            var cardsViewParameters = GetParameter<MainParameters>();
            cardsViewParameters.Set(p => p.FilterTags.Remove(tag.Key));
        };

        return new Grid("Auto", "Auto, Auto, *")
        {
            Theme.Current.Button(string.Empty)
                .HFill()
                .VFill()
                .FontSize(12)
                .Padding(2)
                .OnClicked(RemoveTag)
                .BackgroundColor(Theme.Current.DarkGrayColor)
                .GridColumnSpan(3),

            new Image("close_white.png")
                .VCenter()
                .HCenter()
                .Margin(5,0,0,0)
                .OnTapped(RemoveTag)
                .WidthRequest(10),

            Theme.Current.Label(tag.Value.Name.ToUpper())
                .VCenter()
                .FontSize(12)
                .Margin(5,0)
                .TextColor(Theme.Current.WhiteColor)
                .OnTapped(RemoveTag)
                .GridColumn(1),
        };
    }

    VisualNode RenderTop()
    {
        return new Grid("64", "64 * 64")
        {
            Microsoft.Maui.Devices.DeviceInfo.Idiom == Microsoft.Maui.Devices.DeviceIdiom.Phone ?
            Theme.Current.ImageButton("menu_black.png")
                .Aspect(Aspect.Center)
                .OnClicked(_openFlyoutAction)
                : null,

            Theme.Current.H1("Cards")
                .TextColor(Theme.Current.BlackColor)
                .GridColumn(1).VCenter().HCenter(),

            Theme.Current.ImageButton("plus_black.png")
                .Aspect(Aspect.Center)
                .OnClicked(OnAddCard)
                .GridColumn(2),
        };
    }
    #endregion

    #region Events
    async void OnAddCard()
    {
        if (Navigation == null)
            return;

        Validate.EnsureNotNull(_addOrEditCardAction);
        
        var cardsViewParameters = GetParameter<MainParameters>();

        await _addOrEditCardAction.Invoke(props =>
        {
            props.OnCardAdded = (Card cardModel) => 
            {
                cardsViewParameters.Set(p =>
                {
                    p.Cards.Add(new IndexedModel<Card>(cardModel, p.Cards.Count));
                    p.Cards = p.Cards.OrderBy(_=>_.Model.Name).ToList();

                    p.RefreshAllTags();

                });
            };
            props.OnCardRemoved = (Card removedCard) => 
            {
                cardsViewParameters.Set(p =>
                {
                    var indexOfModifiedCard = p.Cards.FindIndex(_ => _.Model.Id == removedCard.Id);
                    p.Cards.RemoveAt(indexOfModifiedCard);

                    p.RefreshAllTags();

                });
            };
            props.OnClose = () =>
            {
                Invalidate();
            };
        });
    }

    async void OnEditCard(IndexedModel<Card>? cardModel)
    {
        if (cardModel == null)
            return;

        if (Navigation == null)
            return;


        Validate.EnsureNotNull(_addOrEditCardAction);
        var cardsViewParameters = GetParameter<MainParameters>();

        await _addOrEditCardAction.Invoke(props =>
        {
            props.CardId = cardModel.Model.Id;
            props.OnCardModified = (Card editedCard) =>
            { 
                cardsViewParameters.Set(p =>
                {
                    var indexOfModifiedCard = p.Cards.FindIndex(_ => _.Model.Id == editedCard.Id);
                    p.Cards[indexOfModifiedCard] =
                        new IndexedModel<Card>(editedCard, p.Cards[indexOfModifiedCard].Index);

                    p.RefreshAllTags();

                });            
            };
            props.OnCardRemoved = (Card removedCard)=> 
            {
                cardsViewParameters.Set(p =>
                {
                    var indexOfModifiedCard = p.Cards.FindIndex(_ => _.Model.Id == removedCard.Id);
                    p.Cards.RemoveAt(indexOfModifiedCard);

                    p.RefreshAllTags();

                });            
            };
            props.OnClose = () =>
            {
                Invalidate();
            };
        });
    }
    #endregion
}

