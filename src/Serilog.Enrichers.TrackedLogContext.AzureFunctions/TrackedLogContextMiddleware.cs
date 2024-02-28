using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using System.Threading.Tasks;

namespace Serilog
{
    internal class TrackedLogContextMiddleware : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
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
