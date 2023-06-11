using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Firewall
{
    /// <summary>
    /// A Firewall rule which permits access to a given list of IP address ranges (expressed as CIDR notations).
    /// </summary>
    public sealed class IPAddressRangeRule : IFirewallRule
    {
        private readonly IFirewallRule _nextRule;
        private readonly IList<CIDRNotation> _cidrNotations;
		private readonly bool _proxyAware;

		/// <summary>
		/// Initialises a new instance of <see cref="IPAddressRangeRule"/>.
		/// </summary>
		public IPAddressRangeRule(IFirewallRule nextRule, IList<CIDRNotation> cidrNotations, bool proxyAware = false)
        {
            _nextRule = nextRule ?? throw new ArgumentNullException(nameof(nextRule));
            _cidrNotations = cidrNotations ?? throw new ArgumentNullException(nameof(cidrNotations));
            _proxyAware = proxyAware;
		}

        /// <summary>
        /// Denotes whether a given <see cref="HttpContext"/> is permitted to access the web server.
        /// </summary>
        public bool IsAllowed(HttpContext context)
        {
	        var remoteIpAddress = context.GetRemoteOrProxy(_proxyAware);
            var (isAllowed, cidr) = MatchesAnyIPAddressRange(remoteIpAddress);

            context.LogDebug(
                typeof(IPAddressRangeRule),
                isAllowed,
                isAllowed
                    ? "it belongs to '{cidr}' address range"
                    : "it didn't belong to any known address range",
                cidr);

            return isAllowed || _nextRule.IsAllowed(context);
        }

        private (bool, CIDRNotation) MatchesAnyIPAddressRange(IPAddress remoteIpAddress)
        {
            if (_cidrNotations != null && _cidrNotations.Count > 0)
                foreach (var cidr in _cidrNotations)
                    if (cidr.Contains(remoteIpAddress))
                        return (true, cidr);

            return (false, null);
        }
    }
}