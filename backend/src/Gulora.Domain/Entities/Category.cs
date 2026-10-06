using Gulora.Domain.Common;
using Gulora.Domain.Exceptions;

namespace Gulora.Domain.Entities;

/// <summary>
/// Ürün kategorisi — öz-referans hiyerarşi (üst → alt kategori).
/// Kök kategorilerin <see cref="ParentId"/> alanı null'dır.
/// Soft delete: <see cref="IsActive"/> false yapılır.
/// </summary>
public sealed class Category : BaseAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public Guid? ParentId { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<Category> _children = [];
    public IReadOnlyList<Category> Children => _children.AsReadOnly();

    private Category() { }

    public static Category Create(
        string name,
        string slug,
        string? description = null,
        string? imageUrl = null,
        Guid? parentId = null,
        int displayOrder = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        return new Category
        {
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            Description = description?.Trim(),
            ImageUrl = imageUrl?.Trim(),
            ParentId = parentId,
            DisplayOrder = displayOrder,
            IsActive = true
        };
    }

    public void Update(string name, string slug, string? description, string? imageUrl, int displayOrder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Description = description?.Trim();
        ImageUrl = imageUrl?.Trim();
        DisplayOrder = displayOrder;
        MarkUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkUpdated();
    }

    /// <summary>Kök kategori mi? (üst kategorisi yok)</summary>
    public bool IsRoot => ParentId is null;
}
