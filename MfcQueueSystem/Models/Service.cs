using System;
using System.Collections.Generic;

namespace MfcQueueSystem.Models;

public partial class Service
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = null!;

    public string ServiceGroup { get; set; } = null!;

    public int AvgServiceTimeMin { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
