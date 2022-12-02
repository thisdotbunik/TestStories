using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using TestStories.Common;

using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace TestStories.API
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                             .Enrich.FromLogContext()
                             .WriteTo.Debug()
                             .WriteTo.Console(theme: AnsiConsoleTheme.Grayscale, outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3} {env:j} {app:j} {version:j}] {Message:lj} {Exception}{NewLine}")
                             .Filter.ByExcluding(x => x.Properties.Any(p => p.Value.ToString().Contains("swagger")) || x.Properties.Any(p => p.Value.ToString().Contains("api/documentation")))
                             .Enrich.WithMachineName()
                             .Enrich.WithProperty("env", $"env:{EnvironmentVariables.Env}")
                             .Enrich.WithProperty("app", $"app:{EnvironmentVariables.ServiceName}")
                             .Enrich.WithProperty("version", $"version:{EnvironmentVariables.ServiceVersion}")
                             .ReadFrom.Configuration(Startup.Configuration)
                             .CreateLogger();

                var webHost = WebHost.CreateDefaultBuilder(args)
                                     .UseKestrel(options =>
                                     {
                                        options.AddServerHeader = false;
                                     })
                                     .UseStartup<Startup>()
                                     .UseSerilog()
                                     .Build();

                await webHost.RunAsync();
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Fatal error during application start. Application is terminating...");
            }
        }
    }
}
