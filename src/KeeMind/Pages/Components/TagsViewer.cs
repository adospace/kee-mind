using KeeMind.Services.Data;
using KeeMind.Resources;
using System.Linq;
using KeeMind.Services;
using MauiReactor;
using System.Collections.Generic;

namespace KeeMind.Pages.Components;

partial class TagsViewer : Component
{
    [Prop]
    IReadOnlyList<TagEntry> _entries = default!;

    #region Render
    public override VisualNode Render() 
        => HScrollView(
            HStack(spacing: 5,
                [.. _entries.Select(RenderTagItem)]
            )
        )
        .HeightRequest(24)
        .Margin(16, 11)
        .Orientation(ScrollOrientation.Horizontal);

    Label RenderTagItem(TagEntry tag) 
        => Label(tag.Tag.Name.ToUpper())
            .BackgroundColor(AppTheme.Current.BlackColor)
            .TextColor(AppTheme.Current.WhiteColor)
            .VerticalTextAlignment(TextAlignment.Center)
            .HorizontalTextAlignment(TextAlignment.Center)
            .Padding(12, 0)
            ;
    #endregion
}
