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
    public string CurrentTagName { get; set; } = string.Empty;
}

class TagsEditor : Component<TagsEditorState>
{
    #region Initialization
    Card? _card;

    public TagsEditor Card(Card card)
    {
        _card = card;
        return this;
    }
    #endregion

    #region Render
    public override VisualNode Render()
    {
        ValidateExtensions.ThrowIfNull(_card);

        return new Grid("*", "*, Auto")
        {
            new VStack(spacing: 8)
            {
                new ScrollView
                {
                    new HStack(spacing: 5)
                    {
                        _card.Tags
                            .Where(_=>_.EditMode != EditMode.Deleted)
                            .Select(RenderTagItem)
                    }
                }
                .Orientation(ScrollOrientation.Horizontal)
                ,

                new BorderlessEntry()
                    .Text(State.CurrentTagName)
                    .OnTextChanged(text => State.CurrentTagName = text)
                    .OnCompleted(OnAddTag)
                    .When(string.IsNullOrWhiteSpace(State.CurrentTagName), _=>_.Placeholder("Enter tags separated by spaces..."))
                    .TextColor(Theme.Current.BlackColor)
                    .PlaceholderColor(Theme.Current.MediumGrayColor)
                    .FontSize(20)
                    .IsVisible(_card.Tags.Count(_=>_.EditMode != EditMode.Deleted) < 3),
            }
            .Margin(16)
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

    VisualNode RenderTagItem(TagEntry tag)
    {
        var removeAction = () => OnRemoveTag(tag);
        return new Grid("Auto", "Auto, Auto, *")
        {
            Theme.Current.Button(string.Empty)
                .HFill()
                .VFill()
                .FontSize(12)
                .Padding(2)
                .OnClicked(removeAction)
                .BackgroundColor(Theme.Current.DarkGrayColor)
                .GridColumnSpan(3),

            new Image("close_white.png")
                .VCenter()
                .HCenter()
                .Margin(5,0,0,0)
                .OnTapped(removeAction)
                .WidthRequest(10),

            Theme.Current.Label(tag.Tag.Name.ToUpper())
                .VCenter()
                .FontSize(12)
                .Margin(5,0)
                .TextColor(Theme.Current.WhiteColor)
                .OnTapped(removeAction)
                .GridColumn(1),
        }
        .GridRow(4);
    }
    #endregion

    #region Events
    async void OnAddTag()
    {
        ValidateExtensions.ThrowIfNull(_card);

        var repository = Services.GetRequiredService<IRepository>();
        await using var db = repository.OpenArchive();

        var tags = State.CurrentTagName
            .Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var allTags = await db.Tags.ToDictionaryAsync(_ => _.Name, _ => _);

        foreach (var tag in tags)
        {
            if (!allTags.TryGetValue(tag, out var value))
            {
                db.Tags.Add(value = new Tag { Name = tag, EditMode = EditMode.New });
                await db.SaveChangesAsync();
            }
            if (!_card.Tags.Any(_=> string.Compare(tag, _.Tag.Name, true) == 0))
            {
                _card.Tags.Add(new TagEntry { Card = _card, Tag = value, EditMode = EditMode.New });
            }
        }

        State.CurrentTagName = string.Empty;

        Invalidate();
    }

    void OnRemoveTag(TagEntry tag)
    {
        if (tag.EditMode == EditMode.New)
        {
            _card.ThrowIfNull().Tags.Remove(tag);
        }
        else
        {
            tag.EditMode = EditMode.Deleted;
        }

        Invalidate();
    }
    #endregion
}
