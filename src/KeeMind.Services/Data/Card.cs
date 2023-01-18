using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace KeeMind.Services.Data;

public class Card
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public bool IsFavorite {  get; set; }

    public List<Item> Items { get; } = new();

    public List<TagEntry> Tags { get; } = new();

    public List<Attachment> Attachments { get; } = new();

    [NotMapped]
    public EditMode EditMode { get; set; }

}

//public partial class EditableCard
//{
//    private string? _name;

//    public EditableCard()
//    {
//        EditMode = EditMode.New;
//    }

//    public EditableCard(Card card)
//    {
//        Id = card.Id;
//        _name = card.Name;
//        _items = card.Items.ToList().Select(_ => new EditableItem(_)).ToList();
//        _tags = card.Tags.OrderBy(_ => _.Tag.Name).ToList().Select(_ => new EditableTagEntry(_)).ToList();
//    }

//    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

//    public string? Name 
//    {
//        get => _name;
//        set
//        {
//            _name = value;
//            if (EditMode == EditMode.None)
//            {
//                EditMode = EditMode.Modified;
//            }
//        }
//    }


//    public EditMode EditMode { get; private set; }

//    public async Task SaveChangesTo(Realm db)
//    {
//        await db.WriteAsync(() =>
//        {
//            if (EditMode == EditMode.Deleted)
//            {
//                var cardToRemove = db.Find<Card>(Id).ThrowIfNull();
//                db.Remove(cardToRemove);
//            }
//            else if (EditMode == EditMode.New)
//            {
//                var cardToAdd = this.ToCard();
//                db.Add(cardToAdd);

//                foreach (var item in AllItems)
//                {
//                    if (item.EditMode == EditMode.Deleted)
//                    {
//                        _items.Remove(item);
//                    }

//                    item.SaveChangesTo(db, cardToAdd);
//                }

//                foreach (var item in AllTags)
//                {
//                    if (item.EditMode == EditMode.Deleted)
//                    {
//                        _tags.Remove(item);
//                    }

//                    item.SaveChangesTo(db, cardToAdd);
//                }
//            }
//            else //if (EditMode == EditMode.Modified)
//            {
//                var cardToUpdate = db.Find<Card>(Id).ThrowIfNull();
//                this.Populate(cardToUpdate);

//                foreach (var item in AllItems.ToArray())
//                {
//                    if (item.EditMode == EditMode.Deleted)
//                    {
//                        _items.Remove(item);
//                    }

//                    item.SaveChangesTo(db, cardToUpdate);
//                }

//                foreach (var item in AllTags.ToArray())
//                {
//                    if (item.EditMode == EditMode.Deleted)
//                    {
//                        _tags.Remove(item);
//                    }

//                    item.SaveChangesTo(db, cardToUpdate);
//                }
//            }


//            foreach (var emptyTag in db.All<Tag>().ToArray())
//            {
//                if (!emptyTag.Entries!.Any())
//                {
//                    db.Remove(emptyTag);
//                }                
//            }
//        });

//        EditMode = EditMode.None;
//    }
//}

//public partial class EditableCard
//{
//    private readonly List<EditableItem> _items = new();

//    public IReadOnlyList<EditableItem> AllItems => _items;

//    public IReadOnlyList<EditableItem> Items => _items.Where(_ => _.EditMode != EditMode.Deleted).ToList();

//    public IReadOnlyList<EditableItem> DeletedItems => _items.Where(_ => _.EditMode == EditMode.Deleted).ToList();

//    public IReadOnlyList<EditableItem> AddedItems => _items.Where(_ => _.EditMode == EditMode.New).ToList();

//    public void AddItem(EditableItem editableItem)
//    {
//        _items.Add(editableItem);
//        if (EditMode == EditMode.None)
//        {
//            EditMode = EditMode.Modified;
//        }
//    }

//    public void RemoveItem(EditableItem editableItem)
//    {
//        if (EditMode == EditMode.New)
//        {
//            _items.Remove(editableItem);
//            return;
//        }

//        editableItem.EditMode = EditMode.Deleted;
//        if (EditMode == EditMode.None)
//        {
//            EditMode = EditMode.Modified;
//        }
//    }
//}

//public partial class EditableCard
//{
//    private readonly List<EditableTagEntry> _tags = new();

//    public IReadOnlyList<EditableTagEntry> AllTags => _tags;

//    public IReadOnlyList<EditableTagEntry> Tags => _tags.Where(_ => _.EditMode != EditMode.Deleted).ToList();

//    public IReadOnlyList<EditableTagEntry> DeletedTags => _tags.Where(_ => _.EditMode == EditMode.Deleted).ToList();

//    public IReadOnlyList<EditableTagEntry> AddedTags => _tags.Where(_ => _.EditMode == EditMode.New).ToList();

//    public void AddTag(EditableTag editableItem)
//    {
//        if (_tags.Any(_=>_.Tag?.Id == editableItem.Id))
//        {
//            return;
//        }

//        _tags.Add(new EditableTagEntry { Tag = editableItem.ToTag() });

//        if (EditMode == EditMode.None)
//        {
//            EditMode = EditMode.Modified;
//        }
//    }

//    public void RemoveTag(EditableTagEntry editableItem)
//    {
//        if (EditMode == EditMode.New)
//        {
//            _tags.Remove(editableItem);
//            return;
//        }

//        editableItem.EditMode = EditMode.Deleted;

//        if (EditMode == EditMode.None)
//        {
//            EditMode = EditMode.Modified;
//        }
//    }
//}



//public static class EditableCardExtensions
//{
//    public static EditableCard ToEditableCard(this Card card)
//    {
//        return new EditableCard(card);
//    }

//    public static Card ToCard(this EditableCard card)
//    {
//        return new Card
//        {
//            Id = card.Id,
//            Name = card.Name.ThrowIfNull(),
//        };
//    }

//    public static void Populate(this EditableCard editableCard, Card card)
//    {
//        card.Name = editableCard.Name.ThrowIfNull();
//    }
//}