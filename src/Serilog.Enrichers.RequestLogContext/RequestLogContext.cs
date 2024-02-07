namespace Serilog
{
    public static class RequestLogContext
    {
        /// <summary>
        /// Push a property and value onto the <see cref="RequestLogContext"/> so that all
        /// subsequent logs emitted during the scope of this request are enriched with it
        /// </summary>
        /// 
        /// <param name="name">The name of the property</param>
        /// <param name="value">The value of the property</param>
        /// 
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="name"/> is <code>null</code>
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="name"/> only consists of white space characters
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        ///     <see cref="PushProperty(string, object?)"/> is called before the
        ///     <see cref="RequestLogContextMiddleware"/> has been added to the middleware pipeline
        /// </exception>
        public static void PushProperty(string name, object? value) =>
            RequestLogContextEnricher.PushProperty(name, value);
    }
}