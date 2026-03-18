using Storefront.Domain;

namespace Storefront.Application;

public sealed class ProductService(IProductRepository repository) : IProductService
{
    private readonly IProductRepository _repository = repository;

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await _repository.GetAllAsync(cancellationToken);

        return products
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Description = p.Description,
                IsActive = p.IsActive
            })
            .ToArray();
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return null;
        }

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Description = product.Description,
            IsActive = product.IsActive
        };
    }
}

