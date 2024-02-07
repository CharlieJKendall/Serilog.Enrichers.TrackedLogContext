using Serilog.Core;
using Serilog.Events;

namespace Serilog.Enrichers.RequestLogContext.Tests.Helpers
{
    internal class ScalarLogEventPropertyFactory : ILogEventPropertyFactory
    {
        public LogEventProperty CreateProperty(string name, object? value, bool destructureObjects = false)
        {
            return new LogEventProperty(name, new ScalarValue(value));
        }
    }
}
