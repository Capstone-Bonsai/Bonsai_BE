using Application.ViewModels.CustomerGardenViewModels;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.CustomerBonsaiViewModels
{
    public class CustomerBonsaiViewModel
    {
        public Guid BonsaiId { get; set; }
        public Guid CustomerGardenId { get; set; }
        public Bonsai Bonsai { get; set; }
        public CustomerGardenViewModel CustomerGarden { get; set; }
        public Guid Id { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
