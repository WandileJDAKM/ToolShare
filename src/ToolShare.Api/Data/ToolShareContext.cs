using Microsoft.EntityFrameworkCore; using ToolShare.Api.Models;
namespace ToolShare.Api.Data;
public class ToolShareContext(DbContextOptions<ToolShareContext> options) : DbContext(options) {
  public DbSet<User> Users => Set<User>(); public DbSet<Tool> Tools => Set<Tool>(); public DbSet<Loan> Loans => Set<Loan>();
  protected override void OnModelCreating(ModelBuilder b) {
    b.Entity<User>().HasIndex(u => u.Email).IsUnique();
    b.Entity<Loan>().HasOne(l => l.Tool).WithMany().HasForeignKey(l => l.ToolId).OnDelete(DeleteBehavior.Restrict);
    b.Entity<Loan>().HasOne(l => l.User).WithMany().HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.Restrict);
  }
}
