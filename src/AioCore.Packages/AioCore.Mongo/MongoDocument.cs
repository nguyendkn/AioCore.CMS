using AioCore.Mongo.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace AioCore.Mongo;

public class MongoDocument
{
    [BsonId] [MongoKey] public Guid Id { get; set; } = Guid.NewGuid();
}