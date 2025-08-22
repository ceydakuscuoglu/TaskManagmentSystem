using System.ComponentModel.DataAnnotations;

namespace TaskManagmentSystem.Entities;

public class Department : BaseEntity
{
    [MaxLength(100)]
    public string Department_Name { get; set; }

}