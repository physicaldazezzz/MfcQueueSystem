using System;
using System.Collections.Generic;

namespace MfcQueueSystem.Models
{
    public partial class Service
    {
        public int ServiceId { get; set; }

        public string ServiceName { get; set; } = null!;

        public string ServiceGroup { get; set; } = null!;

        public int AvgServiceTimeMin { get; set; }

        public string ApplicantType { get; set; } = null!; // Тип заявителя (Physical/Legal)

        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        // Связь с таблицей EmployeeService
        public virtual ICollection<EmployeeService> EmployeeServices { get; set; } = new List<EmployeeService>();
    }
}