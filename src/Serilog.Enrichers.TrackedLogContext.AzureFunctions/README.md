# Serilog.Enrichers.TrackedLogContext.AzureFunctions

This package adds the ability to enrich logs within Azure Functions scopes using the TrackedLogContext.

To start enriching logs, ensure that the enricher is configured and the middleware has been added:

``` cs
// Setup logger to enrich from TrackedLogContext
Log.Logger = new LoggerConfiguration()
    .Enrich.FromTrackedLogContext()
    .CreateLogger();

// Ensure middleware is added to the request pipeline
app.UseTrackedLogContext();
```