using Application.Interfaces;
using Application.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructures.Repositories
{
    public class BonsaiExpectedPriceRepository : GenericRepository<BonsaiExpectedPrice>, IBonsaiExpectedPriceRepository
    {
        protected DbSet<BonsaiExpectedPrice> _dbSet;
        public BonsaiExpectedPriceRepository(AppDbContext context, ICurrentTime timeService, IClaimsService claimsService) : base(context, timeService, claimsService)
        {

            _dbSet = context.Set<BonsaiExpectedPrice>();
        }
        public double GetExpectedPrice(float height)
        {
            var list = _dbSet.OrderBy(c => c.MaxHeight). ToList();
            if (list ==null || list.Count == 0)
                throw new Exception("Chưa có giá chăm sóc cây.");
            var price = list.FirstOrDefault(c => c.MaxHeight >= height);
            if (price == null)
            {
                price = _dbSet.FirstOrDefault(c=>c.MaxHeight ==null);
                if (price == null)
                    throw new Exception("Đã xảy ra lỗi trong quá trình lấy giá chăm sóc cây.");
            }
            return price.ExpectedPrice;
        }
    }
}
