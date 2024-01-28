
using ReactorData;
using System.ComponentModel.DataAnnotations.Schema;

namespace KeeMind.Services.Data;

[Model]
[Table("Attachments")]
public partial class Attachment
{
    public int Id { get; set; }

    public required Card Card { get; set; }

    public required string Name { get; set; }

    public required byte[] Data { get; set; }

    public string? ContentType { get; set; }
}
