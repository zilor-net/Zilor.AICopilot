using Microsoft.AspNetCore.Hosting;
using Zilor.AICopilot.Services.Common.Contracts;

namespace Zilor.AICopilot.Infrastructure.Storage;

public class LocalFileStorageService(IWebHostEnvironment environment) : IFileStorageService
{
    private const string UploadRoot = "uploads";

    public async Task<string> SaveAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        // 1. 构建存储路径：uploads/2025/12/01/guid_filename.pdf
        var datePath = DateTime.Now.ToString("yyyy/MM/dd");
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var relativePath = Path.Combine(UploadRoot, datePath);
        
        // 获取程序根目录
        var rootPath = environment.ContentRootPath;
        var fullDirectory = Path.Combine(rootPath, relativePath);

        if (!Directory.Exists(fullDirectory))
        {
            Directory.CreateDirectory(fullDirectory);
        }

        var fullPath = Path.Combine(fullDirectory, uniqueFileName);

        // 2. 写入文件
        await using var fileStream = new FileStream(fullPath, FileMode.Create);
        if (stream.CanSeek) stream.Position = 0;
        await stream.CopyToAsync(fileStream, cancellationToken);

        // 3. 返回相对路径（统一使用正斜杠，方便跨平台和URL访问）
        return Path.Combine(relativePath, uniqueFileName).Replace("\\", "/");
    }

    public Task<Stream?> GetAsync(string path, CancellationToken cancellationToken = default)
    {
        var rootPath = environment.WebRootPath;
        var fullPath = Path.Combine(rootPath, path);

        if (!File.Exists(fullPath)) return Task.FromResult<Stream?>(null);

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var rootPath = environment.WebRootPath;
        var fullPath = Path.Combine(rootPath, path);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}