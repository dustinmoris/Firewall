using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace Firewall
{

    /// <summary>
    /// Rules engine to configure Firewall rules for request filtering.
    /// </summary>
    public static class FirewallRulesEngine
    {
        /// <summary>
        /// Configures the Firewall to deny all access.
        /// <para>Use this as the base rule before configuring other rules.</para>
        /// </summary>
        public static IFirewallRule DenyAllAccess() =>
            new DenyAllRule();

        /// <summary>
        /// Configures the Firewall to allow requests from localhost.
        /// </summary>
        public static IFirewallRule ExceptFromLocalhost(this IFirewallRule rule) =>
            new LocalhostRule(rule);

        /// <summary>
        /// Configures the Firewall to allow requests from specific IP addresses.
        /// </summary>
        public static IFirewallRule ExceptFromIPAddresses(
            this IFirewallRule rule,
            IList<IPAddress> ipAddresses,
            bool proxyAware = false) =>
            new IPAddressRule(rule, ipAddresses, proxyAware);

        /// <summary>
        /// Configures the Firewall to allow requests from IP addresses which belong to a list of specific IP address ranges.
        /// </summary>
        public static IFirewallRule ExceptFromIPAddressRanges(
            this IFirewallRule rule,
            IList<CIDRNotation> cidrNotations,
            bool proxyAware = false) =>
            new IPAddressRangeRule(rule, cidrNotations, proxyAware);

        /// <summary>
        /// Configures the Firewall to allow requests from IP addresses which belong to Cloudflare.
        /// </summary>
        /// <param name="rule">Base rule which gets validated when the request did not come from Cloudflare.</param>
        /// <param name="ipv4ListUrl">URL which returns a list of all Cloudflare IPv4 address ranges.</param>
        /// <param name="ipv6ListUrl">URL which returns a list of all Cloudflare IPv6 address ranges.</param>
        public static IFirewallRule ExceptFromCloudflare(
            this IFirewallRule rule,
            string ipv4ListUrl = null,
            string ipv6ListUrl = null)
        {
            var helper = new CloudflareHelper(new HttpClient());
            var (ips, cidrs) = helper.GetIPAddressRangesAsync(ipv4ListUrl, ipv6ListUrl).Result;

            return new IPAddressRule(new IPAddressRangeRule(rule, cidrs), ips);
        }

        /// <summary>
        /// Configures the Firewall to allow requests from specific countries.
        /// </summary>
        public static IFirewallRule ExceptFromCountries(
            this IFirewallRule rule,
            IList<CountryCode> countries, 
            bool proxyAware = false) =>
            new CountryRule(rule, countries, proxyAware: proxyAware);

        /// <summary>
        /// Configures the Firewall to allow requests which satisfy a custom <paramref name="filter"/>.
        /// </summary>
        public static IFirewallRule ExceptWhen(
            this IFirewallRule rule,
            Func<HttpContext, bool> filter) =>
            new CustomRule(rule, filter);
    }
}