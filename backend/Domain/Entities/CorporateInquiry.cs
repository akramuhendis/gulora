using Gulora.Domain.Common;
using Gulora.Domain.Events;

namespace Gulora.Domain.Entities;

/// <summary>
/// Kurumsal müşteri veya toplu sipariş talebi formundan gelen kayıt.
/// Satış ekibi tarafından işlendikçe <see cref="IsHandled"/> güncellenir.
/// </summary>
public class CorporateInquiry : BaseEntity
{
    /// <summary>Şirket veya kurum unvanı.</summary>
    public string Company { get; private set; } = default!;
    /// <summary>İletişim kurulacak kişinin adı.</summary>
    public string ContactName { get; private set; } = default!;
    /// <summary>E-posta adresi (küçük harfe normalize).</summary>
    public string Email { get; private set; } = default!;
    /// <summary>Telefon numarası.</summary>
    public string Phone { get; private set; } = default!;
    /// <summary>Etkinlik türü: düğün, açılış vb. (isteğe bağlı).</summary>
    public string? Occasion { get; private set; }
    /// <summary>Tahmini adet (isteğe bağlı).</summary>
    public int? Quantity { get; private set; }
    /// <summary>Tahmini bütçe (isteğe bağlı).</summary>
    public decimal? Budget { get; private set; }
    /// <summary>Serbest metin talep açıklaması.</summary>
    public string Message { get; private set; } = default!;
    /// <summary>CRM / operasyon tarafından talep kapatıldıysa true.</summary>
    public bool IsHandled { get; private set; }

    private CorporateInquiry() { }

    /// <summary>Yeni kurumsal talep kaydı oluşturur.</summary>
    public static CorporateInquiry Create(string company, string contactName,
        string email, string phone, string message,
        string? occasion = null, int? quantity = null, decimal? budget = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(company);
        ArgumentException.ThrowIfNullOrWhiteSpace(contactName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(phone);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        if (quantity is < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Miqdar mənfi ola bilməz.");
        if (budget is < 0)
            throw new ArgumentOutOfRangeException(nameof(budget), "Büdcə mənfi ola bilməz.");

        var inquiry = new CorporateInquiry
        {
            Company = company.Trim(),
            ContactName = contactName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Phone = phone.Trim(),
            Message = message.Trim(),
            Occasion = string.IsNullOrWhiteSpace(occasion) ? null : occasion.Trim(),
            Quantity = quantity,
            Budget = budget
        };
        inquiry.RegisterDomainEvent(new CorporateInquirySubmittedDomainEvent(inquiry.Id));
        return inquiry;
    }

    /// <summary>Talep işlendi olarak işaretlenir (təkrar çağrıda idempotent).</summary>
    public void MarkHandled()
    {
        if (IsHandled) return;
        IsHandled = true;
        SetUpdated();
    }
}
