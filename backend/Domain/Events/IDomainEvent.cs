namespace Gulora.Domain.Events;

/// <summary>
/// Domain içinde meydana gelen ve yan etkileri (e-posta, denormalizasyon, denetim günlüğü)
/// tetiklemek için kullanılabilecek olayların ortak sözleşmesi.
/// İsteğe bağlıdır; MediatR veya dahili bir olay dağıtıcı ile entegre edilebilir.
/// </summary>
public interface IDomainEvent
{
    /// <summary>Olayın gerçekleştiği an (UTC).</summary>
    DateTime OccurredOn { get; }
}
