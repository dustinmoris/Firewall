using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Firewall
{
    /// <summary>
    /// An ASP.NET Core middlware for IP address filtering.
    /// <para>Firewall checks the IP address of an incoming HTTP request and validates it against a list of single IP addresses or CIDR notations.</para>
    /// <para>IP addresses and/or CIDR notations can be IPv4 or IPv6 address spaces.</para>
    /// </summary>
    public sealed class FirewallMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        private readonly bool _allowLocalRequests;
        private readonly IList<IPAddress> _vipList;
        private readonly IList<CIDRNotation> _guestList;

        /// <summary>
        /// Instantiates a new object of type <see cref="FirewallMiddleware"/>.
        /// </summary>
        public FirewallMiddleware(
            RequestDelegate next,
            bool allowLocalRequests,
            IList<IPAddress> vipList,
            IList<CIDRNotation> guestList,
            ILogger<FirewallMiddleware> logger)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
            _logger = logger;

            _allowLocalRequests = allowLocalRequests;
            _vipList = vipList;
            _guestList = guestList;

            LogFirewallSettings();
        }

        private void LogFirewallSettings()
        {
            if (_logger == null) return;

            if (_vipList == null || _vipList.Count == 0)
                _logger.LogInformation("No VIP list specified.");
            else
                _logger.LogInformation("Firewall has been configured with the following VIP list: {vipList}.", _vipList);

            if (_guestList == null || _guestList.Count == 0)
                _logger.LogInformation("No Guest list specified.");
            else
                _logger.LogInformation("Firewall has been configured with the following Guest list: {guestList}.", _guestList);
        }

        /// <summary>
        /// Filters an incoming HTTP request based on the configured VIP and guest list.
        /// </summary>
        public Task Invoke (HttpContext context)
        {
            var localIpAddress = context.Connection.LocalIpAddress;
            var remoteIpAddress = context.Connection.RemoteIpAddress;

            var isLocalRequest =
                (remoteIpAddress != null
                    && remoteIpAddress.ToString() != "::1"
                    && remoteIpAddress.Equals(localIpAddress))
                || IPAddress.IsLoopback(remoteIpAddress);

            return
                ((_allowLocalRequests && isLocalRequest)
                    || IsVip(remoteIpAddress)
                    || IsInvitedGuest(remoteIpAddress))
                ? _next.Invoke(context)
                : DenyAccess(context);
        }

        private bool IsVip(IPAddress remoteAddress)
        {
            if (_vipList != null && _vipList.Count > 0)
            {
                foreach(var ip in _vipList)
                {
                    if (ip.IsEqualTo(remoteAddress))
                        return true;
                }
            }

            return false;
        }

        private bool IsInvitedGuest(IPAddress remoteAddress)
        {
            if (_guestList != null && _guestList.Count > 0)
            {
                foreach(var cidr in _guestList)
                {
                    if (cidr.Contains(remoteAddress))
                        return true;
                }
            }

            return false;
        }

        private Task DenyAccess(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return context.Response.WriteAsync("You're not authorized to access this resource.");
        }
    }
}