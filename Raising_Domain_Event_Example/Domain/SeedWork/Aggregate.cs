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
