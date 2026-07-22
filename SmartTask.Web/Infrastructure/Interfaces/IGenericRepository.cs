using System.Linq.Expressions;

namespace SmartTask.Web.Infrastructure.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        // Query
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        // Commands
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);

    }
}