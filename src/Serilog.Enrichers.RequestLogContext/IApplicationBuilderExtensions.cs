using Microsoft.AspNetCore.Builder;

namespace Serilog
{
    public static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// Add <see cref="RequestLogContextMiddleware"/> to the middleware pipeline. This should be added
        /// to the pipeline higher up than any code that calls <see cref="RequestLogContext.PushProperty(string, object?)"/>
        /// </summary>
        public static IApplicationBuilder UseLogRequstContext(this IApplicationBuilder app) =>
            app.UseMiddleware<RequestLogContextMiddleware>();
    }
}
