using System.Numerics;

namespace RealEstate.Core.Interfaces;

/// <summary>
/// 블록체인 네트워크와의 상호작용을 위한 서비스 인터페이스입니다.
/// </summary>
public interface IBlockchainService
{
    Task<decimal> GetAccountBalanceAsync(string address, CancellationToken cancellationToken = default);
    Task<string> TransferPropertyTokenAsync(string toAddress, BigInteger tokenId, CancellationToken cancellationToken = default);
    Task<string?> GetTransactionStatusAsync(string txHash, CancellationToken cancellationToken = default);
}