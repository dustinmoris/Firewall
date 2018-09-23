using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Firewall;
using Microsoft.AspNetCore.Http;
using Xunit;

public class FirewallMiddlewareTests
{
    [Fact]
    public async Task Firewall_With_Single_VIP_And_Request_With_Valid_IP_Returns_Success()
    {
        var isSuccess = false;
        var allowedIpAddress = IPAddress.Parse("12.34.56.78");
        var vipList = new List<IPAddress> { allowedIpAddress };
        var firewall = new FirewallMiddleware(
            async (innerCtx) =>
                {
                    isSuccess = true;
                    await innerCtx.Response.WriteAsync("Success!");
                },
            allowLocalRequests: false,
            vipList: vipList,
            guestList: null,
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
        var vipList = new List<IPAddress> { allowedIpAddress };
        var firewall = new FirewallMiddleware(
            async (innerCtx) =>
                {
                    isSuccess = true;
                    await innerCtx.Response.WriteAsync("Success!");
                },
            allowLocalRequests: false,
            vipList: vipList,
            guestList: null,
            logger: null);

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("10.99.99.99");

        await firewall.Invoke(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
        Assert.False(isSuccess);
    }
}

public class CIDRNotationTests
{
    [Fact]
    public void Parse_WithNull_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => CIDRNotation.Parse(null));
    }

    [Fact]
    public void Parse_WithEmptyString_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => CIDRNotation.Parse(""));
    }

    [Fact]
    public void Parse_WithWhitespaceString_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => CIDRNotation.Parse(" "));
    }

    [Theory]
    [InlineData("test")]
    [InlineData("/")]
    [InlineData("10.10.10.10.10/10")]
    [InlineData("10.10.10.10//10")]
    [InlineData("10.10.10.10//100")]
    public void Parse_WithInvalidString_ThrowsException(string value)
    {
        Assert.Throws<ArgumentException>(() => CIDRNotation.Parse(value));
    }

    [Theory]
    [InlineData("10.10.10.10/8", "10.10.10.10", 8)]
    [InlineData("100.20.30.255/0", "100.20.30.255", 0)]
    [InlineData("57.99.180.255/32", "57.99.180.255", 32)]
    [InlineData("2002::1234:abcd:ffff:c0a8:101/64", "2002::1234:abcd:ffff:c0a8:101", 64)]
    public void Parse_WithValidString_ParsesCorrectObject(
        string value, string expectedIpAddress, int expectedMaskBits)
    {
        var cidrNotation = CIDRNotation.Parse(value);

        Assert.Equal(expectedIpAddress, cidrNotation.Address.ToString());
        Assert.Equal(expectedMaskBits, cidrNotation.MaskBits);
        Assert.Equal(value, cidrNotation.ToString());
    }

    [Theory]
    [InlineData("174.74.115.192/28", "174.74.115.193")]
    [InlineData("174.74.115.192/28", "174.74.115.197")]
    [InlineData("174.74.115.192/28", "174.74.115.200")]
    [InlineData("174.74.115.192/28", "174.74.115.202")]
    [InlineData("174.74.115.192/28", "0:0:0:0:0:ffff:ae4a:73ca")]
    [InlineData("139.114.208.0/20", "139.114.208.1")]
    [InlineData("139.114.208.0/20", "139.114.223.254")]
    [InlineData("2002::1234:abcd:ffff:c0a8:101/64", "2002:0000:0000:1234:0000:0000:0000:0000")]
    [InlineData("2002::1234:abcd:ffff:c0a8:101/64", "2002:0000:0000:1234:ffff:ffff:ffff:ffff")]
    public void Contains_WithValidIp_ReturnsTrue(string cidr, string ip)
    {
        var cidrNotation = CIDRNotation.Parse(cidr);
        var ipAddress = IPAddress.Parse(ip);

        Assert.True(cidrNotation.Contains(ipAddress));
    }

    [Theory]
    [InlineData("174.74.115.192/28", "174.74.115.191")]
    [InlineData("174.74.115.192/28", "174.74.115.208")]
    [InlineData("139.114.208.0/20", "139.114.207.255")]
    [InlineData("139.114.208.0/20", "139.114.224.0")]
    [InlineData("2002::1234:abcd:ffff:c0a8:101/64", "2002:0000:0000:1233:0000:0000:0000:0000")]
    [InlineData("2002::1234:abcd:ffff:c0a8:101/64", "2002:0000:0000:1235:0000:0000:0000:0000")]
    public void Contains_WithOutOfRangeIp_ReturnsFalse(string cidr, string ip)
    {
        var cidrNotation = CIDRNotation.Parse(cidr);
        var ipAddress = IPAddress.Parse(ip);

        Assert.False(cidrNotation.Contains(ipAddress));
    }
}