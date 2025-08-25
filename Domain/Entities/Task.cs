using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TaskManagmentSystem.Entities;

public class Task : BaseEntity
{
    [MaxLength(100)]
    public string Task_Title { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    public TaskStatus Task_Status { get; set; }

    public DateTime? Approved_At { get; set; } = null;
    public DateTime? Completed_At { get; set; } = null;

    public enum TaskStatus
    {
        Pending,
        Approved,
        Completed,
        Rejected
    }

    //FK
    public Guid AssignedToUserID { get; set; }
    [JsonIgnore]
    public User? User { get; set; }


}
