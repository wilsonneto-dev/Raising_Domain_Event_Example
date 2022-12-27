class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(
        IServiceProvider serviceProvider, 
        ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task Dispatch<T>(T domainEvent) where T : IDomainEvent
    {
        var eventType = domainEvent.GetType();
        var listeners = _serviceProvider.GetServices(typeof(IDomainEventListener<>).MakeGenericType(eventType));
        if(!listeners.Any())
        {
            _logger.LogWarning("No listener found in the DI container for event {eventType}", eventType);
            return;
        }

        foreach(var listener in listeners)
        {
            await (Task)listener.GetType().GetMethod("HandleEvent").Invoke(listener, new object[] { domainEvent });
        }
    }
}
