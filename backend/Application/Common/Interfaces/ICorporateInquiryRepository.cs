using Gulora.Domain.Entities;

namespace Gulora.Application.Common.Interfaces;

/// <summary>
/// Kurumsal/özel (corporate) talep formu <see cref="CorporateInquiry"/> ekleme ve
/// (ileride) durum yönetimi. Şimdilik sadece ekleme+SaveChanges.
/// </summary>
public interface ICorporateInquiryRepository
{
    /// <summary>Form domain nesnesi oluşturulduktan sonra repositoriye ekler.</summary>
    Task AddAsync(CorporateInquiry inquiry, CancellationToken ct = default);

    /// <summary>Kalıcı hale getirir.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
