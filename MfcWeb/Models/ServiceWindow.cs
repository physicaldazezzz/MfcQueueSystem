using System;
using System.Collections.Generic;

namespace MfcWeb.Models
{
    public partial class ServiceWindow
    {
        public int WindowId { get; set; }
        public int WindowNumber { get; set; }
        public string Status { get; set; } = null!;

        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}