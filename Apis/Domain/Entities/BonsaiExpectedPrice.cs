﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class BonsaiExpectedPrice : BaseEntity
    {
        public float? MaxHeight { get; set; }
        public double ExpectedPrice { get; set; }
    }
}