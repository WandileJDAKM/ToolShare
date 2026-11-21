namespace ToolShare.Api.Models;
public class User { public int Id { get; set; } public string Name { get; set; } = ""; public string Email { get; set; } = ""; public string PasswordHash { get; set; } = ""; public string Role { get; set; } = "Member"; }
public class Tool { public int Id { get; set; } public string Name { get; set; } = ""; public string Description { get; set; } = ""; public string Category { get; set; } = ""; public bool IsAvailable { get; set; } = true; }
public class Loan {
  public int Id { get; set; } public int ToolId { get; set; } public Tool? Tool { get; set; } public int UserId { get; set; } public User? User { get; set; }
  public DateTime BorrowedAt { get; set; } = DateTime.UtcNow; public DateTime DueDate { get; set; } public DateTime? ReturnedAt { get; set; }
}
