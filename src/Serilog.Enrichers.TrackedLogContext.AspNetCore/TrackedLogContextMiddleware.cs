using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Serilog
{
    internal class TrackedLogContextMiddleware
    {
        private readonly RequestDelegate _next;

        public TrackedLogContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                TrackedLogContext.Initialise();
                await _next(context);
            }
            finally
            {
                TrackedLogContext.CleanUp();
            }
        }
    }
}
