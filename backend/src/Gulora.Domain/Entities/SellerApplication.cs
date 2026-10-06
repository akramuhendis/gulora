using Gulora.Domain.Common;
using Gulora.Domain.Enums;
using Gulora.Domain.Exceptions;

namespace Gulora.Domain.Entities;

/// <summary>Satıcı (mağaza) başvuru kaydı.</summary>
public sealed class SellerApplication : BaseAggregateRoot
{
    public string FullName { get; private set; } = string.Empty;
    public string Company { get; private set; } = string.Empty;
    public string TaxNumber { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string? Message { get; private set; }
    public SellerApplicationStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }

    private SellerApplication() { }

    public static SellerApplication Submit(
        string fullName,
        string company,
        string taxNumber,
        string email,
        string phone,
        string city,
        string category,
        string? message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(company);
        ArgumentException.ThrowIfNullOrWhiteSpace(taxNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(phone);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        return new SellerApplication
        {
            FullName = fullName.Trim(),
            Company = company.Trim(),
            TaxNumber = taxNumber.Trim(),
            Email = email.Trim(),
            Phone = phone.Trim(),
            City = city.Trim(),
            Category = category.Trim(),
            Message = message?.Trim(),
            Status = SellerApplicationStatus.Pending,
            RejectionReason = null
        };
    }

    public void SetUnderReview()
    {
        if (Status != SellerApplicationStatus.Pending)
            throw new BusinessRuleException("Yalnızca bekleyen başvurular incelemeye alınabilir.");

        Status = SellerApplicationStatus.UnderReview;
        MarkUpdated();
    }

    public void Approve()
    {
        if (Status is not (SellerApplicationStatus.Pending or SellerApplicationStatus.UnderReview))
            throw new BusinessRuleException("Bu başvuru onaylanamaz.");

        Status = SellerApplicationStatus.Approved;
        RejectionReason = null;
        MarkUpdated();
    }

    public void Reject(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        if (Status == SellerApplicationStatus.Approved)
            throw new BusinessRuleException("Onaylanmış başvuru reddedilemez.");

        Status = SellerApplicationStatus.Rejected;
        RejectionReason = reason.Trim();
        MarkUpdated();
    }
}
