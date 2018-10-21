using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Firewall;

namespace BasicApp
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseForwardedHeaders(
                new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor,
                    ForwardLimit = 1
                }
            );

            // Register Firewall after error handling and forwarded headers,
            // but before other middleware:
            var allowedIPAddresses =
                new List<IPAddress>
                    {
                        IPAddress.Parse("10.20.30.40"),
                        IPAddress.Parse("1.2.3.4"),
                        IPAddress.Parse("5.6.7.8")
                    };

            var allowedIPAddressRanges =
                new List<CIDRNotation>
                    {
                        CIDRNotation.Parse("110.40.88.12/28"),
                        CIDRNotation.Parse("88.77.99.11/8")
                    };

            app.UseFirewall(
                FirewallRulesEngine
                    .DenyAllAccess()
                    .ExceptFromLocalhost()
                    .ExceptFromCloudflare()
                    .ExceptFromIPAddresses(allowedIPAddresses)
                    .ExceptFromIPAddressRanges(allowedIPAddressRanges));

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
