﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class CategoryExpectedPrice : BaseEntity
    {
        [ForeignKey("Category")]
        public Guid CategoryId { get; set; }
        public float MinHeight { get; set; }
        public double ExpectedPrice { get; set; }
        public virtual Category Category { get; set; }
    }
}