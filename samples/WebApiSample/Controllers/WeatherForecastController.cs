using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace WebApiSample.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet("{id}")]
    public ActionResult<WeatherForecast> Get(int id)
    {
        TrackedLogContext.PushProperty("WeatherForecastId", id);

        // Enriched with 'WeatherForecastId'
        _logger.LogInformation("Hello from the weather forecast controller");

        throw new Exception("Oops");
    }
}
