namespace AioCore.Domain.AggregateModels.PostAggregate;

public class Post : MongoDocument
{
    public string Title { get; set; } = default!;

    public Guid Parent { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ModifiedAt { get; set; }

    public string? Content { get; set; }
}