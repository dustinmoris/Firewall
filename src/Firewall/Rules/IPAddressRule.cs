using System;
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
        private readonly bool _proxyAware;

        /// <summary>
        /// Initialises a new instance of <see cref="IPAddressRule"/>.
        /// </summary>
        public IPAddressRule(IFirewallRule nextRule, IList<IPAddress> ipAddresses, bool proxyAware = false)
        {
            _nextRule = nextRule ?? throw new ArgumentNullException(nameof(nextRule));
            _ipAddresses = ipAddresses ?? throw new ArgumentNullException(nameof(ipAddresses));
            _proxyAware = proxyAware;
        }

        /// <summary>
        /// Denotes whether a given <see cref="HttpContext"/> is permitted to access the web server.
        /// </summary>
        public bool IsAllowed(HttpContext context)
        {
	        var remoteIpAddress = context.GetRemoteOrProxy(_proxyAware);
            var (isAllowed, ip) = MatchesAnyIPAddress(remoteIpAddress);

            context.LogDebug(
                typeof(IPAddressRule),
                isAllowed,
                isAllowed
                    ? "it matched '{ipAddress}'"
                    : "it didn't match any known IP address",
                ip);

            return isAllowed || _nextRule.IsAllowed(context);
        }

        private (bool, IPAddress) MatchesAnyIPAddress(IPAddress remoteIpAddress)
        {
            if (_ipAddresses != null && _ipAddresses.Count > 0)
                foreach (var ip in _ipAddresses)
                    if (ip.IsEqualTo(remoteIpAddress))
                        return (true, ip);

            return (false, null);
        }
    }
}