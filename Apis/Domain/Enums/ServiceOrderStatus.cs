﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum ServiceOrderStatus
    {
        Pending = 1, WaitingForPayment = 2, Paid = 3, Processing = 4, Fail = 5, Canceled = 6, TaskFinished = 7, 
        Completed = 8, Complained = 99, ProcessingComplaint = 10, ProcessedComplaint = 11, DoneTaskComplaint = 12
    }
}
