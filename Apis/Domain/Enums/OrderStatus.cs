using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum OrderStatus
    {
        Waiting = 1, Paid = 2 , Preparing = 3,  Delivering = 4 , Delivered = 5, Failed = 6, Canceled= 7
    }
}
