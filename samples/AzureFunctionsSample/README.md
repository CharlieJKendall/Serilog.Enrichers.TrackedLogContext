# Azure Functions Sample

The `HttpFunction` function pushes the `User-Agent` request header value onto the `TrackedLogContext` so all logs emitted further along the request are enriched with this data.

Launching the application and making a `GET api/HttpFunction` request results in the following logs, all of which contain the `UserAgent`:

```
{"@t":"2024-03-01T05:08:04.6282883Z","@m":"C# HTTP trigger function processed a request.", "UserAgent":["Mozilla/5.0","(Windows NT 10.0; Win64; x64)","AppleWebKit/537.36","(KHTML, like Gecko)","Chrome/122.0.0.0","Safari/537.36"]}
{"@t":"2024-03-01T05:08:06.6786984Z","@m":"Oops", "UserAgent":["Mozilla/5.0","(Windows NT 10.0; Win64; x64)","AppleWebKit/537.36","(KHTML, like Gecko)","Chrome/122.0.0.0","Safari/537.36"]}
```