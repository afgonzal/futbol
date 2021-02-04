using System.Collections.Generic;
using System.Threading.Tasks;

namespace Futbol.Seasons.DataRepository
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity> GetByKeyAsync(object key);
        Task<List<TEntity>> GetAllAsync();
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task<List<TEntity>> QueryById(object id);
        Task<TEntity> GetByKeyAsync(object hashKey, object sortKey);

        Task DeleteAsync(object hashKey, object sortKey);
        Task DeleteAsync(object hashKey);

        Task<List<TEntity>> QueryBetweenKeysAsync(object hashKey, object sortKeyFrom, object sortKeyTo);

        Task BatchUpsertAsync(IEnumerable<TEntity> entities);

        Task BatchDeleteAsync(IEnumerable<TEntity> entities);
    }
}