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
        var timeStamp = DateTime.Now;
        var logged = _clientService.Logged();
        if (!logged) return;
        var pageView = new PageView
        {
            UserClient = userClient,
            Host = _clientService.GetHost(),
            IP = _clientService.GetIP(),
            Url = _clientService.GetUrl(),
            UserAgent = _clientService.GetUserAgent(),
            Timestamp = timeStamp,
            TimestampShort = timeStamp.ToString("dd/MM/yyyy"),
            Scheme = _clientService.GetScheme()
        };
        await _context.PageViews.AddAsync(pageView);
    }
}