using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TaskManagmentSystem.Entities;

public class User : BaseEntity

{
    [MaxLength(100)]
    public string First_Name { get; set; }

    [MaxLength(100)]
    public string Last_Name { get; set; }

    [MaxLength(100)]
    public string Email { get; set; }

    [MaxLength(100)]
    public string Password { get; set; }

    [MaxLength(20)]
    public string Phone_Number { get; set; }

    [MaxLength(100)]
    public string Title { get; set; }

    //FK
    public Guid DepartmentID { get; set; }
    [JsonIgnore]
    public Department? Department { get; set; }




}