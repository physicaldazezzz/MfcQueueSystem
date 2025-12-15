using System;
using System.Collections.Generic;

namespace MfcWeb.Models
{
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

        // Поля, которые мы добавляли
        public string? ClientName { get; set; }
        public int Priority { get; set; }
        public DateTime? AppointmentTime { get; set; }
        public string? BookingCode { get; set; }
        public string? PhoneNumber { get; set; }

        public virtual Employee? Employee { get; set; }
        public virtual ICollection<QueueLog> QueueLogs { get; set; } = new List<QueueLog>();
        public virtual Service Service { get; set; } = null!;
        public virtual ServiceWindow? Window { get; set; }
    }
}