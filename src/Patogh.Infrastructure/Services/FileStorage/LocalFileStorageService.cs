using Patogh.Application.Interfaces;

namespace Patogh.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    public async Task<string> SaveAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken)
    {
        var uploadsFolder = Path.Combine(
            Directory.GetCurrentDirectory(), "Uploads");

        Directory.CreateDirectory(uploadsFolder);

        var uniqueName = $"{Guid.NewGuid()}_{fileName}";
        var path = Path.Combine(uploadsFolder, uniqueName);

        await using var file = new FileStream(path, FileMode.Create);
        await stream.CopyToAsync(file, cancellationToken);

        return $"/uploads/{uniqueName}";
    }
}