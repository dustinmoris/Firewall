using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Firewall;
using Xunit;

public class CloudflareHelperTests
{
    [Fact]
    public async Task GetIPAddressRangesAsync_ReturnsOnlyGuestList()
    {
        var helper = new CloudflareHelper(new HttpClient());

        var (vips, guests) = await helper.GetIPAddressRangesAsync();

        Assert.Empty(vips);
        Assert.NotEmpty(guests);

        var cidrNotations = guests.Select(g => g.ToString());

        Assert.Contains("2400:cb00::/32", cidrNotations);
        Assert.Contains("2405:b500::/32", cidrNotations);
        Assert.Contains("2606:4700::/32", cidrNotations);
        Assert.Contains("2803:f800::/32", cidrNotations);
        Assert.Contains("2c0f:f248::/32", cidrNotations);
        Assert.Contains("2a06:98c0::/29", cidrNotations);

        Assert.Contains("103.21.244.0/22", cidrNotations);
        Assert.Contains("141.101.64.0/18", cidrNotations);
        Assert.Contains("172.64.0.0/13", cidrNotations);
    }
}
