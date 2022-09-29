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
        IClientService clientService)
    {
        var userClient = clientService.GetClient();
        context.Response.Cookies.Append(RequestHeaders.UserClient, userClient,
            new CookieOptions
            {
                SameSite = SameSiteMode.None,
                Secure = environment.IsProduction()
            });
        _ = Task.Run(async () => { await LogPageView(userClient); });
        await _next(context);
    }

    private async Task LogPageView(string userClient)
    {
    }
}

public static class LoadMiddlewareExtension
{
    public static void UseLoading(this WebApplication application)
    {
        application.UseMiddleware<LoadingMiddleware>();
    }
}