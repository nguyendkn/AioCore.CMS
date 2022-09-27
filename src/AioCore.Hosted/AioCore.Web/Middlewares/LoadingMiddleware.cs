namespace AioCore.Web.Middlewares;

public class LoadingMiddleware
{
    private readonly RequestDelegate _next;

    public LoadingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        await _next(context);
    }
}

public static class LoadMiddlewareExtension
{
    public static void UseLoading(this WebApplication application)
    {
        application.UseMiddleware<LoadingMiddleware>();
    }
}