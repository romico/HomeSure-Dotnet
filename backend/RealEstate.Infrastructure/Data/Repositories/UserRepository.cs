using Microsoft.EntityFrameworkCore;
using RealEstate.Core.Entities;
using RealEstate.Core.Interfaces;

namespace RealEstate.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core를 사용하여 사용자 데이터를 관리하는 리포지토리 구현체입니다.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // 데이터를 수정하지 않는 조회 작업이므로 AsNoTracking()을 사용하여 성능을 최적화합니다.
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        // SaveChangesAsync는 Unit of Work 패턴에 따라 서비스 계층에서 호출하는 것이 일반적입니다.
        // 여기서는 단순화를 위해 리포지토리에서 바로 호출하지 않습니다.
        // await _context.SaveChangesAsync(cancellationToken);
        return user;
    }
}