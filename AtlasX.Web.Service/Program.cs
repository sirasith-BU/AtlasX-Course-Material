using AtlasX.Engine.RemoteDirectory;
using AtlasX.Engine.RemoteDirectory.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System;
using System.IO;

namespace AtlasX.Web.Service;

public class Program
{
    private static string ASPNETCORE_ENVIRONMENT => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

    public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{ASPNETCORE_ENVIRONMENT}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();

    public static void Main(string[] args)
    {
        LoggerInit();

        try
        {
            Log.Information("Getting the AtlasX Web Service running...");
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.Sources.Clear();

                config.AddConfiguration(Configuration);

                if (args != null)
                {
                    config.AddCommandLine(args);
                }
            })
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
            .UseSerilog();
    }

    private static void LoggerInit()
    {
        string fileSourceName = Configuration.GetSection("Logging").GetValue<string>("FileSource");
        string rootPath = Configuration.GetSection("Logging").GetValue<string>("RootPath");

        FileSource fileSource = Configuration.GetSection($"WebServiceSettings:FileServer:FileSource:{fileSourceName}")
            .Get<FileSource>();
        RemoteConnector remoteConnector = new RemoteConnector(
            fileSource.RemotePath
            , fileSource.Username
            , fileSource.Password
            , fileSource.Domain
        );
        DirectoryAccess dirAccess = remoteConnector.Connect();
        dirAccess.PathName = rootPath;

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Logger(logger => logger.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
                .WriteTo.File(new CompactJsonFormatter(), $@"{dirAccess.DestinationPath}/Information/log.ndjson",
                    rollingInterval: RollingInterval.Month))
            .WriteTo.Logger(logger => logger.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
                .WriteTo.File(new CompactJsonFormatter(), $@"{dirAccess.DestinationPath}/Warning/log.ndjson",
                    rollingInterval: RollingInterval.Month))
            .WriteTo.Logger(logger => logger.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error)
                .WriteTo.File(new CompactJsonFormatter(), $@"{dirAccess.DestinationPath}/Error/log.ndjson",
                    rollingInterval: RollingInterval.Month))
            .WriteTo.Logger(logger => logger.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Fatal)
                .WriteTo.File(new CompactJsonFormatter(), $@"{dirAccess.DestinationPath}/Fatal/log.ndjson",
                    rollingInterval: RollingInterval.Month))
            .CreateLogger();

        Log.Information($"The output logging directory is: {dirAccess.DestinationPath}");
    }
}