using Microsoft.EntityFrameworkCore;
using RealEstate.Core.Entities;
using RealEstate.Core.Interfaces;

namespace RealEstate.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core를 사용하여 거래 데이터를 관리하는 리포지토리 구현체입니다.
/// </summary>
public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // 데이터를 수정하지 않는 조회 작업이므로 AsNoTracking()을 사용하여 성능을 최적화합니다.
        return await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Property)
            .Include(t => t.Buyer)
            .Include(t => t.Seller)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<List<Transaction>> ListByPropertyIdAsync(int propertyId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.PropertyId == propertyId)
            .Include(t => t.Buyer)
            .Include(t => t.Seller)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Transaction>> ListByUserIdAsync(int userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.BuyerId == userId || t.SellerId == userId)
            .Include(t => t.Property)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Transaction> AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
        return transaction;
    }

    public void Update(Transaction transaction)
    {
        _context.Entry(transaction).State = EntityState.Modified;
    }
}