class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _context;

    public AccountRepository(AppDbContext context) => _context = context;

    public async Task Insert(Account account) => await _context.Accounts.AddAsync(account);
}
