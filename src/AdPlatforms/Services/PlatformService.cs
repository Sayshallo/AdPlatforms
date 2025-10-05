using AdPlatforms.Models;
using System.Text;

namespace AdPlatforms.Services;

public class PlatformService : IPlatformService
{
    // хранение: location -> set(platformName)
    private Dictionary<string, HashSet<string>> _locationToPlatforms = new(StringComparer.OrdinalIgnoreCase);
    // platform -> set(locations)
    private Dictionary<string, HashSet<string>> _platformToLocations = new(StringComparer.OrdinalIgnoreCase);

    public async Task<LoadResult> LoadFromStreamAsync(Stream stream)
    {
        var result = new LoadResult();
        var newLocationToPlatforms = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var newPlatformToLocations = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        using var reader = new StreamReader(stream, Encoding.UTF8);
        string? line;
        int lineNumber = 0;
        while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
        {
            lineNumber++;
            line = line.Trim();
            if (string.IsNullOrEmpty(line)) continue;
            if (line.StartsWith("#")) continue; // allow comments

            // проверка на наличие доветочи€ в строке
            var colonIndex = line.IndexOf(':');
            if (colonIndex <= 0)
            {
                result.Errors.Add($"Line {lineNumber}: missing ':'");
                continue;
            }

            // проверка на наличие платформы (с начала строки до двоеточи€)
            var platformName = line.Substring(0, colonIndex).Trim();
            if (string.IsNullOrEmpty(platformName))
            {
                result.Errors.Add($"Line {lineNumber}: empty platform name");
                continue;
            }

            // делим "локации" по зап€тым через LINQ
            var locationsPart = line.Substring(colonIndex + 1);
            var rawLocs = locationsPart.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => NormalizeLocation(s))
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (rawLocs.Count == 0)
            {
                result.Errors.Add($"Line {lineNumber}: no valid locations for '{platformName}'");
                continue;
            }

            if (!newPlatformToLocations.TryGetValue(platformName, out var platSet))
            {
                platSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                newPlatformToLocations[platformName] = platSet;
            }

            foreach (var loc in rawLocs)
            {
                platSet.Add(loc);
                if (!newLocationToPlatforms.TryGetValue(loc, out var set))
                {
                    set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    newLocationToPlatforms[loc] = set;
                }
                set.Add(platformName);
            }

            result.PlatformsParsed++;
        }

        // заменить снепшот
        Interlocked.Exchange(ref _locationToPlatforms, newLocationToPlatforms);
        Interlocked.Exchange(ref _platformToLocations, newPlatformToLocations);

        result.UniqueLocations = _locationToPlatforms.Count;
        return result;
    }

    // ѕоиск: перебираем префиксы запроса (от самого специфичного к менее специфичному)
    public IEnumerable<string> FindPlatformsForLocation(string location)
    {
        if (string.IsNullOrWhiteSpace(location)) return Enumerable.Empty<string>();
        var normalized = NormalizeLocation(location);
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var cur = normalized;

        while (true)
        {
            if (_locationToPlatforms != null && _locationToPlatforms.TryGetValue(cur, out var set))
            {
                foreach (var p in set) result.Add(p);
            }

            var idx = cur.LastIndexOf('/');
            if (idx <= 0) break; // idx==0 значит cur = "/ru" -> stop after checking it
            cur = cur.Substring(0, idx);
        }

        return result.OrderBy(x => x).ToList();
    }

    public IEnumerable<PlatformInfo> GetAllPlatforms()
    {
        if (_platformToLocations == null) return Enumerable.Empty<PlatformInfo>();
        return _platformToLocations
            .Select(kvp => new PlatformInfo { Name = kvp.Key, Locations = kvp.Value.OrderBy(x => x).ToList() })
            .OrderBy(p => p.Name)
            .ToList();
    }

    private static string NormalizeLocation(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        s = s.Trim();
        if (s.Length > 1 && s.EndsWith("/")) s = s.TrimEnd('/');
        return s.ToLowerInvariant();
    }
}
