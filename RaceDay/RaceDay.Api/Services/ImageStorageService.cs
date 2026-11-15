using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
namespace RaceDay.Api.Services;
public class ImageStorageService : IImageStorageService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    public ImageStorageService(IConfiguration configuration, IWebHostEnvironment environment)
    { _configuration=configuration; _environment=environment; }

    public async Task<string> SaveAsync(IFormFile file, string folder, CancellationToken cancellationToken = default)
    {
        var extension=Path.GetExtension(file.FileName).ToLowerInvariant();
        var safeName=$"{Guid.NewGuid():N}{extension}";
        var connection=_configuration["AzureStorage:ConnectionString"];
        if(!string.IsNullOrWhiteSpace(connection))
        {
            var containerName=_configuration["AzureStorage:ContainerName"] ?? "raceday-images";
            var container=new BlobContainerClient(connection,containerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.Blob,cancellationToken:cancellationToken);
            var blob=container.GetBlobClient($"{folder}/{safeName}");
            await using var stream=file.OpenReadStream();
            await blob.UploadAsync(stream,new BlobHttpHeaders { ContentType=file.ContentType },cancellationToken:cancellationToken);
            return blob.Uri.ToString();
        }

        // Local fallback makes the project usable before an Azure account is configured.
        var webRoot=_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath,"wwwroot");
        var folderPath=Path.Combine(webRoot,"uploads",folder);
        Directory.CreateDirectory(folderPath);
        var fullPath=Path.Combine(folderPath,safeName);
        await using(var output=File.Create(fullPath)) await file.CopyToAsync(output,cancellationToken);
        return $"/uploads/{folder}/{safeName}";
    }
}
