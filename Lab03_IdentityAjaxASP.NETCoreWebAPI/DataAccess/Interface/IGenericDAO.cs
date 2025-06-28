using System.Linq.Expressions;
using DataAccess.Paginated;

namespace DataAccess.Interface
{
    public interface IGenericDAO<T> where T : class
    {
        // Queryable property
        IQueryable<T> Entities { get; }

        // Synchronous operations
        T? GetById(object id);
        void Insert(T entity);
        void InsertRange(List<T> entities);
        void Update(T entity);
        void Delete(object entity);
        void Save();

        // Asynchronous operations
        Task<T?> GetByIdAsync(object id);
        Task<IEnumerable<T>> GetAllAsync();
        Task InsertAsync(T entity);
        Task InsertRangeAsync(List<T> entities);
        Task UpdateAsync(T entity);
        Task DeleteAsync(object entity);
        Task SaveAsync();
        Task<PaginatedList<T>> GetPagging(IQueryable<T> query, int index, int pageSize);

        // Query operations
        T? Find(Expression<Func<T, bool>> predicate);
    }
}
