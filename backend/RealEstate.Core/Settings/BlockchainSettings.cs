namespace RealEstate.Core.Settings;

/// <summary>
/// appsettings.json의 블록체인 설정값을 담는 클래스입니다.
/// </summary>
public class BlockchainSettings
{
    public string NetworkUrl { get; set; } = string.Empty;
    public long? NetworkId { get; set; }
    public string ContractAddress { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty; // 주의: 개발용으로만 사용해야 합니다.
}