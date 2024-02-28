using Serilog.Events;
using Serilog.Parsing;
using System;

namespace Serilog.Enrichers.TrackedLogContext.Tests.Helpers
{
    internal static class LogEventFactory
    {
        public static LogEvent CreateEmpty() => new LogEvent(
            timestamp: DateTimeOffset.UtcNow,
            level: LogEventLevel.Information,
            exception: null,
            messageTemplate: new MessageTemplate(Array.Empty<MessageTemplateToken>()),
            properties: Array.Empty<LogEventProperty>());

        public static LogEvent CreateWithScalarValue(string name, object value)
        {
            var logEvent = CreateEmpty();
            logEvent.AddPropertyIfAbsent(new LogEventProperty(name, new ScalarValue(value)));
            return logEvent;
        }
    }
}
