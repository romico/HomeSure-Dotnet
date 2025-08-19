using WorkerService1.Domain;

namespace WorkerService1.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken);
}