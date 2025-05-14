using Valuator.Models;

namespace Valuator.Services;

public interface IUserStorageService
{
    Task<User?> FindByUserNameAsync(string userName);
    Task<User> CreateAsync(string userName, string password);
}