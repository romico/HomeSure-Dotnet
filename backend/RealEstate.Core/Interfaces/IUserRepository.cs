using RealEstate.Core.Entities;

namespace RealEstate.Core.Interfaces;

/// <summary>
/// 사용자 데이터에 접근하기 위한 리포지토리 인터페이스입니다.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
}