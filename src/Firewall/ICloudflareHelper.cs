using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Firewall
{
    /// <summary>
    /// A contract which specifies a collection of members which help to retrieve the latest lists of IPv4 and IPv6 IP ranges from Cloudflare.
    /// </summary>
    public interface ICloudflareHelper
    {
        /// <summary>
        /// Retrieves the latest list of IP addresses from Cloudflare.
        /// </summary>
        Task<(IList<IPAddress>, IList<CIDRNotation>)> GetIPAddressRangesAsync(
            string ipv4ListUrl = null,
            string ipv6ListUrl = null);

        /// <summary>
        /// Retrieves the latest list of IP addresses from Cloudflare.
        /// </summary>
        (IList<IPAddress>, IList<CIDRNotation>) GetIPAddressRanges(
            string ipv4ListUrl = null,
            string ipv6ListUrl = null);
    }
}