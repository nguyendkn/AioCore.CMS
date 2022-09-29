using System.Collections.Concurrent;
using AioCore.Domain.AggregateModels.CategoryAggregate;
using AioCore.Shared.Extensions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Polly;
using Polly.Retry;

namespace AioCore.Domain;

public class AioCoreContextSeed
{
    public static async Task SeedAsync(AioCoreContext context,
        ILogger<AioCoreContextSeed>? logger)
    {
        var policy = CreatePolicy(logger, nameof(AioCoreContextSeed));

        await policy.ExecuteAsync(async () =>
        {
            var categories = await context.Categories.ToListAsync();
            var posts = await context.Posts.ToListAsync();
            if (categories.Any())
            {
                var concurrentDictionary = categories.Where(x => x.Active && !string.IsNullOrEmpty(x.Slug))
                    .Select(x => new KeyValuePair<string, Category>(x.Slug!, x)).DistinctBy(x => x.Key);
                AioCoreContext.CachedCategories = new ConcurrentDictionary<string, Category>(concurrentDictionary);
            }

            if (posts.Any())
            {
                var concurrentDictionary = posts.Where(x => x.Active && !string.IsNullOrEmpty(x.Slug))
                    .Select(x => new KeyValuePair<string, Post>(x.Slug!, x)).DistinctBy(x => x.Key);
                AioCoreContext.CachedPosts = new ConcurrentDictionary<string, Post>(concurrentDictionary);
            }
        });
    }

    private static AsyncRetryPolicy CreatePolicy(ILogger? logger, string prefix, int retries = 3)
    {
        return Policy.Handle<MongoException>().WaitAndRetryAsync(
            retryCount: retries,
            sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
            onRetry: (exception, timeSpan, retry, ctx) =>
            {
                logger?.LogWarning(exception,
                    "[{Prefix}] Exception {ExceptionType} with message {Message} detected on attempt {Retry} of {Retries}",
                    prefix, exception.GetType().Name, exception.Message, retry, retries);
            }
        );
    }
}