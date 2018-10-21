using System.Net;
using Firewall;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

public class CountryRuleTests
{
    [Fact]
    public void IsAllowed_WithBritishIPAddressAndUKCountryCode_ReturnsTrue()
    {
        var ukIPAddress = IPAddress.Parse("217.146.29.77");
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = ukIPAddress;

        var rule = new CountryRule(new DenyAllRule(), new [] { CountryCode.GB });
        var result = rule.IsAllowed(httpContext);

        Assert.True(result);
    }

    [Fact]
    public void IsAllowed_WithJapaneseIPAddressAndUKCountryCode_ReturnsFalse()
    {
        var japaneseIPAddress = IPAddress.Parse("161.202.65.101");
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = japaneseIPAddress;

        var rule = new CountryRule(new DenyAllRule(), new [] { CountryCode.GB });
        var result = rule.IsAllowed(httpContext);

        Assert.False(result);
    }
}
