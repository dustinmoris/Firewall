# ![Firewall](https://raw.githubusercontent.com/dustinmoris/Firewall/master/firewall.png) Firewall

ASP.NET Core middleware for IP address filtering.

Firewall adds IP address filtering capabilities to an ASP.NET Core web application by limiting access to a pre-configured range of IP addresses.

[![NuGet Info](https://buildstats.info/nuget/Firewall?includePreReleases=true)](https://www.nuget.org/packages/Firewall/)

| Windows | Linux |
| :------ | :---- |
| [![Build status](https://ci.appveyor.com/api/projects/status/6x0pse65273xp9rw/branch/develop?svg=true)](https://ci.appveyor.com/project/dustinmoris/firewall/branch/develop) | [![Linux Build status](https://travis-ci.org/dustinmoris/Firewall.svg?branch=develop)](https://travis-ci.org/dustinmoris/Firewall/builds?branch=develop) |
| [![Windows Build history](https://buildstats.info/appveyor/chart/dustinmoris/Firewall?branch=develop&includeBuildsFromPullRequest=false)](https://ci.appveyor.com/project/dustinmoris/Firewall/history?branch=develop) | [![Linux Build history](https://buildstats.info/travisci/chart/dustinmoris/Firewall?branch=develop&includeBuildsFromPullRequest=false)](https://travis-ci.org/dustinmoris/Firewall/builds?branch=develop) |

## About

Firewall is an ASP.NET Core middleware which enables IP address filtering based on individual IP addresses (VIP list) or IP address ranges (guest lists).

IP address filtering can add an extra layer of security to a publicly exposed API or to force all API access through a certain set of proxy servers (e.g. [Cloudflare](https://www.cloudflare.com/)).

## Using with Cloudflare

[Cloudflare](https://www.cloudflare.com/) is a popular internet service which provides enhanced performance and security features to any website. It is currently being used by more than 8 million websites world wide and requires no additional hardware or software as it operates at the DNS level.

The typical request flow for a website which is not protected by Cloudflare looks a little bit like this:

![without-cloudflare](https://raw.githubusercontent.com/dustinmoris/Firewall/master/assets/without-cloudflare.png)

When a website is protected by Cloudflare then Cloudflare essentially acts as a man in the middle, shielding a website from all sorts of malicious internet activity and giving a website administrator enhanced performance and security features such as HTTPS, Caching, CDNs, API rate limiting and more:

![with-cloudflare](https://raw.githubusercontent.com/dustinmoris/Firewall/master/assets/with-cloudflare.png)

The only problem with this configuration is that an attacker can still access the origin server by [sending requests directly to its IP address](http://www.chokepoint.net/2017/10/exposing-server-ips-behind-cloudflare.html) and therefore [bypassing all additional security and performance layers provided by Cloudflare](https://blog.christophetd.fr/bypassing-cloudflare-using-internet-wide-scan-data/).

In order to prevent anyone from talking directly to the origin server and forcing all access through Cloudflare one has to block all IP addresses which do not belong to Cloudflare.

Cloudflare [maintains two public lists](https://www.cloudflare.com/ips/) of all their [IPv4](https://www.cloudflare.com/ips-v4) and [IPv6](https://www.cloudflare.com/ips-v6) address ranges which can be used to configure an origin server's IP address filtering.

[Firewall supports IP filtering for Cloudflare]() out of the box.

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

The terms `vipList` and `guestList` are basically fancier terms which have been chosen to replace the more dated description of "whitelist" when explaining the IP address filtering features of Firewall.

## License

[Apache 2.0](https://raw.githubusercontent.com/dustinmoris/Firewall/master/LICENSE)

## Credits

Logo is made by [Smashicons](https://www.flaticon.com/authors/smashicons) from [Flaticon](https://www.flaticon.com/) and is licensed under [Creative Commons 3.0](http://creativecommons.org/licenses/by/3.0/).