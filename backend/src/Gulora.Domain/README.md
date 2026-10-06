# Gulora.Domain — katman rehberi

Bu proje, Gülora backend çözümünün **Domain (çekirdek iş kuralları)** katmanıdır. Burada yalnızca iş modeli, kurallar, değer nesneleri, domain olayları ve kalıcılık **sözleşmeleri** (arayüzler) bulunur. **EF Core**, **MediatR**, **HTTP**, **veritabanı sürücüleri** ve benzeri dış çerçeve bağımlılıkları bu projede **bilinçli olarak yoktur** (`Gulora.Domain.csproj` içinde harici NuGet paketi tanımlı değildir).

---

## Mimari konum ve bağımlılık yönü

| Katman | Bu projeye bağımlılık |
|--------|------------------------|
| **Gulora.Application** | Evet — komut/sorgu işleyicileri domain türlerini ve arayüzlerini kullanır. |
| **Gulora.Infrastructure** | Evet — `IRepository<>`, `IUnitOfWork`, domain servis arayüzlerinin **uygulamalarını** sağlar. |
| **Gulora.API** | Doğrudan değil; API genelde Application üzerinden gider. |
| **Gulora.Domain** | Hayır — domain, diğer Gülora projelerine referans vermez. |

**Özet:** Domain “iç” katmandır; dış dünya ona bağlanır, domain dışarıya bağlanmaz.

---

## Proje dosyası (`Gulora.Domain.csproj`)

- **Hedef çerçeve:** `net9.0`
- **Nullable:** açık (`<Nullable>enable</Nullable>`)
- **Implicit usings:** açık
- **Kök ad alanı / derleme adı:** `Gulora.Domain`
- **Yorum:** “Domain katmanında dış NuGet bağımlılığı yoktur.” — bu, katmanın saf tutulması ve test edilebilirliği içindir.

---

## Klasör yapısı (kaynak)

Aşağıdaki yapı `bin/`, `obj/` ve derleme çıktıları hariç kaynak düzenidir.

```
Gulora.Domain/
├── Common/           # BaseEntity, BaseAggregateRoot, IDomainEvent
├── Entities/         # Varlıklar (aggregate root ve alt entity'ler)
├── Enums/            # Durum ve tür enum'ları (çoğunda açık sayısal değerler)
├── Events/           # IDomainEvent uygulayan kayıt türleri
├── Exceptions/       # DomainException hiyerarşisi
├── Interfaces/       # Repository + IUnitOfWork + okuma modelleri (ör. SellerDashboardOrderSnapshot)
├── Services/         # Domain servis **arayüzleri** (uygulama Infrastructure'da)
├── ValueObjects/     # Email, Money, AddressSnapshot vb.
├── Gulora.Domain.csproj
└── README.md         # Bu dosya
```

---

## `Common/` — temel altyapı

### `BaseEntity`

Tüm kalıcı varlıkların tabanı.

| Üye | Erişim | Açıklama |
|-----|--------|----------|
| `Id` | `public get`, `protected set` | Varsayılan: `Guid.NewGuid()`. |
| `CreatedAt` | `public get`, `protected set` | Oluşturulma anı; varsayılan `DateTime.UtcNow`. |
| `UpdatedAt` | `public get`, `protected set` | Son güncelleme; `MarkUpdated()` ile set edilir. |
| `DomainEvents` | Salt okunur liste | Varlık üzerinde toplanan `IDomainEvent` örnekleri. |
| `AddDomainEvent` | `protected` | Olay ekler. |
| `ClearDomainEvents` | `public` | Persist / dağıtım sonrası listeyi temizlemek için (ör. SaveChanges interceptor veya UoW). |
| `MarkUpdated` | `protected` | `UpdatedAt = DateTime.UtcNow`. |

**Not:** XML açıklamasında belirtildiği gibi bu sınıfta **EF Core attribute'ları kullanılmaz**; eşleme Infrastructure’da yapılır.

### `BaseAggregateRoot`

`BaseEntity` türevi; DDD’de **aggregate kökü** işaretçisi. Şu an ek alan yok; ileride optimistic concurrency (`RowVersion`), kök-seviye politikalar vb. için genişletilebilir. Yorumda: alt entity’ler doğrudan repository ile değil **köke** üzerinden değiştirilmelidir.

### `IDomainEvent`

Tüm domain olaylarının işaretleyicisi.

- `DateTime OccurredOn { get; }` — olayın oluştuğu UTC zamanı.
- Application katmanında MediatR `INotification` ile eşleme yapılabilir (dağıtım ayrı kural dosyalarında anlatılır).

---

## `Entities/` — varlıklar

Genel kurallar:

- Çoğu özellik **`private set`** ile kapsüllenir; değişim **davranış metodları** veya `static` fabrika metodlarıyla yapılır.
- EF Core için **parametresiz özel `private` yapıcı** (`private Entity() { }`) yaygın desendir.
- İş kuralı ihlalleri `BusinessRuleException`, `NotFoundException` vb. ile bildirilir.
- **Aggregate root** olanlar `BaseAggregateRoot` türetir; alt entity’ler doğrudan `BaseEntity` türetir.

Aşağıda her sınıf, alanlar ve önemli davranışlarla özetlenir.

### `User` — aggregate root (`BaseAggregateRoot`)

**Rol:** Kimlik, profil, adresler, refresh token’lar, e-posta/telefon doğrulama durumu.

**Önemli özellikler:** `FirstName`, `LastName`, `Email`, `PhoneNumber?`, `PasswordHash`, `Role`, `IsActive`, `EmailVerified`, `EmailVerificationToken?`, `EmailVerificationExpiresAt?`, `PhoneVerified`, `LastLoginAt?`, koleksiyonlar: `RefreshTokens`, `Addresses`.

**Fabrika:** `User.Create(...)` — `Email` değer nesnesi (`ValueObjects.Email`) ile doğrulama; `UserRegisteredEvent` ekler. Parola burada **zaten hash’lenmiş** kabul edilir (hash’leme `IPasswordHasher` ile Infrastructure/Application tarafında).

**Davranışlar (örnekler):** `UpdateProfile`, `ChangePassword` (+ `UserPasswordChangedEvent`), `SetPendingEmailVerification` / `CompleteEmailVerification`, `VerifyPhoneNumber`, `RecordLogin`, `Deactivate` / `Activate`, `AddRefreshToken` (aktif token’ları rotate ederek iptal), `AddPasswordResetToken`, `RevokeAllRefreshTokens`, `AddAddress` (en fazla **5** adres), `RemoveAddress`.

**Soft delete:** `IsActive = false` (`Deactivate`).

### `Order` — aggregate root

**Rol:** Sipariş yaşam döngüsü, tutarlar, ödeme durumu, adres anlık görüntüsü, kargo bilgisi, kalemler ve iz kayıtları.

**Alanlar:** `UserId`, `OrderNumber`, `Status`, `DeliveryType`, `PaymentMethod`, `PaymentStatus`, `SubTotal`, `DeliveryFee`, `DiscountAmount`, `TotalAmount`, `CouponCode?`, `AddressSnapshot`, `TrackingNumber?`, `CargoCompany?`, `CancellationReason?`, `DeliveredAt?`, `Items`, `Trackings`.

**Fabrika:** `Order.Create(...)` — en az bir kalem zorunlu; `SubTotal` kalemlerden; `TotalAmount = SubTotal + DeliveryFee - DiscountAmount`; negatif toplam yasak; ilk `OrderTracking` “Sipariş alındı.”; `OrderCreatedEvent`.

**Durum makinesi (geçişler):** `Confirm` (Pending→Confirmed), `StartProcessing`, `Ship` (kargo firması + takip no zorunlu), `MarkDelivered`, `Cancel` (yalnızca Pending veya Confirmed; `OrderCancelledEvent`), `RequestReturn` (Delivered→ReturnRequested), `CompleteReturn`. Geçersiz geçişlerde `BusinessRuleException`. Her anlamlı durum değişiminde `OrderStatusChangedEvent` ve yeni `OrderTracking`.

**Yardımcı:** `UpdatePaymentStatus`, `CanBeCancelled`, `CanBeReturned`.

### `OrderItem` — alt entity

Sipariş anındaki **snapshot:** `ProductName`, `ProductSku?`, `UnitPrice`, `Quantity`; `LineTotal` hesaplanmış özellik. Negatif fiyat veya ≤0 miktar yasak.

### `OrderTracking` — alt entity

Durum geçmişi; `Status`, `Note?`, `CargoCompany?`, `TrackingNumber?`, `ChangedByUserId?`. Silinmez denetim izi modeli.

### `Product` — aggregate root

**Rol:** Katalog ürünü, stok, fiyat, kategori, satıcı, görseller, yorumlar (navigation), puan özeti.

**Alanlar:** `Name`, `Slug`, açıklamalar, `Price`, `DiscountedPrice?`, `StockQuantity`, `Sku?`, `Brand?`, `CategoryId`, `SellerId?`, `IsActive`, `IsFeatured`, `AverageRating`, `ReviewCount`, `Images`, `Reviews`.

**Fabrika / güncelleme:** `Create`, `UpdateDetails`, `UpdatePrice` (indirimli fiyat normalden küçük olmalı).

**Stok:** `DecrementStock` / `IncrementStock` — yetersiz stokta `BusinessRuleException` (kural kodu `INSUFFICIENT_STOCK`); her değişimde `ProductStockUpdatedEvent`.

**Diğer:** `AddImage` / `RemoveImage` (en fazla **10** görsel), `Deactivate` / `Activate`, `RecalculateRating` (Application `IReviewRepository` istatistikleriyle beslenir), `PrimaryImageUrl`, `EffectivePrice`, `IsAvailableForPurchase`.

**Soft delete:** `IsActive`.

### `ProductImage` — alt entity

`Url`, `AltText?`, `DisplayOrder`, `IsActive`. `UpdateOrder`, `Deactivate`.

### `Category` — aggregate root

Hiyerarşi: `ParentId?`, `Children` koleksiyonu (navigation). `Name`, `Slug`, `Description?`, `ImageUrl?`, `DisplayOrder`, `IsActive`. `IsRoot` = `ParentId is null`.

**Soft delete:** `IsActive`.

### `Address` — alt entity (`User` altında)

Teslimat adresi alanları; `CountryCode` varsayılan `"AZ"`. `Create`, `Update`, `SetAsDefault` / `ClearDefault`. Fiziksel silme desteklenir (`User.RemoveAddress`).

### `RefreshToken` — alt entity (`User` altında)

`Token`, `ExpiresAt`, `RevokedAt?`, `ReplacedByToken?`. `IsActive`, `IsRevoked`, `IsExpired` hesaplanmış. `Create`, `Revoke`. **Güvenlik:** token değeri loglanmamalı (XML yorumu).

### `Wishlist` — aggregate root

Kullanıcı başına istek listesi. `AddProduct` (duplicate’te `PRODUCT_ALREADY_IN_WISHLIST`), `RemoveProduct`, `Clear`, `Contains`, `Count`.

### `WishlistItem` — alt entity

`WishlistId`, `ProductId`.

### `Review` — entity (`BaseEntity`; `Product` içinde koleksiyon)

Moderasyon: başlangıç `ReviewStatus.Pending`. `Create` → `ReviewSubmittedEvent`. `Approve` / `Reject` / `Update` (güncellemede tekrar Pending). `IsVisible` = Approved.

### `Payment` — aggregate root

Siparişe bağlı ödeme kaydı; **PCI:** kart/CVV saklanmaz. `Method`, `Status`, `Amount`, `Currency` (varsayılan `AZN`), `ProviderTransactionId?`, `IdempotencyKey`, `PaidAt?`, `RefundedAt?`, `FailureReason?`.

Durum akışı: `MarkCompleted`, `MarkFailed`, `Refund`, `PartialRefund` — geçersiz geçişlerde iş kuralı istisnası.

### `Notification` — entity

`UserId`, `NotificationType`, `Title`, `Body`, `IsRead`, `ReadAt?`, `RelatedEntityId?`. `MarkAsRead` (idempotent).

### `DiscountCoupon` — aggregate root

`Code` (büyük harf), `DiscountType`, `Value`, eşikler, kullanım limiti/sayısı, tarih aralığı, `RestrictedUserId?`, `IsActive`.

**Doğrulama:** `IsValidAt`, `ValidateFor` (geçersiz kupon / min tutar / kullanıcı kısıtı → `BusinessRuleException` veya `ForbiddenException`). `IncrementUsage`, `Deactivate`, `UpdateMetadata`.

### `SellerShop` — entity (`BaseEntity`, aggregate root değil)

Satıcı vitrin: `UserId`, `StoreName`, `StoreSlug` (`NormalizeSlug`), görseller/açıklama. Kullanıcı ile 1:1 ilişki amaçlanır (yorum).

### `SellerApplication` — aggregate root

Başvuru alanları + `SellerApplicationStatus`. `Submit` → Pending. `SetUnderReview`, `Approve`, `Reject` (durum ön koşulları kodda).

### `PhoneOtpChallenge` — entity

`PhoneNumber`, `CodeHash` (düz metin OTP yok), `ExpiresAt`, `SentAt`. `IsExpired`.

---

## `Enums/`

Çoğu enum üye başına **açık tam sayı** atar (serileştirme/DB tutarlılığı için).

| Dosya | Değerler (özet) |
|--------|------------------|
| `OrderStatus` | Pending(1) … Returned(8) — sıra dokümante. |
| `DeliveryType` | Standard(1), Express(2), SameDay(3). |
| `PaymentMethod` | CreditCard, DebitCard, CashOnDelivery, BankTransfer. |
| `PaymentStatus` | Pending, Completed, Failed, Refunded, PartiallyRefunded. |
| `UserRole` | Customer, Admin, Seller. |
| `DiscountType` | Percent, Fixed. |
| `ReviewStatus` | Pending, Approved, Rejected. |
| `NotificationType` | Sipariş/şifre/promosyon kodları. |
| `SellerApplicationStatus` | Pending, UnderReview, Approved, Rejected. |

---

## `ValueObjects/`

| Tür | Amaç |
|-----|------|
| `Email` | `Of` ile trim + lower + regex; geçersizse `ArgumentException`. |
| `PhoneNumber` | Boşluk/tire temizleme; `+` ve rakam deseni. |
| `Money` | `Amount` + `Currency` (varsayılan AZN); aynı para birimi dışı toplama yasak; negatif tutar yasak; `Add`/`Subtract`/`Multiply`/`ApplyPercent`. |
| `OrderNumber` | `GUL-` önek doğrulaması; `Generate(DateOnly, int)` formatı `GUL-{yyyyMMdd}-{seq:D4}`. |
| `AddressSnapshot` | Sipariş anı adresi; `Create` doğrulaması; `ToDisplayString()`. |

---

## `Events/` — domain olayları

Tümü `IDomainEvent` + `OccurredOn = DateTime.UtcNow` (çoğu `record` ile immutable payload).

| Olay | Payload özeti |
|------|-----------------|
| `OrderCreatedEvent` | OrderId, UserId, OrderNumber, TotalAmount, PaymentMethod, DeliveryType. |
| `OrderStatusChangedEvent` | OrderId, UserId, OrderNumber, OldStatus, NewStatus. |
| `OrderCancelledEvent` | OrderId, UserId, OrderNumber, CancellationReason. |
| `UserRegisteredEvent` | UserId, Email, FullName, Role. |
| `UserPasswordChangedEvent` | UserId, Email. |
| `ProductStockUpdatedEvent` | ProductId, ProductName, OldStock, NewStock; `IsLowStock` / `IsOutOfStock` yardımcıları. |
| `ReviewSubmittedEvent` | ReviewId, ProductId, UserId, Rating. |

**Yaşam döngüsü:** Varlık `AddDomainEvent` ile toplar; persist sonrası `ClearDomainEvents` ve handler tetikleme Application/Infrastructure sorumluluğundadır.

---

## `Exceptions/`

| Sınıf | Taban | Tipik HTTP eşlemesi (yorum/XML) |
|--------|--------|----------------------------------|
| `DomainException` | `Exception` | Soyut kök. |
| `NotFoundException` | `DomainException` | 404. |
| `ValidationException` | `DomainException` | 400/422; `Errors` sözlüğü veya mesaj-only ctor. |
| `BusinessRuleException` | `DomainException` | 400; isteğe bağlı `RuleCode`. |
| `ConflictException` | `DomainException` | 409. |
| `ForbiddenException` | `DomainException` | 403. |
| `UnauthorizedException` | `DomainException` | 401. |
| `TooManyRequestsException` | `DomainException` | 429. |

API katmanında bir middleware bu türleri `ProblemDetails`’e çevirerek kullanıcıya güvenli yanıt üretir (ayrı kural dosyası).

---

## `Interfaces/`

### `IRepository<T>` (`T : BaseEntity`)

Genel kalıcılık sözleşmesi: `GetByIdAsync`, `FindAsync`, `ListAsync`, `ListPagedAsync`, `CountAsync`, `ExistsAsync`, `Add` / `AddRange`, `Update`, `Remove`. **Not:** `Add` senkron; gerçek commit `IUnitOfWork.SaveChangesAsync` ile.

### `IUnitOfWork`

`SaveChangesAsync`, transaction: `BeginTransactionAsync`, `CommitTransactionAsync`, `RollbackTransactionAsync`, `IDisposable`.

### Özelleştirilmiş repository arayüzleri

Her biri `IRepository<...>` genişletir ve **sorgu desenlerini** tanımlar (uygulama EF Core ile Infrastructure’da):

| Arayüz | Başlıca ek metodlar |
|--------|---------------------|
| `IUserRepository` | E-posta ile getir, e-posta müsaitlik, refresh token ile kullanıcı, e-posta doğrulama jetonu ile getir. |
| `IOrderRepository` | Sipariş no, kullanıcı sayfalama, durum listesi, tarih aralığı, günlük sıra (`GetDailySequenceAsync`), satıcıya özgü sipariş kontrolleri ve sayfalama, satıcıya yazılan tutarlar sözlüğü, `GetSellerDashboardOrderSnapshotAsync`. |
| `IProductRepository` | Slug, kategori, arama, katalog filtre/sort, satıcı vitrin sayıları, slug çakışması, atomik stok azaltma, satıcı panel listeleri, stok kovaları. |
| `ICategoryRepository` | Slug, kökler, çocuklar, ürün sayısı. |
| `IReviewRepository` | Onaylı/sayfalı, ürün bazlı tüm, pending, kullanıcı değerlendirdi mi, istatistikler. |
| `IDiscountCouponRepository` | Koda göre getir, kullanıcı kupon kullandı mı. |
| `IPaymentRepository` | Idempotency ile getir, siparişe göre listele. |
| `INotificationRepository` | Okunmamışlar, sayfalama, sayaçlar. |
| `IWishlistRepository` | Kullanıcıya göre, get-or-create. |
| `ISellerShopRepository` | Kullanıcı/slug ile getir, slug çakışma kontrolü. |
| `ISellerApplicationRepository` | Vergi no için bekleyen başvuru var mı. |
| `IPhoneOtpRepository` | Telefona göre son challenge, telefona göre silme. |

### `SellerDashboardOrderSnapshot` (kayıt / okuma modeli)

Repository çıktısı için **DTO benzeri** saf kayıt: toplam sipariş sayısı, bekleyen fulfillment sayısı, durum başına sayılar, satıcı satır tutarlarına ilişkin GMV/realize metrikleri, günlük bucketing sözlükleri, son siparişler listesi. Domain entity değil; **sorgu sonucu şekli** olarak `Interfaces` altında durur.

---

## `Services/` — domain servis arayüzleri

Uygulama kodu iş kuralını burada **arayüz** olarak sabitler; **somut sınıflar** Infrastructure’da (DB, dış API, BCrypt vb.).

| Arayüz | Sorumluluk |
|---------|------------|
| `IDeliveryFeeCalculator` | `DeliveryType` + `Money` alt toplamına göre ücret; ücretsiz eşik üzeri `Money.Zero`. |
| `IDiscountCalculator` | Kupon + sipariş toplamı + kullanıcı için indirim `Money`; ihlalde `BusinessRuleException`. |
| `IPasswordHasher` | Hash ve verify (BCrypt vb. Infrastructure). |
| `IOrderNumberService` | `OrderNumber` üretimi; thread-safe olmalı (XML). |

---

## Soft delete ve silme stratejisi (özet)

| Varlık | Strateji |
|--------|----------|
| `User`, `Product`, `Category`, `DiscountCoupon` | `IsActive` veya durum ile **soft** devre dışı. |
| `Address`, `Review` (kullanıcı silmesi) | **Fiziksel** silme / kaldırma desteklenir (entity yorumları). |
| `OrderTracking` | Denetim izi; silme modeli iş kuralına bağlı (genelde append-only). |

EF Core global query filter kullanımı Infrastructure kuralında tanımlanır; Domain yalnızca anlamı taşır.

---

## Tasarım ilkeleri (bu repoda gözlenenler)

1. **Kapsülleme:** `private set` + anlamlı metot adları.
2. **Invariant’lar:** Fabrika ve metotlarda tutarlılık (negatif tutar, yanlış durum geçişi vb.).
3. **Snapshot:** Sipariş adresi ve kalem satırları, katalog değişse bile geçmişi bozmaz.
4. **Olaylar:** Yan etkileri domain’den çıkarmak için `IDomainEvent` toplama.
5. **Güvenlik:** OTP düz metin değil hash; ödeme hassas alanları yok; refresh token log uyarısı.
6. **Uluslararasılaştırma:** Birçok iş kuralı mesajı Türkçe sabit metin — API katmanında kullanıcı diline çeviri katmanı eklenebilir.

---

## İlgili dokümantasyon ve kurallar

- Varlık ve olay özeti: `.cursor/rules/gulora-domain-entities.mdc`
- EF eşlemesi ve DTO sınırları: `.cursor/rules/gulora-infrastructure-contracts.mdc`
- MediatR / pipeline: `.cursor/rules/gulora-cqrs-patterns.mdc`
- Çözüm genel mimarisi: `.cursor/rules/gulora-architecture.mdc`

---

## Dosya envanteri (`.cs` kaynakları)

**Common:** `BaseEntity.cs`, `BaseAggregateRoot.cs`, `IDomainEvent.cs`

**Entities:** `Address.cs`, `Category.cs`, `DiscountCoupon.cs`, `Notification.cs`, `Order.cs`, `OrderItem.cs`, `OrderTracking.cs`, `Payment.cs`, `PhoneOtpChallenge.cs`, `Product.cs`, `ProductImage.cs`, `RefreshToken.cs`, `Review.cs`, `SellerApplication.cs`, `SellerShop.cs`, `User.cs`, `Wishlist.cs`, `WishlistItem.cs`

**Enums:** `DeliveryType`, `DiscountType`, `NotificationType`, `OrderStatus`, `PaymentMethod`, `PaymentStatus`, `ReviewStatus`, `SellerApplicationStatus`, `UserRole`

**Events:** `OrderCancelledEvent`, `OrderCreatedEvent`, `OrderStatusChangedEvent`, `ProductStockUpdatedEvent`, `ReviewSubmittedEvent`, `UserPasswordChangedEvent`, `UserRegisteredEvent`

**Exceptions:** `BusinessRuleException`, `ConflictException`, `DomainException`, `ForbiddenException`, `NotFoundException`, `TooManyRequestsException`, `UnauthorizedException`, `ValidationException`

**Interfaces:** `ICategoryRepository`, `IDiscountCouponRepository`, `INotificationRepository`, `IOrderRepository`, `IPaymentRepository`, `IPhoneOtpRepository`, `IProductRepository`, `IRepository`, `IReviewRepository`, `ISellerApplicationRepository`, `ISellerShopRepository`, `IUnitOfWork`, `IUserRepository`, `IWishlistRepository`, `SellerDashboardOrderSnapshot`

**Services:** `IDeliveryFeeCalculator`, `IDiscountCalculator`, `IOrderNumberService`, `IPasswordHasher`

**ValueObjects:** `AddressSnapshot`, `Email`, `Money`, `OrderNumber`, `PhoneNumber`

---

*Bu README, `Gulora.Domain` kaynak ağacının mevcut haliyle uyumlu olacak şekilde oluşturulmuştur. Yeni entity veya arayüz eklendiğinde ilgili bölümlerin güncellenmesi önerilir.*
