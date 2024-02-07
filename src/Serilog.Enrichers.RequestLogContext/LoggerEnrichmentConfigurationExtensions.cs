using Serilog.Configuration;

namespace Serilog
{
    public static class LoggerEnrichmentConfigurationExtensions
    {
        /// <summary>
        /// Enrich log events for requests with properties pushed onto <see cref="RequestLogContext"/>.
        /// </summary>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration FromRequestLogContext(this LoggerEnrichmentConfiguration configuration) =>
            configuration.With<RequestLogContextEnricher>();
    }
}
