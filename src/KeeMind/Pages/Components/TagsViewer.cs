using KeeMind.Services.Data;
using KeeMind.Resources;
using System.Linq;
using KeeMind.Services;
using MauiReactor;
using System.Collections.Generic;

namespace KeeMind.Pages.Components;

partial class TagsViewer : Component
{
    #region Initialization
    [Prop]
    IReadOnlyList<TagEntry> _entries = default!;

    #endregion

    #region Render
    public override VisualNode Render()
    {
        return new ScrollView
        {
            new HStack(spacing: 5)
            {
                _entries.Select(RenderTagItem)
            }
        }
        .HeightRequest(24)
        .Margin(16,11)
        .Orientation(ScrollOrientation.Horizontal);
    }

    VisualNode RenderTagItem(TagEntry tag)
    {
        return new Label(tag.Tag.Name.ToUpper())
            .BackgroundColor(Theme.Current.BlackColor)
            .TextColor(Theme.Current.WhiteColor)
            .VerticalTextAlignment(TextAlignment.Center)
            .HorizontalTextAlignment(TextAlignment.Center)
            .Padding(12,0)
            ;
    }
    #endregion
}
