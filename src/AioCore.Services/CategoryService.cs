using AioCore.Domain.AggregateModels.CategoryAggregate;

namespace AioCore.Services;

public interface ICategoryService
{
    Task<List<Category>> List();
}

public class CategoryService : ICategoryService
{
    public Task<List<Category>> List()
    {
        throw new NotImplementedException();
    }
}