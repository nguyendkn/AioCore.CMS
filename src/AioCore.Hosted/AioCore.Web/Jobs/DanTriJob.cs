using AioCore.Web.Helpers.HangfireHelpers;

namespace AioCore.Web.Jobs;

public class DanTriJob : ICronJob
{
    public Task<string> Run()
    {
        return Task.FromResult("OK");
    }
}