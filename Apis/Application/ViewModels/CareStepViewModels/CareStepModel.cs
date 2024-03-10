using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.CareStepViewModels
{
    public class CareStepModel
    {
        public Guid CategoryId { get; set; }
        public List<string> CareSteps { get; set; } = default!;
    }
}
