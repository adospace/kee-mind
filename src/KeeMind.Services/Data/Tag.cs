using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeeMind.Services.Data;

public class Tag
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public ICollection<TagEntry>? Entries { get; }
    
    [NotMapped]
    public EditMode EditMode { get; set; }
}

[Table("TagEntries")]
public class TagEntry
{
    public int Id { get; set; }

    public required Tag Tag { get; set; }

    public required Card Card { get; set; }

    [NotMapped]
    public EditMode EditMode { get; set; }
}


//public class EditableTagEntry
//{
//    public EditableTagEntry()
//    {
//        EditMode = EditMode.New;
//    }

//    public EditableTagEntry(TagEntry tagEntry)
//    {
//        Id = tagEntry.Id;
//        Tag = tagEntry.Tag;
//        EditMode = EditMode.None;
//        Card = tagEntry.Card;
//        Name = tagEntry.Tag?.Name;
//    }

//    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

//    public string? Name { get; }
//    public Tag? Tag { get; set; }
//    public Card? Card { get; set; }

//    public EditMode EditMode { get; internal set; }

//    public bool IsValid => Tag != null && Card != null;

//    public bool IsEmpty() => Tag == null && Card == null;

//    internal void SaveChangesTo(Realm db, Card card)
//    {
//        Card = card;

//        if (EditMode == EditMode.Deleted)
//        {
//            var itemToRemove = db.Find<TagEntry>(Id);
//            if (itemToRemove != null)
//            {
//                db.Remove(itemToRemove);
//            }
//        }
//        else if (EditMode == EditMode.New)
//        {
//            var itemToAdd = this.ToTagEntry();
//            db.Add(itemToAdd);
//        }
//        else if (EditMode == EditMode.Modified)
//        {
//            var itemToUpdate = db.Find<TagEntry>(Id);
//            this.Populate(itemToUpdate);
//        }

//        EditMode = EditMode.None;
//    }
//}

//public static class EditableTagEntryExtensions
//{
//    public static TagEntry ToTagEntry(this EditableTagEntry TagEntry)
//    {
//        return new TagEntry
//        {
//            Id = TagEntry.Id,
//            Tag = TagEntry.Tag.ThrowIfNull(),
//            Card = TagEntry.Card.ThrowIfNull(),
//        };
//    }

//    public static EditableTagEntry ToEditableTagEntry(this TagEntry TagEntry)
//    {
//        return new EditableTagEntry(TagEntry);
//    }

//    public static TagEntry Populate(this EditableTagEntry editableTagEntry, TagEntry TagEntry)
//    {
//        TagEntry.Tag = editableTagEntry.Tag.ThrowIfNull();
//        TagEntry.Card = editableTagEntry.Card.ThrowIfNull();
//        return TagEntry;
//    }
//}

//public class EditableTag
//{
//    private string? _name;

//    public EditableTag()
//    {
//        EditMode = EditMode.New;
//    }

//    public EditableTag(Tag tag)
//    {
//        Id = tag.Id;
//        _name = tag.Name;
//        EditMode = EditMode.None;
//    }

//    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

//    internal Tag? Tag { get; set; }

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

//    public EditMode EditMode { get; internal set; }

//    public bool IsValid => !string.IsNullOrWhiteSpace(Name);

//    public bool IsEmpty() => string.IsNullOrWhiteSpace(Name);

//    internal void SaveChangesTo(Realm db, Card card)
//    {
//        if (EditMode == EditMode.Deleted)
//        {
//            var itemToRemove = db.Find<Tag>(Id);
//            if (itemToRemove != null)
//            {
//                db.Remove(itemToRemove);
//            }
//        }
//        else if (EditMode == EditMode.New)
//        {
//            var itemToAdd = this.ToTag();
//            db.Add(itemToAdd);
//        }
//        else if (EditMode == EditMode.Modified)
//        {
//            var itemToUpdate = db.Find<Tag>(Id);
//            this.Populate(itemToUpdate);
//        }

//        EditMode = EditMode.None;
//    }
//}

//public static class EditableTagExtensions
//{
//    public static Tag ToTag(this EditableTag tag)
//    {
//        return tag.Tag ??= new Tag
//        {
//            Id = tag.Id,
//            Name = tag.Name.ThrowIfNull(),
//        };
//    }

//    public static EditableTag ToEditableTag(this Tag tag)
//    {
//        return new EditableTag(tag);
//    }

//    public static Tag Populate(this EditableTag editableTag, Tag tag)
//    {
//        tag.Name = editableTag.Name.ThrowIfNull();

//        return tag;
//    }
//}
