using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace BasicApp
{
    public static class Program
    {
        public static void Main(string[] args) =>
            RunWebServer(args);

        private static void RunWebServer(string[] args)
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
