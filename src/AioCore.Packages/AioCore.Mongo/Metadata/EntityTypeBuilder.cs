using System.Linq.Expressions;
using Humanizer;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace AioCore.Mongo.Metadata;

public class EntityTypeBuilder<TEntity>
{
    private readonly IMongoCollection<TEntity> _collection;
    private Action<BsonClassMap<TEntity>>? _keyMapper;

    public EntityTypeBuilder(IMongoDatabase database)
    {
        _collection = database.GetCollection<TEntity>(typeof(TEntity).Name.Pluralize());
    }

    public void HasKey(Expression<Func<TEntity, object>>? expression)
    {
        _keyMapper = cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(expression);
        };
    }

    public void HasIndex(params CreateIndexModel<TEntity>[] indexes)
    {
        _collection.Indexes.CreateMany(indexes);
    }
}