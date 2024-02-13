using KeeMind.Controls;
using KeeMind.Services.Data;
using KeeMind.Pages.Components;
using KeeMind.Resources;
using KeeMind.Services;
using MauiReactor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReactorData;
using System.Collections.ObjectModel;

namespace KeeMind.Pages;

class EditEntryPageState
{
    public bool IsLoading { get; set; }

    public bool IsClosing { get; set; }

    public Card Card { get; set; } = default!;

    public IQuery<Card> Cards { get; set; } = default!;

    public IQuery<Item> Items { get; set; } = default!;

    public IQuery<Tag> Tags { get; set; } = default!;

    public IQuery<TagEntry> TagEntries { get; set; } = default!;

    public bool IsEditing { get; set; }

    public IModelContext ScopedContext { get; set; } = default!;
}

class EditEntryPageProps
{
    public int? CardId { get; set; }
}

partial class EditCardPage : Component<EditEntryPageState, EditEntryPageProps>
{
    [Inject]
    IModelContext _modelContext;

    [Prop]
    int? _cardId;

    [Prop]
    Action? _onCardRemoved;

    #region Initialization

    protected override void OnMountedOrPropsChanged()
    {
        var cardId = Props.CardId ?? _cardId;

        State.ScopedContext = _modelContext.CreateScope();

        State.Items = State.ScopedContext.Query<Item>(_ => _.OrderBy(x => x.Id));

        State.Tags = State.ScopedContext.Query<Tag>(_ => _.OrderBy(x => x.Name));

        State.TagEntries = State.ScopedContext.Query<TagEntry>(_ => _.OrderBy(x => x.Tag.Name));

        State.Items.CollectionChanged += (sender, args) => Invalidate();

        State.TagEntries.CollectionChanged += (sender, args) => Invalidate();

        State.Cards = State.ScopedContext.Query<Card>();

        State.Cards.CollectionChanged += (sender, args) =>
        {
            if (args.NewItems?.Count == 1)
            {
                var newCard = (Card)args.NewItems[0]!;
                SetState(s => s.Card = newCard);
            }
        };

        State.ScopedContext.Load<Tag>();

        if (cardId != null)
        {
            State.ScopedContext.Load<Card>(x => x.Where(_ => _.Id == cardId));
            State.ScopedContext.Load<Item>(x => x.Where(_ => _.CardId == cardId));
            State.ScopedContext.Load<TagEntry>(x => x.Where(_ => _.Card!.Id == cardId).Include(_ => _.Tag));
        }
        else
        {
            State.ScopedContext.Add(
                State.Card = new Card { Name = string.Empty },
                new Item { Card = State.Card, Label = "Email", Value = string.Empty },
                new Item { Card = State.Card, Label = "Password", Value = string.Empty, IsMasked = true }
                );

            State.IsEditing = true;
        }

        base.OnMountedOrPropsChanged();
    }

    #endregion

    #region Render
    public override VisualNode Render()
    {
        if (DeviceInfo.Idiom == DeviceIdiom.Phone)
        {
            return new ContentPage
            {
                RenderBody()
            }
            .Set(MauiControls.Shell.NavBarIsVisibleProperty, false);
        }
        else
        {
            return RenderBody();
        }
    }

    Grid RenderBody() 
        => State.Card == null ? Grid() : new Grid("108, *, 24, Auto, Auto", "*")
        {
            State.IsEditing ?
            RenderEditingTop()
            :
            RenderTop(),

            RenderItems()
                .GridRow(1)
                ,

            RenderTags(),

            State.IsEditing && (_cardId ?? Props.CardId) != null ?
            RenderBottomCommands() : null
        }
        .BackgroundColor(Theme.Current.WhiteColor);

    Grid RenderTop()
        => Grid("108", "64 * 48 64",

            DeviceInfo.Idiom == DeviceIdiom.Phone ?

            Theme.Current.ImageButton("back_white.png")
                .Aspect(Aspect.Center)
                .HeightRequest(64)
                .BackgroundColor(Colors.Transparent)
                .OnClicked(OnBack)
                :null,

            Theme.Current.H3(State.Card.Name ?? string.Empty)
                .GridColumn(1)
                .VCenter()
                .HCenter(),

            Theme.Current.ImageButton(State.Card.IsFavorite ? "favorites_white_fill.png" : "favorites_white.png")
                .Aspect(Aspect.Center)
                .HeightRequest(64)
                .BackgroundColor(Colors.Transparent)
                .GridColumn(2)
                .OnClicked(ToggleFavorite),

            Theme.Current.ImageButton("edit_white.png")
                .Aspect(Aspect.Center)
                .HeightRequest(64)
                .BackgroundColor(Colors.Transparent)
                .GridColumn(3)
                .OnClicked(() => SetState(s => s.IsEditing = true))
        )
        .BackgroundColor(Theme.Current.BlackColor);

    Grid RenderEditingTop() 
        => Grid("108", "64 * 64",
            Theme.Current.ImageButton("close_white.png")
                .Aspect(Aspect.Center)
                .HeightRequest(64)
                .BackgroundColor(Colors.Transparent)
                .OnClicked(OnCancelCard),

            Entry()
                .Text(State.Card?.Name ?? string.Empty)
                .TextColor(Theme.Current.WhiteColor)
                .BackgroundColor(Theme.Current.BlackColor)                
                .OnTextChanged(newName =>
                {
                    State.Card!.Name = newName;
                    State.ScopedContext.Update(State.Card);
                })
                .Placeholder("Untitled")
                .PlaceholderColor(Theme.Current.WhiteColor)
                .FontSize(18)
                .GridColumn(1)
                .VCenter()
                .HCenter(),

            Theme.Current.ImageButton("confirm_accent.png")
                .Aspect(Aspect.Center)
                .HeightRequest(64)
                .BackgroundColor(Colors.Transparent)
                .GridColumn(2)
                .OnClicked(SaveCard)
        )
        .BackgroundColor(Theme.Current.BlackColor);

    ScrollView RenderItems()
        => VScrollView(

            VStack([..
                    State.Items
                        .Where(_ => State.ScopedContext.GetEntityStatus(_) != EntityStatus.Added)
                        .Concat(State.Items.Where(_ => State.ScopedContext.GetEntityStatus(_) == EntityStatus.Added))
                        .Select(RenderItem)
                        .Concat(State.IsEditing && !State.Items.Any(_=>string.IsNullOrWhiteSpace(_.Label)) ?
                        [
                            Theme.Current.Button("ADD")
                                .HStart()
                                .VStart()
                                .OnClicked(OnAddItem)
                                .BackgroundColor(Theme.Current.DarkGrayColor)
                                .Margin(16, 0)
                        ]: [])
                        ]
            )
            .Spacing(16)
            .Margin(0, 16)
        )
        .GridRow(1);

    VisualNode RenderItem(Item item)
    {
        return new ItemComponent()
            .Item(item)
            .IsEditing(State.IsEditing)
            .OnUpdate(() => State.ScopedContext.Update(item))
            .OnDelete(()=> State.ScopedContext.Delete(item));
    }

    VisualNode[] RenderTags()
    {
        if (State.TagEntries.Count == 0 && !State.IsEditing)
        {
            return [];
        }

        return
        [
            Label()
                .BackgroundColor(Theme.Current.LightGrayColor)
                .TextColor(Theme.Current.BlackColor)
                .Text("TAGS")
                .VerticalTextAlignment(TextAlignment.Center)
                .Padding(16,0)
                .GridRow(2),

            State.IsEditing ?
            new TagsEditor()
                .ScopedContext(State.ScopedContext)
                .Card(State.Card)
                .Entries(State.TagEntries)
                .Tags(State.Tags)
                .GridRow(3)
                :
            new TagsViewer()
                .Entries(State.TagEntries)
                .GridRow(3)
        ];
    }

    Grid RenderBottomCommands() 
        => Grid("60", "Auto, Auto, *",
            Theme.Current.Button(string.Empty)
                .HFill()
                .VFill()
                .OnClicked(OnRemoveCard)
                .BackgroundColor(Theme.Current.DarkGrayColor)
                .GridColumnSpan(3)
                .CornerRadius(0),

            Image("delete_white.png")
                .VCenter()
                .HCenter()
                .Margin(18,0)
                .OnTapped(OnRemoveCard)
                .WidthRequest(20),

            Theme.Current.Label("Delete")
                .VCenter()
                .TextColor(Theme.Current.WhiteColor)
                .OnTapped(OnRemoveCard)
                .GridColumn(1)
        )
        .GridRow(4);
    #endregion

    #region Events
    void OnAddItem()
    {
        var newItem = new Item() { Card = State.Card, Label = string.Empty, Value = string.Empty };

        State.ScopedContext.Add(newItem);

        Invalidate();
    }

    async void ToggleFavorite()
    {
        if (State.Card == null)
        {
            return;
        }

        State.Card.IsFavorite = !State.Card.IsFavorite;

        State.ScopedContext.Update(State.Card);

        State.ScopedContext.Save();

        await State.ScopedContext.Flush();

        var cardId = _cardId ?? Props.CardId;

        _modelContext.Load<Card>(_ => _.Where(_ => _.Id == cardId).Include(x => x.Tags).ThenInclude(x => x.Tag));
    }

    async void SaveCard()
    {
        if (ContainerPage == null)
        {
            return;
        }

        if (State.IsClosing)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(State.Card?.Name))
        {
            await ContainerPage.DisplayAlert("Invalid Card", "Please enter a valid card name", "OK");
            return;
        }

        foreach (var item in State.Items.ToArray())
        {
            if (string.IsNullOrWhiteSpace(item.Label) && string.IsNullOrWhiteSpace(item.Value))
            {
                State.ScopedContext.Delete(item);
            }
            else if (string.IsNullOrWhiteSpace(item.Label))
            {
                item.Label = "Label";
                State.ScopedContext.Update(item);
            }
        }

        State.ScopedContext.Save();

        await State.ScopedContext.Flush();

        _modelContext.Load<Card>(_ => _.Where(_ => _.Id == State.Card.Id).Include(x => x.Tags).ThenInclude(x => x.Tag), compareFunc: (c1, c2) => false);
        _modelContext.Load<Tag>(_ => _.Where(x => x.Entries!.Any()), forceReload: true);

        SetState(s => s.IsEditing = false);
    }

    async void OnCancelCard()
    {
        if (ContainerPage == null)
        {
            return;
        }

        if (Navigation == null)
        {
            return;
        }

        if (State.IsClosing)
        {
            return;
        }

        async Task ClosePage()
        {
            State.IsClosing = true;

            await Navigation.PopAsync();
        }

        if (State.Card?.Id == 0 && 
            (State.Items.Count == 0 || State.Items.First().IsEmpty()))
        {
            await ClosePage();
            return;
        }

        var cardId = _cardId ?? Props.CardId;

        if (!await ContainerPage.DisplayAlert(
            title: cardId == null ? "Undo Card Creation" : "Cancel Editing", 
            message: cardId == null ? "Are you sure you want to not add the new card?" : "Are you sure you want to cancel any modifications?", 
            accept: "Cancel", 
            cancel: cardId == null ? "Back to New Card" : "Back To Edit Card"))
        {
            return;
        }

        if (cardId == null)
        {
            await ClosePage();
        }
        else
        {
            State.ScopedContext.DiscardChanges();
            SetState(s => s.IsEditing = false);
        }
    }

    async void OnRemoveCard()
    {
        if (ContainerPage == null)
        {
            return;
        }

        if (Navigation == null)
        {
            return;
        }

        if (State.IsClosing)
        {
            return;
        }

        if (!await ContainerPage.DisplayAlert(
            title: "Delete Card",
            message: "Are you sure you want to delete the card?",
            accept: "Delete",
            cancel: "Back"))
        {
            return;
        }

        await State.ScopedContext.Flush();

        State.ScopedContext.Delete(State.Items.ToArray());
        State.ScopedContext.Delete(State.TagEntries.ToArray());

        State.ScopedContext.Save();

        await State.ScopedContext.Flush();

        _modelContext.Delete(State.Card);

        _modelContext.Save();

        await _modelContext.Flush();

        _modelContext.Load<Tag>(_ => _.Where(x => x.Entries!.Any()), forceReload: true);

        if (DeviceInfo.Current.Idiom == DeviceIdiom.Desktop)
        {
            State.IsEditing = false;

            _onCardRemoved?.Invoke();
        }
        else
        {
            State.IsClosing = true;

            await Navigation.PopAsync();
        }
    }

    async void OnBack()
    {
        if (Navigation == null)
        {
            return;
        }

        if (State.IsClosing)
        {
            return;
        }

        State.IsClosing = true;

        await Navigation.PopAsync();        
    }
    #endregion
}
