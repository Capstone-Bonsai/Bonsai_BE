using Application.Commons;
using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Repositories
{
    public interface IGenericRepository<TEntity> where TEntity : BaseEntity
    {
        Task<List<TEntity>> GetAllAsync();
        Task<TEntity?> GetByIdAsync(Guid id);
        Task AddAsync(TEntity entity);
        void Update(TEntity entity);
        void UpdateRange(List<TEntity> entities);
        void SoftRemove(TEntity entity);
        Task AddRangeAsync(List<TEntity> entities);
        void SoftRemoveRange(List<TEntity> entities);
        IQueryable<TEntity> GetAllQueryable();

        Task<Pagination<TEntity>> GetAsync(Expression<Func<TEntity, bool>> expression = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, bool isDisableTracking = true, bool isTakeAll = false, int pageSize = 0, int pageIndex = 0, Expression<Func<TEntity, object>> expressionInclude = null);
    }
}
