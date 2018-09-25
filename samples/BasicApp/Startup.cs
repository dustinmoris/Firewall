using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Firewall;

namespace BasicApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Register dependencies:
            services.AddFirewall();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Register middleware before other middleware:
            app.UseFirewall(
                allowLocalRequests: true,
                vipList: new List<IPAddress> { IPAddress.Parse("10.20.30.40") },
                guestList: new List<CIDRNotation> { CIDRNotation.Parse("110.40.88.12/28") }
            );

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
