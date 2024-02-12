using ReactorData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeeMind.Services.Data;

[Model]
public partial class Tag
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public ICollection<TagEntry>? Entries { get; }
    
    //[NotMapped]
    //public EditMode EditMode { get; set; }
}

[Table("TagEntries")]
[Model]
public partial class TagEntry
{
    public int Id { get; set; }

    public required Tag Tag { get; set; }

    public Card? Card { get; set; }

    //[NotMapped]
    //public EditMode EditMode { get; set; }
}
