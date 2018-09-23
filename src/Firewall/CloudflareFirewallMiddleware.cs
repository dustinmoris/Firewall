using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Firewall
{
    /// <summary>
    /// An ASP.NET Core middlware to only allow Cloudflare IP addresses access.
    /// </summary>
    public sealed class CloudflareFirewallMiddleware
    {
        private readonly FirewallMiddleware _firewallMiddleware;

        /// <summary>
        /// Instantiates a new object of type <see cref="CloudflareFirewallMiddleware"/>.
        /// </summary>
        public CloudflareFirewallMiddleware(
            RequestDelegate next,
            bool allowLocalRequests,
            string ipv4ListUrl,
            string ipv6ListUrl,
            IList<IPAddress> additionalVipList,
            IList<CIDRNotation> additionalGuestList,
            ICloudflareHelper helper,
            ILogger<FirewallMiddleware> logger)
        {
            var (vips, guests) = helper.GetIPAddressRanges(ipv4ListUrl, ipv6ListUrl);

            _firewallMiddleware = new FirewallMiddleware(
                next,
                allowLocalRequests,
                vips.Concat(additionalVipList).ToList(),
                guests.Concat(additionalGuestList).ToList(),
                logger);
        }

        /// <summary>
        /// Filters an incoming HTTP request based on the configured VIP and guest list.
        /// </summary>
        public Task Invoke (HttpContext context)
        {
            return _firewallMiddleware.Invoke(context);
        }
    }
}