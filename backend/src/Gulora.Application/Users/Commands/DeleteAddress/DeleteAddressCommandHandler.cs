using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Users.Commands.DeleteAddress;

public sealed class DeleteAddressCommandHandler : IRequestHandler<DeleteAddressCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAddressCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteAddressCommand cmd, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(cmd.UserId, ct)
            ?? throw new NotFoundException("Kullanıcı", cmd.UserId);

        user.RemoveAddress(cmd.AddressId);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
