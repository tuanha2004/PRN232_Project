using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace API.Models;

public partial class ProjectPrn232Context : DbContext
{
    public ProjectPrn232Context()
    {
    }

    public ProjectPrn232Context(DbContextOptions<ProjectPrn232Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<CheckinRecord> CheckinRecords { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<JobAssignment> JobAssignments { get; set; }

    public virtual DbSet<Timetable> Timetables { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.ApplicationId).HasName("PK__Applicat__C93A4C99A1F54C07");

            entity.Property(e => e.AppliedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.StudentYear).HasMaxLength(20);
            entity.Property(e => e.WorkType).HasMaxLength(20);

            entity.HasOne(d => d.Job).WithMany(p => p.Applications)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__Applicati__JobId__52593CB8");

            entity.HasOne(d => d.Student).WithMany(p => p.Applications)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Applicati__Stude__5165187F");
        });

        modelBuilder.Entity<CheckinRecord>(entity =>
        {
            entity.HasKey(e => e.CheckinId).HasName("PK__CheckinR__F3C85D71FBAE307C");

            entity.Property(e => e.CheckinTime).HasColumnType("datetime");
            entity.Property(e => e.CheckoutTime).HasColumnType("datetime");

            entity.HasOne(d => d.Job).WithMany(p => p.CheckinRecords)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__CheckinRe__JobId__5DCAEF64");

            entity.HasOne(d => d.Student).WithMany(p => p.CheckinRecords)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__CheckinRe__Stude__5CD6CB2B");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.JobId).HasName("PK__Jobs__056690C238BD5C46");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.Salary).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(24)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Provider).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.ProviderId)
                .HasConstraintName("FK__Jobs__ProviderId__4D94879B");
        });

        modelBuilder.Entity<JobAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__JobAssig__32499E77B754EED1");

            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Working");

            entity.HasOne(d => d.Job).WithMany(p => p.JobAssignments)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__JobAssign__JobId__5812160E");

            entity.HasOne(d => d.Student).WithMany(p => p.JobAssignments)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__JobAssign__Stude__571DF1D5");
        });

        modelBuilder.Entity<Timetable>(entity =>
        {
            entity.HasKey(e => e.TimetableId).HasName("PK__Timetabl__68413F60D5E5D5BE");

            entity.Property(e => e.DayOfWeek).HasMaxLength(10);

            entity.HasOne(d => d.Job).WithMany(p => p.Timetables)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__Timetable__JobId__619B8048");

            entity.HasOne(d => d.Student).WithMany(p => p.Timetables)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Timetable__Stude__60A75C0F");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C3AABA034");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053401B31D89").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.Status)
                .HasMaxLength(24)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
