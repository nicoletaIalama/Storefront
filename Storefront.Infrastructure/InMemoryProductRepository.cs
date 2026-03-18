using Storefront.Application;
using Storefront.Domain;

namespace Storefront.Infrastructure;

public sealed class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products;

    public InMemoryProductRepository()
    {
        _products =
        [
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Mechanical Keyboard",
                Price = 129.99m,
                Description = "Compact mechanical keyboard with hot-swappable switches.",
                IsActive = true
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Wireless Mouse",
                Price = 49.99m,
                Description = "Ergonomic wireless mouse with long battery life.",
                IsActive = true
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "USB-C Hub",
                Price = 39.99m,
                Description = "Multi-port USB-C hub with HDMI and Ethernet.",
                IsActive = true
            }
        ];
    }

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Product> snapshot = _products.ToArray();
        return Task.FromResult(snapshot);
    }
}

