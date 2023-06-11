using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Xunit;
using Firewall;
using Microsoft.Extensions.Primitives;

public class FirewallMiddlewareTests
{
    [Theory]
    [InlineData("12.34.56.78", "127.0.0.1", "12.34.56.78", false, true)]
    [InlineData("127.0.0.1", "12.34.56.78", "12.34.56.78", true, true)]
    [InlineData("10.99.99.99", "127.0.0.1", "12.34.56.78", false, false)]
    [InlineData("127.0.0.1", "10.99.99.99", "12.34.56.78", true, false)]
	public async Task Firewall_With_Single_VIP(
	    string remoteAddress, string xForwardedFor, string allowedAddress, bool proxyAware, bool success)
    {
        var isSuccess = false;
        var allowedIpAddress = IPAddress.Parse(allowedAddress);
        var allowedIpAddresses = new List<IPAddress> { allowedIpAddress };

        var firewall = new FirewallMiddleware(
            async (innerCtx) =>
                {
                    isSuccess = true;
                    await innerCtx.Response.WriteAsync("Success!");
                },
            FirewallRulesEngine
                .DenyAllAccess()
                .ExceptFromIPAddresses(allowedIpAddresses, proxyAware),
            logger: null);

        var httpContext = new DefaultHttpContext
        {
	        Request =
	        {
		        Headers = { new KeyValuePair<string, StringValues>("x-forwarded-for", xForwardedFor) }
	        },
	        Connection =
	        {
		        RemoteIpAddress = IPAddress.Parse(remoteAddress)
			}
        };

		await firewall.Invoke(httpContext);

		Assert.Equal(success ? StatusCodes.Status200OK : StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
		Assert.Equal(success, isSuccess);
	}

	[Theory]
	[InlineData("174.74.115.197", "192.168.1.1", false, true)]
	[InlineData("192.168.1.1", "174.74.115.197", true, true)]
	[InlineData("174.74.115.210", "192.168.1.1", false, false)]
	[InlineData("192.168.1.1", "174.74.115.210", true, false)]
	public async Task Firewall_With_Single_CIDR(
		string remoteAddress, string xForwardedFor, bool proxyAware, bool success)
    {
        var isSuccess = false;
        var cidrNotations = new List<CIDRNotation> { CIDRNotation.Parse("174.74.115.192/28") };
        var firewall = new FirewallMiddleware(
            async (innerCtx) =>
                {
                    isSuccess = true;
                    await innerCtx.Response.WriteAsync("Success!");
                },
            FirewallRulesEngine
                .DenyAllAccess()
                .ExceptFromIPAddressRanges(cidrNotations, proxyAware),
            logger: null);

        var httpContext = new DefaultHttpContext
        {
	        Request =
	        {
		        Headers = { new KeyValuePair<string, StringValues>("x-forwarded-for", xForwardedFor) }
	        },
	        Connection =
	        {
		        RemoteIpAddress = IPAddress.Parse(remoteAddress)
			}
        };

		await firewall.Invoke(httpContext);

		Assert.Equal(success ? StatusCodes.Status200OK : StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
        Assert.Equal(success, isSuccess);
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