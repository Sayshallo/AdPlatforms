namespace AdPlatforms.Models;

public class LoadResult
{
    public int PlatformsParsed { get; set; } = 0;
    public int UniqueLocations { get; set; } = 0;
    public List<string> Errors { get; set; } = new();
}
