using Gulora.API.Extensions;
using Gulora.Application.Addresses.Commands.CreateAddress;
using Gulora.Application.Addresses.Commands.DeleteAddress;
using Gulora.Application.Addresses.Commands.UpdateAddress;
using Gulora.Application.Addresses.Queries.GetAddresses;
using Gulora.Contracts.Addresses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AddressesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AddressDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AddressDto>>> List(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await mediator.Send(new GetAddressesQuery(userId), ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AddressDto>> Create([FromBody] CreateAddressRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var cmd = new CreateAddressCommand(
            userId,
            request.Title,
            request.RecipientName,
            request.RecipientPhone,
            request.City,
            request.District,
            request.Line,
            request.PostalCode,
            request.IsDefault);
        var result = await mediator.Send(cmd, ct);
        return Ok(result);
    }

    [HttpPut("{addressId:guid}")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AddressDto>> Update(
        Guid addressId,
        [FromBody] UpdateAddressRequest request,
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        var cmd = new UpdateAddressCommand(
            userId,
            addressId,
            request.Title,
            request.RecipientName,
            request.RecipientPhone,
            request.City,
            request.District,
            request.Line,
            request.PostalCode,
            request.IsDefault);
        var result = await mediator.Send(cmd, ct);
        return Ok(result);
    }

    [HttpDelete("{addressId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Delete(Guid addressId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await mediator.Send(new DeleteAddressCommand(addressId, userId), ct);
        return Ok();
    }
}
