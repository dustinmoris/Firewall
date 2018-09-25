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

IP address filtering can be a useful feature to add an extra layer of security to a publicly exposed API or to force all API access through a certain set of proxy servers (e.g. [Cloudflare](https://www.cloudflare.com/)).

## Using with Cloudflare

[Cloudflare](https://www.cloudflare.com/) is a popular service which provides enhanced performance and security features to any website. It is currently used by more than 8 million websites world wide and requires no additional hardware or software as it operates at the DNS level.



## License

[Apache 2.0](https://raw.githubusercontent.com/dustinmoris/Firewall/master/LICENSE)

## Credits

Logo is made by [Smashicons](https://www.flaticon.com/authors/smashicons) from [Flaticon](https://www.flaticon.com/) and is licensed under [Creative Commons 3.0](http://creativecommons.org/licenses/by/3.0/).