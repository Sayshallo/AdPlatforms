using AdPlatforms.Services;
using System.Text;

namespace AdPlatforms.Tests;

public class PlatformServiceTests
{
    [Fact]
    public async Task LoadAndSearch_Works_AsExpected()
    {
        var svc = new PlatformService();
        var content = @"������.������:/ru
            ���������� �������:/ru/svrd/revda,/ru/svrd/pervik
            ������ ��������� ���������:/ru/msk,/ru/permobl,/ru/chelobl
            ������ �������:/ru/svrd
            ";

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var res = await svc.LoadFromStreamAsync(ms);
        Assert.Equal(4, res.PlatformsParsed);

        var p1 = svc.FindPlatformsForLocation("/ru/msk").ToList();
        Assert.Contains("������.������", p1);
        Assert.Contains("������ ��������� ���������", p1);

        var p2 = svc.FindPlatformsForLocation("/ru/svrd/revda").ToList();
        Assert.Contains("������.������", p2);
        Assert.Contains("������ �������", p2);
        Assert.Contains("���������� �������", p2);

        var p3 = svc.FindPlatformsForLocation("/ru").ToList();
        Assert.Contains("������.������", p3);
        Assert.DoesNotContain("������ �������", p3);
    }
}
