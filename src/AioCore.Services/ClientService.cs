using AioCore.Shared.Common;
using Microsoft.AspNetCore.Http;

namespace AioCore.Services;

public interface IClientService
{
    DeviceType GetDeviceType();
}

public class ClientService : IClientService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClientService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DeviceType GetDeviceType()
    {
        var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"];
        return userAgent.ToString()!.ToLower().Contains("mobile") ? DeviceType.Mobile : DeviceType.Desktop;
    }
}