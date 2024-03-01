using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromTrackedLogContext()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateLogger();

new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(applicationBuilder =>
    {
        // Add TrackedLogContext middleware
        applicationBuilder.UseTrackedLogContext();
        applicationBuilder.UseMiddleware(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                // Enriched with 'UserAgent' from HttpFunction
                context.GetLogger("Middleware").LogError(ex, "Oops");
            }
        });
    })
    .ConfigureLogging(loggingBuilder => loggingBuilder.AddSerilog())
    .Build()
    .Run();
