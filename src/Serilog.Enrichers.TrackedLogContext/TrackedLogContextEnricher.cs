using Serilog.Core;
using Serilog.Events;
using System;

namespace Serilog
{
    internal class TrackedLogContextEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent == null)
            {
                throw new ArgumentNullException(nameof(logEvent));
            }

            if (propertyFactory == null)
            {
                throw new ArgumentNullException(nameof(propertyFactory));
            }

            foreach (var property in TrackedLogContext.GetTrackedProperties())
            {
                var logEventProperty = propertyFactory.CreateProperty(property.Name, property.Value);
                logEvent.AddPropertyIfAbsent(logEventProperty);
            }
        }
    }
}
