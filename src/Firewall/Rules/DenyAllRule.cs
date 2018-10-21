using Microsoft.AspNetCore.Http;

namespace Firewall
{
    /// <summary>
    /// A Firewall rule which denies all connections access to the web server.
    /// <para>This rule should be used as the base rule for all other rules so that access to the web server gets denied if no other rule permitted access to a given HTTP request.</para>
    /// </summary>
    public sealed class DenyAllRule : IFirewallRule
    {
        /// <summary>
        /// Denotes whether a given <see cref="HttpContext"/> is permitted to access the web server.
        /// </summary>
        public bool IsAllowed(HttpContext context)
        {
            return false;
        }
    }
}