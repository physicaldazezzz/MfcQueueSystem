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

    public virtual DbSet<QueueLog> QueueLogs { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<ServiceWindow> ServiceWindows { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=PHYSICALDAZE;Database=mfc111;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__7AD04F11A47905A5");

            entity.HasIndex(e => e.Login, "UQ__Employee__5E55825BC471CD63").IsUnique();

            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Login)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Window).WithMany(p => p.Employees)
                .HasForeignKey(d => d.WindowId)
                .HasConstraintName("FK__Employees__Windo__3E52440B");

            entity.HasMany(d => d.Services).WithMany(p => p.Employees)
                .UsingEntity<Dictionary<string, object>>(
                    "EmployeeService",
                    r => r.HasOne<Service>().WithMany()
                        .HasForeignKey("ServiceId")
                        .HasConstraintName("FK__EmployeeS__Servi__4222D4EF"),
                    l => l.HasOne<Employee>().WithMany()
                        .HasForeignKey("EmployeeId")
                        .HasConstraintName("FK__EmployeeS__Emplo__412EB0B6"),
                    j =>
                    {
                        j.HasKey("EmployeeId", "ServiceId").HasName("PK__Employee__C681F411DE83AA70");
                        j.ToTable("EmployeeServices");
                    });
        });

        modelBuilder.Entity<QueueLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__QueueLog__5E548648BB2B54E1");

            entity.Property(e => e.EventType).HasMaxLength(50);
            entity.Property(e => e.Note).HasMaxLength(500);

            entity.HasOne(d => d.Employee).WithMany(p => p.QueueLogs)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__QueueLogs__Emplo__4AB81AF0");

            entity.HasOne(d => d.Ticket).WithMany(p => p.QueueLogs)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__QueueLogs__Ticke__49C3F6B7");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__Services__C51BB00A3FDF6D34");

            entity.HasIndex(e => e.ServiceName, "UQ__Services__A42B5F99240A7382").IsUnique();

            entity.Property(e => e.ServiceGroup).HasMaxLength(100);
            entity.Property(e => e.ServiceName).HasMaxLength(255);
        });

        modelBuilder.Entity<ServiceWindow>(entity =>
        {
            entity.HasKey(e => e.WindowId).HasName("PK__ServiceW__1EEC64298F57D6E1");

            entity.HasIndex(e => e.WindowNumber, "UQ__ServiceW__B26FE3D8A763BF09").IsUnique();

            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__Tickets__712CC607D753549B");

            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TicketNumber)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.Employee).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__Tickets__Employe__45F365D3");

            entity.HasOne(d => d.Service).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tickets__Service__44FF419A");

            entity.HasOne(d => d.Window).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.WindowId)
                .HasConstraintName("FK__Tickets__WindowI__46E78A0C");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
