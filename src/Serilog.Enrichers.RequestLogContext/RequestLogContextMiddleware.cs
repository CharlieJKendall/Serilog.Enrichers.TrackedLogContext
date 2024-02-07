using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Serilog
{
    internal class RequestLogContextMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                RequestLogContextEnricher.Initialise();
                await next(context);
            }
            finally
            {
                RequestLogContextEnricher.CleanUp();
            }
        }
    }
}
