using Microsoft.EntityFrameworkCore;
using RealEstate.Core.Entities;
using RealEstate.Core.Interfaces;

namespace RealEstate.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core를 사용하여 부동산 매물 데이터를 관리하는 리포지토리 구현체입니다.
/// </summary>
public class PropertyRepository : IPropertyRepository
{
    private readonly ApplicationDbContext _context;

    public PropertyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Property?> GetByIdAsync(int id, bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Properties.AsQueryable();

        if (includeDetails)
        {
            query = query.Include(p => p.Owner).Include(p => p.Images);
        }

        // 데이터를 수정하지 않는 조회 작업이므로 AsNoTracking()을 사용하여 성능을 최적화합니다.
        return await query.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<List<Property>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Properties.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<List<Property>> ListByStatusAsync(string status,
        CancellationToken cancellationToken = default)
    {
        return await _context.Properties.AsNoTracking().Where(p => p.Status == status).ToListAsync(cancellationToken);
    }

    public async Task<Property> AddAsync(Property property, CancellationToken cancellationToken = default)
    {
        await _context.Properties.AddAsync(property, cancellationToken);
        return property;
    }

    public void Update(Property property)
    {
        // DbContext가 엔티티의 상태를 Modified로 추적하도록 설정합니다. 실제 DB 저장은 SaveChangesAsync에서 일어납니다.
        _context.Entry(property).State = EntityState.Modified;
    }

    public void Delete(Property property)
    {
        _context.Properties.Remove(property);
    }
}