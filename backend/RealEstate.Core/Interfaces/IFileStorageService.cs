namespace RealEstate.Core.Interfaces;

/// <summary>
/// 파일 저장소와의 상호작용을 위한 서비스 인터페이스입니다.
/// </summary>
public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);
}