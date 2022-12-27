using Microsoft.EntityFrameworkCore;
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
