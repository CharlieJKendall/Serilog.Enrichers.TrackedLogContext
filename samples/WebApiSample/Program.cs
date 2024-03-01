using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromTrackedLogContext()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSerilog();
builder.Services.AddControllers();

var app = builder.Build();

// Add TrackedLogContext middleware
app.UseTrackedLogContext();
app.Use(async (httpContext, next) =>
{
    try
    {
        await next(httpContext);
    }
    catch (Exception ex)
    {
        // Enriched with 'WeatherForecastId' from endpoint
        Log.Error(ex, "Unhandled exception in request");
    }
});

app.MapControllers();

app.Run();
