using System;
using System.Collections.Generic;

namespace MfcQueueSystem.Models;

public partial class Ticket
{
    public int TicketId { get; set; }

    public string TicketNumber { get; set; } = null!;

    public int ServiceId { get; set; }

    public DateTime TimeCreated { get; set; }

    public DateTime? TimeCalled { get; set; }

    public DateTime? TimeStart { get; set; }

    public DateTime? TimeEnd { get; set; }

    public string Status { get; set; } = null!;

    public int? EmployeeId { get; set; }

    public int? WindowId { get; set; }

    // --- ДОБАВЛЕННЫЕ ПОЛЯ ---
    public string? ClientName { get; set; } // ФИО Клиента
    public int Priority { get; set; }       // Приоритет (1 или 2)
    // ------------------------

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<QueueLog> QueueLogs { get; set; } = new List<QueueLog>();

    public virtual Service Service { get; set; } = null!;

    public virtual ServiceWindow? Window { get; set; }
}