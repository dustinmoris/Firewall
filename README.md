# ![Firewall](https://raw.githubusercontent.com/dustinmoris/Firewall/master/firewall.png) Firewall

ASP.NET Core middleware for IP address filtering.

Firewall adds IP address filtering capabilities to an ASP.NET Core web application by limiting access to a pre-configured range of IP addresses.

[![NuGet Info](https://buildstats.info/nuget/Firewall?includePreReleases=true)](https://www.nuget.org/packages/Firewall/)

| Windows | Linux |
| :------ | :---- |
| [![Build status](https://ci.appveyor.com/api/projects/status/6x0pse65273xp9rw/branch/develop?svg=true)](https://ci.appveyor.com/project/dustinmoris/firewall/branch/develop) | [![Linux Build status](https://travis-ci.org/dustinmoris/Firewall.svg?branch=develop)](https://travis-ci.org/dustinmoris/Firewall/builds?branch=develop) |
| [![Windows Build history](https://buildstats.info/appveyor/chart/dustinmoris/Firewall?branch=develop&includeBuildsFromPullRequest=false)](https://ci.appveyor.com/project/dustinmoris/Firewall/history?branch=develop) | [![Linux Build history](https://buildstats.info/travisci/chart/dustinmoris/Firewall?branch=develop&includeBuildsFromPullRequest=false)](https://travis-ci.org/dustinmoris/Firewall/builds?branch=develop) |

## Table of contents

- [About](#about)
- [Using with Cloudflare](#using-with-cloudflare)
- [Getting Started](#getting-started)
- [Diagnostics](#diagnostics)
- [Contributing](#contributing)
- [Support](#support)
- [License](#license)
- [Credits](#credits)

## About

Firewall is an ASP.NET Core middleware which enables IP address filtering based on individual IP addresses (VIP list) or IP address ranges (guest lists).

IP address filtering can be added as an extra layer of security to a publicly exposed API or to force all API access through a certain set of proxy servers (e.g. [Cloudflare](https://www.cloudflare.com/)).

### How is it different to ASP.NET Core's IP safelist feature?

Simply ASP.NET Core's [safelist](https://docs.microsoft.com/en-us/aspnet/core/security/ip-safelist?view=aspnetcore-2.1) feature doesn't support IPv4 and IPv6 address ranges specified through CIDR notations, which makes is somewhat unusable in the real world where a web application might need to "safelist" a CIDR notation which can span across hundreds or thousands of different unique IPv6 addresses.

## Using with Cloudflare

[Cloudflare](https://www.cloudflare.com/) is a popular internet service which provides enhanced performance and security features to any website. It is currently being used by more than 8 million websites world wide and requires no additional hardware or software as it operates at the DNS level.

The typical request flow for a website which is not protected by Cloudflare looks a little bit like this:

![without-cloudflare](https://raw.githubusercontent.com/dustinmoris/Firewall/master/assets/without-cloudflare.png)

*Image source: [blog.christophetd.fr](https://blog.christophetd.fr/)*

When a website is protected by Cloudflare then Cloudflare essentially acts as a man in the middle, shielding a website from all sorts of malicious internet activity and giving a website administrator enhanced performance and security features such as HTTPS, Caching, CDNs, API rate limiting and more:

![with-cloudflare](https://raw.githubusercontent.com/dustinmoris/Firewall/master/assets/with-cloudflare.png)

*Image source: [blog.christophetd.fr](https://blog.christophetd.fr/)*

The only problem with this configuration is that an attacker can still access the origin server by [sending requests directly to its IP address](http://www.chokepoint.net/2017/10/exposing-server-ips-behind-cloudflare.html) and therefore [bypassing all additional security and performance layers provided by Cloudflare](https://blog.christophetd.fr/bypassing-cloudflare-using-internet-wide-scan-data/).

In order to prevent anyone from talking directly to the origin server and forcing all access through Cloudflare one has to block all IP addresses which do not belong to Cloudflare.

Cloudflare [maintains two public lists](https://www.cloudflare.com/ips/) of all their [IPv4](https://www.cloudflare.com/ips-v4) and [IPv6](https://www.cloudflare.com/ips-v6) address ranges which can be used to configure an origin server's IP address filtering.

[Firewall supports IP filtering for Cloudflare](#built-in-support-for-cloudflare-ip-ranges) out of the box.

## Getting Started

First install the [Firewall](https://www.nuget.org/packages/Firewall/) NuGet package using PowerShell:

```powershell
PM> Install-Package Firewall
```

...or via the dotnet command line:

```
dotnet add [PROJECT] package Firewall --package-directory [PACKAGE_CIRECTORY]
```

Then add the Firewall middleware to your ASP.NET Core `Startup` class and register all dependencies:

```csharp
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
```

Firewall provides three options to configure IP address filtering:

- `allowLocalRequests`: Boolean flag which controls whether requests from the local IP address are allowed or not (useful when debugging on localhost).
- `vipList`: A list of individual `IPAddress` objects which are granted access to the web application (VIPs).
- `guestList`: A list of `CIDRNotation` objects which are granted access to the web application (all other guests).

The terms `vipList` and `guestList` are basically fancier terms which have been chosen to replace the more dated description of "whitelist" when explaining the IP address filtering capabilities of Firewall.

### Built in support for Cloudflare IP ranges

If an ASP.NET Core web application is going to sit behind Cloudflare then Firewall can be configured with the built in `UseCloudflareFirewall` extension method:

```csharp
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
            // Register Firewall middleware:
            app.UseCloudflareFirewall();

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
```

The `UseCloudflareFirewall` extension method will automatically pull the latest list of IPv4 and IPv6 address ranges from Cloudflare and register the `FirewallMiddleware` with those values.

Optionally one can specify custom URLs to link to IP address ranges and/or specify additional VIP and/or guest lists by setting the respective parameters:

```csharp
app.UseCloudflareFirewall(
    allowLocalRequests: true,
    ipv4ListUrl: "https://www.cloudflare.com/ips-v4",
    ipv6ListUrl: "https://www.cloudflare.com/ips-v6",
    additionalVipList: new List<IPAddress> { IPAddress.Parse("10.20.30.40") },
    additionalGuestList: new List<CIDRNotation> { CIDRNotation.Parse("110.40.88.12/28") }
);
```

The easiest way to generate a custom list of `IPAddress` or `CIDRNotation` objects is by making use of the `IPAddress.Parse("0.0.0.0")` and `CIDRNotation.Parse("0.0.0.0/32")` helper methods.

If you have another proxy sitting between Cloudflare and the origin server (e.g. load balancer) then you'll have to enable the `ForwardedHeader` middleware, which will make sure to set Cloudflare's IP address to the `RemoteIpAddress` property of the `HttpContext.Connection` object (by reading the correct value from the `X-Forwarded-For` HTTP header):

```csharp
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
    app.UseCloudflareFirewall();

    app.Run(async (context) =>
    {
        await context.Response.WriteAsync("Hello World!");
    });
}
```

Please be aware that the `ForwardedHeaders` middleware must be registered before the `FirewallMiddleware` and also note that it is not recommended to set `ForwardedLimit` to a value greater than 1 unless you also provide a list of trusted proxies.

## Diagnostics

If you're having troubles with Firewall and you want to get more insight into which requests are being blocked then you can  turn up the log level to a minimum of `Information` of your ASP.NET Core application in order to get more diagnostics information:

```csharp
// In this example Serilog is used to log to the console,
// but any .NET Core logger will work:
public class Program
{
    public static void Main(string[] args)
    {
        RunWebserver(args);
    }

    public static void RunWebserver(string[] args)
    {
        Log.Logger =
            new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();
        WebHost
            .CreateDefaultBuilder(args)
            .UseSerilog()
            .UseStartup<Startup>()
            .Build()
            .Run();
    }
}
```

Sample console output when log level is set to `Information`:

```
[20:30:45 INF] Firewall: Requests from the local IP address are allowed.
[20:30:45 INF] Firewall: VIP list: ["10.20.30.40", "1.2.3.4", "5.6.7.8"].
[20:30:45 INF] Firewall: Guest list: ["110.40.88.12/28", "88.77.99.11/8"].
Hosting environment: Development
Content root path: /Redacted/Firewall/samples/BasicApp
Now listening on: https://localhost:5001
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
[20:37:09 INF] Request starting HTTP/1.1 GET http://localhost:5000/
[20:37:09 INF] Request finished in 27.1814ms 200
[20:37:29 INF] Request starting HTTP/1.1 GET http://localhost:5000/
[20:37:29 WRN] Firewall: Unauthorized access from IP Address '23.53.121.53' trying to reach '/' has been blocked.
[20:37:29 INF] Request finished in 10.052ms 403
```

## Contributing

Feedback is welcome and pull requests get accepted!

## Support

If you've got value from any of the content which I have created, but pull requests are not your thing, then I would also very much appreciate your support by buying me a coffee.

<a href="https://www.buymeacoffee.com/dustinmoris" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/yellow_img.png" alt="Buy Me A Coffee" style="height: auto !important;width: auto !important;" ></a>

## License

[Apache 2.0](https://raw.githubusercontent.com/dustinmoris/Firewall/master/LICENSE)

## Credits

Logo is made by [Smashicons](https://www.flaticon.com/authors/smashicons) from [Flaticon](https://www.flaticon.com/) and is licensed under [Creative Commons 3.0](http://creativecommons.org/licenses/by/3.0/).