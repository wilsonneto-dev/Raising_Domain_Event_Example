class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;
    private readonly Dictionary<Type, Type> _listenersTypes;

    public DomainEventDispatcher(
        IServiceProvider serviceProvider, 
        ILogger<DomainEventDispatcher> logger,
        Dictionary<Type, Type> listenerTypes)
    {
        _serviceProvider = serviceProvider;
        _listenersTypes = listenerTypes;
        _logger = logger;
    }

    public async Task Dispatch<T>(T domainEvent) where T : IDomainEvent
    {
        var eventType = domainEvent.GetType();
        if(!_listenersTypes.ContainsKey(eventType))
        {
            _logger.LogWarning("No listener registered found for event {eventType}", eventType);
            return;
        }
        
        var listenerType = _listenersTypes[eventType];
        var listeners = _serviceProvider.GetServices(listenerType);
        if(listeners.Count() == 0)
        {
            _logger.LogWarning("No listener found in the DI container for event {eventType}", eventType);
            return;
        }

        foreach(var listener in listeners)
            await (Task) listener.GetType().GetMethod("HandleEvent").Invoke(listener, new object[] { domainEvent });
    }
}

class DomainEventDispatcherConfiguration
{
    private readonly Dictionary<Type, Type> _listenersTypes;
    public Dictionary<Type, Type> GetListenersTypes() => _listenersTypes;

    public DomainEventDispatcherConfiguration() => _listenersTypes = new Dictionary<Type, Type>();

    public DomainEventDispatcherConfiguration AddListener<TDomainEvent>() where TDomainEvent : IDomainEvent
    {
        _listenersTypes.Add(typeof(TDomainEvent), typeof(IDomainEventListener<TDomainEvent>));
        return this;
    }
}

static class ServiceCollectionExtension
{
    public static IServiceCollection AddDomainEventDispatcher(
        this IServiceCollection services,
        Func<DomainEventDispatcherConfiguration, DomainEventDispatcherConfiguration> configure)
    {
        services.AddSingleton<IDomainEventDispatcher>(serviceProvider =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<DomainEventDispatcher>>();
            var configuration = new DomainEventDispatcherConfiguration();
            var listenersTypes = configure(configuration).GetListenersTypes();
            return new DomainEventDispatcher(serviceProvider, logger, listenersTypes);
        });
        return services;
    }
}