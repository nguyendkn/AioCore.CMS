using System.Collections.Concurrent;
using AioCore.Domain.AggregateModels.CategoryAggregate;
using AioCore.Domain.AggregateModels.EventsAggregate;

namespace AioCore.Domain;

public class AioCoreContext : MongoContext
{
    public AioCoreContext(IMongoContextBuilder modelBuilder) : base(modelBuilder)
    {
    }

    public MongoSet<Category> Categories { get; set; } = default!;

    public static ConcurrentDictionary<string, Category> CachedCategories { get; set; } = new();

    public MongoSet<Post> Posts { get; set; } = default!;

    public static ConcurrentDictionary<string, Post> CachedPosts { get; set; } = new();

    public MongoSet<PageView> PageViews { get; set; } = default!;
}