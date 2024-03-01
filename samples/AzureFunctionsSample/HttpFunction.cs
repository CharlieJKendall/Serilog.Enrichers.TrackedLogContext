using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Serilog;

namespace AzureFunctionsSample
{
    public class HttpFunction
    {
        private readonly ILogger<HttpFunction> _logger;

        public HttpFunction(ILogger<HttpFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(HttpFunction))]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData request)
        {
            if (request.Headers.TryGetValues("User-Agent", out var userAgent))
            {
                TrackedLogContext.PushProperty("UserAgent", userAgent);
            }

            // Enriched with 'UserAgent' if the header is present
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            throw new Exception("Oops");
        }
    }
}
