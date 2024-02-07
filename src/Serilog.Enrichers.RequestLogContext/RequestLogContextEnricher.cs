using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Serilog
{
    internal class RequestLogContextEnricher : ILogEventEnricher
    {
        public static readonly AsyncLocal<ConcurrentBag<RequestLogContextProperty>?> TrackedProperties = new AsyncLocal<ConcurrentBag<RequestLogContextProperty>?>();

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

            var trackedProperties = TrackedProperties.Value;
            if (trackedProperties == null)
            {
                return;
            }

            foreach (var tracked in trackedProperties)
            {
                var logEventProperty = propertyFactory.CreateProperty(tracked.Name, tracked.Value);
                logEvent.AddPropertyIfAbsent(logEventProperty);
            }
        }

        public static void Initialise()
        {
            TrackedProperties.Value = new ConcurrentBag<RequestLogContextProperty>();
        }
        
        public static void CleanUp()
        {
            TrackedProperties.Value = null!;
        }

        public static void PushProperty(string name, object? value)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Property name must not only contain white space characters",
                    nameof(name));
            }

            var tracker = TrackedProperties.Value;
            if (tracker == null)
            {
                throw new InvalidOperationException(
                    "Can not push property onto RequestLogContext before RequestLogContextMiddleware");
            }

            tracker.Add(new RequestLogContextProperty(name, value));
        }
    }
}
