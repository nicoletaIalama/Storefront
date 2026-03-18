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
                Id = Guid.Parse("a3f1c2d4-7b8e-4c1a-9f2d-5e6b7c8d9a01"),
                Name = "Mechanical Keyboard",
                Price = 129.99m,
                Description = "Compact mechanical keyboard with hot-swappable switches.",
                IsActive = true
            },
            new Product
            {
                Id =Guid.Parse("b7e2d1f3-9a4c-4f6b-8d1e-2c3a5f7b8d02") ,
                Name = "Wireless Mouse",
                Price = 49.99m,
                Description = "Ergonomic wireless mouse with long battery life.",
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("c9a4e7b2-1d3f-4b8c-9e2a-6f5d3c1b7a03"),
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

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product);
    }
}

