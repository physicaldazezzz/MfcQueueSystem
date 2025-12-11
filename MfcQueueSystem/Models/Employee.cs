using System;
using System.Collections.Generic;

namespace MfcQueueSystem.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string FullName { get; set; } = null!;

    public string Login { get; set; } = null!;

    public int? WindowId { get; set; }

    public virtual ICollection<QueueLog> QueueLogs { get; set; } = new List<QueueLog>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual ServiceWindow? Window { get; set; }

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
