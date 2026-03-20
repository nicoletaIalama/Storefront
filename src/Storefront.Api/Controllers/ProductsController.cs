using Microsoft.AspNetCore.Authorization;
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

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> CreateAsync(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var product = await _productService.CreateAsync(request, cancellationToken);
            return Created($"/api/products/{product.Id}", product);
        }
        catch (ArgumentException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _productService.DeleteAsync(id, cancellationToken);
        if (deleted)
        {
            return NoContent();
        }

        return Problem(
            detail: $"No product exists with id '{id}'.",
            statusCode: StatusCodes.Status404NotFound);
    }
}
