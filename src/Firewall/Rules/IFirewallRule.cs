using Microsoft.AspNetCore.Http;

namespace Firewall
{
    /// <summary>
    /// Public interface for specifying a <see cref="Firewall"/> rule.
    /// </summary>
    public interface IFirewallRule
    {
        /// <summary>
        /// Denotes whether a given <see cref="HttpContext"/> is permitted to access the web server.
        /// </summary>
        bool IsAllowed (HttpContext context);
    }
}