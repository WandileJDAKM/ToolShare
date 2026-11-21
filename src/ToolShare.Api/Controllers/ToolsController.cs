using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore;
using ToolShare.Api.Data; using ToolShare.Api.Models; using ToolShare.Api.Security;
namespace ToolShare.Api.Controllers;
public record ToolDto(string Name, string Description, string Category);
[ApiController, Route("api/tools"), SessionAuth]
public class ToolsController(ToolShareContext db) : ControllerBase {
  /// <summary>Lists tools. Optional search text and availability filter.</summary>
  [HttpGet]
  public async Task<IActionResult> List(string? search, bool? available) {
    var q = db.Tools.AsQueryable();
    if (!string.IsNullOrWhiteSpace(search)) { var s = search.Trim().ToLower(); q = q.Where(t => t.Name.ToLower().Contains(s) || t.Category.ToLower().Contains(s)); }
    if (available != null) q = q.Where(t => t.IsAvailable == available);
    return Ok(await q.OrderBy(t => t.Name).ToListAsync());
  }
  /// <summary>Gets one tool.</summary>
  [HttpGet("{id}")] public async Task<IActionResult> Get(int id) { var t = await db.Tools.FindAsync(id); return t == null ? NotFound() : Ok(t); }
  /// <summary>Adds a tool to the library (Admin only).</summary>
  [HttpPost, SessionAuth("Admin")]
  public async Task<IActionResult> Create(ToolDto d) {
    if (string.IsNullOrWhiteSpace(d.Name)) return BadRequest("Name is required.");
    var t = new Tool { Name = d.Name.Trim(), Description = d.Description ?? "", Category = d.Category ?? "" };
    db.Tools.Add(t); await db.SaveChangesAsync(); return Created($"/api/tools/{t.Id}", t);
  }
  /// <summary>Updates a tool (Admin only).</summary>
  [HttpPut("{id}"), SessionAuth("Admin")]
  public async Task<IActionResult> Update(int id, ToolDto d) {
    var t = await db.Tools.FindAsync(id); if (t == null) return NotFound();
    if (string.IsNullOrWhiteSpace(d.Name)) return BadRequest("Name is required.");
    t.Name = d.Name.Trim(); t.Description = d.Description ?? ""; t.Category = d.Category ?? ""; await db.SaveChangesAsync(); return Ok(t);
  }
  /// <summary>Deletes a tool that has never been lent out (Admin only).</summary>
  [HttpDelete("{id}"), SessionAuth("Admin")]
  public async Task<IActionResult> Delete(int id) {
    var t = await db.Tools.FindAsync(id); if (t == null) return NotFound();
    if (await db.Loans.AnyAsync(l => l.ToolId == id)) return Conflict("This tool has loan history and cannot be deleted.");
    db.Tools.Remove(t); await db.SaveChangesAsync(); return NoContent();
  }
}
