using Serilog.Configuration;

namespace Serilog.Enrichers.TrackedLogContext
{
    /// <summary>
    /// Extension methods for <see cref="LoggerEnrichmentConfiguration"/>
    /// </summary>
    public static class LoggerEnrichmentConfigurationExtensions
    {
        /// <summary>
        /// Enrich log events for requests with properties pushed onto <see cref="TrackedLogContext"/>
        /// </summary>
        /// <returns>Configuration object allowing method chaining</returns>
        public static LoggerConfiguration FromTrackedLogContext(this LoggerEnrichmentConfiguration configuration) =>
            configuration.With<TrackedLogContextEnricher>();
    }
}
