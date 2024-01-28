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
    Func<Action<EditEntryPageProps>, Task>? _onAddOrEditCard;

    [Inject]
    IModelContext _modelContext;

    protected override void OnMounted()
    {
        _modelContext.Load<Card>();
        _modelContext.Load<Tag>();

        State.Cards = _modelContext.Query<Card>(query => query.OrderBy(_ => _.Name));

        base.OnMounted();
    }

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
                .Color(Theme.Current.BlackColor)
        );

    VisualNode RenderEntryList()
        => CollectionView()
            .ItemsSource(State.Cards, RenderCardItem)
            .ItemSizingStrategy(MauiControls.ItemSizingStrategy.MeasureFirstItem)
            .GridRow(2);

    VisualNode RenderCardItem(Card cardModel)
    {
        //System.Diagnostics.Debug.WriteLine($"RenderCardItem({cardModel.Index})");
        return new Grid("64", "* Auto 42")
        {
            Theme.Current.Label(cardModel.Name ?? string.Empty)
                .VCenter()
                .Margin(16,0),

            new HStack(spacing: 5)
            {
                cardModel.Tags.OrderBy(_=>_.Tag.Name).Select(RenderTag)
            }
            .HeightRequest(24)
            .GridColumn(1),

            new Image("right_black.png")
                .GridColumn(2)
                .Aspect(Aspect.Center)
                .VCenter()
                .HCenter()
        }
        //.When(cardModel.Index % 2 == 1, grid => grid.BackgroundColor(Theme.Current.GrayColor))
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
            cardsViewParameters.Set(p =>
            {
                p.FilterTags.Remove(tag.Key);
                p.Refresh();
            });
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
                .OnClicked(_onOpenFlyout)
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

        Validate.EnsureNotNull(_onAddOrEditCard);

        var cardsViewParameters = GetParameter<MainParameters>();

        await _onAddOrEditCard.Invoke(props =>
        {
            props.OnCardAdded = (Card cardToAdd) =>
            {
                //cardsViewParameters.Set(p =>
                //{
                //    p.Cards.AddCard(cardToAdd);
                //    p.Refresh();

                //});
            };
            props.OnCardRemoved = (Card cardToRemove) =>
            {
                //cardsViewParameters.Set(p =>
                //{
                //    p.Cards.RemoveCard(cardToRemove.Id);
                //    p.Refresh();
                //});
            };
        });
    }

    async void OnEditCard(Card cardModel)
    {
        if (cardModel == null)
            return;

        if (Navigation == null)
            return;


        Validate.EnsureNotNull(_onAddOrEditCard);
        var cardsViewParameters = GetParameter<MainParameters>();

        await _onAddOrEditCard.Invoke(props =>
        {
            props.Card = cardModel;
            props.OnCardModified = (Card cardToReplace) =>
            {
                //cardsViewParameters.Set(p =>
                //{
                //    p.Cards.ReplaceCard(cardToReplace);
                //    p.Refresh();
                //});

            };
            props.OnCardRemoved = (Card cardToRemove) =>
            {
                //cardsViewParameters.Set(p =>
                //{
                //    p.Cards.RemoveCard(cardToRemove.Id);
                //    p.Refresh();
                //});
            };
        });
    }
    #endregion
}

static class CardsListExtensions
{
    public static void AddCard(this ObservableCollection<IndexedModel<Card>> list, Card newCard)
    {
        bool inserted = false;
        var newCardModel = new IndexedModel<Card>(newCard, list.Count);
        for (int i = 0; i < list.Count; i++)
        {
            if (string.Compare(list[i].Model.Name, newCard.Name, StringComparison.OrdinalIgnoreCase) > 0)
            {
                list.Insert(i, newCardModel);
                inserted = true;
                break;
            }
        }
        if (!inserted)
        {
            list.Add(newCardModel);
        }
    }

    public static void ReplaceCard(this ObservableCollection<IndexedModel<Card>> list, Card editedCard)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Model.Id == editedCard.Id)
            {
                list.RemoveAt(i);
                break;
            }
        }

        bool inserted = false;
        var newCardModel = new IndexedModel<Card>(editedCard, list.Count);
        for (int i = 0; i < list.Count; i++)
        {
            if (string.Compare(list[i].Model.Name, newCardModel.Model.Name, StringComparison.OrdinalIgnoreCase) > 0)
            {
                list.Insert(i, newCardModel);
                inserted = true;
                break;
            }
        }
        if (!inserted)
        {
            list.Add(newCardModel);
        }
    }

    public static void RemoveCard(this ObservableCollection<IndexedModel<Card>> list, int cardToRemoveId)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Model.Id == cardToRemoveId)
            {
                list.RemoveAt(i);
                break;
            }
        }
    }
}