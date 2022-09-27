using System.Linq.Expressions;
using AioCore.Mongo.Abstracts;
using AioCore.Mongo.Metadata;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace AioCore.Mongo;

public static class MongoExtension
{
    public static void AddMongoContext<TMongoContext>(
        this IServiceCollection services,
        MongoConfigs mongoConfigs)
        where TMongoContext : MongoContext
    {
        services.AddSingleton(_ =>
        {
            var settings = MongoClientSettings.FromConnectionString(mongoConfigs.ConnectionString);
            var client = new MongoClient(settings);

            return client.GetDatabase(mongoConfigs.DatabaseName);
        });
        services.AddSingleton<IMongoContextBuilder>(provider =>
        {
            var requiredService = provider.GetRequiredService<IMongoDatabase>();
            return new MongoContextBuilder(requiredService, mongoConfigs);
        });
        services.AddSingleton<TMongoContext>();
    }

    public static BsonDocument ToBsonQuery<T>(this FilterDefinition<T> filter)
    {
        var serializerRegistry = BsonSerializer.SerializerRegistry;
        var documentSerializer = serializerRegistry.GetSerializer<T>();
        return filter.Render(documentSerializer, serializerRegistry);
    }

    public static IAggregateFluent<BsonDocument>? Search(this IAggregateFluent<BsonDocument> aggregateFluent, int take)
    {
        return aggregateFluent.Limit(take);
    }

    public static IAggregateFluent<BsonDocument>? Take(this IAggregateFluent<BsonDocument> aggregateFluent, int take)
    {
        return aggregateFluent.Limit(take);
    }

    public static IFindFluent<TEntity, TEntity> Take<TEntity>(this IFindFluent<TEntity, TEntity> fluent, int? limit)
    {
        return fluent.Limit(limit);
    }

    public static IFindFluent<TEntity, TEntity> OrderBy<TEntity>(this IFindFluent<TEntity, TEntity> fluent,
        Expression<Func<TEntity, object>> expression)
    {
        return fluent.SortBy(expression);
    }

    public static IFindFluent<TEntity, TEntity> OrderByDescending<TEntity>(this IFindFluent<TEntity, TEntity> fluent,
        Expression<Func<TEntity, object>> expression)
    {
        return fluent.SortByDescending(expression);
    }

    public static async Task<bool> UpdateAsync<TEntity>(
        this IMongoCollection<TEntity> collection, Guid id, TEntity entity)
        where TEntity : MongoDocument
    {
        var replaceOneResult = await collection.ReplaceOneAsync(x => x!.Id.Equals(id), entity);
        return replaceOneResult.IsAcknowledged;
    }

    public static async Task<TEntity?> FirstOrDefaultAsync<TEntity>(
        this IMongoCollection<TEntity> collection,
        Expression<Func<TEntity, bool>> expression)
        where TEntity : MongoDocument
    {
        var document = await collection.FindAsync(expression);
        return await document.FirstOrDefaultAsync();
    }
    
    public static IFindFluent<TEntity, TEntity> Where<TEntity>(this IMongoCollection<TEntity> collection,
        Expression<Func<TEntity, bool>> expression, string? keyword = null) where TEntity : MongoDocument
    {
        var builder = Builders<TEntity>.Filter;
        var text = Builders<TEntity>.Filter.Text(keyword ?? string.Empty);
        var where = Builders<TEntity>.Filter.Where(expression);
        if (string.IsNullOrEmpty(keyword))
        {
            var findFluent = collection.Find(where);
            Console.WriteLine(findFluent.ToString());
            return findFluent;
        }
        else
        {
            var filters = builder.And(text, where);
            var findFluent = collection.Find(filters);
            Console.WriteLine(findFluent.ToString());
            return findFluent;
        }
    }
    
    public static async Task<long> CountAsync<TEntity>(this IMongoCollection<TEntity> collection, 
        Expression<Func<TEntity, bool>> expression, CountOptions? options = null)
    {
        return await collection.CountDocumentsAsync(expression, options);
    }
    
    public static async Task<bool> AnyAsync<TEntity>(this IMongoCollection<TEntity> collection, 
        Expression<Func<TEntity, bool>> expression)
    {
        return (await CountAsync(collection, expression) > 0);
    }

    public static async Task<TEntity> AddAsync<TEntity>(
        this IMongoCollection<TEntity> collection, TEntity entity)
        where TEntity : MongoDocument
    {
        await collection.InsertOneAsync(entity);
        return entity;
    }

    public static async Task AddRangeAsync<TEntity>(
        this IMongoCollection<TEntity> collection,
        IEnumerable<TEntity> entities)
        where TEntity : MongoDocument
    {
        await collection.InsertManyAsync(entities);
    }
}