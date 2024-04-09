using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum ContractStatus
    {
        Waiting =1, Paid = 2, Processing = 3, Faild = 4, Canceled = 5 , Completed = 6, Complained = 7, ProcessingComplaint = 8,ProcessedComplaint = 9
    }
}
