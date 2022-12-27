class OtherAccountSuspendedEventListener : IDomainEventListener<AccountSuspendedEvent>
{
    private readonly ILogger<OtherAccountSuspendedEventListener> _logger;

    public OtherAccountSuspendedEventListener(ILogger<OtherAccountSuspendedEventListener> logger) => _logger = logger;

    public Task HandleEvent(AccountSuspendedEvent domainEvent)
    {
        _logger.LogInformation("Suspendend... This listenners isn`t expected t run", domainEvent.Email);
        return Task.CompletedTask;
    }
}
