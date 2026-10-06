using Gulora.Contracts.SellerApplications;
using Gulora.Domain.Entities;
using Gulora.Domain.Enums;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.SellerApplications.Commands.SubmitSellerApplication;

public sealed class SubmitSellerApplicationCommandHandler
    : IRequestHandler<SubmitSellerApplicationCommand, SellerApplicationResponse>
{
    private readonly ISellerApplicationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitSellerApplicationCommandHandler(
        ISellerApplicationRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SellerApplicationResponse> Handle(
        SubmitSellerApplicationCommand cmd,
        CancellationToken ct)
    {
        if (await _repository.HasPendingApplicationForTaxNumberAsync(cmd.TaxNumber, ct))
        {
            throw new ConflictException(
                "Bu vergi numarasıyla bekleyen veya incelenen bir başvuru zaten var.");
        }

        var app = SellerApplication.Submit(
            cmd.FullName,
            cmd.Company,
            cmd.TaxNumber,
            cmd.Email,
            cmd.Phone,
            cmd.City,
            cmd.Category,
            cmd.Message);

        _repository.Add(app);
        await _unitOfWork.SaveChangesAsync(ct);

        return ToResponse(app);
    }

    private static SellerApplicationResponse ToResponse(SellerApplication app)
        => new(
            app.Id,
            app.FullName,
            app.Company,
            app.Email,
            StatusLabel(app.Status),
            app.RejectionReason,
            app.CreatedAt,
            app.UpdatedAt);

    private static string StatusLabel(SellerApplicationStatus s) => s switch
    {
        SellerApplicationStatus.Pending => "Pending",
        SellerApplicationStatus.UnderReview => "UnderReview",
        SellerApplicationStatus.Approved => "Approved",
        SellerApplicationStatus.Rejected => "Rejected",
        _ => s.ToString()
    };
}
