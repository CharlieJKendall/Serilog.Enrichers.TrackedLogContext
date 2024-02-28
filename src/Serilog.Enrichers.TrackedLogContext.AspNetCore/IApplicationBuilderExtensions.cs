using Microsoft.AspNetCore.Builder;

namespace Serilog
{
    public static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// Add <see cref="TrackedLogContextMiddleware"/> to the middleware pipeline. This should be added
        /// to the pipeline higher up than any code that calls <see cref="TrackedLogContext.PushProperty(string, object?)"/>
        /// </summary>
        public static IApplicationBuilder UseTrackedLogContext(this IApplicationBuilder app) =>
            app.UseMiddleware<TrackedLogContextMiddleware>();
    }
}
