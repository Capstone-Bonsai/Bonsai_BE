using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.StyleViewModels
{
    public class StyleCountViewModel
    {
        public Guid Id { get; set; }
        public string name { get; set; } = default!;
        public int count { get; set; }
    }
}
