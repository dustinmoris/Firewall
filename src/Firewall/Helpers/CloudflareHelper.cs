using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Firewall
{
    /// <summary>
    /// A helper class to retrieve the latest set of IP Address ranges from Cloudflare.
    /// </summary>
    public sealed class CloudflareHelper
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Instantiates an object of type <see cref="CloudflareHelper"/>.
        /// </summary>
        public CloudflareHelper(HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        /// <summary>
        /// Retrieves the latest lists of IP address ranges from Cloudflare.
        /// </summary>
        public async Task<(IList<IPAddress>, IList<CIDRNotation>)> GetIPAddressRangesAsync(
            string ipv4ListUrl = null,
            string ipv6ListUrl = null)
        {
            var (ipv4Addresses, ipv4Ranges) =
                ParseResponse(
                    await _httpClient.GetStringAsync(
                        ipv4ListUrl ?? "https://www.cloudflare.com/ips-v4"));

            var (ipv6Addresses, ipv6Ranges) =
                ParseResponse(
                    await _httpClient.GetStringAsync(
                        ipv6ListUrl ?? "https://www.cloudflare.com/ips-v6"));

            var ips = ipv4Addresses.Concat(ipv6Addresses).ToList();
            var cidrs = ipv4Ranges.Concat(ipv6Ranges).ToList();

            return (ips, cidrs);
        }

        private (IList<IPAddress>, IList<CIDRNotation>) ParseResponse(string response)
        {
            var ips = new List<IPAddress>();
            var cidrs = new List<CIDRNotation>();
            var lines = response.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach(var line in lines)
            {
                var value = line.Trim();
                if (string.IsNullOrWhiteSpace(value))
                    break;

                if (value.Contains("/"))
                    cidrs.Add(CIDRNotation.Parse(value));
                else
                    ips.Add(IPAddress.Parse(value));
            }

            return (ips, cidrs);
        }
    }
}