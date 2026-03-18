namespace Storefront.Application;

public sealed class ProductDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public string? Description { get; init; }

    public bool IsActive { get; init; }
}

