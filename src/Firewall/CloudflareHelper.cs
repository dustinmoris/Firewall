using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Firewall
{
    /// <summary>
    /// The default implementation of <see cref="ICloudflareHelper"/>.
    /// </summary>
    public sealed class CloudflareHelper : ICloudflareHelper
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Instantiates an object of type <see cref="CloudflareHelper"/>.
        /// </summary>
        public CloudflareHelper(HttpClient httpClient)
        {
            if (httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));

            _httpClient = httpClient;
        }

        /// <summary>
        /// Retrieves the latest lists of IP address ranges from Cloudflare.
        /// </summary>
        public async Task<(IList<IPAddress>, IList<CIDRNotation>)> GetIPAddressRangesAsync(
            string ipv4ListUrl = null,
            string ipv6ListUrl = null)
        {
            var (ipv4Vips, ipv4Guests) =
                ParseResponse(
                    await _httpClient.GetStringAsync(
                        ipv4ListUrl ?? "https://www.cloudflare.com/ips-v4"));

            var (ipv6Vips, ipv6Guests) =
                ParseResponse(
                    await _httpClient.GetStringAsync(
                        ipv6ListUrl ?? "https://www.cloudflare.com/ips-v6"));

            var vips = ipv4Vips.Concat(ipv6Vips).ToList();
            var guests = ipv4Guests.Concat(ipv6Guests).ToList();

            return (vips, guests);
        }

        /// <summary>
        /// Retrieves the latest lists of IP address ranges from Cloudflare.
        /// </summary>
        public (IList<IPAddress>, IList<CIDRNotation>) GetIPAddressRanges(
            string ipv4ListUrl = null,
            string ipv6ListUrl = null)
        {
            var (ipv4Vips, ipv4Guests) =
                ParseResponse(
                    _httpClient.GetStringAsync(
                        ipv4ListUrl ?? "https://www.cloudflare.com/ips-v4").Result);

            var (ipv6Vips, ipv6Guests) =
                ParseResponse(
                    _httpClient.GetStringAsync(
                        ipv6ListUrl ?? "https://www.cloudflare.com/ips-v6").Result);

            var vips = ipv4Vips.Concat(ipv6Vips).ToList();
            var guests = ipv4Guests.Concat(ipv6Guests).ToList();

            return (vips, guests);
        }

        private (IList<IPAddress>, IList<CIDRNotation>) ParseResponse(string response)
        {
            var vips = new List<IPAddress>();
            var guests = new List<CIDRNotation>();
            var lines = response.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach(var line in lines)
            {
                var value = line.Trim();
                if (string.IsNullOrWhiteSpace(value))
                    break;

                if (value.Contains("/"))
                    guests.Add(CIDRNotation.Parse(value));
                else
                    vips.Add(IPAddress.Parse(value));
            }

            return (vips, guests);
        }
    }
}