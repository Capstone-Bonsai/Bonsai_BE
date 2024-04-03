using Application.Interfaces;
using Application.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructures.Repositories
{
    public class CategoryExpectedPriceRepository : GenericRepository<CategoryExpectedPrice>, ICategoryExpectedPriceRepository
    {
        protected DbSet<CategoryExpectedPrice> _dbSet;
        public CategoryExpectedPriceRepository(AppDbContext context, ICurrentTime timeService, IClaimsService claimsService) : base(context, timeService, claimsService)
        {

            _dbSet = context.Set<CategoryExpectedPrice>();
        }
        public double GetExpectedPrice(float height)
        {
            var price =  _dbSet.OrderBy(c => c.MinHeight).FirstOrDefault(c => c.MinHeight <= height);
            if (price == null)
            {
                return 0;
            }
            return price.ExpectedPrice;
        }
    }
}
