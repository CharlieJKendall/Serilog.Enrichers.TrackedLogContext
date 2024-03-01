# Web API Sample

The `GET WeatherForecast/{id}` endpoint pushes the `id` route parameter onto the `TrackedLogContext` so all logs emitted further along the request are enriched with this data.

Launching the application and making a `GET WeatherForecast/1` request results in the following logs, all of which contain the `WeatherForecastId`:

```
{"@t":"2024-03-01T04:55:06.1740191Z","@m":"Hello from the weather forecast controller","WeatherForecastId":1}
{"@t":"2024-03-01T04:55:07.6719497Z","@m":"Executed action \"WebApiSample.Controllers.WeatherForecastController.Get (WebApiSample)\" in 1524.2693ms","WeatherForecastId":1}
{"@t":"2024-03-01T04:55:07.6733171Z","@m":"Executed endpoint '\"WebApiSample.Controllers.WeatherForecastController.Get (WebApiSample)\"'","WeatherForecastId":1}
{"@t":"2024-03-01T04:55:07.6998640Z","@m":"Unhandled exception in request","WeatherForecastId":1}
```