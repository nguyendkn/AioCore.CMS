namespace AioCore.Domain.AggregateModels.EventsAggregate;

public class PageView : MongoDocument
{
    public string UserClient { get; set; } = default!;

    public string Host { get; set; } = default!;

    public long Onsite { get; set; }

    public string IP { get; set; } = default!;

    public string Url { get; set; } = default!;

    public string UserAgent { get; set; } = default!;

    public DateTime Timestamp { get; set; }
}