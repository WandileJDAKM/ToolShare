using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore;
using ToolShare.Api.Data; using ToolShare.Api.Models; using ToolShare.Api.Security;
namespace ToolShare.Api.Controllers;
public record RegisterDto(string Name, string Email, string Password);
public record LoginDto(string Email, string Password);
[ApiController, Route("api/auth")]
public class AuthController(ToolShareContext db) : ControllerBase {
  /// <summary>Creates an account. The very first account becomes the Admin; everyone after is a Member.</summary>
  [HttpPost("register")]
  public async Task<IActionResult> Register(RegisterDto d) {
    if (string.IsNullOrWhiteSpace(d.Name) || string.IsNullOrWhiteSpace(d.Email) || string.IsNullOrEmpty(d.Password) || d.Password.Length < 8)
      return BadRequest("Name, email and a password of 8+ characters are required.");
    var email = d.Email.Trim().ToLowerInvariant();
    if (await db.Users.AnyAsync(u => u.Email == email)) return Conflict("Email already registered.");
    var u = new User { Name = d.Name.Trim(), Email = email, PasswordHash = BCrypt.Net.BCrypt.HashPassword(d.Password), Role = await db.Users.AnyAsync() ? "Member" : "Admin" };
    db.Users.Add(u); await db.SaveChangesAsync();
    return Created("/api/auth/me", new { u.Id, u.Name, u.Role });
  }
  /// <summary>Logs in and stores the user id and role in the server-side session.</summary>
  [HttpPost("login")]
  public async Task<IActionResult> Login(LoginDto d) {
    var email = (d.Email ?? "").Trim().ToLowerInvariant();
    var u = await db.Users.FirstOrDefaultAsync(x => x.Email == email);
    if (u == null || !BCrypt.Net.BCrypt.Verify(d.Password ?? "", u.PasswordHash)) return Unauthorized("Invalid credentials.");
    HttpContext.Session.SetInt32("UserId", u.Id); HttpContext.Session.SetString("Role", u.Role);
    return Ok(new { u.Id, u.Name, u.Role });
  }
  /// <summary>Ends the session.</summary>
  [HttpPost("logout"), SessionAuth] public IActionResult Logout() { HttpContext.Session.Clear(); return NoContent(); }
  /// <summary>Returns the logged-in user.</summary>
  [HttpGet("me"), SessionAuth]
  public async Task<IActionResult> Me() {
    var u = await db.Users.FindAsync(HttpContext.Session.GetInt32("UserId")!.Value);
    return u == null ? NotFound() : Ok(new { u.Id, u.Name, u.Email, u.Role });
  }
}
