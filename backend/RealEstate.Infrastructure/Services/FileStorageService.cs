using Microsoft.Extensions.Logging;
using RealEstate.Core.Interfaces;

namespace RealEstate.Infrastructure.Services;

/// <summary>
/// 파일 저장소 서비스의 스텁(Stub) 구현체입니다. 실제 파일을 저장하는 대신 로그를 기록합니다.
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(ILogger<FileStorageService> logger)
    {
        _logger = logger;
    }

    public Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var fakeUrl = $"https://fake-storage.com/uploads/{Guid.NewGuid()}-{fileName}";
        _logger.LogInformation("파일 업로드 시뮬레이션: {FileName} ({ContentType}) -> {Url}", fileName, contentType, fakeUrl);
        return Task.FromResult(fakeUrl);
    }

    public Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("파일 삭제 시뮬레이션: {Url}", fileUrl);
        return Task.CompletedTask;
    }
}