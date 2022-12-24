using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICreateAccountUseCase, CreateAccountUseCase>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWorkEF>();
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("PocApp"));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.MapPost("/accounts", 
    async ([FromBody] CreateAccountInput input, ICreateAccountUseCase useCase) => await useCase.Execute(input))
    .WithOpenApi();

app.MapGet("/accounts", 
    async (AppDbContext context) => await context.Accounts.ToListAsync())
    .WithOpenApi();

app.Run();

// DTOs / use case input/output
record CreateAccountInput(string Name, string Email);
record CreateAccountOutput(Guid Id);

// interfaces
interface ICreateAccountUseCase { Task<CreateAccountOutput> Execute(CreateAccountInput input); }
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

    public async Task<CreateAccountOutput> Execute(CreateAccountInput input)
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
class Account
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }

    public Account(string name, string email)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
    }
}

// persistence
class AppDbContext : DbContext
{
    public DbSet<Account> Accounts => Set<Account>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Account>(entity => entity.HasKey(e => e.Id));
}

class UnitOfWorkEF : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWorkEF(AppDbContext context) => _context = context;

    public Task Commit() => _context.SaveChangesAsync();
}

class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _context;

    public AccountRepository(AppDbContext context) => _context = context;

    public async Task Insert(Account account) => await _context.Accounts.AddAsync(account);
}