using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Users.Commands.UpdateAddress;

public sealed class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAddressCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateAddressCommand cmd, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(cmd.UserId, ct)
            ?? throw new NotFoundException("Kullanıcı", cmd.UserId);

        var address = user.Addresses.FirstOrDefault(a => a.Id == cmd.AddressId)
            ?? throw new NotFoundException("Adres", cmd.AddressId);

        address.Update(
            cmd.Title,
            cmd.RecipientName,
            cmd.PhoneNumber,
            cmd.City,
            cmd.District,
            cmd.FullLine,
            cmd.PostalCode);

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
