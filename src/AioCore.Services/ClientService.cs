using AioCore.Shared.Common.Constants;
using AioCore.Shared.Common.Enums;
using AioCore.Shared.Extensions;
using Microsoft.AspNetCore.Http;

namespace AioCore.Services;

public interface IClientService
{
    DeviceType GetDeviceType();

    string GetClient();
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
        var userAgent = _httpContextAccessor.HttpContext?.Request.Headers[RequestHeaders.UserAgent];
        return userAgent.ToString()!.ToLower().Contains("mobile") ? DeviceType.Mobile : DeviceType.Desktop;
    }

    public string GetClient()
    {
        var client = _httpContextAccessor.HttpContext?.Request.Cookies[RequestHeaders.UserClient];
        return client.ParseGuid() ? client! : Guid.NewGuid().ToString();
    }

    public string GetIP()
    {
        var ip = _httpContextAccessor.HttpContext?.Request.Headers[RequestHeaders.XForwardedFor];
        return ip.ToString() ?? string.Empty;
    }
}