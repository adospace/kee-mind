using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeeMind.Resources
{
    abstract class Theme
    {
        public static Theme Current { get; } = new LightTheme();

        public abstract MauiControls.Brush BlackBrush { get; }
        public abstract MauiControls.Brush DarkBlackBrush { get; }
        public abstract MauiControls.Brush WhiteBrush { get; }
        public abstract MauiControls.Brush AccentBrush { get; }

        public abstract MauiControls.Brush GrayBrush { get; }
        public abstract MauiControls.Brush LightGrayBrush { get; }


        public abstract Color WhiteColor { get; }
        public abstract Color BlackColor { get; }
        public abstract Color DarkBlackColor { get; }

        public abstract Color AccentColor { get; }

        public abstract Color GrayColor { get; }
        public abstract Color LightGrayColor { get; }
        public abstract Color DeepGrayColor { get; }
        public abstract Color DarkGrayColor { get; }
        public abstract Color MediumGrayColor { get; }

        public Button Button(string text) => new Button()
            .Text(text)
            .TextColor(WhiteColor)
            .BackgroundColor(BlackColor)
            .CornerRadius(0)
            .BorderWidth(0)
            ;

        public Button ToggleButton(string text, bool selected) => new Button()
            .Text(text)
            .TextColor(selected ? WhiteColor : BlackColor)
            .BackgroundColor(selected ? BlackColor : WhiteColor)
            .BorderWidth(1)
            .BorderColor(BlackColor)
            .CornerRadius(0)
            .BorderWidth(0)
            ;

        public Button TransparentButton(string text) => new Button()
            .Text(text)
            .TextColor(WhiteColor)
            .BackgroundColor(Colors.Transparent)
            .CornerRadius(0)
            .BorderWidth(0)
            ;

        public ImageButton ImageButton(string imageSource) => new ImageButton()
            .Source(imageSource)
            .BackgroundColor(WhiteColor)
            .BorderWidth(0)
            ;

        public ImageButton ImageButton(Func<string> imageSource) 
            => new ImageButton()
                .Source(imageSource)
                .BackgroundColor(WhiteColor)
                .BorderWidth(0)
                ;

        public Label Label(string text) => new Label()
            .Text(text)
            .FontSize(18)
            .TextColor(BlackColor)
            ;

        public Label Label(Func<string> text) => new Label()
            .Text(text)
            .FontSize(18)
            .TextColor(BlackColor)
            ;

        public Label H1(string text) => new Label()
            .Text(text)
            .FontSize(28)
            .TextColor(WhiteColor)
            ;

        public Label H2(string text) => new Label()
            .Text(text)
            .FontSize(22)
            .TextColor(WhiteColor)
            ;

        public Label H3(string text) => new Label()
            .Text(text)
            .FontSize(20)
            .TextColor(WhiteColor)
            ;
    }

    class LightTheme : Theme
    {
        public override MauiControls.Brush BlackBrush { get; } = new MauiControls.SolidColorBrush(Color.FromArgb("#140D1B"));
        public override MauiControls.Brush DarkBlackBrush { get; } = new MauiControls.SolidColorBrush(Color.FromArgb("#110B17"));
        public override MauiControls.Brush WhiteBrush { get; } = new MauiControls.SolidColorBrush(Color.FromArgb("#FFFFFF"));
        public override MauiControls.Brush AccentBrush { get; } = new MauiControls.SolidColorBrush(Color.FromArgb("#00FFD1"));
        public override MauiControls.Brush GrayBrush { get; } = new MauiControls.SolidColorBrush(Color.FromArgb("#F6F6F6"));
        public override MauiControls.Brush LightGrayBrush { get; } = new MauiControls.SolidColorBrush(Color.FromArgb("#F0F0F0"));

        public override Color DarkBlackColor { get; } = Color.FromArgb("#110B17");
        public override Color BlackColor { get; } = Color.FromArgb("#140D1B");
        public override Color WhiteColor { get; } = Color.FromArgb("#FFFFFF");
        public override Color AccentColor { get; } = Color.FromArgb("#00FFD1");
        public override Color GrayColor { get; } = Color.FromArgb("#F6F6F6");
        public override Color LightGrayColor { get; } = Color.FromArgb("#F0F0F0");
        public override Color DeepGrayColor { get; } = Color.FromArgb("#454E5C");
        public override Color DarkGrayColor { get; } = Color.FromArgb("#1C1C1C");
        public override Color MediumGrayColor { get; } = Color.FromArgb("#B7BDC7");

    }


}
