namespace AioCore.Domain;

public class AioCoreContext : MongoContext
{
    public AioCoreContext(IMongoContextBuilder modelBuilder) : base(modelBuilder)
    {
    }

    public MongoSet<Post> Posts { get; set; } = default!;
}