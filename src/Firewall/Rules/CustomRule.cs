using System;
using Microsoft.AspNetCore.Http;

namespace Firewall
{
    /// <summary>
    /// A Firewall rule which applies a custom request filter.
    /// </summary>
    public sealed class CustomRule : IFirewallRule
    {
        private readonly IFirewallRule _nextRule;
        private readonly Func<HttpContext, bool> _filter;

        /// <summary>
        /// Initialises a new instance of <see cref="CustomRule"/>.
        /// </summary>
        public CustomRule(IFirewallRule nextRule, Func<HttpContext, bool> filter)
        {
            _nextRule = nextRule;
            _filter = filter;
        }

        /// <summary>
        /// Denotes whether a given <see cref="HttpContext"/> is permitted to access the web server.
        /// </summary>
        public bool IsAllowed(HttpContext context)
        {
            return _filter(context) || _nextRule.IsAllowed(context);
        }
    }
}