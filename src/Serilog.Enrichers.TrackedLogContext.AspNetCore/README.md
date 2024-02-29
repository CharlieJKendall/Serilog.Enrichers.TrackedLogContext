# Serilog.Enrichers.TrackedLogContext.AspNetCore

This package adds the ability to enrich logs within request scope for an ASP.NET web application using the TrackedLogContext.

To start enriching logs, ensure that the enricher is configured and the middleware has been added:

``` cs
// Setup logger to enrich from TrackedLogContext
Log.Logger = new LoggerConfiguration()
    .Enrich.FromTrackedLogContext()
    .CreateLogger();

// Ensure middleware is added to the request pipeline
app.UseTrackedLogContext();
```