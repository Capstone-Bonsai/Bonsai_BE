using Application.Interfaces;
using Application.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructures.Repositories
{
    public class BonsaiRepository : GenericRepository<Bonsai>, IBonsaiRepository
    {
        protected DbSet<Bonsai> _dbSet;
        public BonsaiRepository(AppDbContext context, ICurrentTime timeService, IClaimsService claimsService) : base(context, timeService, claimsService)
        {
            _dbSet = context.Set<Bonsai>();
        }
    }
}
