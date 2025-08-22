using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagmentSystem.Data;
using Task = TaskManagmentSystem.Entities.Task;

[ApiController]
[Route("api/[controller]")]
public class TaskController : ControllerBase
{
    private readonly AppDbContext _context;

    public TaskController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("list")]
    public IActionResult ListTasks(
     string? assignedUserName,
     string? assignedUserEmail,
     string? departmentName,
     string? taskTitle,
     Guid? createdById,
     Task.TaskStatus? status)
    {
        var query = _context.Tasks
            .Include(t => t.User)
                .ThenInclude(u => u.Department)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(assignedUserName))
            query = query.Where(t => (t.User.First_Name + " " + t.User.Last_Name).Contains(assignedUserName.Trim()));

        if (!string.IsNullOrWhiteSpace(assignedUserEmail))
            query = query.Where(t => t.User.Email.Contains(assignedUserEmail.Trim()));

        if (!string.IsNullOrWhiteSpace(departmentName))
            query = query.Where(t => t.User.Department.Department_Name.Contains(departmentName.Trim()));

        if (!string.IsNullOrWhiteSpace(taskTitle))
            query = query.Where(t => (t.Task_Title ?? "").Contains(taskTitle.Trim()) || (t.Description ?? "").Contains(taskTitle.Trim()));

        if (createdById.HasValue)
            query = query.Where(t => t.Created_By == createdById.Value);

        if (status.HasValue)
            query = query.Where(t => t.Task_Status == status.Value);

        var tasks = query.Select(t => new
        {
            t.ID,
            t.Task_Title,
            t.Description,
            TaskStatus = t.Task_Status.ToString(),
            AssignedUser = t.User.First_Name + " " + t.User.Last_Name,
            AssignedUserEmail = t.User.Email,
            DepartmentName = t.User.Department.Department_Name,
            CreatedBy = t.User.First_Name + " " + t.User.Last_Name
        }).ToList();

        return Ok(tasks);
    }

    [HttpGet("detail/{taskId}")]
    public IActionResult TaskDetail(Guid taskId) //returns detailed information about a specific task
    {
        var task = _context.Tasks.Where(t => t.ID == taskId)
            .Select(t => new
            {
                t.ID,
                t.Task_Title,
                t.Description,
                TaskStatus = t.Task_Status.ToString(),
                Department = t.User.Department.Department_Name,
                AssignedTo = t.User != null ? $"{t.User.First_Name} {t.User.Last_Name}" : null,
                CreatedBy = t.User != null ? $"{t.User.First_Name} {t.User.Last_Name}" : null,
            }).FirstOrDefault();

        if (task == null) return NotFound();
        return Ok(task);
    }

    [HttpPost("create")]
    public IActionResult CreateTask(Task task, Guid currentUserId) // Create a new task
    {
        task.Task_Status = Task.TaskStatus.Pending;
        task.Created_By = currentUserId;
        task.Created_At = DateTime.UtcNow;

        _context.Tasks.Add(task);
        _context.SaveChanges();
        return Ok("Task created successfully.");
    }

    [HttpPut("approve/{taskId}/{currentUser}")]
    public IActionResult ApproveTask(Guid taskId, Guid currentUser) // Approve a task
    {
        var task = _context.Tasks.Find(taskId);
        if (task == null) return NotFound();
        var user = _context.Users
            .FirstOrDefault(u => u.ID == currentUser);
        if (!AuthorizationHelper.IsAssigneeOrSameDepartment(task, user))
            return Unauthorized();

        task.Task_Status = Task.TaskStatus.Approved;
        task.Updated_By = currentUser;
        task.Updated_At = DateTime.UtcNow;
        _context.SaveChanges();

        return Ok("Task approved successfully.");
    }

    [HttpPut("reject/{taskId}/{currentUser}")]
    public IActionResult RejectTask(Guid taskId, Guid currentUser) // Reject a task
    {
        var task = _context.Tasks.Find(taskId);
        if (task == null) return NotFound();

        var user = _context.Users
                 .FirstOrDefault(u => u.ID == currentUser);
        if (!AuthorizationHelper.IsAssigneeOrSameDepartment(task, user))
            return Unauthorized();
        task.Task_Status = Task.TaskStatus.Rejected;
        task.Updated_By = currentUser;
        task.Updated_At = DateTime.UtcNow;
        _context.SaveChanges();

        return Ok("Task rejected successfully.");
    }

    [HttpGet("assigned/{userId}")]
    public IActionResult ListAssignedTasks(Guid userId) // List tasks assigned to a specific user
    {
        var user = _context.Users.Find(userId);
        if (user == null) return NotFound();

        var tasks = _context.Tasks
            .Include(t => t.User)
                .ThenInclude(u => u.Department)
            .Include(t => t.User)
            .Include(t => t.User)
            .Where(t => t.User.ID == userId || t.User.DepartmentID == user.DepartmentID)
            .Select(t => new
            {
                t.ID,
                t.Task_Title,
                TaskStatus = t.Task_Status.ToString(),
                TaskDescription = t.Description,
                CreatedTime = t.Created_At,
                AssignedUser = t.User != null ? $"{t.User.First_Name} {t.User.Last_Name}" : null,
                AssignedDepartment = t.User != null && t.User.Department != null ? t.User.Department.Department_Name : null,
                CreatedBy = _context.Users.Where(u => u.ID == t.Created_By)
                                      .Select(u => u.First_Name + " " + u.Last_Name)
                                      .FirstOrDefault()
            })
            .ToList();

        return Ok(tasks);
    }

    [HttpPut("complete/{taskId}/{userId}")]
    public IActionResult CompleteTask(Guid taskId, Guid userId) // Complete a task
    {
        var task = _context.Tasks
            .Include(t => t.User)
            .FirstOrDefault(t => t.ID == taskId);

        if (task == null) return NotFound();

        var user = _context.Users
          .FirstOrDefault(u => u.ID == userId);
        if (!AuthorizationHelper.IsAssigneeOrSameDepartment(task, user))
            return Unauthorized();

        task.Task_Status = Task.TaskStatus.Completed;
        task.Updated_By = userId;
        task.Updated_At = DateTime.UtcNow;
        _context.SaveChanges();

        return Ok("Task completed successfully.");
    }


    [HttpPut("update/{taskId}/{userId}")]
    public IActionResult UpdateTask(Guid taskId, Guid userId, [FromBody] Task updateData) // Update task information
    {
        var task = _context.Tasks.Find(taskId);
        if (task == null) return NotFound("Task not found.");

        var user = _context.Users
                 .FirstOrDefault(u => u.ID == userId);
        if (!AuthorizationHelper.IsTaskOwner(task, user))
            return Unauthorized("Only the task owner can update the task.");

        task.Task_Title = updateData.Task_Title;
        task.Description = updateData.Description;
        task.Task_Status = updateData.Task_Status;
        task.Updated_By = userId;
        task.Updated_At = DateTime.UtcNow;

        _context.SaveChanges();
        return Ok("Task updated successfully.");
    }

    [HttpDelete("delete/{taskId}/{userId}")]
    public IActionResult DeleteTask(Guid taskId, Guid userId) // Delete a task
    {
        var task = _context.Tasks.Find(taskId);
        if (task == null) return NotFound("Task not found.");

        var user = _context.Users
                 .FirstOrDefault(u => u.ID == userId);
        if (!AuthorizationHelper.IsTaskOwner(task, user))
            return Unauthorized("Only the task owner can update the task.");

        task.Deleted_At = DateTime.UtcNow;
        task.Deleted_By = userId;

        _context.Tasks.Remove(task);
        _context.SaveChanges();

        return Ok("Task deleted successfully.");
    }
}
