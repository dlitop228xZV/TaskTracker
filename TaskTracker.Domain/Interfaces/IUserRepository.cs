namespace TaskTracker.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> ExistsAsync(int userId);
    }
}