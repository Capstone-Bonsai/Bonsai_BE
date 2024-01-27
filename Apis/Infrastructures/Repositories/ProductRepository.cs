using Application.Interfaces;
using Application.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructures.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        protected DbSet<Product> _dbSet;
        public ProductRepository(AppDbContext context, ICurrentTime timeService, IClaimsService claimsService) : base(context, timeService, claimsService)
        {
            _dbSet = context.Set<Product>();
        }
        public async Task<List<String?>> GetTreeShapeList()=> await _dbSet.Select(x => x.TreeShape).Distinct().ToListAsync();
    }
}
