using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Firewall
{
    internal static class HttpContextExtensions
    {
        private static ILogger GetLogger(this HttpContext context)
        {
            return (ILogger)context.RequestServices?.GetService(typeof(ILogger<FirewallMiddleware>));
        }

        internal static void LogDebug(
            this HttpContext context,
            string className,
            bool isGranted,
            string reason,
            params object[] args)
        {
            var logger = context.GetLogger();
            if (logger != null)
                logger.LogDebug(
                    "Firewall.{class}: Remote IP Address '{ip}' has been {result} access, because {reason}.",
                    className,
                    context.Connection.RemoteIpAddress,
                    isGranted ? "granted" : "denied",
                    reason);
        }
    }
}