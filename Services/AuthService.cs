using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagmentSystem.Data;
using Microsoft.AspNetCore.Authentication;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(string email, string password) // Authenticate user
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
        if (user == null)
            return Unauthorized("Unvalid email or password");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.ID.ToString()),
            new(ClaimTypes.Email, user.Email)
        };

        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
        await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity));

        return Ok("Login successful");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout() // Logout user
    {
        await HttpContext.SignOutAsync("Cookies");
        return Ok("Logout successful");
    }
}
