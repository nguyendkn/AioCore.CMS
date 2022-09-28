namespace AioCore.Domain.AggregateModels.CategoryAggregate;

public class Category : MongoDocument
{
    public string Title { get; set; } = default!;

    public List<Guid>? Parents { get; set; }

    public string? Slug { get; set; }

    public string? Thumbnail { get; set; }

    public string? Keywords { get; set; }

    public string? Description { get; set; }

    public string? Source { get; set; }

    public bool Active { get; set; }

    public void Update(string? title, string? description, string? thumbnail, string? keywords, string? slug)
    {
        Title = string.IsNullOrEmpty(title) ? Title : title;
        Description = string.IsNullOrEmpty(title) ? Description : description;
        Thumbnail = string.IsNullOrEmpty(title) ? Thumbnail : thumbnail;
        Keywords = string.IsNullOrEmpty(title) ? Keywords : keywords;
        Slug = string.IsNullOrEmpty(slug) ? Slug : slug;
        Active = true;
    }

    public void Update(List<Guid>? parents)
    {
        Parents = parents ?? default!;
    }
}