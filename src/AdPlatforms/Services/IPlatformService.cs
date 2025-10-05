using AdPlatforms.Models;

namespace AdPlatforms.Services;

public interface IPlatformService
{
    Task<LoadResult> LoadFromStreamAsync(Stream stream);
    IEnumerable<string> FindPlatformsForLocation(string location);
    IEnumerable<PlatformInfo> GetAllPlatforms();
}
