using System.Net;

namespace Firewall
{
    internal static class IPAddressExtensions
    {
        internal static byte[] GetMappedAddressBytes(this IPAddress address) =>
            address.IsIPv4MappedToIPv6
                ? address.MapToIPv4().GetAddressBytes()
                : address.GetAddressBytes();

        internal static bool IsEqualTo(this IPAddress address, IPAddress otherAddress) =>
            address.GetAddressBytes().IsEqualTo(
                otherAddress.GetMappedAddressBytes());
    }
}