using System.Numerics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Web3;
using RealEstate.Core.Interfaces;
using RealEstate.Core.Settings;
using Account = Nethereum.Web3.Accounts.Account;

namespace RealEstate.Infrastructure.Services;

/// <summary>
/// Nethereum을 사용하여 블록체인과 상호작용하는 서비스 구현체입니다.
/// </summary>
public class BlockchainService : IBlockchainService
{
    private readonly BlockchainSettings _settings;
    private readonly Web3 _web3;
    private readonly Account? _account;
    private readonly ILogger<BlockchainService> _logger;

    public BlockchainService(IOptions<BlockchainSettings> settings, ILogger<BlockchainService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        if (string.IsNullOrEmpty(_settings.PrivateKey))
        {
            _logger.LogWarning("블록체인 개인 키가 설정되지 않았습니다. 읽기 전용 작업만 가능합니다.");
            _account = null;
            _web3 = new Web3(_settings.NetworkUrl);
        }
        else
        {
            _account = new Account(_settings.PrivateKey, _settings.NetworkId);
            _web3 = new Web3(_account, _settings.NetworkUrl);
        }
    }

    public async Task<decimal> GetAccountBalanceAsync(string address, CancellationToken cancellationToken = default)
    {
        var balance = await _web3.Eth.GetBalance.SendRequestAsync(address);
        return Web3.Convert.FromWei(balance.Value);
    }

    public Task<string> TransferPropertyTokenAsync(string toAddress, BigInteger tokenId, CancellationToken cancellationToken = default)
    {
        if (_account is null)
        {
            throw new InvalidOperationException("블록체인 쓰기 작업을 위한 개인 키가 설정되지 않았습니다.");
        }

        // TODO: 실제 ERC721 컨트랙트의 ABI와 함수 이름을 사용하여 구현해야 합니다.
        // var contract = _web3.Eth.GetContract(abi, _settings.ContractAddress);
        // var transferFunction = contract.GetFunction("safeTransferFrom");
        // return await transferFunction.SendTransactionAsync(_account.Address, null, null, toAddress, tokenId);
        
        _logger.LogInformation("블록체인 트랜잭션 시뮬레이션: {To}, TokenId: {TokenId}", toAddress, tokenId);
        return Task.FromResult($"0x_fake_tx_hash_{Guid.NewGuid()}");
    }

    public async Task<string?> GetTransactionStatusAsync(string txHash, CancellationToken cancellationToken = default)
    {
        var receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);
        // 실제 블록체인에서는 영수증이 즉시 생성되지 않을 수 있으므로 폴링 로직이 필요할 수 있습니다.
        return receipt?.Status?.Value == 1 ? "Success" : "Failed";
    }
}