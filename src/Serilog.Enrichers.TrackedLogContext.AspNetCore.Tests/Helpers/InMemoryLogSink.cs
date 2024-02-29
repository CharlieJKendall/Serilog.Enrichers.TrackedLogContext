using Serilog.Core;
using Serilog.Events;
using System.Collections.Generic;

namespace Serilog.Enrichers.TrackedLogContext.AspNetCore.Tests.Helpers
{
    internal class InMemoryLogSink : ILogEventSink
    {
        private readonly List<LogEvent> _logEvents = new List<LogEvent>();

        public void Emit(LogEvent logEvent)
        {
            _logEvents.Add(logEvent);
        }

        public IReadOnlyList<LogEvent> LogEvents => _logEvents;
    }
}