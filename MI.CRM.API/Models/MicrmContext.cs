using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MI.CRM.API.Models;

public partial class MicrmContext : DbContext
{
    public MicrmContext()
    {
    }

    public MicrmContext(DbContextOptions<MicrmContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActivityType> ActivityTypes { get; set; }

    public virtual DbSet<Budget> Budgets { get; set; }

    public virtual DbSet<BudgetCategory> BudgetCategories { get; set; }

    public virtual DbSet<BudgetType> BudgetTypes { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectBudgetEntry> ProjectBudgetEntries { get; set; }

    public virtual DbSet<ProjectManager> ProjectManagers { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SubContractor> SubContractors { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<TaskStatus> TaskStatuses { get; set; }

    public virtual DbSet<TasksNew> TasksNews { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=micrm-back;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<ActivityType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Activity__3214EC07943EA686");

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Budget__3214EC077FE6C2A6");

            entity.ToTable("Budget");

            entity.Property(e => e.BudgetType).HasMaxLength(100);
            entity.Property(e => e.Construnction).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Contractual).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Equipment).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.FringeBenefits).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IndirectCharges).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Other).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Personal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Supplies).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalDirectCharges).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Totals).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Travel).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Project).WithMany(p => p.Budgets)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK__Budget__ProjectI__44FF419A");
        });

        modelBuilder.Entity<BudgetCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BudgetCa__3214EC079AB80A2E");

            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BudgetType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BudgetTy__3214EC071255B2FD");

            entity.HasIndex(e => e.Name, "UQ__BudgetTy__737584F67DE1384C").IsUnique();

            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("PK__Project__761ABEF029BAD850");

            entity.HasIndex(e => e.AwardNumber, "UQ_Projects_AwardNumber").IsUnique();

            entity.Property(e => e.Agency).HasMaxLength(100);
            entity.Property(e => e.AwardNumber).HasMaxLength(100);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Company).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.ProjectManager).WithMany(p => p.Projects)
                .HasForeignKey(d => d.ProjectManagerId)
                .HasConstraintName("FK__Project__Project__47DBAE45");

            entity.HasOne(d => d.SubContractor).WithMany(p => p.Projects)
                .HasForeignKey(d => d.SubContractorId)
                .HasConstraintName("FK__Project__SubCont__48CFD27E");
        });

        modelBuilder.Entity<ProjectBudgetEntry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProjectB__3214EC077386FDA3");

            entity.HasIndex(e => new { e.ProjectId, e.CategoryId, e.TypeId }, "UQ_Project_Category_Type").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.AwardNumber).HasMaxLength(100);
            entity.Property(e => e.Notes)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.AwardNumberNavigation).WithMany(p => p.ProjectBudgetEntryAwardNumberNavigations)
                .HasPrincipalKey(p => p.AwardNumber)
                .HasForeignKey(d => d.AwardNumber)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProjectBudgetEntries_AwardNumber");

            entity.HasOne(d => d.Category).WithMany(p => p.ProjectBudgetEntries)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProjectBudgetEntries_Category");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectBudgetEntryProjects)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProjectBudgetEntries_Project");

            entity.HasOne(d => d.Type).WithMany(p => p.ProjectBudgetEntries)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProjectBudgetEntries_Type");
        });

        modelBuilder.Entity<ProjectManager>(entity =>
        {
            entity.HasKey(e => e.ProjectManagerId).HasName("PK__ProjectM__35F0311138C32202");

            entity.ToTable("ProjectManager");

            entity.HasOne(d => d.Role).WithMany(p => p.ProjectManagers)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__ProjectMa__RoleI__49C3F6B7");

            entity.HasOne(d => d.User).WithMany(p => p.ProjectManagers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__ProjectMa__UserI__4AB81AF0");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1AC6192F04");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<SubContractor>(entity =>
        {
            entity.HasKey(e => e.SubContractorId).HasName("PK__SubContr__1D56997D7BE82CB5");

            entity.ToTable("SubContractor");

            entity.HasOne(d => d.Role).WithMany(p => p.SubContractors)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__SubContra__RoleI__4BAC3F29");

            entity.HasOne(d => d.User).WithMany(p => p.SubContractors)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__SubContra__UserI__4CA06362");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tasks__3214EC074B2066B0");

            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.ActivityType).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.ActivityTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tasks__ActivityT__56E8E7AB");

            entity.HasOne(d => d.Project).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tasks__ProjectId__55F4C372");

            entity.HasOne(d => d.Status).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tasks__StatusId__57DD0BE4");
        });

        modelBuilder.Entity<TaskStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TaskStat__3214EC074D2E8679");

            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<TasksNew>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tasks_ne__3214EC0738843018");

            entity.ToTable("Tasks_new");

            entity.Property(e => e.ActivityType).HasMaxLength(100);
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.ProjectId).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(255);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C1CD36A72");

            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Password)
                .HasMaxLength(512)
                .IsUnicode(false);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
