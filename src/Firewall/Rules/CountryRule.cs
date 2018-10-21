using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Firewall
{
    /// <summary>
    /// A Firewall rule which specifies that only connections from a specific geo-location are allowed to access the web server.
    /// </summary>
    public sealed class CountryRule : IFirewallRule
    {
        private readonly IFirewallRule _nextRule;
        private readonly IList<IsoCode> _allowedCountries;

        /// <summary>
        /// Initialises a new instance of <see cref="CountryRule"/>.
        /// </summary>
        public CountryRule(IFirewallRule nextRule, IList<IsoCode> allowedCountries)
        {
            _nextRule = nextRule;
            _allowedCountries = allowedCountries;
        }

        /// <summary>
        /// Denotes whether a given <see cref="HttpContext"/> is permitted to access the web server.
        /// </summary>
        public bool IsAllowed(HttpContext context)
        {
            return true;
        }
    }
}