namespace AdPlatforms.Models;

public class PlatformInfo
{
    public string Name { get; set; } = string.Empty;
    public List<string> Locations { get; set; } = new();
}
