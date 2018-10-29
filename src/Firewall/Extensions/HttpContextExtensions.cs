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

        internal static void Log(
            this HttpContext context,
            LogLevel logLevel,
            Type type,
            string message,
            params object[] args)
        {
            var logger = context.GetLogger();

            if (logger == null || !logger.IsEnabled(logLevel))
                return;

            logger.Log(
                logLevel,
                $"{{type}}: {message}",
                (new[] { type }).Concat(args));
        }

        internal static void LogDebug(
            this HttpContext context,
            Type type,
            bool isGranted,
            string reason,
            params object[] args)
        {
            var args0 =
                new object []
                {
                    context.Connection.RemoteIpAddress,
                    isGranted ? "granted" : "denied"
                };

            context.Log(
                LogLevel.Debug,
                type,
                $"Remote IP Address '{{ipAddress}}' has been {{firewallResult}} access, because {reason}.",
                args0.Concat(args));
        }
    }
}