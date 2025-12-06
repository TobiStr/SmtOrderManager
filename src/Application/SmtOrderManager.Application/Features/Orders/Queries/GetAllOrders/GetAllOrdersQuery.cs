using MediatR;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Primitives;

namespace SmtOrderManager.Application.Features.Orders.Queries.GetAllOrders;

/// <summary>
/// Query to retrieve all orders for the current user.
/// </summary>
public record GetAllOrdersQuery : IRequest<Result<IEnumerable<Order>>>;
