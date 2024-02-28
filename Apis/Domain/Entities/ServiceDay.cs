using System.ComponentModel.DataAnnotations.Schema;
namespace Domain.Entities
{
    public class ServiceDay:BaseEntity
    {
        [ForeignKey("DayInWeek")]
        public Guid DayInWeekId { get; set; }
        [ForeignKey("ServiceOrder")]
        public Guid ServiceOrderId { get; set; }
        public int? NumberGardener { get; set; }
        public virtual DayInWeek DayInWeek { get; set; }
        public virtual ServiceOrder ServiceOrder { get; set; }

    }
}
