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

    public virtual DbSet<DisbursementLog> DisbursementLogs { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectBudgetEntry> ProjectBudgetEntries { get; set; }

    public virtual DbSet<ProjectManager> ProjectManagers { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SubContractor> SubContractors { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<TaskLog> TaskLogs { get; set; }

    public virtual DbSet<TaskStatus> TaskStatuses { get; set; }

    public virtual DbSet<TasksNew> TasksNews { get; set; }

    public virtual DbSet<User> Users { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivityType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Activity__3214EC07A0C44FD3");

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
            entity.HasKey(e => e.Id).HasName("PK__BudgetCa__3214EC07C6C19E6A");

            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BudgetType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BudgetTy__3214EC07F56F4EFD");

            entity.HasIndex(e => e.Name, "UQ__BudgetTy__737584F6AAFD47D6").IsUnique();

            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<DisbursementLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Disburse__3214EC07B4EE032C");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DisbursedAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DisbursementDate).HasColumnType("datetime");
            entity.Property(e => e.Rate).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Units).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Category).WithMany(p => p.DisbursementLogs)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DisbursementLogs_Category");

            entity.HasOne(d => d.Document).WithMany(p => p.DisbursementLogs)
                .HasForeignKey(d => d.DocumentId)
                .HasConstraintName("FK_DisbursementLogs_Document");

            entity.HasOne(d => d.Project).WithMany(p => p.DisbursementLogs)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DisbursementLogs_Project");

            entity.HasOne(d => d.User).WithMany(p => p.DisbursementLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DisbursementLogs_User");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Document__3214EC077779A5AA");

            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.DocumentName).HasMaxLength(255);
            entity.Property(e => e.DocumentUrl).HasMaxLength(500);
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.DeletedByNavigation).WithMany(p => p.DocumentDeletedByNavigations)
                .HasForeignKey(d => d.DeletedBy)
                .HasConstraintName("FK_Document_DeletedBy_User");

            entity.HasOne(d => d.Project).WithMany(p => p.Documents)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Document_Project");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.DocumentUploadedByNavigations)
                .HasForeignKey(d => d.UploadedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Document_UploadedBy_User");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("PK__Project__761ABEF029BAD850");

            entity.HasIndex(e => e.AwardNumber, "UQ_Projects_AwardNumber").IsUnique();

            entity.Property(e => e.Agency).HasMaxLength(100);
            entity.Property(e => e.AwardNumber).HasMaxLength(100);
            entity.Property(e => e.BilledNotPaid).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Company).HasMaxLength(100);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.ProjectStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.TotalApprovedBudget).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalDisbursedBudget).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalRemainingBudget).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.ProjectManager).WithMany(p => p.Projects)
                .HasForeignKey(d => d.ProjectManagerId)
                .HasConstraintName("FK__Project__Project__47DBAE45");

            entity.HasOne(d => d.SubContractor).WithMany(p => p.Projects)
                .HasForeignKey(d => d.SubContractorId)
                .HasConstraintName("FK__Project__SubCont__48CFD27E");
        });

        modelBuilder.Entity<ProjectBudgetEntry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProjectB__3214EC0751C2DD10");

            entity.HasIndex(e => new { e.ProjectId, e.CategoryId, e.TypeId }, "UQ_Project_Category_Type").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.AwardNumber).HasMaxLength(100);
            entity.Property(e => e.Notes)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Category).WithMany(p => p.ProjectBudgetEntries)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProjectBudgetEntries_Category");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectBudgetEntries)
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
            entity.HasKey(e => e.ProjectManagerId).HasName("PK__ProjectM__35F03111BACD3BD6");

            entity.ToTable("ProjectManager");

            entity.HasOne(d => d.User).WithMany(p => p.ProjectManagers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__ProjectMa__UserI__4AB81AF0");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1AACBCFC2B");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<SubContractor>(entity =>
        {
            entity.HasKey(e => e.SubContractorId).HasName("PK__SubContr__1D56997D7BE82CB5");

            entity.ToTable("SubContractor");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tasks__3214EC0760E17E66");

            entity.Property(e => e.CompletedOn).HasColumnType("datetime");
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.DeliverableType).HasMaxLength(50);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.ActivityType).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.ActivityTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tasks__ActivityT__6A30C649");

            entity.HasOne(d => d.AssignedToNavigation).WithMany(p => p.TaskAssignedToNavigations)
                .HasForeignKey(d => d.AssignedTo)
                .HasConstraintName("FK_Tasks_AssignedTo_Users");

            entity.HasOne(d => d.CompletedByNavigation).WithMany(p => p.TaskCompletedByNavigations)
                .HasForeignKey(d => d.CompletedBy)
                .HasConstraintName("FK_Tasks_CompletedBy_Users");

            entity.HasOne(d => d.Project).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tasks__ProjectId__55F4C372");

            entity.HasOne(d => d.Status).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tasks__StatusId__6C190EBB");
        });

        modelBuilder.Entity<TaskLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TaskLogs__3214EC07212E2EF4");

            entity.Property(e => e.ActionTimestamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ActionType).HasMaxLength(50);
            entity.Property(e => e.FieldChanged).HasMaxLength(100);

            entity.HasOne(d => d.Task).WithMany(p => p.TaskLogs)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TaskLogs_Tasks");

            entity.HasOne(d => d.User).WithMany(p => p.TaskLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TaskLogs_Users");
        });

        modelBuilder.Entity<TaskStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TaskStat__3214EC07DECA3F48");

            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<TasksNew>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tasks_ne__3214EC07EDCEBD4D");

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
