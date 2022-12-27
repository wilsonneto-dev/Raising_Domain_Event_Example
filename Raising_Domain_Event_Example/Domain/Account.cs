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
