// TaskTracker.Domain/Interfaces/IRepository.cs

using System.Linq.Expressions;

namespace TaskTracker.Domain.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<List<T>> GetAllAsync(); // Изменили возвращаемый тип
        IQueryable<T> GetAll(); // Добавили этот метод
        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<bool> DeleteAsync(int id);
    }
}