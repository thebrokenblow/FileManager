using FileManager.Domain.Entities;

namespace FileManager.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task<int> AddAsync(User user);
}