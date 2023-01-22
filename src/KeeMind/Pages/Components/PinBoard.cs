using KeeMind.Resources;
using MauiReactor.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeeMind.Pages.Components;

class PinBoardState
{
    public string PIN { get; set; } = string.Empty;
}

class PinBoard : Component<PinBoardState>
{
    #region Initialization
    private string? _label;
    private Action<string>? _onPinEnteredAction;

    public PinBoard Label(string label)
    {
        _label = label;

        return this;
    }

    public PinBoard OnPinEntered(Action<string> onPinEnteredAction)
    {
        _onPinEnteredAction = onPinEnteredAction;
        return this;
    }
    #endregion

    #region Render
    public override VisualNode Render()
    {
        return new Grid("Auto,*", "*")
        {
            new VerticalStackLayout
            {
                Theme.Current.H2(_label ?? string.Empty) //
                    .HCenter(),

                new FlexLayout
                {
                    Enumerable.Range(1, 6)
                        .Select(index=>
                        {
                            bool numberEntered = index <= State.PIN.Length;

                            if (numberEntered)
                            {
                                return (VisualNode)new Grid
                                {
                                    new Ellipse()
                                        .HeightRequest(28)
                                        .WidthRequest(28)
                                        .Stroke(Theme.Current.AccentBrush)
                                        .StrokeThickness(1),
                                    new Ellipse()
                                        .HeightRequest(20)
                                        .WidthRequest(20)
                                        .Margin(4)
                                        .Fill(Theme.Current.AccentBrush)
                                };
                            }

                            return new Ellipse()
                                .HeightRequest(28)
                                .WidthRequest(28)
                                .Stroke(Theme.Current.AccentBrush)
                                .StrokeThickness(1);
                        })
                }
                .HCenter()
                .JustifyContent(Microsoft.Maui.Layouts.FlexJustify.SpaceBetween)
            }
            .VEnd()
            .Margin(60)
            .Spacing(20)
            ,

            RenderKeyboard().GridRow(1)
        }
        .MaximumWidthRequest(480);
    }

    Grid RenderKeyboard()
    {
        return new Grid("* * * *", "* * *")
        {
            RenderKeyboardButton("1", 0, 0),
            RenderKeyboardButton("2", 0, 1),
            RenderKeyboardButton("3", 0, 2),
            RenderKeyboardButton("4", 1, 0),
            RenderKeyboardButton("5", 1, 1),
            RenderKeyboardButton("6", 1, 2),
            RenderKeyboardButton("7", 2, 0),
            RenderKeyboardButton("8", 2, 1),
            RenderKeyboardButton("9", 2, 2),
            RenderKeyboardButton("0", 3, 1),

            Theme.Current.ImageButton("cancel_white.png")
                .Padding(12)
                .HeightRequest(72)
                .Aspect(Aspect.Center)
                .GridRow(3)
                .GridColumn(2)
                .BackgroundColor(Theme.Current.BlackColor)
                .IsVisible(State.PIN.Length > 0)
                .OnClicked(()=> SetState(s => s.PIN = s.PIN[..^1]))
        }
        .Margin(20, 0, 20, 60);
    }

    VisualNode RenderKeyboardButton(string v, int row, int col)
    {
        return Theme.Current.TransparentButton(v)
            .FontSize(32)
            .HeightRequest(72)
            .FontAttributes(Microsoft.Maui.Controls.FontAttributes.Bold)
            .GridRow(row)
            .GridColumn(col)
            .OnClicked(() => NumberEntered(v));
    }
    #endregion

    #region Events
    void NumberEntered(string number)
    {
        State.PIN += number;

        if (State.PIN.Length == 6)
        {
            _onPinEnteredAction?.Invoke(State.PIN);
            State.PIN = string.Empty;
        }

        Invalidate();
    }
    #endregion
}
