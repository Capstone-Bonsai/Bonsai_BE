using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class CareStep:BaseEntity
    {
        [ForeignKey("Category")]
        public Guid CategoryId { get; set; }
        public int OrderStep { get; set; }
        public string Step { get; set; }
        public virtual Category Category { get; set; }
        public IList<BonsaiCareStep> BonsaiCareSteps { get; set; }
    }
}
