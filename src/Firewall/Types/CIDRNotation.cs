using System;
using System.Net;
using System.Net.Sockets;

namespace Firewall
{
    /// <summary>
    /// Type to represent a CIDR notation (e.g. "120.30.24.123/20").
    /// </summary>
    public sealed class CIDRNotation
    {
        /// <summary>
        /// The IP address part of the CIDR notation.
        /// </summary>
        public readonly IPAddress Address;

        /// <summary>
        /// The mask bits of the CIDR notation.
        /// </summary>
        public readonly int MaskBits;

        private CIDRNotation(string cidrNotation)
        {
            if (string.IsNullOrEmpty(cidrNotation))
                throw new ArgumentException("A CIDR notation string cannot be null or empty.");

            var parts = cidrNotation.Split('/');

            if (parts.Length != 2)
                throw new ArgumentException($"Invalid CIDR notation: {cidrNotation}.");

            var isValid = IPAddress.TryParse(parts[0], out var address);

            if (!isValid)
                throw new ArgumentException($"Invalid address in CIDR notation: {cidrNotation}.");

            var maskBits = Convert.ToInt32(parts[1], 10);
            var maxMaskBit = address.AddressFamily == AddressFamily.InterNetwork ? 32 : 128;

            if (maskBits < 0 || maskBits > maxMaskBit)
                throw new ArgumentException($"Invalid bits in CIDR notation: {maskBits}.");

            Address = address;
            MaskBits = maskBits;
        }

        /// <summary>
        /// Parses a given <paramref name="cidrNotation"/> string to a type of <see cref="CIDRNotation"/>.
        /// </summary>
        public static CIDRNotation Parse(string cidrNotation) => new CIDRNotation(cidrNotation);

        /// <summary>
        /// Checks if an <paramref name="address"/> is within the address space defined by this CIDR notation.
        /// </summary>
        public bool Contains(IPAddress address) =>
            CompareAddressBytes(
                this.Address.GetAddressBytes(),
                address.GetMappedAddressBytes(),
                this.MaskBits);

        private static bool CompareAddressBytes(byte[] cidr, byte[] address, int bits)
        {
            if (cidr.Length != address.Length) return false;

            var index = 0;

            for (; bits >= 8; bits -= 8)
            {
                if (address[index] != cidr[index])
                    return false;
                index++;
            }

            if (bits <= 0)
                return true;

            var mask = (byte)~(255 >> bits);

            return (address[index] & mask) == (cidr[index] & mask);
        }

        /// <summary>
        /// Converts the <see cref="CIDRNotation"/> object into a <see cref="string"/> object.
        /// </summary>
        public override string ToString() => $"{this.Address.ToString()}/{this.MaskBits}";
    }
}
