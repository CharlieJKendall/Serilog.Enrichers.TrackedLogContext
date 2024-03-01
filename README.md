# Serilog.Enrichers.TrackedLogContext 🌟

[![CI](https://github.com/CharlieJKendall/Serilog.Enrichers.TrackedLogContext/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/CharlieJKendall/Serilog.Enrichers.TrackedLogContext/actions/workflows/ci.yml)

## What does it do?

Use the `TrackedLogContext` to build up a bank of useful properties throughout the scope of a request that automatically enrich logs emitted. Allows powerful log correlation and analysis with minimal overhead!

``` cs
[HttpGet("{id}")]
public IActionResult GetBook(int id)
{
    TrackedLogContext.PushProperty("BookId", id);

    var book = _GetBookFromDatabase(id);
    if (book == null)
    {
        // Enriched with: BookId, BookError
        _logger.Warning("Could not find book :(");
        return NotFound();
    }
    
    // Enriched with: BookId, BookIsbn
    _logger.LogInformation("Found book!");
    return book;
}

private Book _GetBookFromDatabase(int id)
{
    // Enriched with: BookId
    _logger.LogInformation("Trying to find book...");

    try
    {
        var book = ...
    }
    catch (BookException ex)
    {
        TrackedLogContext.PushProperty("BookError", ex.Error);

        // Enriched with: BookId, BookError
        _logger.LogError(ex, "Error trying to find book");
        return null;
    }

    TrackedLogContext.PushProperty("BookIsbn", book.Isbn);

    return book;
}
```

## Getting Started

You'll need to add one of the following NuGet packages, depending on the type of application that you are building:

- ASP.NET Web Application 👉 [Serilog.Enrichers.TrackedLogContext.AspNetCore](https://www.nuget.org/packages/Serilog.Enrichers.TrackedLogContext.AspNetCore)
- Azure Functions Application 👉 [Serilog.Enrichers.TrackedLogContext.AzureFunctions](https://www.nuget.org/packages/Serilog.Enrichers.TrackedLogContext.AzureFunctions)
- Class library 👉 [Serilog.Enrichers.TrackedLogContext](https://www.nuget.org/packages/Serilog.Enrichers.TrackedLogContext)

As with all Serilog enrichers, we need set the logger up use it. There are a few ways to set this up which are [covered in more detail in the Serilog docs](https://github.com/serilog/serilog/wiki/Configuration-Basics#enrichers). We're going to go the simplest route and configure it in code at the point that we create our `Logger`:

``` cs
Log.Logger = new LoggerConfiguration()
    .Enrich.FromTrackedLogContext() // Add this line to enrich logs from the TrackedLogContext
    .CreateLogger();
```

Next up, register the middleware in the application pipeline. This should be registered above any other pipeline components that emit logs and would benefit from being enriched with properties associated with the `TrackedLogContext`. In simple terms, this means you should add it to the pipeline before most other registrations.

``` cs
// Add the middleware to the top of your pipeline
app.UseTrackedLogContext();

// other middleware ...

app.UseExceptionHandler(); 
app.UseAuthorization();
app.MapControllers();

// etc.
```

Aaaaaaaaand... that's pretty much it. You can now push properties onto the `TrackedLogContext` from around your application and all further logs emitted from the same async scope will be enriched with them!

## Why do I need this?

It is not uncommon for useful information to become available further down a call stack than the logs that would benefit from being enriched with it. A common example of this is exception handling middleware, for example we might want an error log to be enriched with information about the particular resource that was being requested:

``` cs
// Setup logger to enrich from TrackedLogContext
Log.Logger = new LoggerConfiguration()
    .Enrich.FromTrackedLogContext()
    .CreateLogger();

// Add TrackedLogContext middleware
app.UseTrackedLogContext();

app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        Log.Error(ex); // This log is enriched with the TodoId property 🥳
    }
});

app.MapGet("/todos/{id}", (string id) =>
{
    // Push TodoId onto the TrackedLogContext
    TrackedLogContext.PushProperty("TodoId", id);
    throw new Exception("Oops");
});
```

The above is slightly contrived, however the point still stands: it can be valuable to ensure all further logs emitted from within the scope of a request are enriched with certain pieces of data. Think of it like `LogContext.PushProperty`, but for the scope of the entire request instead of the `IDisposable` lifetime 💅

## Samples

There are some minimal code samples for Azure Functions and Web API projects in the repository [here](samples)
