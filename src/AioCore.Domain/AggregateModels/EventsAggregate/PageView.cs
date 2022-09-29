namespace AioCore.Domain.AggregateModels.EventsAggregate;

public class PageView : MongoDocument
{
    public string UserClient { get; set; } = default!;

    public string Host { get; set; } = default!;

    public string IP { get; set; } = default!;

    public string? Url { get; set; }

    public string? UserAgent { get; set; }

    public DateTime Timestamp { get; set; }

    public string? TimestampShort { get; set; }

    public string? Scheme { get; set; }
}