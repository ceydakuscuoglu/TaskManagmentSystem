using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TaskManagmentSystem.Entities;

public abstract class BaseEntity
{
    [Key]
    [JsonIgnore]
    public Guid ID { get; set; } = Guid.NewGuid();

    public DateTime? Created_At { get; set; } = DateTime.UtcNow;
    public DateTime? Updated_At { get; set; } = DateTime.UtcNow;
    public DateTime? Deleted_At { get; set; }
    public Guid? Created_By { get; set; }
    public Guid? Updated_By { get; set; }
    public Guid? Deleted_By { get; set; }

}