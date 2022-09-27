using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using AioCore.Mongo.Abstracts;
using AioCore.Mongo.Attributes;
using Humanizer;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AioCore.Mongo;

public class MongoSet<TEntity> : IQueryable<TEntity>, IMongoSet<TEntity>
    where TEntity : MongoDocument
{
    private readonly IMongoDatabase _database;
    private readonly MongoConfigs _mongoConfigs;
    private readonly IMongoCollection<TEntity> _collection;

    public MongoSet(IMongoDatabase database, MongoConfigs mongoConfigs)
    {
        _database = database;
        _mongoConfigs = mongoConfigs;
        var collectionName = typeof(TEntity).Name.Pluralize();
        _collection = database.GetCollection<TEntity>(collectionName);
    }

    IEnumerator GetEnumerator()
        => _collection.AsQueryable().GetEnumerator();

    IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        => _collection.AsQueryable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => _collection.AsQueryable().GetEnumerator();

    Type IQueryable.ElementType
        => _collection.AsQueryable().ElementType;

    Expression IQueryable.Expression
        => _collection.AsQueryable().Expression;

    IQueryProvider IQueryable.Provider
        => _collection.AsQueryable().Provider;

    public IMongoCollection<TEntity> Collection(string name)
    {
        var settings = MongoClientSettings.FromConnectionString(_mongoConfigs.ConnectionString);
        return new MongoClient(settings).GetDatabase(_mongoConfigs.DatabaseName).GetCollection<TEntity>(name);
    }

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        await _collection.InsertManyAsync(entities);
    }

    public async Task<bool> UpdateAsync(object id, TEntity entity)
    {
        var replaceOneResult = await _collection.ReplaceOneAsync(x => x!.Id.Equals(id), entity);
        return replaceOneResult.IsAcknowledged;
    }

    public async Task<bool> RemoveAsync(object id)
    {
        var deleteResult = await _collection.DeleteOneAsync(x => x!.Id.Equals(id));
        return deleteResult.IsAcknowledged;
    }

    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> expression)
    {
        var document = await _collection.FindAsync(expression);
        return await document.FirstOrDefaultAsync();
    }

    public async Task<long> CountAsync(CountOptions? options = null)
    {
        return await _collection.CountDocumentsAsync(x => true, options);
    }

    public async Task<long> CountAsync(Expression<Func<TEntity, bool>> expression, CountOptions? options = null)
    {
        return await _collection.CountDocumentsAsync(expression, options);
    }

    public async Task<bool> AnyAsync()
    {
        return (await CountAsync() > 0);
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression)
    {
        return (await CountAsync(expression) > 0);
    }

    public async Task<TEntity> FindAsync(object key)
    {
        if (key.Equals(Guid.Empty)) return default!;
        var document = await _collection.FindAsync(x => x!.Id.Equals(key));
        return await document.FirstOrDefaultAsync();
    }

    public IFindFluent<TEntity, TEntity> Where(Expression<Func<TEntity, bool>> expression)
    {
        return Where(expression, string.Empty);
    }

    private IFindFluent<TEntity, TEntity> Where(Expression<Func<TEntity, bool>> expression, string? keyword)
    {
        var builder = Builders<TEntity>.Filter;
        var text = Builders<TEntity>.Filter.Text(keyword ?? string.Empty);
        var where = Builders<TEntity>.Filter.Where(expression);
        if (string.IsNullOrEmpty(keyword)) return _collection.Find(where);
        var filters = builder.And(text, where);
        var fluent = _collection.Find(filters);
        return fluent;
    }

    public IAggregateFluent<BsonDocument> Include<TProperty>(Expression<Func<TEntity, TProperty>> expression)
    {
        var targetProperty = typeof(TProperty).Name;
        var targetCollection = targetProperty.Pluralize();
        var localFieldProperty = typeof(TEntity).GetProperties()
            .FirstOrDefault(x =>
                x.GetCustomAttribute<MongoLocalFieldAttribute>() != null);
        var localField = localFieldProperty?.GetCustomAttribute<MongoLocalFieldAttribute>()?.LocalField;
        var foreignField = typeof(TProperty).GetProperties()
            .FirstOrDefault(x =>
                x.GetCustomAttribute<MongoKeyAttribute>() != null)?.Name;
        return _collection.Aggregate().Lookup(targetCollection, localField, foreignField, localFieldProperty?.Name);
    }
}