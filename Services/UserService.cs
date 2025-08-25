using TaskManagmentSystem.Entities;
using TaskManagmentSystem.Data;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context) // Constructor to inject the database context
    {
        _context = context;
    }

    [HttpGet("myProfile/{userId}")]
    public IActionResult GetMyProfile(Guid userId) // Get the profile of the current user
    {
        var user = _context.Users
            .Where(u => u.ID == userId)
            .Select(u => new
            {
                u.First_Name,
                u.Last_Name,
                u.Email,
                u.Phone_Number,
                DepartmentName = u.Department != null ? u.Department.Department_Name : null,
                u.Title
            })
            .FirstOrDefault();

        if (user == null) return NotFound();

        return Ok(user);
    }

    [HttpGet("all")]
    public IActionResult GetAllUsers() // Get a list of all users
    {
        var users = _context.Users
            .Select(u => new
            {
                u.ID,
                FullName = u.First_Name + " " + u.Last_Name,
                u.Email,
                PhoneNumber = u.Phone_Number,
                DepartmentName = u.Department != null ? u.Department.Department_Name : null
            })
            .ToList();


        return Ok(users);
    }

    [HttpPost("create")]
    public IActionResult CreateUser([FromBody] User request)
    {
        var user = new User
        {
            ID = Guid.NewGuid(),
            First_Name = request.First_Name,
            Last_Name = request.Last_Name,
            Email = request.Email,
            Password = request.Password,
            Phone_Number = request.Phone_Number,
            DepartmentID = request.DepartmentID,
            Title = request.Title,
            Created_At = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return Ok(new { Message = "User created successfully", UserID = user.ID });
    }


    [HttpPut("update/{userId}")]
    public IActionResult UpdateUser(Guid userId, User updateData) // Update user information
    {
        var user = _context.Users.Find(userId);
        if (user == null) return NotFound();

        user.First_Name = updateData.First_Name;
        user.Last_Name = updateData.Last_Name;
        user.Phone_Number = updateData.Phone_Number;
        user.Email = updateData.Email;
        user.Password = updateData.Password;
        user.Updated_At = DateTime.UtcNow;

        _context.SaveChanges();
        return Ok("User updated successfully.");
    }
}
