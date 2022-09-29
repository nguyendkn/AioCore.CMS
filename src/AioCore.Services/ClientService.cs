using System.Web;
using AioCore.Shared.Common.Constants;
using AioCore.Shared.Common.Enums;
using AioCore.Shared.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace AioCore.Services;

public interface IClientService
{
    DeviceType GetDeviceType();

    bool Logged();

    string GetClient();

    string GetHost();

    string GetIP();

    string? GetUrl();

    string? GetScheme();

    string? GetUserAgent();
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

    public bool Logged()
    {
        var host = GetUrl();
        if (string.IsNullOrEmpty(host)) return false;
        var ignoreUrl = new List<string>
        {
            "/_Host",
            "/_blazor",
            "/_blazor/negotiate",
            "/_blazor/initializers",
            "/_blazor/disconnect"
        };
        return !ignoreUrl.Contains(host);
    }

    public string GetClient()
    {
        var client = _httpContextAccessor.HttpContext?.Request.Cookies[RequestHeaders.UserClient];
        return client.ParseGuid() ? client! : Guid.NewGuid().ToString();
    }

    public string GetHost()
    {
        var path = _httpContextAccessor.HttpContext?.Request.GetDisplayUrl();
        var urlDecode = HttpUtility.UrlDecode(path);
        if (string.IsNullOrEmpty(urlDecode)) return default!;
        var uri = new Uri(urlDecode);
        return uri.Host;
    }

    public string GetIP()
    {
        var ip = _httpContextAccessor.HttpContext?.Request.Headers[RequestHeaders.XForwardedFor];
        return ip.ToString() ?? string.Empty;
    }

    public string? GetUrl()
    {
        return _httpContextAccessor.HttpContext?.Request.Path;
    }

    public string? GetScheme()
    {
        return _httpContextAccessor.HttpContext?.Request.Scheme;
    }

    public string? GetUserAgent()
    {
        return _httpContextAccessor.HttpContext?.Request.Headers[RequestHeaders.UserAgent];
    }
}