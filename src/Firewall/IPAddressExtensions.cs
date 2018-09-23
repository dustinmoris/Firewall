using System.Net;

namespace Firewall
{
    /// <summary>
    /// Useful extension methods for an <see cref="IPAddress"/> object.
    /// </summary>
    public static class IPAddressExtensions
    {
        /// <summary>
        /// Checks if a given <paramref name="address"/> is an IPv4 address mapped to an IPv6 address and returns the bytes for the correct address space.
        /// </summary>
        public static byte[] GetMappedAddressBytes(this IPAddress address) =>
            address.IsIPv4MappedToIPv6
                ? address.MapToIPv4().GetAddressBytes()
                : address.GetAddressBytes();

        /// <summary>
        /// Compares two addresses by comparing their byte representation. If <paramref name="otherAddress"/> is an IPv4 address mapped to an IPv6 address, then it will first convert the IPv6 address to an IPv4 address before comparing their byte representation.
        /// </summary>
        public static bool IsEqualTo(this IPAddress address, IPAddress otherAddress)
        {
            var addressBytes = address.GetAddressBytes();
            var otherBytes = otherAddress.GetMappedAddressBytes();

            return
                addressBytes.Length == otherBytes.Length
                && addressBytes.Equals(otherBytes);
        }
    }
}