using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace BasicApp
{
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
}
