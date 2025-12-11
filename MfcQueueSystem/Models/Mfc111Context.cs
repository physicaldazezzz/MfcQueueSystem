using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MfcQueueSystem.Models;

public partial class Mfc111Context : DbContext
{
    public Mfc111Context()
    {
    }

    public Mfc111Context(DbContextOptions<Mfc111Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Employee> Employees { get; set; }
    public virtual DbSet<EmployeeService> EmployeeServices { get; set; }
    public virtual DbSet<QueueLog> QueueLogs { get; set; }
    public virtual DbSet<Service> Services { get; set; }
    public virtual DbSet<ServiceWindow> ServiceWindows { get; set; }
    public virtual DbSet<Ticket> Tickets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code.
        => optionsBuilder.UseSqlServer("Server=PHYSICALDAZE;Database=mfc111;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Login).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(50); // Пароль
            entity.HasOne(d => d.Window).WithMany(p => p.Employees)
                .HasForeignKey(d => d.WindowId);
        });

        // ВАЖНО: Настройка таблицы связей
        modelBuilder.Entity<EmployeeService>(entity =>
        {
            entity.HasKey(e => new { e.EmployeeId, e.ServiceId }); // Составной ключ

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeServices)
                .HasForeignKey(d => d.EmployeeId);

            entity.HasOne(d => d.Service).WithMany(p => p.EmployeeServices)
                .HasForeignKey(d => d.ServiceId);
        });

        modelBuilder.Entity<QueueLog>(entity =>
        {
            entity.HasKey(e => e.LogId);
            entity.Property(e => e.EventType).HasMaxLength(50);
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.HasOne(d => d.Ticket).WithMany(p => p.QueueLogs)
                .HasForeignKey(d => d.TicketId);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId);
            entity.Property(e => e.ApplicantType).HasMaxLength(50).HasDefaultValue("Physical");
            entity.Property(e => e.ServiceGroup).HasMaxLength(100);
            entity.Property(e => e.ServiceName).HasMaxLength(255);
        });

        modelBuilder.Entity<ServiceWindow>(entity =>
        {
            entity.HasKey(e => e.WindowId);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId);
            entity.Property(e => e.ClientName).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TicketNumber).HasMaxLength(10);
            entity.HasOne(d => d.Employee).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.EmployeeId);
            entity.HasOne(d => d.Service).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.ServiceId);
            entity.HasOne(d => d.Window).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.WindowId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}