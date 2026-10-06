using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Users.Commands.AddAddress;

public sealed class AddAddressCommandHandler : IRequestHandler<AddAddressCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddAddressCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(AddAddressCommand cmd, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(cmd.UserId, ct)
            ?? throw new NotFoundException("Kullanıcı", cmd.UserId);

        var address = Address.Create(
            cmd.UserId,
            cmd.Title,
            cmd.RecipientName,
            cmd.PhoneNumber,
            cmd.City,
            cmd.District,
            cmd.FullLine,
            cmd.PostalCode,
            cmd.CountryCode,
            cmd.IsDefault);

        user.AddAddress(address);
        await _unitOfWork.SaveChangesAsync(ct);

        return address.Id;
    }
}
