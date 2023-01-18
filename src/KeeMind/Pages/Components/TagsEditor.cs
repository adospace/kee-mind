using KeeMind.Controls;
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

namespace KeeMind.Pages.Components;

class TagsEditorState
{
    public Dictionary<string, Tag> AllTags { get; set; } = new();

    public string CurrentTagName { get; set; } = string.Empty;
}

class TagsEditor : Component<TagsEditorState>
{
    Card? _card;

    public TagsEditor Card(Card card)
    {
        _card = card;
        return this;
    }

    protected override void OnMounted()
    {
        Task.Run(InitializeTagList);

        base.OnMounted();
    }

    async Task InitializeTagList()
    {
        var repository = Services.GetRequiredService<IRepository>();

        await using var db = repository.OpenArchive();

        State.AllTags = await db.Tags
            .OrderBy(_ => _.Name)
            .ToDictionaryAsync(_ => _.Name, _ => _);
    }

    public override VisualNode Render()
    {
        return new Grid("*", "*, Auto")
        {
            new VStack(spacing: 2)
            {
                new ScrollView
                {
                    new HStack(spacing: 5)
                    {
                        _card.ThrowIfNull().Tags.Where(_=>_.EditMode != EditMode.Deleted).Select(RenderTagItem)
                    }
                }
                .Orientation(ScrollOrientation.Horizontal)
                .Margin(16,16,0,0),

                new BorderlessEntry()
                    .Text(State.CurrentTagName)
                    .OnTextChanged(text => SetState(s =>
                    {
                        s.CurrentTagName = text;
                    }, false))
                    .OnCompleted(OnAddTag)
                    .When(string.IsNullOrWhiteSpace(State.CurrentTagName), _=>_.Placeholder("Enter tags separated by spaces..."))
                    .TextColor(Theme.Current.BlackColor)
                    .PlaceholderColor(Theme.Current.MediumGrayColor)
                    .FontSize(20)
                    .Margin(16,0)


                    
                    .IsVisible(_card.ThrowIfNull().Tags.Count < 3),
            }
            .VCenter(),

            //Theme.Current.Button("ADD")
            //    .HStart()
            //    .VStart()
            //    .IsEnabled(()=> !string.IsNullOrWhiteSpace(State.CurrentTagName))
            //    .OnClicked(OnAddTag)
            //    .Margin(16, 0, 16,5)
            //    .IsVisible(_card.ThrowIfNull().Tags.Count < 3)
            //    .GridColumn(1)
            //    .VCenter()
        };
    }

    private async void OnAddTag()
    {
        ValidateExtensions.ThrowIfNull(_card);

        var repository = Services.GetRequiredService<IRepository>();
        await using var db = repository.OpenArchive();

        var tags = State.CurrentTagName
            .Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var tag in tags)
        {
            if (!State.AllTags.TryGetValue(tag, out var value))
            {
                db.Tags.Add(value = new Tag { Name = tag, EditMode = EditMode.New });
                await db.SaveChangesAsync();
                State.AllTags[tag] = value;
            }

            _card.Tags.Add(new TagEntry { Card = _card, Tag = value, EditMode = EditMode.New });
        }

        State.CurrentTagName = string.Empty;

        Invalidate();
    }

    private VisualNode RenderTagItem(TagEntry tag)
    {
        return new Grid("Auto", "Auto, Auto, *")
        {
            Theme.Current.Button(string.Empty)
                .HFill()
                .VFill()
                .FontSize(12)
                .Padding(2)
                .OnClicked(()=>OnRemoveTag(tag))
                .BackgroundColor(Theme.Current.DarkGrayColor)
                .GridColumnSpan(3),

            new Image("close_white.png")
                .VCenter()
                .HCenter()
                .Margin(5,0,0,0)
                .WidthRequest(10),

            Theme.Current.Label(tag.Tag.Name.ToUpper())
                .VCenter()
                .FontSize(12)
                .Margin(5,0)
                .TextColor(Theme.Current.WhiteColor)
                .GridColumn(1),
        }
        .GridRow(4);
    }

    private void OnRemoveTag(TagEntry tag)
    {
        if(tag.EditMode == EditMode.New)
        {
            _card.ThrowIfNull().Tags.Remove(tag);
        }
        else
        {
            tag.EditMode = EditMode.Deleted;
        }

        Invalidate();
    }
}
