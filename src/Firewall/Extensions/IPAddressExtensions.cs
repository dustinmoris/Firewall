using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Firewall
{
    /// <summary>
    /// 
    /// </summary>
	public static class IPAddressExtensions
    {
        internal static byte[] GetMappedAddressBytes(this IPAddress address) =>
            address.IsIPv4MappedToIPv6
                ? address.MapToIPv4().GetAddressBytes()
                : address.GetAddressBytes();

        internal static bool IsEqualTo(this IPAddress address, IPAddress otherAddress) =>
            address.GetAddressBytes().IsEqualTo(
                otherAddress.GetMappedAddressBytes());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="proxyAware"></param>
        /// <returns></returns>
        public static IPAddress GetRemoteOrProxy(this HttpContext context, bool proxyAware)
        {
	        const string xForwardedForHeader = "x-forwarded-for";

	        if (!proxyAware)
		        return context.Connection.RemoteIpAddress;

	        var xff = context.Request.Headers.FirstOrDefault(x => x.Key.ToLower() == xForwardedForHeader);

	        if (string.IsNullOrEmpty(xff.Key))
		        return context.Connection.RemoteIpAddress;

	        return IPAddress.Parse(xff.Value.First());
		}
    }
}