using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Serilog
{
    internal class TrackedLogContextMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                TrackedLogContext.Initialise();
                await next(context);
            }
            finally
            {
                TrackedLogContext.CleanUp();
            }
        }
    }
}
