using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("PocApp"));
builder.Services.AddMediatR(typeof(CreateAccountUseCase));
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWorkEF>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.MapPost("/accounts", 
    async ([FromBody] CreateAccountInput input, IMediator mediator) => await mediator.Send(input))
    .WithOpenApi();

app.MapGet("/accounts", 
    async (AppDbContext context) => await context.Accounts.ToListAsync())
    .WithOpenApi();

app.Run();

// DTOs / use case input/output
record CreateAccountInput(string Name, string Email) : IRequest<CreateAccountOutput>;
record CreateAccountOutput(Guid Id);

// interfaces
interface ICreateAccountUseCase : IRequestHandler<CreateAccountInput, CreateAccountOutput> { };
interface IAccountRepository { Task Insert(Account account); }
interface IUnitOfWork { Task Commit(); }

// use cases
class CreateAccountUseCase : ICreateAccountUseCase
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

// entities / domain
interface IDomainEvent { }

abstract class Aggregate
{
    public Guid Id { get; internal set; }

    private readonly List<IDomainEvent> _events;
    public IReadOnlyCollection<IDomainEvent> Events => _events;

    public Aggregate()
    {
        Id = Guid.NewGuid();
        _events = new();
    }

    internal void RaiseEvent(IDomainEvent @event) => _events.Add(@event);
    internal void ClearEvents() => _events.Clear();
}

record AccountCreatedEvent(Guid Id, string Name, string Email) : IDomainEvent;

class Account : Aggregate
{
    public string Name { get; set; }
    public string Email { get; set; }

    public Account(string name, string email) : base()
    {
        Name = name;
        Email = email;

        RaiseEvent(new AccountCreatedEvent(Id, Name, Email));
    }
}

// persistence
class AppDbContext : DbContext
{
    public DbSet<Account> Accounts => Set<Account>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Account>(entity => { 
            entity.HasKey(e => e.Id);
            entity.Ignore(e => e.Events);
        });
}

class UnitOfWorkEF : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly ILogger<UnitOfWorkEF> _logger;

    public UnitOfWorkEF(AppDbContext context, ILogger<UnitOfWorkEF> logger)
    {
        _logger = logger;
        _context = context;
    }

    public Task Commit()
    {
        RaiseDomainEvents();
        return _context.SaveChangesAsync();
    }

    private void RaiseDomainEvents()
    {
        var modifiedAggregatesChangeTrackers = _context.ChangeTracker
            .Entries<Aggregate>()
            .Where(x => x.Entity.Events != null && x.Entity.Events.Any());
        _logger.LogInformation("Commit: {number} aggregates modifieds", modifiedAggregatesChangeTrackers.Count());
        
        var domainEvents = modifiedAggregatesChangeTrackers
            .SelectMany(x => x.Entity.Events)
            .ToList();
        _logger.LogInformation("Commit: {number} domain events raised", domainEvents.Count());

        modifiedAggregatesChangeTrackers.ToList().ForEach(aggregates => aggregates.Entity.ClearEvents());
    }
}

class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _context;

    public AccountRepository(AppDbContext context) => _context = context;

    public async Task Insert(Account account) => await _context.Accounts.AddAsync(account);
}
