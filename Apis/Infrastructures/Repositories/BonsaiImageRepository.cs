using Application.Interfaces;
using Application.Repositories;
using Domain.Entities;

namespace Infrastructures.Repositories
{
    public class BonsaiImageRepository : GenericRepository<BonsaiImage>, IBonsaiImageRepository
    {
        public BonsaiImageRepository(AppDbContext context, ICurrentTime timeService, IClaimsService claimsService) : base(context, timeService, claimsService)
        {
        }
    }
}
