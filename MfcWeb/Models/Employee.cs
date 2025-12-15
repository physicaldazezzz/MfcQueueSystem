using System;
using System.Collections.Generic;

namespace MfcWeb.Models
{
    public partial class Employee
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; } = null!;
        public string Login { get; set; } = null!;
        public int? WindowId { get; set; }

        public virtual ServiceWindow? Window { get; set; }
        public virtual ICollection<EmployeeService> EmployeeServices { get; set; } = new List<EmployeeService>();
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        // ВОТ ЭТОЙ СТРОКИ НЕ ХВАТАЛО, ИЗ-ЗА НЕЁ ВСЁ РУШИЛОСЬ:
        public virtual ICollection<QueueLog> QueueLogs { get; set; } = new List<QueueLog>();
    }
}