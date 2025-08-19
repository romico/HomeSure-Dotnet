using RealEstate.Core.Entities;

namespace RealEstate.Core.Interfaces;

/// <summary>
/// 거래 데이터에 접근하기 위한 리포지토리 인터페이스입니다.
/// </summary>
public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Transaction>> ListByPropertyIdAsync(int propertyId, CancellationToken cancellationToken = default);
    Task<List<Transaction>> ListByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<Transaction> AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
    void Update(Transaction transaction);
}