namespace AioCore.Web.Helpers.HangfireHelpers;

public class HangfireJob
{
    public string Id { get; set; } = default!;

    public string Title { get; set; } = default!;

    public string Detail { get; set; } = default!;

    public string CronExpression { get; set; } = default!;
}