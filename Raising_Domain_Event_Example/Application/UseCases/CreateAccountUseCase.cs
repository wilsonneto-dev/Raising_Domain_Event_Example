using MediatR;

record CreateAccountInput(string Name, string Email) : IRequest<CreateAccountOutput>;
record CreateAccountOutput(Guid Id);

class CreateAccountUseCase : IRequestHandler<CreateAccountInput, CreateAccountOutput>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateAccountUseCase> _logger;

    public CreateAccountUseCase(ILogger<CreateAccountUseCase> logger, IAccountRepository repository, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _accountRepository = repository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<CreateAccountOutput> Handle(CreateAccountInput input, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating account for {email}", input.Email);
        var account = new Account(input.Name, input.Email);
        await _accountRepository.Insert(account);
        await _unitOfWork.Commit();
        _logger.LogInformation("Created account for {email} - transaction commited", input.Email);
        return new CreateAccountOutput(account.Id);
    }
}
