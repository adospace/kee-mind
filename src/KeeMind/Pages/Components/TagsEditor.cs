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
using MauiReactor;
using ReactorData;

namespace KeeMind.Pages.Components;

class TagsEditorState
{
    public string CurrentTagName { get; set; } = string.Empty;
}

partial class TagsEditor : Component<TagsEditorState>
{
    private static readonly char[] _tagsSeparators = [',', ';', ' '];

    #region Initialization
    [Prop]
    IReadOnlyList<TagEntry> _entries = default!;

    [Prop]
    IReadOnlyList<Tag> _tags = default!;

    [Prop]
    IModelContext _scopedContext = default!;

    [Prop]
    Card _card = default!;

    #endregion

    #region Render
    public override VisualNode Render()
    {
        return new Grid("*", "*, Auto")
        {
            new VStack(spacing: 8)
            {
                new ScrollView
                {
                    new HStack(spacing: 5)
                    {
                        _entries
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
                    .IsVisible(_entries.Count < 3),
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
    void OnAddTag()
    {
        var tagTokens = State.CurrentTagName
            .Split(_tagsSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var allTags = _tags.GroupBy(_ => _.Name).ToDictionary(_ => _.Key, _ => _.First());

        foreach (var tagToken in tagTokens)
        {
            if (!allTags.TryGetValue(tagToken, out var tag))
            {
                tag = new Tag { Name = tagToken, EditMode = EditMode.New };

                _scopedContext.Add(tag);
            }

            if (!_entries.Any(_ => string.Compare(tagToken, _.Tag.Name, true) == 0))
            {
                _scopedContext.Add(new TagEntry { Card = _card, Tag = tag });
            }
        }

        //var repository = Services.GetRequiredService<IRepository>();
        //await using var db = repository.OpenArchive();

        //var tags = State.CurrentTagName
        //    .Split(_tagsSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        //var allTags = await db.Tags.ToDictionaryAsync(_ => _.Name, _ => _);

        //foreach (var tag in tags)
        //{
        //    if (!allTags.TryGetValue(tag, out var value))
        //    {
        //        db.Tags.Add(value = new Tag { Name = tag, EditMode = EditMode.New });
        //        await db.SaveChangesAsync();
        //    }
        //    if (!_card.Tags.Any(_=> string.Compare(tag, _.Tag.Name, true) == 0))
        //    {
        //        _card.Tags.Add(new TagEntry { Card = _card, Tag = value, EditMode = EditMode.New });
        //    }
        //}



        State.CurrentTagName = string.Empty;

        Invalidate();
    }

    void OnRemoveTag(TagEntry tag)
    {
        _scopedContext.Delete(tag);


        //if (tag.EditMode == EditMode.New)
        //{
        //    _card.ThrowIfNull().Tags.Remove(tag);
        //}
        //else
        //{
        //    tag.EditMode = EditMode.Deleted;
        //}

        Invalidate();
    }
    #endregion
}
