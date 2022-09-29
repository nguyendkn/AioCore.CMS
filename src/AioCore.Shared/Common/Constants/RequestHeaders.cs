using System.Collections.Concurrent;

namespace AioCore.Shared.Common.Constants;

public static class RequestHeaders
{
    public const string UserAgent = "User-Agent";
    
    public const string UserClient = "User-Client";

    public const string XForwardedFor = "X-Forwarded-For";
}