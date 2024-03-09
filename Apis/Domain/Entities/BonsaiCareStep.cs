using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class BonsaiCareStep: BaseEntity
    {
        [ForeignKey("CareStep")]
        public Guid CareStepId { get; set; }

        [ForeignKey("Contract")]
        public Guid ContractId { get; set; }
        public string? Note { get; set; }
        public DateTime? CompletedTime { get; set; }


        public virtual CareStep CareStep { get; set; }
        public virtual Contract Contract { get; set; }
    }
}
