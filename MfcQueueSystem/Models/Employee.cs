using System;
using System.Collections.Generic;

namespace MfcQueueSystem.Models
{
    public partial class Employee
    {
        public int EmployeeId { get; set; } // Главный ключ

        public string FullName { get; set; } = null!;

        public string Login { get; set; } = null!;

        public string Password { get; set; } = null!; // Пароль

        public int? WindowId { get; set; }

        public virtual ServiceWindow? Window { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        // Связь с таблицей EmployeeService
        public virtual ICollection<EmployeeService> EmployeeServices { get; set; } = new List<EmployeeService>();
    }
}