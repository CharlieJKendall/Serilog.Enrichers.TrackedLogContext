namespace Serilog
{
    /// <summary>
    /// A container for a name and value of a property tracked by the <see cref="TrackedLogContext"/>
    /// </summary>
    public class TrackedLogContextProperty
    {
        /// <summary>
        /// Initialises a new instance of <see cref="TrackedLogContextProperty"/>
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="value">The value of the property</param>
        public TrackedLogContextProperty(string name, object? value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// The name of the property
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The value of the property
        /// </summary>
        public object? Value { get; }
    }
}
