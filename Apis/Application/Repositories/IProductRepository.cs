using Domain.Entities;

namespace Application.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<List<String?>> GetTreeShapeList();
    }
}
