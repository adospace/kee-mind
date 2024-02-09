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

    //public double EntranceTransitionX { get; set; }
}

class EditEntryPageProps
{
    public int? CardId { get; set; }
}

partial class EditCardPage : Component<EditEntryPageState, EditEntryPageProps>
{
    [Inject]
    IModelContext _modelContext;

    #region Initialization
    private MauiControls.Entry? _titleEntryRef;

    protected override void OnMountedOrPropsChanged()
    {
        State.ScopedContext = _modelContext.CreateScope();

        State.Items = State.ScopedContext.Query<Item>(_ => _.OrderBy(x => x.Id == 0 ? int.MaxValue : x.Id));

        State.Tags = State.ScopedContext.Query<Tag>(_ => _.OrderBy(x => x.Name));

        State.TagEntries = State.ScopedContext.Query<TagEntry>(_ => _.OrderBy(x => x.Tag.Name));

        State.Items.CollectionChanged += (sender, args) => Invalidate();

        State.TagEntries.CollectionChanged += (sender, args) => Invalidate();

        State.Cards = State.ScopedContext.Query<Card>();

        State.Cards.CollectionChanged += (sender, args) => SetState(s => s.Card = (Card)args.NewItems![0]!);

        State.ScopedContext.Load<Tag>();

        if (Props.CardId != null)
        {
            //State.ScopedContext.Load<Card>(_ => _.Where(_ => _.Id == Props.CardId).Include(_ => _.Items).Include(x => x.Tags).ThenInclude(x => x.Tag));

            State.ScopedContext.Load<Card>(x => x.Where(_ => _.Id == Props.CardId));
            State.ScopedContext.Load<Item>(x => x.Where(_ => _.CardId == Props.CardId));
            State.ScopedContext.Load<TagEntry>(x => x.Where(_ => _.Card!.Id == Props.CardId).Include(_ => _.Tag));
        }
        else
        {
            State.ScopedContext.Add(
                State.Card = new Card { Name = string.Empty, EditMode = EditMode.New },
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
                State.Card == null ? null :
                RenderBody()
            }
            //.TranslationX(State.EntranceTransitionX)
            //.WithAnimation(easing: Easing.CubicOut, duration: 200)
            .Set(MauiControls.NavigationPage.HasNavigationBarProperty, false);
        }
        else
        {
            return RenderBody();
        }
    }

    VisualNode RenderBody()
    {
        return new Grid("108, *, 24, Auto, Auto", "*")
        {
            State.IsEditing ?
            RenderEditingTop()
            :
            RenderTop(),

            RenderItems()
                .GridRow(1)
                ,

            RenderTags(),

            State.IsEditing && Props.CardId != null ?
            RenderBottomCommands() : null
        };
    }

    VisualNode RenderTop()
    {
        return new Grid("108", "64 * 48 64")
        {

            Microsoft.Maui.Devices.DeviceInfo.Idiom == Microsoft.Maui.Devices.DeviceIdiom.Phone ?

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
                .OnClicked(()=>SetState(s => s.IsEditing = true)),
        }
        .BackgroundColor(Theme.Current.BlackColor);
    }

    VisualNode RenderEditingTop()
    {
        return new Grid("108", "64 * 64")
        {
            Theme.Current.ImageButton("close_white.png")
                .Aspect(Aspect.Center)
                .HeightRequest(64)
                .BackgroundColor(Colors.Transparent)
                .OnClicked(OnCancelCard)
                ,

            new BorderlessEntry(entryRef => _titleEntryRef = entryRef)
                .Text(State.Card?.Name ?? string.Empty)
                .TextColor(Theme.Current.WhiteColor)
                .OnTextChanged(newName =>
                {
                    State.Card!.Name = newName;
                    State.ScopedContext.Update(State.Card);
                    //if (State.Card.EditMode == EditMode.None)
                    //{
                    //    State.Card.EditMode = EditMode.Modified;
                    //}                    
                })
                .Placeholder("Untitled")
                .PlaceholderColor(Theme.Current.WhiteColor)
                .FontSize(18)
                .GridColumn(1)
                .VCenter()
                .HCenter()
                //.OnLoaded(()=> _titleEntryRef?.Focus())
                ,

            Theme.Current.ImageButton("confirm_accent.png")
                .Aspect(Aspect.Center)
                .HeightRequest(64)
                .BackgroundColor(Colors.Transparent)
                .GridColumn(2)
                .OnClicked(SaveCard),
        }
        .BackgroundColor(Theme.Current.BlackColor);
    }

    //VisualNode RenderItems()
    //    => new VScrollView
    //    {
    //        VStack([..
    //            State.Items
    //                .Where(_=>_.EditMode != EditMode.Deleted)
    //                .Select(RenderItem)
    //                .Concat(State.IsEditing ?
    //                new[]
    //                {
    //                    Theme.Current.Button("ADD")
    //                        .HStart()
    //                        .VStart()
    //                        .OnClicked(OnAddItem)
    //                        .BackgroundColor(Theme.Current.DarkGrayColor)
    //                        .Margin(16, 0)
    //                }: Array.Empty<VisualNode>())
    //        ])
    //        .Spacing(16)
    //        .Margin(0, 16)
    //    }
    //    .GridRow(1);


    ScrollView RenderItems()
        => VScrollView(
        
            VStack([..
                    State.Items
                        //.Where(_=>_.EditMode != EditMode.Deleted)
                        .Select(RenderItem)
                        .Concat(State.IsEditing ?
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
        .Orientation(ScrollOrientation.Vertical)
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
            new Label()
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

    VisualNode RenderBottomCommands()
    {
        return new Grid("60", "Auto, Auto, *")
        {
            Theme.Current.Button(string.Empty)
                .HFill()
                .VFill()
                .OnClicked(OnRemoveCard)
                .BackgroundColor(Theme.Current.DarkGrayColor)
                .GridColumnSpan(3),

            new Image("delete_white.png")
                .VCenter()
                .HCenter()
                .Margin(18,0)
                .OnTapped(OnRemoveCard)
                .WidthRequest(20),

            Theme.Current.Label("Delete")
                .VCenter()
                .TextColor(Theme.Current.WhiteColor)
                .OnTapped(OnRemoveCard)
                .GridColumn(1),
        }
        .GridRow(4)
        ;
    }
    #endregion

    #region Events
    //void OnDeleteItem(Item item)
    //{
    //    //item.EditMode = EditMode.Deleted;
    //    //State.Items.Remove(item);
    //    State.ModelContext.Delete(item);        

    //    //Invalidate();
    //}

    void OnAddItem()
    {
        var newItem = new Item() { Card = State.Card, Label = string.Empty, Value = string.Empty, EditMode = EditMode.New };
        //State.Items.Add(newItem);

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

        _modelContext.Load<Card>(_ => _.Where(_ => _.Id == Props.CardId).Include(x => x.Tags).ThenInclude(x => x.Tag));
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
                //State.Items.Remove(item);
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
            //Props.OnEditCanceled?.Invoke(State.Card);

            State.IsClosing = true;

            //Props.OnClose?.Invoke();

            await Navigation.PopAsync();
        }

        //if (State.Card.EditMode == EditMode.None && 
        //    State.Items.All(_ => _.EditMode == EditMode.None))
        //{
        //    //await LoadCard();
        //    return;
        //}

        if (State.Card?.Id == 0 && 
            (State.Items.Count == 0 || State.Items.First().IsEmpty()))
        {
            await ClosePage();
            return;
        }

        if (!await ContainerPage.DisplayAlert(
            title: Props.CardId == null ? "Undo Card Creation" : "Cancel Editing", 
            message: Props.CardId == null ? "Are you sure you want to not add the new card?" : "Are you sure you want to cancel any modifications?", 
            accept: "Cancel", 
            cancel: Props.CardId == null ? "Back to New Card" : "Back To Edit Card"))
        {
            return;
        }

        if (Props.CardId == null)
        {
            await ClosePage();
        }
        else
        {
            State.ScopedContext.DiscardChanges();
            SetState(s => s.IsEditing = false);
            //await LoadCard();
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

        async Task ClosePage()
        {
            State.Card.EditMode = EditMode.Deleted;

            //Props.OnCardRemoved?.Invoke(State.Card);
            
            //Props.OnEditCanceled?.Invoke(State.Card);

            State.IsClosing = true;

            await _modelContext.Flush();

            //Props.OnClose?.Invoke();

            await Navigation.PopAsync();
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
        State.ScopedContext.Delete(State.Card);

        State.ScopedContext.Save();

        await State.ScopedContext.Flush();

        //_modelContext.Load<Card>(_ => _.Where(_ => _.Id == Props.CardId).Include(x => x.Tags).ThenInclude(x => x.Tag));
        //_modelContext.Load<Tag>(_ => _.Where(x => x.Entries!.Any()));

        _modelContext.Load<Card>(_ => _.Where(_ => _.Id == Props.CardId).Include(x => x.Tags).ThenInclude(x => x.Tag));
        _modelContext.Load<Tag>(_ => _.Where(x => x.Entries!.Any()), forceReload: true);

        await ClosePage();
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

        //Props.OnClose?.Invoke();

        await Navigation.PopAsync();        
    }
    #endregion
}
