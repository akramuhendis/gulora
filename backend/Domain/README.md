# Gülora.Domain — Katman Rehberi

Bu proje, Gülora backend’inin **iş kurallarını ve veri modelini** taşıyan saf domain katmanıdır. `Gulora.Domain` ad alanı altında toplanır; **.NET 9** hedeflenir ve harici çerçevelere (EF Core, MediatR, HTTP, FluentValidation vb.) **bağımlılık içermez**. Böylece uygulama ve altyapı değişse bile iş modeli tek yerde kalır.

---

## Dizin yapısı (özet)

| Klasör | Amaç |
|--------|------|
| `Common/` | Tüm kalıcı varlıklar için ortak taban (`BaseEntity`). |
| `Entities/` | Aggregate kökleri ve ilişkili varlıklar; kalıcı domain modeli. |
| `Enums/` | Durum, rol, ödeme ve teslimat gibi sabit kümeler. |
| `ValueObjects/` | Kimliği olmayan, değer eşitliği ile karşılaştırılan tipler (`record`). |
| `Exceptions/` | İş kuralı ve uygulama sınırı istisnaları. |
| `Events/` | Domain içi olay sözleşmesi ve temel kayıt tipi (isteğe bağlı entegrasyon). |

Derleme çıktıları (`bin/`, `obj/`) kaynak yapısının parçası değildir; sürüm kontrolüne genelde dahil edilmez.

---

## `Common/` — Ortak taban

### `BaseEntity`

Tüm kalıcı entity’ler `BaseEntity` üzerinden türetilir:

- **`Id`**: `Guid`; yeni örnekte varsayılan olarak `Guid.NewGuid()`.
- **`CreatedAt`**: UTC oluşturulma zamanı.
- **`UpdatedAt`**: İlk kayıtta `null`; anlamlı değişikliklerde doldurulur.
- **`SetUpdated()`**: `protected`; türetilmiş sınıflar domain metotları içinden çağırır.

Kimlik ve zaman damgaları dışarıdan doğrudan atanmaz; `protected set` ile yalnızca türetilmiş sınıflar günceller.

**Not:** `BaseEntity` üzerinde domain olay kuyruğu yoktur; olay tabanlı tasarım seçilirse ya tabana koleksiyon + yardımcı metotlar eklenir ya da yalnızca ilgili aggregate köklerinde tutulur.

---

## `Entities/` — Varlıklar

Her dosya genelde **tek bir aggregate parçası** veya **ilişkili entity**’dir. Ortak kalıplar:

1. **`BaseEntity`’den türeme** — ortak `Id` / zaman damgaları.
2. **`private set`** (veya `protected set` yalnızca tabanda) — dışarıdan rastgele atama yok.
3. **Parametresiz `private` ctor** — EF Core (veya başka ORM) için zorunlu desen.
4. **Fabrika veya domain metotları** — örn. `Order.Create(...)`, `Confirm()`, `Cancel()`; durum geçişleri yalnızca bu metotlarla.
5. **Koleksiyonlar** — içeride `List<T>`, dışarıya `IReadOnlyCollection<T>` ile kapsülleme.

Projede yer alan entity örnekleri:

| Dosya | Rol (kısa) |
|-------|------------|
| `User.cs` | Kullanıcı hesabı. |
| `Product.cs`, `Category.cs` | Ürün ve kategori. |
| `Order.cs` | Sipariş kökü; satırlar ve takip ile birlikte aggregate. |
| `OrderItem.cs`, `OrderTracking.cs` | Sipariş satırı ve durum zaman çizelgesi. |
| `Address.cs` | Kullanıcı adresi. |
| `Coupon.cs` | Kupon tanımı. |
| `Review.cs`, `Favorite.cs` | Değerlendirme ve favori. |
| `SellerApplication.cs` | Satıcı başvurusu. |
| `CorporateInquiry.cs` | Kurumsal talep / iletişim. |
| `RefreshToken.cs` | Oturum yenileme token’ı. |

Yeni entity eklerken aynı klasör ve adlandırma düzenini korumak, CQRS / API katmanlarıyla uyumu kolaylaştırır.

### Model notları (bilerek veya sonradan tamamlanacak davranışlar)

Aşağıdakiler hata değil; **ürün geliştirme sırasında netleştirilecek** boşlukların özeti:

| Alan | Durum |
|------|--------|
| **`Product`** | Birçok alan (`OriginalPrice`, `DiscountPercent`, `Badge`, `IsSameDay`, `IsFreeDelivery`, `Description`, `IsActive`, fiyat/ad/slug/kategori) için henüz domain metodu yok; stok **artırma** (iptal/iade) için `DecreaseStock`’un tersi tanımlı değil. |
| **`Category`** | `SortOrder` ve `ImageUrl` oluşturma sonrası güncellemek için metot yok. |
| **`Review`** | Düzenleme / silme / moderasyon akışı yok; `Verify` idempotent değil. |
| **`Product.UpdateRating`** | Puan ve sayaç aralığı domain içinde sıkı doğrulanmıyor (çağıran katmanın sorumluluğunda). |
| **`Coupon`** | Pasifleştirme, alan güncelleme, kullanım geri alma senaryoları yok. |
| **`User`** | `PromoteToSeller` dışında rol yönetimi (ör. admin atama) yok. |
| **`Favorite`** | Aynı kullanıcı+ürün tekrarı domain seviyesinde ele alınmıyor; genelde veritabanı benzersiz indeksi ile korunur. |
| **`OrderLine`** | `record`; doğrulama büyük ölçüde `OrderItem.Create` ve uygulama katmanına dayanır. İstenirse `static Create` ile tek doğrulama noktası eklenebilir. |

---

## `Enums/` — Numaralandırmalar

İş anlamı taşıyan sabit kümeler burada tutulur. Örnek kavramlar: sipariş durumu, teslimat tipi, ödeme yöntemi, indirim tipi, kullanıcı rolü, satıcı başvuru durumu.

- Açık sayısal değerler (`= 0`, `= 1`, …) veritabanı ve serileştirme için tercih edilir.
- Entity üzerinde “durum makinesi” kuralları, ilgili enum ile domain metotlarında birlikte uygulanır.

---

## `ValueObjects/` — Değer nesneleri

Kimliği olmayan, **eş değerlik** ile anlam taşıyan yapılar `record` olarak tanımlanır:

- **`Money`**: Tutar + para birimi (varsayılan `AZN`); `Add`, `Subtract`, `Multiply` gibi immutabl işlemler yeni örnek döndürür. `Amount` negatifliği ve `Multiply` çarpan işaretini şu an üst katmanlara bırakır; sıkı kurallar istenirse bu VO içinde toplanabilir.
- **`AddressSnapshot`**: Sipariş anındaki adres kopyası; kullanıcı adresi değişse bile sipariş sabit kalır.
- **`OrderLine`**: Sipariş oluştururken ürün / miktar / fiyat gibi satır verisini taşır; kök aggregate içinde `OrderItem`’a dönüşür.

Value object’ler veritabanında ayrı tablo olabilir veya JSON sütununa gömülebilir; bu karar **Infrastructure / Persistence** katmanındadır; domain yalnızca tipi tanımlar.

---

## `Exceptions/` — İstisnalar

| Tip | Kullanım |
|-----|----------|
| `DomainException` | İş kuralı ihlali (geçersiz durum geçişi, boş sepet vb.). API’de genelde 400 / 422 ile eşlenir. |
| `NotFoundException` | Varlık anahtarına göre kayıt bulunamadığında; tip adı + anahtar mesajda yer alır. |
| `UnauthorizedException` | Kimlik / yetki hatası; genelde 401 / 403 ile eşlenir. |

Mesajların güvenli ve mümkünse sabit metin olması log ve istemci için iyidir. Repodaki `.cursor/rules` altında **istisna metin dili** (ör. İngilizce) ile **istemci mesajları** (ör. Azerbaycanca DTO) ayrımı tanımlanmış olabilir; yeni kodda bu kurallara uyum hedeflenmelidir.

---

## `Events/` — Domain olayları

- **`IDomainEvent`**: Olayın gerçekleştiği zaman (`OccurredOn`, UTC) ve genişletilebilir sözleşme.
- **`DomainEventBase`**: `record` taban; `OccurredOn` varsayılanı örnek oluşturma anıdır.

Bu olaylar MediatR, dahili bir dispatcher veya outbox ile **isteğe bağlı** bağlanır; domain projesi bu entegrasyonlara referans vermez.

**Şu anki durum:** Sözleşme ve taban tip mevcuttur; **hiçbir aggregate henüz olay üretmez** ve `BaseEntity` olay koleksiyonu tutmaz. Ya ileride “raise + dispatch” desenine bağlanır ya da kullanılmayacaksa sözleşme sadeleştirilebilir.

---

## Katman kuralları (özet)

1. **Dış bağımlılık yok** — EF, HTTP, doğrulama kütüphaneleri bu projede referanslanmaz.
2. **Kapsülleme** — özellikler `private set`; değişiklik yalnızca domain metotlarıyla.
3. **Doğrulama** — ctor / fabrika veya metot girişinde; geçersiz girişte `ArgumentException` veya `DomainException`.
4. **Klasör sözleşmesi** — `Entities/`, `Enums/`, `ValueObjects/`, `Exceptions/` yapısı korunur; `Common/` ve `Events/` bu yapıya eşlik eder.

Ayrıntılı ekip kuralları için repoda `.cursor/rules/gulora-backend-domain.mdc` dosyasına bakılabilir.

---

## Diğer projelerle ilişki

- **Application**: Komut/sorgu işleyicileri domain tiplerini kullanır; orchestrasyon burada.
- **Infrastructure**: EF eşlemeleri, veritabanı, dış servisler; domain’e ters bağımlılık (interface’ler application’da tanımlanabilir).
- **API**: HTTP; domain istisnalarını HTTP kodlarına çevirir.

`Gulora.Domain.csproj` yalnızca SDK ile minimal tutulur; domain’in “saf” kalması bilinçli bir tasarım seçimidir.

---

## Hızlı kontrol listesi (yeni kod)

- [ ] Entity `BaseEntity`’den türüyor mu?
- [ ] Parametresiz `private` ctor ve anlamlı fabrika / metotlar var mı?
- [ ] Durum ve koleksiyonlar dışarıdan doğrudan değiştirilemiyor mu?
- [ ] Bu projeye altyapı veya web paketi eklenmedi mi?
- [ ] Enum / value object / exception doğru klasörde mi?
- [ ] Yeni iş kuralı için olay yayınlanacaksa: olay tipi, üretim yeri ve temizleme (save öncesi/sonrası) net mi?

Bu README, `backend/Domain` kökünde tutulur ve klasör yapısıyla birlikte güncellenmelidir.
