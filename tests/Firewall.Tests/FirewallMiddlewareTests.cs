using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Xunit;
using Firewall;

public class FirewallMiddlewareTests
{
    [Fact]
    public async Task Firewall_With_Single_VIP_And_Request_With_Valid_IP_Returns_Ok()
    {
        var isSuccess = false;
        var allowedIpAddress = IPAddress.Parse("12.34.56.78");
        var allowedIpAddresses = new List<IPAddress> { allowedIpAddress };

        var firewall = new FirewallMiddleware(
            async (innerCtx) =>
                {
                    isSuccess = true;
                    await innerCtx.Response.WriteAsync("Success!");
                },
            FirewallRulesEngine
                .DenyAllAccess()
                .ExceptFromIPAddresses(allowedIpAddresses),
            logger: null);

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = allowedIpAddress;

        await firewall.Invoke(httpContext);

        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
        Assert.True(isSuccess);
    }

    [Fact]
    public async Task Firewall_With_Single_VIP_And_Request_With_Invalid_IP_Returns_AccessDenied()
    {
        var isSuccess = false;
        var allowedIpAddress = IPAddress.Parse("12.34.56.78");
        var allowedIpAddresses = new List<IPAddress> { allowedIpAddress };
        var firewall = new FirewallMiddleware(
            async (innerCtx) =>
                {
                    isSuccess = true;
                    await innerCtx.Response.WriteAsync("Success!");
                },
            FirewallRulesEngine
                .DenyAllAccess()
                .ExceptFromIPAddresses(allowedIpAddresses),
            logger: null);

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("10.99.99.99");

        await firewall.Invoke(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
        Assert.False(isSuccess);
    }

    [Fact]
    public async Task Firewall_With_Single_CIDR_And_Request_With_Valid_IP_Returns_Ok()
    {
        var isSuccess = false;
        var allowedIpAddress = IPAddress.Parse("174.74.115.197");
        var cidrNotations = new List<CIDRNotation> { CIDRNotation.Parse("174.74.115.192/28") };
        var firewall = new FirewallMiddleware(
            async (innerCtx) =>
                {
                    isSuccess = true;
                    await innerCtx.Response.WriteAsync("Success!");
                },
            FirewallRulesEngine
                .DenyAllAccess()
                .ExceptFromIPAddressRanges(cidrNotations),
            logger: null);

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = allowedIpAddress;

        await firewall.Invoke(httpContext);

        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
        Assert.True(isSuccess);
    }

    [Fact]
    public async Task Firewall_With_Single_CIDR_And_Request_With_Invalid_IP_Returns_AccessDenied()
    {
        var isSuccess = false;
        var allowedIpAddress = IPAddress.Parse("174.74.115.210");
        var cidrNotations = new List<CIDRNotation> { CIDRNotation.Parse("174.74.115.192/28") };
        var firewall = new FirewallMiddleware(
            async (innerCtx) =>
                {
                    isSuccess = true;
                    await innerCtx.Response.WriteAsync("Success!");
                },
            FirewallRulesEngine
                .DenyAllAccess()
                .ExceptFromIPAddressRanges(cidrNotations),
            logger: null);

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = allowedIpAddress;

        await firewall.Invoke(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
        Assert.False(isSuccess);
    }

    [Theory]
    [InlineData("1.2.3.4")]
    [InlineData("10.20.30.40")]
    [InlineData("50.6.70.8")]
    [InlineData("2803:f800:1:2:3:4:5:6")]
    [InlineData("2c0f:f248:ffff:234:ffff:ffff:ffff:ffff")]
    public async Task Firewall_With_Multiple_IPs_And_CIDRs_And_Request_With_Valid_IP_Returns_Ok(string ip)
    {
        var isSuccess = false;
        var allowedIpAddress = IPAddress.Parse(ip);
        var allowedIpAddresses = new List<IPAddress>
            {
                IPAddress.Parse("1.2.3.4"),
                IPAddress.Parse("10.20.30.40"),
                IPAddress.Parse("50.6.70.8")
            };
        var cidrNotations = new List<CIDRNotation>
            {
                CIDRNotation.Parse("2803:f800::/32"),
                CIDRNotation.Parse("2c0f:f248::/32"),
                CIDRNotation.Parse("103.21.244.0/22"),
                CIDRNotation.Parse("141.101.64.0/18"),
                CIDRNotation.Parse("172.64.0.0/13")
            };
        var firewall = new FirewallMiddleware(
            async (innerCtx) =>
                {
                    isSuccess = true;
                    await innerCtx.Response.WriteAsync("Success!");
                },
            FirewallRulesEngine
                .DenyAllAccess()
                .ExceptFromIPAddresses(allowedIpAddresses)
                .ExceptFromIPAddressRanges(cidrNotations),
            logger: null);

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = allowedIpAddress;

        await firewall.Invoke(httpContext);

        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
        Assert.True(isSuccess);
    }

    [Theory]
    [InlineData("1.2.3.3")]
    [InlineData("10.20.33.40")]
    [InlineData("50.6.70.9")]
    [InlineData("2803:f801:1:2:3:4:5:6")]
    [InlineData("2c0f:f247:fff1:234:ffff:ffff:ffff:ffff")]
    public async Task Firewall_With_Multiple_IPs_And_CIDRs_And_Request_With_Invalid_IP_Returns_AccessDenied(string ip)
    {
        var isSuccess = false;
        var allowedIpAddress = IPAddress.Parse(ip);
        var allowedIpAddresses = new List<IPAddress>
            {
                IPAddress.Parse("1.2.3.4"),
                IPAddress.Parse("10.20.30.40"),
                IPAddress.Parse("50.6.70.8")
            };
        var cidrNotations = new List<CIDRNotation>
            {
                CIDRNotation.Parse("2803:f800::/32"),
                CIDRNotation.Parse("2c0f:f248::/32"),
                CIDRNotation.Parse("103.21.244.0/22"),
                CIDRNotation.Parse("141.101.64.0/18"),
                CIDRNotation.Parse("172.64.0.0/13")
            };
        var firewall = new FirewallMiddleware(
            async (innerCtx) =>
                {
                    isSuccess = true;
                    await innerCtx.Response.WriteAsync("Success!");
                },
            FirewallRulesEngine
                .DenyAllAccess()
                .ExceptFromIPAddresses(allowedIpAddresses)
                .ExceptFromIPAddressRanges(cidrNotations),
            logger: null);

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = allowedIpAddress;

        await firewall.Invoke(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
        Assert.False(isSuccess);
    }
}