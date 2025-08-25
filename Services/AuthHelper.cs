using TaskManagmentSystem.Entities;
using Task = TaskManagmentSystem.Entities.Task;

public static class AuthHelper
{
    public static bool IsTaskOwner(Task task, User user) // Checks if the user is the owner of the task
        => task.Created_By == user.ID;

    public static bool IsAssigneeOrSameDepartment(Task task, User user) // Checks if the user is the assignee of the task or in the same department
        => task.AssignedToUserID == user.ID || (task.User != null && task.User.DepartmentID == user.DepartmentID);

    public static bool IsUserAuthorizedForTask(Task task, User? user, bool checkOwnership = false)
    {
        if (user == null)
            return false;

        return checkOwnership
            ? IsTaskOwner(task, user)
            : IsAssigneeOrSameDepartment(task, user);
    }
}


