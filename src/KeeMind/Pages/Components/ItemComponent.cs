using KeeMind.Controls;
using KeeMind.Services.Data;
using KeeMind.Resources;
using MauiReactor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KeeMind.Services;
using Microsoft.Maui.Devices;

namespace KeeMind.Pages.Components;

class ItemComponentState
{
    public bool ShowMaskedValue {  get; set; }
}


partial class ItemComponent : Component<ItemComponentState>
{
    [Prop]
    Item? _item;

    [Prop] 
    bool _isEditing = false;

    [Prop]
    Action? _onDelete;

    [Prop]
    Action? _onUpdate;


    #region Render
    public override VisualNode Render()
    {
        if (_item == null)
        {
            return null!;
        }

        if (_isEditing)
        {
            return RenderEditableItem();
        }
        else
        {
            return RenderReadonlyItem();
        }
    }

    VisualNode RenderEditableItem()
    {
        ValidateExtensions.ThrowIfNull(_item);
        return new Grid("*, Auto", "*, Auto")
        {
            new VStack(spacing: 0)
            {
                new BorderlessEntry()
                    .When(string.IsNullOrWhiteSpace(_item.Label), _=>_.Placeholder("Label"))
                    .Text(_item.Label ?? string.Empty)
                    .TextColor(Theme.Current.BlackColor)
                    .PlaceholderColor(Theme.Current.MediumGrayColor)
                    .OnTextChanged(newValue =>
                    {
                        _item.Label = newValue;
                        _onUpdate?.Invoke();
                        //if (_item.EditMode == EditMode.None)
                        //{
                        //    _item.EditMode = EditMode.Modified;
                        //}
                    })
                    .When(DeviceInfo.Current.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.MacCatalyst, _=>_.Margin(0,5))
                    ,

                new Grid("*", "Auto,*")
                {
                    Theme.Current.ImageButton(_item.IsMasked ? "lock_close.png" : "lock_open.png")
                        .Aspect(Aspect.Center)
                        .OnClicked(() =>
                        {
                            _item.IsMasked = !_item.IsMasked;

                            _onUpdate?.Invoke();
                            //if (_item.EditMode == EditMode.None)
                            //{
                            //    _item.EditMode = EditMode.Modified;
                            //}

                            Invalidate();
                        })
                        .Margin(0, 0, 4, 0),

                    new BorderlessEntry()
                        .When(string.IsNullOrWhiteSpace(_item.Value), _=>_.Placeholder("Value"))
                        .Text(_item.Value ?? string.Empty)
                        .TextColor(Theme.Current.BlackColor)
                        .PlaceholderColor(Theme.Current.MediumGrayColor)
                        .FontSize(20)
                        .OnTextChanged(newValue =>
                        {
                            _item.Value = newValue;
                            _onUpdate?.Invoke();
                            //if (_item.EditMode == EditMode.None)
                            //{
                            //    _item.EditMode = EditMode.Modified;
                            //}
                        })
                        .GridColumn(1),
                }

            }
                .Margin(16, 0),

            Theme.Current.ImageButton("delete_black.png")
                .Aspect(Aspect.Center)
                .OnClicked(_onDelete)
                .GridColumn(1)
                .Margin(16, 0),

            new Border()                    
                .HeightRequest(1)
                .BackgroundColor(Theme.Current.LightGrayColor)
                .GridRow(1)
                .GridColumnSpan(2)
        };
    }

    VisualNode RenderReadonlyItem()
    {
        ValidateExtensions.ThrowIfNull(_item);
        return new VStack(spacing: 0)
        {
            Theme.Current.Label(_item.Label.ToUpperInvariant())
                .FontSize(14)
                .Margin(16,0),

            new Grid("*", "*, Auto")
            {
                Theme.Current.Label(()=> !_item.IsMasked ? _item.Value : (State.ShowMaskedValue ? _item.Value : new string('●', _item.Value.Length)))
                    .FontAttributes(MauiControls.FontAttributes.Bold)
                    ,

                Theme.Current.ImageButton(() => State.ShowMaskedValue ? "eye_open.png" : "eye_close.png")
                    .Aspect(Aspect.Center)
                    .OnClicked(()=>SetState(s => s.ShowMaskedValue = !s.ShowMaskedValue, invalidateComponent: false))
                    .GridColumn(1)
                    .Margin(16, 0)
                    .IsVisible(_item.IsMasked),
            }
            .Margin(16,5),

            new Border()
                .HeightRequest(1)
                
        };
    }
    #endregion
}
