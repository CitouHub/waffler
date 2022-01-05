using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Waffler.API.Security
{
    [AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        public static readonly string Name = "X-Api-Key";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(Name, out var extractedSessionKey))
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 401,
                    Content = "Api Key was not provided"
                };
                return;
            }

            if (!Debugger.IsAttached)
            {
                if (!UserSession.IsValid())
                {
                    context.Result = new ContentResult()
                    {
                        StatusCode = 401,
                        Content = "Session expired"
                    };
                    return;
                }

                if (string.IsNullOrEmpty(UserSession.ApiKey) || !UserSession.ApiKey.Equals(extractedSessionKey))
                {
                    context.Result = new ContentResult()
                    {
                        StatusCode = 401,
                        Content = "Api Key is not valid"
                    };
                    return;
                }

                UserSession.Refresh();
            }

            await next();
        }
    }
}
