using Microsoft.AspNetCore.Mvc;
using Storefront.Application;

namespace Storefront.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateAsync(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var order = await _orderService.CreateAsync(request, cancellationToken);
            return Created($"/api/orders/{order.OrderId}", order);
        }
        catch (ArgumentException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpGet("{orderId:guid}")]
    public async Task<ActionResult<OrderDto>> GetByIdAsync(
        [FromRoute] Guid orderId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var order = await _orderService.GetByIdAsync(orderId, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException)
        {
            return Problem(
                detail: $"No order exists with id '{orderId}'.",
                statusCode: StatusCodes.Status404NotFound);
        }
    }

    [HttpPost("{orderId:guid}/payments/mock")]
    public async Task<ActionResult<OrderDto>> MockPaymentAsync(
        [FromRoute] Guid orderId,
        [FromBody] MockPaymentRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var order = await _orderService.ProcessMockPaymentAsync(orderId, request, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException)
        {
            return Problem(
                detail: $"No order exists with id '{orderId}'.",
                statusCode: StatusCodes.Status404NotFound);
        }
        catch (ArgumentException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
