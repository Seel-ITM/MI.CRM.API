using Microsoft.EntityFrameworkCore;
using MI.CRM.API.Models;

namespace MI.CRM.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ProjectBudgetEntry> ProjectBudgetEntries { get; set; }
        public DbSet<BudgetCategory> BudgetCategories { get; set; }
        public DbSet<BudgetType> BudgetTypes { get; set; }
        public DbSet<Project> Projects { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // AwardNumberNavigation Mapping (AwardNumber → AwardNumber)
            modelBuilder.Entity<ProjectBudgetEntry>()
                .HasOne(p => p.AwardNumberNavigation)
                .WithMany(p => p.ProjectBudgetEntryAwardNumberNavigations)
                .HasForeignKey(p => p.AwardNumber)
                .HasPrincipalKey(p => p.AwardNumber)
                .OnDelete(DeleteBehavior.Restrict);

            // Project Mapping (ProjectId → ProjectId)
            modelBuilder.Entity<ProjectBudgetEntry>()
                .HasOne(p => p.Project)
                .WithMany(p => p.ProjectBudgetEntryProjects)
                .HasForeignKey(p => p.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
