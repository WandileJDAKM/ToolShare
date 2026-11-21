using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore;
using ToolShare.Api.Data; using ToolShare.Api.Models; using ToolShare.Api.Security;
namespace ToolShare.Api.Controllers;
public record BorrowDto(int Days);
[ApiController, Route("api"), SessionAuth]
public class LoansController(ToolShareContext db) : ControllerBase {
  int Me => HttpContext.Session.GetInt32("UserId")!.Value;
  /// <summary>Borrows an available tool for 1 to 30 days.</summary>
  [HttpPost("tools/{id}/borrow")]
  public async Task<IActionResult> Borrow(int id, BorrowDto d) {
    if (d.Days is < 1 or > 30) return BadRequest("Days must be between 1 and 30.");
    var t = await db.Tools.FindAsync(id); if (t == null) return NotFound();
    if (!t.IsAvailable) return Conflict("This tool is already on loan.");
    var l = new Loan { ToolId = id, UserId = Me, DueDate = DateTime.UtcNow.AddDays(d.Days) };
    t.IsAvailable = false; db.Loans.Add(l); await db.SaveChangesAsync();
    return Created("/api/loans/mine", new { l.Id, l.DueDate });
  }
  /// <summary>Returns a tool. Allowed for the borrower or an Admin.</summary>
  [HttpPost("loans/{id}/return")]
  public async Task<IActionResult> Return(int id) {
    var l = await db.Loans.Include(x => x.Tool).FirstOrDefaultAsync(x => x.Id == id); if (l == null) return NotFound();
    if (l.UserId != Me && HttpContext.Session.GetString("Role") != "Admin") return StatusCode(403);
    if (l.ReturnedAt != null) return Conflict("Already returned.");
    l.ReturnedAt = DateTime.UtcNow; l.Tool!.IsAvailable = true; await db.SaveChangesAsync(); return Ok(new { l.Id, l.ReturnedAt });
  }
  /// <summary>Lists the logged-in user's loans.</summary>
  [HttpGet("loans/mine")]
  public async Task<IActionResult> Mine() {
    var now = DateTime.UtcNow;
    return Ok(await db.Loans.Where(l => l.UserId == Me).OrderByDescending(l => l.BorrowedAt)
      .Select(l => new { l.Id, Tool = l.Tool!.Name, l.BorrowedAt, l.DueDate, l.ReturnedAt, Overdue = l.ReturnedAt == null && l.DueDate < now }).ToListAsync());
  }
  /// <summary>Lists all loans, optionally only overdue ones (Admin only).</summary>
  [HttpGet("loans"), SessionAuth("Admin")]
  public async Task<IActionResult> All(bool? overdue) {
    var now = DateTime.UtcNow; var q = db.Loans.AsQueryable();
    if (overdue == true) q = q.Where(l => l.ReturnedAt == null && l.DueDate < now);
    return Ok(await q.OrderByDescending(l => l.BorrowedAt)
      .Select(l => new { l.Id, Tool = l.Tool!.Name, Member = l.User!.Name, l.BorrowedAt, l.DueDate, l.ReturnedAt, Overdue = l.ReturnedAt == null && l.DueDate < now }).ToListAsync());
  }
}
