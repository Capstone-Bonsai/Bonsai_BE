﻿using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Application.ViewModels.CustomerBonsaiViewModels
{
    public class BonsaiModelForCustomer
    {
        public Guid CategoryId { get; set; }
        public Guid StyleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? YearOfPlanting { get; set; }
        public int TrunkDimenter { get; set; }
        public float? Height { get; set; }
        public int NumberOfTrunk { get; set; }
        [NotMapped]
        public List<IFormFile>? Image { get; set; } = default!;
    }
}
