using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Net;
using Firewall;
using Microsoft.Extensions.Primitives;
using Xunit;

public class IPAddressExtensionsTests
{
	[Fact]
	public void Return_LocalHost()
	{
		var httpContext = new DefaultHttpContext
		{
			Connection =
			{
				RemoteIpAddress = IPAddress.Parse("127.0.0.1")
			}
		};

		Assert.Equal("127.0.0.1", httpContext.GetRemoteOrProxy(false).ToString());
	}

	[Theory]
	[InlineData(false, "127.0.0.1")]
	[InlineData(true, "192.168.1.1")]
	public void Return_Correct_IpAddress(bool proxyAware, string expected)
	{
		var httpContext = new DefaultHttpContext
		{
			Request =
			{
				Headers = { new KeyValuePair<string, StringValues>("x-forwarded-for", "192.168.1.1") }
			},
			Connection =
			{
				RemoteIpAddress = IPAddress.Parse("127.0.0.1")
			}
		};

		Assert.Equal(expected, httpContext.GetRemoteOrProxy(proxyAware).ToString());
	}
}