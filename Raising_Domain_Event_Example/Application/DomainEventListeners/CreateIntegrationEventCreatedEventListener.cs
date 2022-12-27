class CreateIntegrationEventCreatedEventListener : IDomainEventListener<AccountCreatedEvent>
{
    private readonly ILogger<CreateIntegrationEventCreatedEventListener> _logger;

    public CreateIntegrationEventCreatedEventListener(ILogger<CreateIntegrationEventCreatedEventListener> logger) => _logger = logger;

    public Task HandleEvent(AccountCreatedEvent domainEvent)
    {
        _logger.LogInformation("Let`s communicate other microservices: Account created event handled for {email}", 
            domainEvent.Email);
        return Task.CompletedTask;
    }
}
