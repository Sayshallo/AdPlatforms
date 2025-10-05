using AdPlatforms.Services;
using System.Text;

namespace AdPlatforms.Tests;

public class PlatformServiceTests
{
    [Fact]
    public async Task LoadAndSearch_Works_AsExpected()
    {
        var svc = new PlatformService();
        var content = @"Яндекс.Директ:/ru
            Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik
            Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl
            Крутая реклама:/ru/svrd
            ";

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var res = await svc.LoadFromStreamAsync(ms);
        Assert.Equal(4, res.PlatformsParsed);

        var p1 = svc.FindPlatformsForLocation("/ru/msk").ToList();
        Assert.Contains("Яндекс.Директ", p1);
        Assert.Contains("Газета уральских москвичей", p1);

        var p2 = svc.FindPlatformsForLocation("/ru/svrd/revda").ToList();
        Assert.Contains("Яндекс.Директ", p2);
        Assert.Contains("Крутая реклама", p2);
        Assert.Contains("Ревдинский рабочий", p2);

        var p3 = svc.FindPlatformsForLocation("/ru").ToList();
        Assert.Contains("Яндекс.Директ", p3);
        Assert.DoesNotContain("Крутая реклама", p3);
    }
}
