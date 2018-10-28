using System;
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

        private static object[] Concat(this object[] front, object[] end)
        {
            var result = new object[front.Length + end.Length];
            Array.Copy(front, result, front.Length);
            Array.Copy(end, 0, result, front.Length, end.Length);
            return result;
        }

        internal static void LogDebug(
            this HttpContext context,
            Type type,
            bool isGranted,
            string reason,
            params object[] args)
        {
            var logger = context.GetLogger();

            if (logger != null && logger.IsEnabled(LogLevel.Debug))
            {
                var args0 =
                    new object []
                    {
                        type,
                        context.Connection.RemoteIpAddress,
                        isGranted ? "granted" : "denied"
                    };

                logger.LogDebug(
                    $"{{type}}: Remote IP Address '{{ip}}' has been {{result}} access, because {reason}.",
                    args0.Concat(args));
            }
        }
    }
}