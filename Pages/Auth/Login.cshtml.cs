using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using TaskManagmentSystem.Data;

using Microsoft.AspNetCore.Authorization;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly AppDbContext _db;
    [BindProperty]
    public LoginInput Input { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public LoginModel(AppDbContext db)
    {
        _db = db;
    }

    public void OnGet() {}

    public async Task<IActionResult> OnPostAsync()
    {
        var user = _db.Users.FirstOrDefault(u => u.Email == Input.Email && u.Password == Input.Password);
        if (user == null)
        {
            ErrorMessage = "Invalid email or password";
            return Page();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
            new Claim(ClaimTypes.Name, user.First_Name + " " + user.Last_Name),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return RedirectToPage("/Index");
    }

    public class LoginInput
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}


