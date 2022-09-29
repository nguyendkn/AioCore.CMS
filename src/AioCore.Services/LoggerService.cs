using AioCore.Domain;
using AioCore.Domain.AggregateModels.EventsAggregate;

namespace AioCore.Services;

public interface ILoggerService
{
    Task LogPageView(string userClient);
}

public class LoggerService : ILoggerService
{
    private readonly AioCoreContext _context;
    private readonly IClientService _clientService;

    public LoggerService(AioCoreContext context, 
        IClientService clientService)
    {
        _context = context;
        _clientService = clientService;
    }

    public async Task LogPageView(string userClient)
    {
        var pageView = new PageView
        {
            UserClient = userClient,
        };
    }
}