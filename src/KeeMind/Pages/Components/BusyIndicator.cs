﻿using KeeMind.Resources;
using MauiReactor;
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

partial class BusyIndicator : Component<BusyIndicatorState>
{
    #region Render
    public override VisualNode Render() 
        => HStack(spacing: 5,
            Ellipse()
                .HeightRequest(16)
                .WidthRequest(16)
                .Scale(()=> 0.7 + State.SizeDiff)
                .Fill(AppTheme.Current.WhiteColor)
                .Opacity(()=> 0.7 + State.OpacityDiff),

            Ellipse()
                .HeightRequest(16)
                .WidthRequest(16)
                .Scale(()=> 1.0 - State.SizeDiff)
                .Fill(AppTheme.Current.WhiteColor)
                .Opacity(()=> 1.0 - State.OpacityDiff),

            Ellipse()
                .HeightRequest(16)
                .WidthRequest(16)
                .Scale(()=> 0.7 + State.SizeDiff)
                .Fill(AppTheme.Current.WhiteColor)
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
        )
        .Center();
    #endregion
}

