using Storefront.Domain;

namespace Storefront.Application;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);
}

