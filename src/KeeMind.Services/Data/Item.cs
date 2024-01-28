using ReactorData;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeeMind.Services.Data;

[Model]
public partial class Item
{
    public int Id { get; set; }

    public int CardId { get; set; }

    public Card? Card { get; set; }

    public required string Label { get; set; }

    public required string Value { get; set; }

    public ItemType Type { get; set; }

    public bool IsMasked { get; set; }

    public bool IsEmpty() => string.IsNullOrWhiteSpace(Label) && string.IsNullOrWhiteSpace(Value);

    [NotMapped]
    public EditMode EditMode { get; set; }
}


//public class EditableItem
//{
//    private string? _label;
//    private string? _value;
//    private ItemType _type;
//    private bool _isFavorite;

//    public EditableItem()
//    {
//        EditMode = EditMode.New;
//    }

//    public EditableItem(Item item)
//    {
//        Id = item.Id;
//        Label = item.Label;
//        Value = item.Value;
//        Type = item.Type;
//        IsFavorite = item.IsFavorite;
//        EditMode = EditMode.None;
//        Card = item.Card;
//    }

//    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

//    public string? Label
//    {
//        get => _label;
//        set
//        {
//            _label = value;
//            if (EditMode == EditMode.None)
//            {
//                EditMode = EditMode.Modified;
//            }
//        }
//    }

//    public string? Value
//    {
//        get => _value; set
//        {
//            _value = value;
//            if (EditMode == EditMode.None)
//            {
//                EditMode = EditMode.Modified;
//            }
//        }
//    }

//    public Card? Card { get; private set; }

//    public ItemType Type
//    {
//        get => _type; set
//        {
//            _type = value;
//            if (EditMode == EditMode.None)
//            {
//                EditMode = EditMode.Modified;
//            }
//        }
//    }

//    public bool IsFavorite 
//    { 
//        get => _isFavorite;
//        set
//        {
//            _isFavorite = value;
//            if (EditMode == EditMode.None)
//            {
//                EditMode = EditMode.Modified;
//            }
//        }
//    }

//    public EditMode EditMode { get; internal set; }

//    public bool IsValid => !string.IsNullOrWhiteSpace(Label) && !string.IsNullOrWhiteSpace(Value);

//    public bool IsEmpty() => string.IsNullOrWhiteSpace(Label) && string.IsNullOrWhiteSpace(Value);

//    internal void SaveChangesTo(Realm db, Card card)
//    {
//        this.Card = card;

//        if (EditMode == EditMode.Deleted)
//        {
//            var itemToRemove = db.Find<Item>(Id);
//            if (itemToRemove != null)
//            {
//                db.Remove(itemToRemove);
//            }            
//        }
//        else if (EditMode == EditMode.New)
//        {
//            var itemToAdd = this.ToItem();
//            db.Add(itemToAdd);
//        }
//        else if (EditMode == EditMode.Modified)
//        {
//            var itemToUpdate = db.Find<Item>(Id);
//            this.Populate(itemToUpdate);
//        }

//        EditMode = EditMode.None;
//    }
//}

//public static class EditableItemExtensions
//{
//    public static Item ToItem(this EditableItem item)
//    {
//        return new Item
//        {
//            Id = item.Id,
//            IsFavorite = item.IsFavorite,
//            Label = item.Label.ThrowIfNull(),
//            Type = item.Type,
//            Value = item.Value.ThrowIfNull(),
//            Card = item.Card.ThrowIfNull(),
//        };
//    }

//    public static EditableItem ToEditableItem(this Item card)
//    {
//        return new EditableItem(card);
//    }

//    public static Item Populate(this EditableItem editableItem, Item item)
//    {
//        item.IsFavorite = editableItem.IsFavorite;
//        item.Label = editableItem.Label.ThrowIfNull();
//        item.Type = editableItem.Type;
//        item.Value = editableItem.Value.ThrowIfNull();
//        item.Card = editableItem.Card.ThrowIfNull();
//        return item;
//    }
//}