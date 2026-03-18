using Storefront.Application;
using Storefront.Domain;

namespace Storefront.Infrastructure;

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly Dictionary<Guid, Order> _orders = new();

    public Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        _orders.Add(order.Id, order);
        return Task.CompletedTask;
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _orders.TryGetValue(id, out var order);
        return Task.FromResult(order);
    }
}

