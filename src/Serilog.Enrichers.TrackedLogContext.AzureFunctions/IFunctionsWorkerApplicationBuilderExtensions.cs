using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;

namespace Serilog
{
    public static class IFunctionsWorkerApplicationBuilderExtensions
    {
        /// <summary>
        /// Add <see cref="TrackedLogContextMiddleware"/> to the middleware pipeline. This should be added
        /// to the pipeline higher up than any code that calls <see cref="TrackedLogContext.PushProperty(string, object?)"/>
        /// </summary>
        public static IFunctionsWorkerApplicationBuilder UseTrackedLogContext(this IFunctionsWorkerApplicationBuilder app) =>
            app.UseMiddleware<TrackedLogContextMiddleware>();
    }
}
