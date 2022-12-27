interface IDomainEventListener<T> where T : IDomainEvent { Task HandleEvent(T domainEvent); }
