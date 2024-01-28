using KeeMind.Services.Data;
using KeeMind.Resources;
using System.Linq;
using KeeMind.Services;
using MauiReactor;

namespace KeeMind.Pages.Components;

class TagsViewer : Component
{
    #region Initialization
    Card? _card;

    public TagsViewer Card(Card card)
    {
        _card = card;
        return this;
    }
    #endregion

    #region Render
    public override VisualNode Render()
    {
        return new ScrollView
        {
            new HStack(spacing: 5)
            {
                _card.ThrowIfNull().Tags.Select(RenderTagItem)
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
