namespace RaceDay.Api.Services;
public interface IImageStorageService
{
    Task<string> SaveAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
}
