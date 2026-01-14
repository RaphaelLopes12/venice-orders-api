using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeniceOrders.Application.Commands.CreateOrder;
using VeniceOrders.Application.Queries.GetOrderById;

namespace VeniceOrders.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PedidosController : ControllerBase
{
    private readonly IMediator _mediator;

    public PedidosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Cria um novo pedido
    /// - Armazena dados principais no SQL Server
    /// - Armazena itens no MongoDB
    /// - Publica evento PedidoCriado na fila RabbitMQ
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.OrderId }, result);
    }

    /// <summary>
    /// Obtém um pedido por ID
    /// - Agrega dados do SQL Server e MongoDB
    /// - Utiliza cache Redis (2 minutos)
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result is null)
        {
            return NotFound(new { message = "Pedido não encontrado" });
        }

        return Ok(result);
    }
}
