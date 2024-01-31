using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Tasks : BaseEntity
    {
        public string Name { get; set; }
        public IList<BaseTask> BaseTasks { get; set; }
        public IList<OrderServiceTask> OrderServiceTasks { get; set; }

    }
}
