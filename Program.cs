using CliWrap;
using Systems_One_SanitizeBarcodes_Service;

// Service metadata
const string ServiceName = "Systems_One_SanitizeBarcode"; // Keep a simple internal name (avoid spaces to prevent SCM issues)
const string ServiceDescription = "Monitors the database and replaces special characters in barcodes with a designated character";

// Simple /Install or /Uninstall handling (adapted from user sample, with fixes)
if (args is { Length: 1 })
{
    try
    {
        // Determine actual executable path (published location). This file IS the service binary.
        string executablePath = Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, AppDomain.CurrentDomain.FriendlyName + ".exe");

        if (args[0].Equals("/Install", StringComparison.OrdinalIgnoreCase))
        {
            // sc.exe requires binPath= "..." with quotes if path has spaces.
            await Cli.Wrap("sc")
                .WithArguments(new[] { "create", ServiceName, $"binPath=\"{executablePath}\"", "start=auto" })
                .ExecuteAsync();

            // Optional description
            await Cli.Wrap("sc")
                .WithArguments(new[] { "description", ServiceName, ServiceDescription })
                .ExecuteAsync();
        }
        else if (args[0].Equals("/Uninstall", StringComparison.OrdinalIgnoreCase))
        {
            // Stop (ignore errors if already stopped / absent)
            try
            {
                await Cli.Wrap("sc")
                    .WithArguments(new[] { "stop", ServiceName })
                    .ExecuteAsync();
            }
            catch { }

            await Cli.Wrap("sc")
                .WithArguments(new[] { "delete", ServiceName })
                .ExecuteAsync();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
    return;
}

var builder = Host.CreateApplicationBuilder(args);

// Enable Windows service integration
builder.Services.AddWindowsService(options => options.ServiceName = ServiceName);

builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection("Database"));
builder.Services.Configure<QueryExecutionOptions>(builder.Configuration.GetSection("QueryExecution"));

builder.Services.AddSingleton<IDbQueryExecutor, SqlDbQueryExecutor>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
