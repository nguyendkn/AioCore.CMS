namespace AioCore.Domain.AggregateModels.CategoryAggregate;

public class Category : MongoDocument
{
    public string Title { get; set; } = default!;

    public Guid Parent { get; set; }

    public string Slug { get; set; } = default!;

    public string Thumbnail { get; set; } = default!;

    public string? Keywords { get; set; }

    public string? Description { get; set; }

    public string? Source { get; set; }

    public void Update(string title, string description, string thumbnail, string keywords)
    {
        Title = string.IsNullOrEmpty(title) ? Title : title;
        Description = string.IsNullOrEmpty(title) ? Description : description;
        Thumbnail = string.IsNullOrEmpty(title) ? Thumbnail : thumbnail;
        Keywords = string.IsNullOrEmpty(title) ? Keywords : keywords;
    }
}