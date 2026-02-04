using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(int id);
        Task<UserDto> CreateUserAsync(string name, string email);
    }
}