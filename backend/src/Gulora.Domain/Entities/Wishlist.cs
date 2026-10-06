using Gulora.Domain.Common;
using Gulora.Domain.Exceptions;

namespace Gulora.Domain.Entities;

/// <summary>
/// Kullanıcının istek listesi (Favoriler) aggregate root'u.
/// Her kullanıcıya ait tek bir istek listesi vardır.
/// Bir ürün birden fazla kez eklenemez.
/// </summary>
public sealed class Wishlist : BaseAggregateRoot
{
    public Guid UserId { get; private set; }

    private readonly List<WishlistItem> _items = [];
    public IReadOnlyList<WishlistItem> Items => _items.AsReadOnly();

    private Wishlist() { }

    public static Wishlist CreateForUser(Guid userId)
    {
        return new Wishlist { UserId = userId };
    }

    /// <summary>Ürünü istek listesine ekler. Zaten mevcutsa exception fırlatır.</summary>
    public void AddProduct(Guid productId)
    {
        if (_items.Any(i => i.ProductId == productId))
            throw new BusinessRuleException("Bu ürün zaten istek listenizde.", "PRODUCT_ALREADY_IN_WISHLIST");

        _items.Add(WishlistItem.Create(Id, productId));
        MarkUpdated();
    }

    /// <summary>Ürünü istek listesinden kaldırır.</summary>
    public void RemoveProduct(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new NotFoundException("Ürün istek listenizde bulunamadı.");

        _items.Remove(item);
        MarkUpdated();
    }

    /// <summary>İstek listesini tamamen temizler.</summary>
    public void Clear()
    {
        _items.Clear();
        MarkUpdated();
    }

    /// <summary>Ürünün listede olup olmadığını kontrol eder.</summary>
    public bool Contains(Guid productId) => _items.Any(i => i.ProductId == productId);

    /// <summary>Listedeki ürün sayısı.</summary>
    public int Count => _items.Count;
}
