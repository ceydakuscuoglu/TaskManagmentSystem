using TaskManagmentSystem.Entities;
using Task = TaskManagmentSystem.Entities.Task;
public static class AuthorizationHelper
{
    public static bool IsTaskOwner(Task task, User user) //checks if the user is the owner of the task
        => task.Created_By == user.ID;

    public static bool IsAssigneeOrSameDepartment(Task task, User user) //checks if the user is the assignee of the task or the same department
        => task.AssignedToUserID == user.ID || task.User.DepartmentID == user.DepartmentID;
}
