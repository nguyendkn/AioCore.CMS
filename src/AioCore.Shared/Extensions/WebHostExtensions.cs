﻿using AioCore.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Polly;

namespace AioCore.Shared.Extensions;

public static class WebHostExtensions
{
    public static WebApplication MigrateDatabase<TContext>(this WebApplication app,
        Action<TContext, IServiceProvider> seeder)
        where TContext : MongoContext
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<TContext>>();
        var context = services.GetService<TContext>();

        try
        {
            logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

            const int retries = 10;
            var retry = Policy.Handle<MongoException>()
                .WaitAndRetry(
                    retryCount: retries,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retry, ctx) =>
                    {
                        logger.LogWarning(exception,
                            "[{Prefix}] Exception {ExceptionType} with message {Message} detected on attempt {Retry} of {Retries}",
                            nameof(TContext), exception.GetType().Name, exception.Message, retry, retries);
                    });

            retry.Execute(() => InvokeSeeder(seeder!, context, services));

            logger.LogInformation("Migrated database associated with context {DbContextName}", typeof(TContext).Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}",
                typeof(TContext).Name);
        }

        return app;
    }

    private static void InvokeSeeder<TContext>(Action<TContext, IServiceProvider> seeder,
        TContext context, IServiceProvider services)
        where TContext : MongoContext?
    {
        seeder(context, services);
    }
}