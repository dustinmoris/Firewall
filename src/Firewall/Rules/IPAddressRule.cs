using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace Firewall
{
    /// <summary>
    /// A Firewall rule which permits access to a list of specific IP addresses.
    /// </summary>
    public sealed class IPAddressRule : IFirewallRule
    {
        private readonly IFirewallRule _nextRule;
        private readonly IList<IPAddress> _ipAddresses;

        /// <summary>
        /// Initialises a new instance of <see cref="IPAddressRule"/>.
        /// </summary>
        public IPAddressRule(IFirewallRule nextRule, IList<IPAddress> ipAddresses)
        {
            _nextRule = nextRule;
            _ipAddresses = ipAddresses;
        }

        /// <summary>
        /// Denotes whether a given <see cref="HttpContext"/> is permitted to access the web server.
        /// </summary>
        public bool IsAllowed(HttpContext context)
        {
            var remoteIpAddress = context.Connection.RemoteIpAddress;

            if (_ipAddresses != null && _ipAddresses.Count > 0)
                foreach (var ip in _ipAddresses)
                    if (ip.IsEqualTo(remoteIpAddress))
                        return true;

            return _nextRule.IsAllowed(context);
        }
    }
}