using Gulora.API.Extensions;
using Gulora.Application.Users.Commands.ChangePassword;
using Gulora.Application.Users.Commands.UpdateProfile;
using Gulora.Contracts.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class UsersController(IMediator mediator) : ControllerBase
{
    [HttpPut("me/profile")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var cmd = new UpdateProfileCommand(userId, request.FirstName, request.LastName, request.Email);
        var result = await mediator.Send(cmd, ct);
        return Ok(result);
    }

    [HttpPost("me/password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await mediator.Send(
            new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword),
            ct);
        return Ok();
    }
}
