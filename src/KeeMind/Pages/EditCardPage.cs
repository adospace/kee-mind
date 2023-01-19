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

namespace KeeMind.Pages;

class EditEntryPageState
{
    public bool IsLoading { get; set; }

    public bool IsClosing { get; set; }

    public Card Card { get; set; } = new Card { Name = string.Empty, EditMode = EditMode.New };

    public bool IsEditing { get; set; }

    public double EntranceTransitionX { get; set; }
}

class EditEntryPageProps
{
    public int? CardId { get; set; }

    public Action<Card>? OnCardAdded;

    public Action<Card>? OnCardModified;

    //public Action<Card>? OnEditCanceled;

    public Action<Card>? OnCardRemoved;

    public Action? OnClose;
}

class EditCardPage : Component<EditEntryPageState, EditEntryPageProps>

{
    private MauiControls.Entry? _titleEntryRef;

    public EditCardPage()
    { }

    public EditCardPage(EditEntryPageProps props)
        :base(props: props)
    { 
    
    }

    protected override void OnMounted()
    {
        if (Props.CardId != null)
        {
            Task.Run(LoadCard);
        }
        else
        {
            State.Card.Items.Add(new Item { Card = State.Card, Label = "Email", Value = "" });
            State.Card.Items.Add(new Item { Card = State.Card, Label = "Password", Value = "", IsMasked = true });
            State.IsEditing = true;
        }

#if ANDROID
        if (MauiControls.Application.Current?.Dispatcher != null)
        {
            State.EntranceTransitionX = 400;
            MauiControls.Application.Current.Dispatcher.Dispatch(() => SetState(s => s.EntranceTransitionX = 0));
        }
#endif
        base.OnMounted();
    }

    protected override void OnPropsChanged()
    {
        if (Props.CardId != null)
        {
            Task.Run(LoadCard);
        }
        else
        {
            State.Card = new Card { Name = string.Empty, EditMode = EditMode.New };
            State.Card.Items.Add(new Item { Card = State.Card, Label = "Email", Value = "" });
            State.Card.Items.Add(new Item { Card = State.Card, Label = "Password", Value = "", IsMasked = true });
            State.IsEditing = true;
        }

        base.OnPropsChanged();
    }

    async Task LoadCard()
    {
        System.Diagnostics.Debug.WriteLine($"Loading card {Props.CardId}");

        ValidateExtensions.ThrowIfNull(Props.CardId);
        var repository = Services.GetRequiredService<IRepository>();

        await using var db = repository.OpenArchive();
        State.Card = await db.Cards
            .Include(_ => _.Items)
            .Include(_ => _.Tags)
            .ThenInclude(_ => _.Tag)
            .Include(_ => _.Attachments)
            .AsNoTracking()
            .FirstAsync(_ => _.Id == Props.CardId);
        
        State.IsEditing = false;

        SetState(s =>
        {
            s.IsLoading = false;
        });
    }

    public override VisualNode Render()
    {

        if (Microsoft.Maui.Devices.DeviceInfo.Idiom == Microsoft.Maui.Devices.DeviceIdiom.Phone)
        {
            return new ContentPage
            {
                RenderBody()
            }
            .TranslationX(State.EntranceTransitionX)
            .WithAnimation(duration: 200)
            .Set(MauiControls.NavigationPage.HasNavigationBarProperty, false);
        }
        else
        {
            return RenderBody();
        }
    }

    private VisualNode RenderBody()
    {
        return new Grid("108, *, 24, Auto, Auto", "*")
        {
            State.IsEditing ?
            RenderEditingTop()
            :
            RenderTop(),

            RenderItems(),


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
                .OnClicked(()=> Task.Run(ToggleFavorite)),

            Theme.Current.ImageButton("edit_white.png")
                .Aspect(Aspect.Center)
                .HeightRequest(64)
                .BackgroundColor(Colors.Transparent)
                .GridColumn(3)
                .OnClicked(()=>SetState(s => s.IsEditing = true)),
        }
        .BackgroundColor(Theme.Current.BlackColor);
    }

    private async Task ToggleFavorite()
    {
        var repository = Services.GetRequiredService<IRepository>();
        await using var db = repository.OpenArchive();

        State.Card.IsFavorite = !State.Card.IsFavorite;
        db.Cards.Update(State.Card);

        await db.SaveChangesAsync();

        Props.OnCardModified?.Invoke(State.Card);

        Invalidate();
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
                .Text(State.Card.Name ?? string.Empty)
                .TextColor(Theme.Current.WhiteColor)
                .OnTextChanged(newName =>
                {
                    State.Card.Name = newName;
                    if (State.Card.EditMode == EditMode.None)
                    {
                        State.Card.EditMode = EditMode.Modified;
                    }                    
                })
                .Placeholder("Untitled")
                .PlaceholderColor(Theme.Current.WhiteColor)
                .FontSize(18)
                .GridColumn(1)
                .VCenter()
                .HCenter()
                .OnLoaded(()=> _titleEntryRef?.Focus()),

            Theme.Current.ImageButton("confirm_accent.png")
                .Aspect(Aspect.Center)
                .HeightRequest(64)
                .BackgroundColor(Colors.Transparent)
                .GridColumn(2)
                .OnClicked(SaveCard),
        }
        .BackgroundColor(Theme.Current.BlackColor);
    }

    VisualNode RenderItems()
    {
        return new VerticalScrollView
        {
            new VerticalStackLayout
            {
                State.Card.Items
                    .Where(_=>_.EditMode != EditMode.Deleted)
                    .Select(RenderItem)
                    .Concat(State.IsEditing ?
                    new[]
                    {
                        Theme.Current.Button("ADD")
                            .HStart()
                            .VStart()
                            .OnClicked(OnAddItem)
                            .BackgroundColor(Theme.Current.DarkGrayColor)
                            .Margin(16, 0)
                    }: Array.Empty<VisualNode>())
            }
            .Spacing(16)
            .Margin(0, 16)
        }
        .GridRow(1);
    }

    VisualNode RenderItem(Item item)
    {
        return new Components.ItemComponent()
            .Item(item)
            .IsEditing(State.IsEditing)
            .OnDelete(()=> OnDeleteItem(item));
    }

    VisualNode[] RenderTags()
    {
        if (State.Card.Tags.Count == 0 && !State.IsEditing)
        {
            return Array.Empty<VisualNode>();
        }

        return new VisualNode[]
        {
            new Label()
                .BackgroundColor(Theme.Current.LightGrayColor)
                .TextColor(Theme.Current.BlackColor)
                .Text("TAGS")
                .VerticalTextAlignment(TextAlignment.Center)
                .Padding(16,0)
                .GridRow(2),

            State.IsEditing ?
            new TagsEditor()
                .Card(State.Card)
                .GridRow(3)
                :
            new TagsViewer()
                .Card(State.Card)
                .GridRow(3)
        };
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
                .WidthRequest(20),

            Theme.Current.Label("Delete")
                .VCenter()
                .TextColor(Theme.Current.WhiteColor)
                .GridColumn(1),
        }
        .GridRow(4)
        ;
    }

    private void OnDeleteItem(Item item)
    {
        item.EditMode = EditMode.Deleted;
        Invalidate();
    }

    void OnAddItem()
    {
        State.Card.Items.Add(new Item() { Card = State.Card, Label = string.Empty, Value = string.Empty, EditMode = EditMode.New });
        Invalidate();
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

        if (string.IsNullOrWhiteSpace(State.Card.Name))
        {
            await ContainerPage.DisplayAlert("Invalid Card", "Please enter a valid card name", "OK");
            return;
        }

        foreach (var item in State.Card.Items.ToArray())
        {
            if (string.IsNullOrWhiteSpace(item.Label) && string.IsNullOrWhiteSpace(item.Value))
            {
                State.Card.Items.Remove(item);
            }
            else if (string.IsNullOrWhiteSpace(item.Label))
            {
                item.Label = "Label";
            }
        }

        var repository = Services.GetRequiredService<IRepository>();

        await using var db = repository.OpenArchive();

        if (State.Card.EditMode == EditMode.New)
        {
            db.Cards.Add(State.Card);
        }
        else 
        {
            if (State.Card.EditMode == EditMode.Modified)
            {
                db.Entry(State.Card).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }

            foreach (var item in State.Card.Items)
            {
                if (item.EditMode == EditMode.Deleted)
                {
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                }
                else if (item.EditMode == EditMode.Modified)
                {
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                else if (item.EditMode == EditMode.New)
                {
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                }
            }       

            foreach (var tagEntry in State.Card.Tags)
            {
                if (tagEntry.EditMode == EditMode.Deleted)
                {
                    db.Entry(tagEntry).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                }
                else if (tagEntry.EditMode == EditMode.Modified)
                {
                    db.Entry(tagEntry).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                else if (tagEntry.EditMode == EditMode.New)
                {
                    db.Entry(tagEntry).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                }
            }       
        }

        foreach (var tag in db.ChangeTracker.Entries<Tag>())
        {
            tag.State = EntityState.Unchanged;
        }

        await db.SaveChangesAsync();

        foreach (var emptyTags in await db.Tags.Where(_ => !_.Entries!.Any()).ToArrayAsync())
        {
            db.Tags.Remove(emptyTags);
        }

        await db.SaveChangesAsync();

        Props.CardId = State.Card.Id;

        var newCard = State.Card.EditMode == EditMode.New;

        await LoadCard();
        
        if (newCard)
        {
            Props.OnCardAdded?.Invoke(State.Card);
        }
        else
        {
            Props.OnCardModified?.Invoke(State.Card);
        }

        State.IsEditing = false;

        Invalidate();
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

            Props.OnClose?.Invoke();

            await Navigation.PopAsync();
        }

        if (State.Card.EditMode == EditMode.None && 
            State.Card.Items.All(_ => _.EditMode == EditMode.None))
        {
            await LoadCard();
            return;
        }

        if (State.Card.EditMode == EditMode.New && 
            (State.Card.Items.Count == 0 || State.Card.Items[0].IsEmpty()))
        {
            await ClosePage();
            return;
        }

        if (!await ContainerPage.DisplayAlert(
            title: Props.CardId == null ? "Undo Card Creation" : "Cancel Editing", 
            message: Props.CardId == null ? "Are you sure you want to cancel the new card?" : "Are you sure you want to cancel any modifications?", 
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
            await LoadCard();
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
            Props.OnCardRemoved?.Invoke(State.Card);

            State.IsClosing = true;

            Props.OnClose?.Invoke();

            await Navigation.PopAsync();
        }

        if (!await ContainerPage.DisplayAlert(
            title: "Delete Card",
            message: "Are you sure you want to delete the card?",
            accept: "Remove",
            cancel: "Back"))
        {
            return;
        }

        var repository = Services.GetRequiredService<IRepository>();

        await using var db = repository.OpenArchive();

        var cardToRemove = await db.Cards
            .Include(_ => _.Items)
            .Include(_ => _.Tags)
            .Include(_ => _.Attachments)
            .FirstAsync(_ => _.Id == Props.CardId);

        db.Cards.Remove(cardToRemove);

        await db.SaveChangesAsync();

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

        Props.OnClose?.Invoke();

        await Navigation.PopAsync();        
    }
}
