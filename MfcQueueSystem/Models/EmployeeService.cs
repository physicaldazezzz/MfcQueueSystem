using System;
using System.Collections.Generic;

namespace MfcQueueSystem.Models
{
    public partial class EmployeeService
    {
        public int EmployeeId { get; set; }
        public int ServiceId { get; set; }

        public virtual Employee Employee { get; set; } = null!;
        public virtual Service Service { get; set; } = null!;
    }
}