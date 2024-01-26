﻿namespace Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }
        public string Image { get; set; }

        public IList<SubCategory> SubCategories { get; set; }
    }
}
