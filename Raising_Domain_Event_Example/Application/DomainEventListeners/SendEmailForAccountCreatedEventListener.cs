class SendEmailForAccountCreatedEventListener : IDomainEventListener<AccountCreatedEvent>
{
    private readonly ILogger<SendEmailForAccountCreatedEventListener> _logger;

    public SendEmailForAccountCreatedEventListener(ILogger<SendEmailForAccountCreatedEventListener> logger) => _logger = logger;

    public Task HandleEvent(AccountCreatedEvent domainEvent)
    {
        _logger.LogInformation("Account created event handled for {email} - sending email...", 
            domainEvent.Email);
        return Task.CompletedTask;
    }
}
