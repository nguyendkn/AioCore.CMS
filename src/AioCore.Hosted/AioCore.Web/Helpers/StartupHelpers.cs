using System.Reflection;
using AioCore.Mongo;
using AioCore.Web.Helpers.HangfireHelpers;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using HangfireBasicAuthenticationFilter;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;

namespace AioCore.Web.Helpers;

public static class StartupHelpers
{
    public static void AddHangfireServer(this IServiceCollection services, MongoConfigs mongoConfigs)
    {
        var connectionString = $"{mongoConfigs.Host}:{mongoConfigs.Port}";
        var mongoUrlBuilder = new MongoUrlBuilder(connectionString)
        {
            DatabaseName = mongoConfigs.Database,
        };
        var mongoClient = new MongoClient(mongoUrlBuilder.ToMongoUrl());
        var mongoMigrationOptions = new MongoMigrationOptions
        {
            MigrationStrategy = new MigrateMongoMigrationStrategy(),
            BackupStrategy = new CollectionMongoBackupStrategy()
        };
        var mongoStorageOptions = new MongoStorageOptions
        {
            MigrationOptions = mongoMigrationOptions,
            Prefix = "hangfire",
            CheckConnection = true,
            CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection
        };

        services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseMongoStorage(mongoClient, mongoUrlBuilder.DatabaseName, mongoStorageOptions)
        );
        JobStorage.Current = new MongoStorage(mongoClient, mongoConfigs.Database, mongoStorageOptions);
        services.AddHangfireServer();
    }

    public static void UseHangfire(this WebApplication app)
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            DashboardTitle = "Hangfire",
            Authorization = new[]
            {
                new HangfireCustomBasicAuthenticationFilter
                {
                    User = "admin",
                    Pass = "admin"
                }
            },
            IgnoreAntiforgeryToken = true
        });
    }

    public static void UseJobs(this WebApplication app, IHostEnvironment environment)
    {
        using var scope = app.Services.CreateScope();

        var jobs = scope.ServiceProvider.GetServices<ICronJob>();
        var scheduleTasks = HangfireJobs(environment).ToDictionary(t => t.Id);

        foreach (var recurringJob in jobs)
        {
            var name = recurringJob.GetType().Name;

            if (scheduleTasks.TryGetValue(name, out var exp))
            {
                RecurringJob.AddOrUpdate(name, () => recurringJob.Run(), exp.CronExpression);
            }
        }
    }

    private static IEnumerable<HangfireJob> HangfireJobs(IHostEnvironment environment)
    {
        var instance = new List<HangfireJob>();
        var environmentName = environment.EnvironmentName;
        var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        var appSettingsPath = Path.Combine(assemblyPath!, $"appsettings.json");
        new ConfigurationBuilder()
            .AddJsonFile(appSettingsPath)
            .Build().Bind("Jobs", instance);
        return instance;
    }
}