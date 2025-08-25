using TaskManagmentSystem.Entities;
using Task = TaskManagmentSystem.Entities.Task;

public static class AuthHelper
{
    public static bool IsTaskOwner(Task task, User user) // Checks if the user is the owner of the task
        => task.Created_By == user.ID;

    public static bool IsAssigneeOrSameDepartment(Task task, User user) // Checks if the user is the assignee of the task or in the same department
    {
        if (task == null || user == null)
            return false;

        bool isAssignee = task.AssignedToUserID == user.ID;
        bool isSameDepartment = task.User != null && task.User.DepartmentID == user.DepartmentID;

        bool isAssigneeOrSameDepartment = isAssignee || isSameDepartment;
        return isAssigneeOrSameDepartment;
    }
}
