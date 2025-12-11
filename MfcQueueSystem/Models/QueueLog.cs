using System;
using System.Collections.Generic;

namespace MfcQueueSystem.Models;

public partial class QueueLog
{
    public int LogId { get; set; }

    public int TicketId { get; set; }

    public DateTime EventTime { get; set; }

    public string EventType { get; set; } = null!;

    public int? EmployeeId { get; set; }

    public string? Note { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;
}
