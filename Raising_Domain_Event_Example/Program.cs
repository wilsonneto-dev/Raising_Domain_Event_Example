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

builder.Services.AddTransient<IDomainEventListener<AccountCreatedEvent>, SendEmailForAccountCreatedEventListener>();
builder.Services.AddTransient<IDomainEventListener<AccountCreatedEvent>, CreateIntegrationEventCreatedEventListener>();
builder.Services.AddTransient<IDomainEventListener<AccountSuspendedEvent>, OtherAccountSuspendedEventListener>();

builder.Services.AddDomainEventDispatcher(cfg => cfg
    .AddListener<AccountCreatedEvent>()
    .AddListener<AccountSuspendedEvent>());

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
