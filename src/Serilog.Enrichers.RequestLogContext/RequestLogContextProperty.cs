namespace Serilog
{
    internal class RequestLogContextProperty
    {
        public RequestLogContextProperty(string name, object? value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public object? Value { get; }
    }
}
