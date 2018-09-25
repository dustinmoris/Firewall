using System;
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
            // Add dependencies

            services.AddFirewall();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Register middleware before other middleware
            app.UseCloudflareFirewall(true);

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
