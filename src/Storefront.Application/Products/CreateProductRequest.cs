namespace Storefront.Application;

public sealed class CreateProductRequest
{
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; } = true;
}
