using Microsoft.Extensions.DependencyInjection;

namespace AioCore.Services;

public static class Register
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddSingleton<IClientService, ClientService>();
    }
}