using KeeMind.Resources;
using MauiReactor.Animations;
using MauiReactor.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeeMind.Pages.Components;

class BusyIndicatorState
{
    public double SizeDiff {  get; set; }
    public double OpacityDiff { get; set; }
}

class BusyIndicator : Component<BusyIndicatorState>
{
    public override VisualNode Render()
    {
        return new HStack(spacing: 5)
        {
            new Ellipse()
                .HeightRequest(16)
                .WidthRequest(16)
                .Scale(()=> 0.7 + State.SizeDiff)
                .Fill(Theme.Current.WhiteColor)
                .Opacity(()=> 0.7 + State.OpacityDiff),

            new Ellipse()
                .HeightRequest(16)
                .WidthRequest(16)
                .Scale(()=> 1.0 - State.SizeDiff)
                .Fill(Theme.Current.WhiteColor)
                .Opacity(()=> 1.0 - State.OpacityDiff),

            new Ellipse()
                .HeightRequest(16)
                .WidthRequest(16)
                .Scale(()=> 0.7 + State.SizeDiff)
                .Fill(Theme.Current.WhiteColor)
                .Opacity(() => 0.7 + State.OpacityDiff),

            new AnimationController()
            {
                new ParallelAnimation
                {
                    new DoubleAnimation()
                        .StartValue(0)
                        .TargetValue(0.3)
                        .OnTick(v => SetState(s => s.SizeDiff = v, false)),

                    new DoubleAnimation()
                        .StartValue(0.0)
                        .TargetValue(0.3)
                        .OnTick(v => SetState(s => s.OpacityDiff = v, false)),
                }
                .Loop(true)
                .RepeatForever()
            }
            .IsEnabled(true)
        }
        .HCenter()
        .VCenter();
    }
}
