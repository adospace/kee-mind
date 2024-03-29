using KeeMind.Resources;
using MauiReactor;
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

partial class PinBoard : Component<PinBoardState>
{
    [Prop]
    string? _label;

    [Prop]
    Action<string>? _onPinEntered;

    #region Render
    public override VisualNode Render()
    {
        return Grid("Auto,*", "*",
            VStack(
                AppTheme.Current.H2(_label ?? string.Empty)
                    .HCenter(),

                FlexLayout(
                    [.. Enumerable.Range(1, 6)
                        .Select(index=>
                        {
                            bool numberEntered = index <= State.PIN.Length;

                            if (numberEntered)
                            {
                                return (VisualNode)Grid(
                                    Ellipse()
                                        .HeightRequest(28)
                                        .WidthRequest(28)
                                        .Stroke(AppTheme.Current.AccentBrush)
                                        .StrokeThickness(1),
                                    Ellipse()
                                        .HeightRequest(20)
                                        .WidthRequest(20)
                                        .Margin(4)
                                        .Fill(AppTheme.Current.AccentBrush)
                                );
                            }

                            return Ellipse()
                                .HeightRequest(28)
                                .WidthRequest(28)
                                .Stroke(AppTheme.Current.AccentBrush)
                                .StrokeThickness(1);
                        })
                        ]
                )
                .HCenter()
                .JustifyContent(Microsoft.Maui.Layouts.FlexJustify.SpaceBetween)
            )
            .VEnd()
            .Margin(60)
            .Spacing(20),

            RenderKeyboard()
                .GridRow(1)
        )
        .When(Microsoft.Maui.Devices.DeviceInfo.Idiom == Microsoft.Maui.Devices.DeviceIdiom.Desktop, _=>
            _.WidthRequest(480)); //MaxWidthRequest not working under Mac
    }

    Grid RenderKeyboard() 
        => Grid("* * * *", "* * *",
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

            AppTheme.Current.ImageButton("cancel_white.png")
                .Padding(12)
                .HeightRequest(72)
                .Aspect(Aspect.Center)
                .GridRow(3)
                .GridColumn(2)
                .BackgroundColor(AppTheme.Current.BlackColor)
                .IsVisible(State.PIN.Length > 0)
                .OnClicked(()=> SetState(s => s.PIN = s.PIN[..^1]))
        )
        .Margin(20, 0, 20, 60);

    Button RenderKeyboardButton(string v, int row, int col) 
        => AppTheme.Current.TransparentButton(v)
            .FontSize(32)
            .HeightRequest(72)
            .FontAttributes(MauiControls.FontAttributes.Bold)
            .GridRow(row)
            .GridColumn(col)
            .OnClicked(() => NumberEntered(v));
    #endregion

    #region Events
    void NumberEntered(string number)
    {
        State.PIN += number;

        if (State.PIN.Length == 6)
        {
            _onPinEntered?.Invoke(State.PIN);
            State.PIN = string.Empty;
        }

        Invalidate();
    }
    #endregion
}
