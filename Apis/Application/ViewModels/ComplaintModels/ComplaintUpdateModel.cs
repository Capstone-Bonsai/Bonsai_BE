using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.ComplaintModels
{
    public class ComplaintUpdateModel
    {
        public Guid ComplaintId { get; set; }
        public ComplaintStatus ComplaintStatus { get; set; }
        public string? CancelReason { get; set; }
    }
}
