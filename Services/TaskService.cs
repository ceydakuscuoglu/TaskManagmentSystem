using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagmentSystem.Data;
using TaskManagmentSystem.Services;
using System.Threading.Tasks;
using TaskEntity = TaskManagmentSystem.Entities.Task;

[ApiController]
[Route("api/[controller]")]
public class TaskController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmailSender _emailSender;

    public TaskController(AppDbContext context, IEmailSender emailSender)
    {
        _context = context;
        _emailSender = emailSender;
    }

    [HttpGet("list")]
    public IActionResult ListTasks(
     string? assignedUserName,
     string? assignedUserEmail,
     string? departmentName,
     string? taskTitle,
     Guid? createdById,
     TaskEntity.TaskStatus? status)
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

    [HttpGet("completedTasks")]
    public IActionResult ListCompletedTasks() // List completed tasks
    {
        var completedTasks = _context.Tasks
            .Where(t => t.Task_Status == TaskEntity.TaskStatus.Completed
                     && t.Approved_At.HasValue
                     && t.Completed_At.HasValue)
            .Select(t => new
            {
                t.ID,
                t.Task_Title,
                t.Description,
                TaskStatus = t.Task_Status.ToString(),
                AssignedUser = t.User.First_Name + " " + t.User.Last_Name,
                DepartmentName = t.User.Department.Department_Name,
                TotalTimeTaken = (t.Completed_At.Value - t.Approved_At.Value).Days + " day "
                    + (t.Completed_At.Value - t.Approved_At.Value).Hours + " hour "
                    + (t.Completed_At.Value - t.Approved_At.Value).Minutes + " minute",
                CompletedBy = t.Updated_By != null
                    ? _context.Users.Where(u => u.ID == t.Updated_By)
                        .Select(u => u.First_Name + " " + u.Last_Name)
                        .FirstOrDefault()
                    : null
            })
            .ToList();
        return Ok(completedTasks);
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
    public IActionResult CreateTask(TaskEntity task, Guid currentUserId) // Create a new task
    {
        if (currentUserId == Guid.Empty)
            return BadRequest("Current user ID cannot be null or empty.");

        task.Task_Status = TaskEntity.TaskStatus.Pending;
        task.Created_By = currentUserId;
        task.Created_At = DateTime.UtcNow;

        _context.Tasks.Add(task);
        _context.SaveChanges();
        return Ok("Task created successfully.");
    }

    [HttpPut("approve/{taskId}/{currentUser}")]
    public async Task<IActionResult> ApproveTask(Guid taskId, Guid currentUser) // Approve a task
    {
        var task = _context.Tasks
     .Include(t => t.User)
     .ThenInclude(u => u.Department)
     .FirstOrDefault(t => t.ID == taskId);
        if (task == null) return NotFound();
        var user = _context.Users.Find(currentUser);
        if (!AuthHelper.IsAssigneeOrSameDepartment(task, user))
            return Unauthorized();
        if (task.Task_Status != TaskEntity.TaskStatus.Pending)
            return BadRequest("Only pending tasks can be approved.");
        var oldStatus = task.Task_Status;
        task.Task_Status = TaskEntity.TaskStatus.Approved;
        task.Approved_At = DateTime.UtcNow;
        task.Updated_By = currentUser;
        task.Updated_At = DateTime.UtcNow;
        _context.SaveChanges();
        await NotifyStatusChangeAsync(task, oldStatus);
        return Ok("Task approved successfully.");
    }

    [HttpPut("reject/{taskId}/{currentUser}")]
    public async Task<IActionResult> RejectTask(Guid taskId, Guid currentUser) // Reject a task
    {
        var task = _context.Tasks
    .Include(t => t.User)
    .ThenInclude(u => u.Department)
    .FirstOrDefault(t => t.ID == taskId);
        if (task == null) return NotFound();

        var user = _context.Users.Find(currentUser);
        if (!AuthHelper.IsAssigneeOrSameDepartment(task, user))
            return Unauthorized();
        if (task.Task_Status != TaskEntity.TaskStatus.Pending)
            return BadRequest("Only pending tasks can be rejected.");
        var oldStatus = task.Task_Status;
        task.Task_Status = TaskEntity.TaskStatus.Rejected;
        task.Updated_By = currentUser;
        task.Updated_At = DateTime.UtcNow;
        _context.SaveChanges();
        await NotifyStatusChangeAsync(task, oldStatus);
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
    public async Task<IActionResult> CompleteTask(Guid taskId, Guid userId) // Complete a task
    {
        var task = _context.Tasks
     .Include(t => t.User)
     .ThenInclude(u => u.Department)
     .FirstOrDefault(t => t.ID == taskId);

        if (task == null) return NotFound();

        var user = _context.Users.Find(userId);
        if (!AuthHelper.IsAssigneeOrSameDepartment(task, user))
            return Unauthorized();
        if (task.Task_Status != TaskEntity.TaskStatus.Approved)
            return BadRequest("Only approved tasks can be marked as completed.");
        var oldStatus = task.Task_Status;
        task.Task_Status = TaskEntity.TaskStatus.Completed;
        task.Completed_At = DateTime.UtcNow;
        task.Updated_By = userId;
        task.Updated_At = DateTime.UtcNow;
        _context.SaveChanges();
        await NotifyStatusChangeAsync(task, oldStatus);
        return Ok("Task completed successfully.");
    }


    [HttpPut("update/{taskId}/{userId}")]
    public async Task<IActionResult> UpdateTask(Guid taskId, Guid userId, [FromBody] TaskEntity updateData) // Update task information
    {
        var task = _context.Tasks.Find(taskId);
        if (task == null) return NotFound("Task not found.");

        var user = _context.Users.Find(userId);
        if (!AuthHelper.IsTaskOwner(task, user))
            return Unauthorized("Only the task owner can update the task.");

        if (task.Task_Status == TaskEntity.TaskStatus.Completed || task.Task_Status == TaskEntity.TaskStatus.Rejected)
            return BadRequest("Completed or rejected tasks cannot be updated.");
        var oldStatus = task.Task_Status;
        task.Task_Title = updateData.Task_Title;
        task.Description = updateData.Description;
        task.Task_Status = updateData.Task_Status;
        task.Updated_By = userId;
        task.Updated_At = DateTime.UtcNow;

        _context.SaveChanges();
        if (oldStatus != task.Task_Status)
        {
            await NotifyStatusChangeAsync(task, oldStatus);
        }
        return Ok("Task updated successfully.");
    }

    [HttpDelete("delete/{taskId}/{userId}")]
    public IActionResult DeleteTask(Guid taskId, Guid userId) // Delete a task
    {
        var task = _context.Tasks.Find(taskId);
        if (task == null) return NotFound("Task not found.");

        var user = _context.Users.Find(userId);
        if (!AuthHelper.IsTaskOwner(task, user))
            return Unauthorized("Only the task owner can delete the task.");

        task.Deleted_At = DateTime.UtcNow;
        task.Deleted_By = userId;

        _context.Tasks.Remove(task);
        _context.SaveChanges();

        return Ok("Task deleted successfully.");
    }
    private async Task NotifyStatusChangeAsync(TaskEntity task, TaskEntity.TaskStatus oldStatus)
    {
        try
        {
            var newStatus = task.Task_Status;
            var taskTitle = task.Task_Title ?? "Task";

            var assigneeEmail = _context.Users
                .Where(u => u.ID == task.AssignedToUserID)
                .Select(u => u.Email)
                .FirstOrDefault();

            string creatorEmail = null;
            if (task.Created_By.HasValue)
            {
                creatorEmail = _context.Users
                    .Where(u => u.ID == task.Created_By.Value)
                    .Select(u => u.Email)
                    .FirstOrDefault();
            }

            var subject = $"Task status changed: {taskTitle}";
            var body = $"The task '{taskTitle}' status changed from {oldStatus} to {newStatus}.";

            if (!string.IsNullOrWhiteSpace(assigneeEmail))
            {
                await _emailSender.SendEmailAsync(assigneeEmail, subject, body);
            }
            if (!string.IsNullOrWhiteSpace(creatorEmail) && creatorEmail != assigneeEmail)
            {
                await _emailSender.SendEmailAsync(creatorEmail, subject, body);
            }
        }
        catch
        {
            // Intentionally swallow exceptions to avoid failing the API call due to email errors
        }
    }
}
