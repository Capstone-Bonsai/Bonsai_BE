﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.BonsaiViewModel
{
    public class FilterBonsaiModel
    {
        public string? Keyword { get; set; }
        public string? Category { get; set; }
        public string? Style { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
    }
}
