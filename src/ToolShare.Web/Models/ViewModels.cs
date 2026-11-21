namespace ToolShare.Web.Models;
public class ToolVm { public int Id { get; set; } public string Name { get; set; } = ""; public string Description { get; set; } = ""; public string Category { get; set; } = ""; public bool IsAvailable { get; set; } = true; }
public class LoanVm {
  public int Id { get; set; } public string Tool { get; set; } = ""; public string? Member { get; set; }
  public DateTime BorrowedAt { get; set; } public DateTime DueDate { get; set; } public DateTime? ReturnedAt { get; set; } public bool Overdue { get; set; }
}
