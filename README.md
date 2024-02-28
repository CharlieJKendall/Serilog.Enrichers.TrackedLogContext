# Serilog.Enrichers.TrackedLogContext 🌟

### Why do I need this?

It is not uncommon for useful information to become available further down the call stack than the logs that would benefit from being enriched with it. A common example of this is exception handling middleware - we might want an error log to be enriched with information about the particular resource that was being requested:

``` cs
// Setup logger to enrich from TrackedLogContext
Log.Logger = new LoggerConfiguration()
    .Enrich.FromTrackedLogContext()
    .CreateLogger();

app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        Log.Error(ex); // This log is enriched with TodoId
    }
});

app.MapGet("/todos/{id}", (string id) =>
{
    // Push TodoId onto the TrackedLogContext
    RequestLogContext.PushProperty("TodoId", id);
    throw new Exception("Oops");
});
```

The above is a slightly contrived example, however the point still stands: it can be valuable to ensure all further logs emitted from within the scope of a request are enriched with certain pieces of data. Think of it like `LogContext.PushProperty`, but for the scope of the entire request instead of the `IDisposable` lifetime 💅