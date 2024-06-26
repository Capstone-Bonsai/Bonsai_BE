﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.CustomerGardenViewModels
{
    public class CustomerGardenModel
    {
        public string Address { get; set; }
        public float Square { get; set; }
        [NotMapped]
        public List<IFormFile>? Image { get; set; } = default!;
        [NotMapped]
        public List<String>? OldImage { get; set; } = default!;
    }
}
