using System;
using System.Net;
using Firewall;
using Xunit;

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