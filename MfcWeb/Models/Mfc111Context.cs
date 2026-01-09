using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MfcWeb.Models
{
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
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Строка для тестов, если не подхватится из appsettings
                optionsBuilder.UseSqlServer("Server=PHYSICALDAZE;Database=mfc111;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.FullName).HasMaxLength(255);
                entity.Property(e => e.Login).HasMaxLength(50);
                entity.HasIndex(e => e.Login, "UQ__Employee__5E55825B").IsUnique();
                entity.HasOne(d => d.Window).WithMany(p => p.Employees).HasForeignKey(d => d.WindowId);
            });

            modelBuilder.Entity<EmployeeService>(entity =>
            {
                entity.HasKey(e => new { e.EmployeeId, e.ServiceId });
                entity.ToTable("EmployeeServices");
                entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeServices).HasForeignKey(d => d.EmployeeId);
                entity.HasOne(d => d.Service).WithMany(p => p.EmployeeServices).HasForeignKey(d => d.ServiceId);
            });

            modelBuilder.Entity<QueueLog>(entity =>
            {
                entity.HasKey(e => e.LogId);
                entity.Property(e => e.EventType).HasMaxLength(50);
                entity.Property(e => e.Note).HasMaxLength(500);
                entity.HasOne(d => d.Employee).WithMany(p => p.QueueLogs).HasForeignKey(d => d.EmployeeId);
                entity.HasOne(d => d.Ticket).WithMany(p => p.QueueLogs).HasForeignKey(d => d.TicketId);
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.Property(e => e.ServiceGroup).HasMaxLength(100);
                entity.Property(e => e.ServiceName).HasMaxLength(255);
                entity.Property(e => e.TargetType).HasMaxLength(10).HasDefaultValue("BOTH");
                entity.HasIndex(e => e.ServiceName, "UQ__Services__ServiceName").IsUnique();
            });

            modelBuilder.Entity<ServiceWindow>(entity =>
            {
                entity.HasKey(e => e.WindowId);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.HasIndex(e => e.WindowNumber, "UQ__ServiceW__WindowNum").IsUnique();
            });

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.TicketNumber).HasMaxLength(10);
                entity.Property(e => e.ClientName).HasMaxLength(255);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.BookingCode).HasMaxLength(10);

                entity.HasOne(d => d.Employee).WithMany(p => p.Tickets).HasForeignKey(d => d.EmployeeId);
                entity.HasOne(d => d.Service).WithMany(p => p.Tickets).HasForeignKey(d => d.ServiceId).OnDelete(DeleteBehavior.ClientSetNull);
                entity.HasOne(d => d.Window).WithMany(p => p.Tickets).HasForeignKey(d => d.WindowId);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}