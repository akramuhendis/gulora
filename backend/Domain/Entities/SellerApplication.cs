using Gulora.Domain.Common;
using Gulora.Domain.Enums;
using Gulora.Domain.Events;
using Gulora.Domain.Exceptions;

namespace Gulora.Domain.Entities;

/// <summary>
/// Platformda satıcı olmak isteyen işletme veya şahıs başvurusunu temsil eder.
/// Onay / red işlemleri <see cref="SellerApplicationStatus"/> üzerinden yönetilir.
/// </summary>
public class SellerApplication : BaseEntity
{
    /// <summary>Başvuru sahibinin adı soyadı.</summary>
    public string FullName { get; private set; } = default!;
    /// <summary>Ticari unvan veya mağaza adı.</summary>
    public string Company { get; private set; } = default!;
    /// <summary>Vergi kimlik numarası (ülkeye göre format uygulama katmanında doğrulanır).</summary>
    public string TaxNumber { get; private set; } = default!;
    /// <summary>İletişim e-postası.</summary>
    public string Email { get; private set; } = default!;
    /// <summary>Telefon.</summary>
    public string Phone { get; private set; } = default!;
    /// <summary>Faaliyet şehri.</summary>
    public string City { get; private set; } = default!;
    /// <summary>Satılacak ürün kategorisi veya uzmanlık alanı.</summary>
    public string Category { get; private set; } = default!;
    /// <summary>Ek açıklama ve deneyim metni.</summary>
    public string Message { get; private set; } = default!;
    /// <summary>Başvuru iş akışı durumu.</summary>
    public SellerApplicationStatus Status { get; private set; } = SellerApplicationStatus.Pending;
    /// <summary>Rədd edildikdə moderator qeydi (opsional).</summary>
    public string? RejectionReason { get; private set; }

    private SellerApplication() { }

    /// <summary>Yeni satıcı başvurusu oluşturur.</summary>
    public static SellerApplication Create(
        string fullName, string company, string taxNumber,
        string email, string phone, string city,
        string category, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(company);
        ArgumentException.ThrowIfNullOrWhiteSpace(taxNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(phone);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var application = new SellerApplication
        {
            FullName = fullName.Trim(),
            Company = company.Trim(),
            TaxNumber = taxNumber.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Phone = phone.Trim(),
            City = city.Trim(),
            Category = category.Trim(),
            Message = message.Trim()
        };
        application.RegisterDomainEvent(new SellerApplicationSubmittedDomainEvent(application.Id));
        return application;
    }

    /// <summary>Formu inceleme kuyruğuna alır.</summary>
    public void StartReview()
    {
        if (Status != SellerApplicationStatus.Pending)
            throw new DomainException("Yalnız gözləmədə olan müraciət nəzərə alınmaya göndərilə bilər.");
        Status = SellerApplicationStatus.UnderReview;
        SetUpdated();
    }

    /// <summary>Başvuruyu onaylandı olarak işaretler (ek onboarding adımları uygulama katmanında).</summary>
    public void Approve()
    {
        if (Status != SellerApplicationStatus.UnderReview)
            throw new DomainException("Yalnız nəzərdən keçirilən müraciət təsdiqlənə bilər.");
        Status = SellerApplicationStatus.Approved;
        RejectionReason = null;
        RegisterDomainEvent(new SellerApplicationApprovedDomainEvent(Id));
        SetUpdated();
    }

    /// <summary>Başvuruyu reddeder; <paramref name="reason"/> opsional izahdır.</summary>
    public void Reject(string? reason = null)
    {
        if (Status != SellerApplicationStatus.UnderReview)
            throw new DomainException("Yalnız nəzərdən keçirilən müraciət rədd edilə bilər.");
        Status = SellerApplicationStatus.Rejected;
        RejectionReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        RegisterDomainEvent(new SellerApplicationRejectedDomainEvent(Id, RejectionReason));
        SetUpdated();
    }
}
