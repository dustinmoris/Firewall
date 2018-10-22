# ![Firewall](https://raw.githubusercontent.com/dustinmoris/Firewall/master/firewall.png) Firewall

ASP.NET Core middleware for request filtering.

Firewall adds IP address-, geo-location and custom filtering capabilities to an ASP.NET Core web application which gives control over which connections are allowed to access the web server.

[![NuGet Info](https://buildstats.info/nuget/Firewall?includePreReleases=true)](https://www.nuget.org/packages/Firewall/)

| Windows | Linux |
| :------ | :---- |
| [![Build status](https://ci.appveyor.com/api/projects/status/6x0pse65273xp9rw/branch/develop?svg=true)](https://ci.appveyor.com/project/dustinmoris/firewall/branch/develop) | [![Linux Build status](https://travis-ci.org/dustinmoris/Firewall.svg?branch=develop)](https://travis-ci.org/dustinmoris/Firewall/builds?branch=develop) |
| [![Windows Build history](https://buildstats.info/appveyor/chart/dustinmoris/Firewall?branch=develop&includeBuildsFromPullRequest=false)](https://ci.appveyor.com/project/dustinmoris/Firewall/history?branch=develop) | [![Linux Build history](https://buildstats.info/travisci/chart/dustinmoris/Firewall?branch=develop&includeBuildsFromPullRequest=false)](https://travis-ci.org/dustinmoris/Firewall/builds?branch=develop) |

## Table of contents

- [About](#about)
- [Using with Cloudflare](#using-with-cloudflare)
- [Getting Started](#getting-started)
- [Documentation](#documentation)
    - [Basics](#basics)
    - [Cloudflare Support](#cloudflare-support)
    - [Custom Rules](#custom-rules)
    - [Custom Filter Rules](#custom-filter-rules)
    - [Miscellaneous](#miscellaneous)
        - [IP Address and CIDR Notation Parsing](#ip-address-and-cidr-notation-parsing)
        - [X-Forwarded-For HTTP Header](#x-forwarded-for-http-header)
        - [Loading Rules from Configuration](#loading-rules-from-configuration)
    - [Diagnostics](#diagnostics)
- [Contributing](#contributing)
- [Support](#support)
- [License](#license)
- [Credits](#credits)

## About

Firewall is an ASP.NET Core middleware which enables IPv4 and IPv6 address-, geo-location and other request filtering features.

Request filtering can be added as an extra layer of security to a publicly exposed API or to force all API access through a certain set of proxy servers (e.g. [Cloudflare](https://www.cloudflare.com/)).

### How is it different to ASP.NET Core's IP safelist feature?

Simply ASP.NET Core's [safelist](https://docs.microsoft.com/en-us/aspnet/core/security/ip-safelist?view=aspnetcore-2.1) feature doesn't support IPv4 and IPv6 address ranges specified through CIDR notations, which makes is somewhat less usable in the real world where a web application might need to "safelist" a CIDR notation as part of its security configuration.

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

Then add the Firewall middleware to your ASP.NET Core `Startup` class:

```csharp
using Firewall;

namespace BasicApp
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            var allowedIPs =
                new List<IPAddress>
                    {
                        IPAddress.Parse("10.20.30.40"),
                        IPAddress.Parse("1.2.3.4"),
                        IPAddress.Parse("5.6.7.8")
                    };

            var allowedCIDRs =
                new List<CIDRNotation>
                    {
                        CIDRNotation.Parse("110.40.88.12/28"),
                        CIDRNotation.Parse("88.77.99.11/8")
                    };

            app.UseFirewall(
                FirewallRulesEngine
                    .DenyAllAccess()
                    .ExceptFromIPAddressRanges(allowedCIDRs)
                    .ExceptFromIPAddresses(allowedIPs));

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
```

## Documentation

### Basics

Firewall uses a rules engine to configure request filtering. The `FirewallRulesEngine` helper class should be used to configure an object of `IFirewallRule` which then can be passed into the `FirewallMiddleware`:

```csharp
var rules =
    FirewallRulesEngine
        .DenyAllAccess()
        .ExceptFromCloudflare()
        .ExceptFromLocalhost();

app.UseFirewall(rules);
```

Currently the following rules can be configures out of the box:

- `DenyAllAccess()`: This is the base rule which should be used at the beginning of the rules configuration. It specifies that if no other rule can be met by an incoming HTTP request then access should be denied.
- `ExceptFromLocalhost()`: This rule specifies that HTTP requests from the local host should be allowed. This might be useful when debugging the application.
- `ExceptFromIPAddresses(IList<IPAddress> ipAddresses)`: This rule enables access to a list of specific IP addresses.
- `ExceptFromIPAddressRanges(IList<CIDRNotation> cidrNotations)`: This rule enables access to a list of specific IP address ranges (CIDR notations).
- `ExceptFromCloudflare(string ipv4Url = null, string ipv6Url = null)`: This rule enables access to requests from Cloudflare servers.
- `ExceptFromCountry(IList<CountryCode> allowedCountries)`: This rule enables access to requests which originated from one of the specified countries.
- `ExceptWhen(Func<HttpContext, bool> filter)`: This rule enables a custom request filter to be applied (see [Custom Filter Rules](#custom-filter-rules) for more info).

A HTTP request only needs to satisfy a single rule in order to pass the Firewall access control layer. The reverse order of the rules specifies the order in which an incoming HTTP request gets validated. It is advisable to specify simple/quick rules at the end as they will get executed first.

### Cloudflare Support

If an ASP.NET Core web application is going to sit behind Cloudflare then Firewall can be configured with the built-in `ExceptFromCloudflare()` Firewall rule:

```csharp
using Firewall;

namespace BasicApp
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseFirewall(
                FirewallRulesEngine
                    .DenyAllAccess()
                    .ExceptFromCloudflare());

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
```

The `ExceptFromCloudflare()` configuration method will automatically pull the latest list of IPv4 and IPv6 address ranges from Cloudflare and register the `FirewallMiddleware` with those values.

Optionally one can specify custom URLs to load the correct IP address ranges from:

```csharp
using Firewall;

namespace BasicApp
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseFirewall(
                FirewallRulesEngine
                    .DenyAllAccess()
                    .ExceptFromCloudflare(
                        ipv4ListUrl: "https://www.cloudflare.com/ips-v4",
                        ipv6ListUrl: "https://www.cloudflare.com/ips-v6"
                    ));

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
```

### Custom Rules

Custom Firewall rules can be added by creating a new class which implements `IFirewallRule`.

For example, if one would like to create a new Firewall rule which filters requests based on [Cloudflare's `CF-IPCountry`](https://support.cloudflare.com/hc/en-us/articles/200170986-How-does-CloudFlare-handle-HTTP-Request-headers-) HTTP header, then you'd start by implementing a new class which implements the `IFirewallRule` interface:

```csharp
public class IPCountryRule : IFirewallRule
{
    private readonly IFirewallRule _nextRule;
    private readonly IList<string> _allowedCountryCodes;

    public IPCountryRule(
        IFirewallRule nextRule,
        IList<string> allowedCountryCodes)
    {
        _nextRule = nextRule;
        _allowedCountryCodes = allowedCountryCodes;
    }

    public bool IsAllowed(HttpContext context)
    {
        const string headerKey = "CF-IPCountry";

        if (!context.Request.Headers.ContainsKey(headerKey))
            return _nextRule.IsAllowed(context);

        var countryCode = context.Request.Headers[headerKey].ToString();
        var isAllowed = _allowedCountryCodes.Contains(countryCode);

        return isAllowed || _nextRule.IsAllowed(context);
    }
}
```

The constructor of the `IPCountryRule` class takes in a list of allowed country codes and the next rule in the pipeline. If a HTTP request originated from an allowed country then the custom rule will return `true`, otherwise it will invoke the next rule of the rules engine.

In order to chain this rule into the existing rules engine one can add an additional extension method:

```csharp
public static class FirewallRulesEngineExtensions
{
    public static IFirewallRule ExceptFromCountryCodes(
        this IFirewallRule rule,
        IList<string> allowedCountryCodes)
    {
        return new IPCountryRule(rule, allowedCountryCodes);
    }
}
```

Afterwards the rule can be enabled by calling `ExceptFromCountryCodes(allowedCountryCodes)` during application setup:

```csharp
public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
        app.UseFirewall(
            FirewallRulesEngine
                .DenyAllAccess()
                .ExceptFromCountryCodes(new [] { "US", "GB", "JP" })
                .ExceptFromCloudflare());

        app.Run(async (context) =>
        {
            await context.Response.WriteAsync("Hello World!");
        });
    }
}
```

### Custom Filter Rules

Another slightly less flexible, but much easier and quicker way of applying a custom rule is by using the `ExceptWhen(Func<HttpContext, bool> filter)` configuration method. The `ExceptWhen` method can be used to set up simple rules by providing a `Func<HttpContext, bool>` predicate:

```csharp
var adminIP = IPAddress.Parse("1.2.3.4");

app.UseFirewall(
    FirewallRulesEngine
        .DenyAllAccess()
        .ExceptFromCloudflare()
        .ExceptWhen(ctx => ctx.Connection.RemoteIpAddress == adminIP));
```

### Miscellaneous

#### IP Address and CIDR Notation Parsing

The easiest way to generate a custom list of `IPAddress` or `CIDRNotation` objects is by making use of the `IPAddress.Parse("0.0.0.0")` and `CIDRNotation.Parse("0.0.0.0/32")` helper methods.

#### X-Forwarded-For HTTP Header

If you have other proxies sitting between Cloudflare and the origin server (e.g. load balancer) then you'll have to enable the `ForwardedHeader` middleware, which will make sure that the correct IP address will be assigned to the `RemoteIpAddress` property of the `HttpContext.Connection` object:

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

Please be aware that the `ForwardedHeaders` middleware must be registered before the `FirewallMiddleware` and also that it is not recommended to set the `ForwardedLimit` to a value greater than 1 unless you also provide a list of trusted proxies.

#### Loading Rules from Configuration

Firewall doesn't prescribe a certain way of how to configure rules outside of the rules engine. It is up to an application author to decide how rules should be loaded from an external configuration provider. [ASP.NET Core offers a wealth of default configuration providers](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.1) which are recommended to use.

Example:

```csharp
public class Startup
{
    private readonly IConfiguration _config;

    public void Configure(IApplicationBuilder app)
    {
        // Load custom config settings from whichever provider has been set up:
        var enableLocalhost = _config.GetValue("AllowRequestsFromLocalhost", false);
        var adminIPAddress = _config.GetValue<string>("AdminIPAddress", null);

        // Configure default Firewall rules:
        var firewallRules =
            FirewallRulesEngine.DenyAllAccess();

        // Add rules according to the config:
        if (enableLocalhost)
            firewallRules = firewallRules.ExceptFromLocalhost();

        if (adminIPAddress != null)
            firewallRules = firewallRules.ExceptFromIPAddresses(new [] { IPAddress.Parse(adminIPAddress) });

        // Enable Firewall with the configured rules:
        app.UseFirewall(firewallRules);

        app.Run(async (context) =>
        {
            await context.Response.WriteAsync("Hello World!");
        });
    }
}
```

### Diagnostics

If you're having troubles with Firewall and you want to get more insight into which requests are being blocked by the Firewall then you can turn up the log level to `Debug` and retrieve more diagnostics:

```csharp
// In this example Serilog is used to log to the console,
// but any .NET Core logger will work:
public class Program
{
    public static void Main(string[] args)
    {
        RunWebServer(args);
    }

    public static void RunWebServer(string[] args)
    {
        Log.Logger =
            new LoggerConfiguration()
                .MinimumLevel.Debug()
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

Sample console output when log level is set to `Debug`:

```
Hosting environment: Development
Content root path: /Redacted/Firewall/samples/BasicApp
Now listening on: https://localhost:5001
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
[09:04:31 DBG] Connection id "0HLHNUJVHUQLD" started.
[09:04:31 DBG] Connection id "0HLHNUJVHUQLE" started.
[09:04:31 INF] Request starting HTTP/1.1 GET http://localhost:5000/
[09:04:31 DBG] Wildcard detected, all requests with hosts will be allowed.
[09:04:31 DBG] Firewall.CountryRule: Remote IP Address '::1' has been denied access, because it couldn't be verified against the current GeoIP2 database..
[09:04:31 DBG] Firewall.IPAddressRule: Remote IP Address '::1' has been denied access, because it didn't match any known IP address.
[09:04:31 DBG] Firewall.IPAddressRangeRule: Remote IP Address '::1' has been denied access, because it didn't belong to any known address range.
[09:04:31 DBG] Firewall.IPAddressRule: Remote IP Address '::1' has been denied access, because it didn't match any known IP address.
[09:04:31 DBG] Firewall.IPAddressRangeRule: Remote IP Address '::1' has been denied access, because it didn't belong to any known address range.
[09:04:31 DBG] Firewall.LocalhostRule: Remote IP Address '::1' has been granted access, because it originated on localhost.
[09:04:31 DBG] Connection id "0HLHNUJVHUQLD" completed keep alive response.
[09:04:31 INF] Request finished in 40.3263ms 200
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