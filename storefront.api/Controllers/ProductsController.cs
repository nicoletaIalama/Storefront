using Microsoft.AspNetCore.Mvc;
using Storefront.Application;

namespace Storefront.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _productService.GetAllAsync(cancellationToken);
    }
}

