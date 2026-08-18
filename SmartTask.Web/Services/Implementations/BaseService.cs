using System.Linq.Expressions;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class BaseService<T> : IBaseService<T> where T : class
    {
        protected readonly IGenericRepository<T> _repository;
        protected readonly IUnitOfWork _unitOfWork;

        public BaseService(
            IGenericRepository<T> repository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
            => await _repository.GetAllAsync();

        public virtual async Task<T?> GetByIdAsync(int id)
            => id <= 0 ? null : await _repository.GetByIdAsync(id);

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => predicate == null ? Enumerable.Empty<T>() : await _repository.FindAsync(predicate);

        public virtual async Task AddAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            await _repository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _repository.Update(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(int id)
        {
            if (id <= 0)
                return;

            var entity = await _repository.GetByIdAsync(id);
            if (entity != null)
            {
                _repository.Delete(entity);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}
