using AioCore.Domain;
using AioCore.Web.Helpers.HangfireHelpers;
using System.Collections.Concurrent;
using AioCore.Domain.AggregateModels.CategoryAggregate;
using AioCore.Domain.AggregateModels.PostAggregate;

namespace AioCore.Web.Jobs.SyncCacheJob;

public class SyncCacheJob : ICronJob
{
    private readonly AioCoreContext _context;

    public SyncCacheJob(AioCoreContext context)
    {
        _context = context;
    }

    public async Task<string> Run()
    {
        try
        {
            var categories = await _context.Categories.ToListAsync();
            var concurrentCategoriesDictionary = categories.Where(x => x.Active && !string.IsNullOrEmpty(x.Slug))
                .Select(x => new KeyValuePair<string, Category>(x.Slug!, x)).DistinctBy(x => x.Key);
            AioCoreContext.CachedCategories =
                new ConcurrentDictionary<string, Category>(concurrentCategoriesDictionary);

            var posts = await _context.Posts.ToListAsync();
            var concurrentPostsDictionary = posts.Where(x => x.Active && !string.IsNullOrEmpty(x.Slug))
                .Select(x => new KeyValuePair<string, Post>(x.Slug!, x)).DistinctBy(x => x.Key);
            AioCoreContext.CachedPosts = new ConcurrentDictionary<string, Post>(concurrentPostsDictionary);

            return "OK";
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
}