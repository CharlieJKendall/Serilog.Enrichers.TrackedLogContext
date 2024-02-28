# Serilog.Enrichers.RequestLogContext

### Why do I need this?

It is not uncommon for useful debugging information to become available further down the request pipeline than the logs that would benefit from being enriched with it. An example would be exception handling middleware:

``` cs
app.Use(async (context, requestDelegate) =>
{
    try
    {
        await requestDelegate(context);
    }
    catch (Exception ex)
    {
        Log.Error(ex); // This log is enriched with TodoId
    }
});

app.MapGet("/todos/{id}", (string id) =>
{
    RequestLogContext.PushProperty("TodoId", id);
    throw new Exception("Oops");
});
```

The above is a pretty contrived example, however the point still stands: it can be very valuable to ensure all further logs emitted from within the scope of a request are enriched with certain pieces of data.

### Usage ideas and examples

#### Common header values

``` cs
app.Use(async (context, requestDelegate) =>
{
    if (context.Request.Headers.TryGetValue("SessionId", out var sessionId))
    {
        RequestLogContext.PushProperty("SessionId", sessionId.ToString());
    }

    await requestDelegate(context);
});
```

### User information

``` cs
app.Use(async (context, requestDelegate) =>
{
    if (context.User.FindFirst(ClaimTypes.NameIdentifier) is Claim nameIdentifier)
    {
        RequestLogContext.PushProperty("UserId", nameIdentifier.Value);
    }

    await requestDelegate(context);
});
```

### Data retrieved from a downstream provider (e.g. database, REST API)

``` cs
public async Task<Item> GetItemDetails(string itemId)
{
    var client = _httpClientFactory.CreateClient();
    var itemDetails = await client.GetFromJsonAsync<Item>($"api/item-details/{itemId}");
    RequestLogContext.PushProperty("ItemOrderId", itemDetails.OrderId);
    return itemDetails;
}
```