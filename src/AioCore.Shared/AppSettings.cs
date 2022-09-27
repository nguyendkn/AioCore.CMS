using AioCore.Mongo;

namespace AioCore.Shared;

public class AppSettings
{
    public MongoConfigs MongoConfigs { get; set; } = default!;
}