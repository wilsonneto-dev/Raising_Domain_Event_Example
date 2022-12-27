interface IDomainEventDispatcher { Task Dispatch<T>(T domainEvent) where T : IDomainEvent; }
