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
    public DateTime? Deleted_At { get; set; } = null;
    public Guid? Created_By { get; set; } = null;
    public Guid? Updated_By { get; set; } = null;
    public Guid? Deleted_By { get; set; } = null;

}