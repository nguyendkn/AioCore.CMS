using AioCore.Domain;
using AioCore.Services;
using AioCore.Shared.Common.Constants;

namespace AioCore.Web.Middlewares;

public class LoadingMiddleware
{
    private readonly RequestDelegate _next;

    public LoadingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context,
        IWebHostEnvironment environment,
        IClientService clientService,
        ILoggerService loggerService)
    {
        var userClient = clientService.GetClient();
        context.Response.Cookies.Append(RequestHeaders.UserClient, userClient,
            new CookieOptions
            {
                SameSite = SameSiteMode.None,
            });
        _ = Task.Run(async () => { await LogPageView(userClient); });
        await _next(context);

        async Task LogPageView(string client)
        {
            await loggerService.LogPageView(client);
        }
    }
}

public static class LoadMiddlewareExtension
{
    public static void UseLoading(this WebApplication application)
    {
        application.UseMiddleware<LoadingMiddleware>();
    }
}