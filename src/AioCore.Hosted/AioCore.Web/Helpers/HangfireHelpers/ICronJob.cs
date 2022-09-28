using Hangfire;

namespace AioCore.Web.Helpers.HangfireHelpers;

[AutomaticRetry(Attempts = 0)]
public interface ICronJob
{
    Task<string> Run();
}