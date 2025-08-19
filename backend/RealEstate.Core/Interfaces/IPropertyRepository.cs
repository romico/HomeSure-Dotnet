using RealEstate.Core.Entities;

namespace RealEstate.Core.Interfaces;

/// <summary>
/// 부동산 매물 데이터에 접근하기 위한 리포지토리 인터페이스입니다.
/// </summary>
public interface IPropertyRepository
{
    Task<Property?> GetByIdAsync(int id, bool includeDetails = false,
        CancellationToken cancellationToken = default);
    Task<List<Property?>> ListAllAsync(CancellationToken cancellationToken = default);
    Task<List<Property>> ListByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<Property> AddAsync(Property property, CancellationToken cancellationToken = default);
    void Update(Property property);
    void Delete(Property property);
}