using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum ServiceStatus
    {
        Waiting = 1, Applied = 2, Paid = 3, OnProcessing = 4, Failed = 6, Canceled = 7, Completed = 8
    }
}
