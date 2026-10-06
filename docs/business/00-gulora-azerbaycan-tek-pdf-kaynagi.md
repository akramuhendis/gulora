# Gülora — Azerbaycan iş paketi (tek PDF kaynağı)

Bu metin aşağıdaki iş belgelerinin **tek dosyada birleştirilmiş** halidir: strateji özeti (03), ICP (01), pilot rehberi (02), pilot taahhüt özeti (05), vaka şablonu (06).

**PDF açılmazsa:** `scripts/export-markdown-to-pdf.ps1` çalıştırıldığında aynı ada sahip dosya **Masaüstüne** de kopyalanır: `Gulora-Azerbaycan-tum-paket.pdf` (klasör adında Türkçe karakter olmayan tam yol; çift tıklayarak açın).

---

# Bölüm 1 — Strateji, kurulum, büyüme ve kar

# Gülora: Azerbaycan pazarında kurulum, büyüme ve kar

**Belge türü:** İş ve ürün stratejisi (iç kullanım)  
**Son güncelleme:** 2026-05-15  
**İlişkili repolar:** [AGENTS.md](../../AGENTS.md), domain özeti [Gulora.Domain README](../../backend/src/Gulora.Domain/README.md)

---

## Feragatname

Bu metin **hukuki, vergi veya yatırım tavsiyesi değildir**. Şirket kuruluşu, sözleşmeler, ödeme lisansları, KDV ve kişisel veri işleme konularında **Azerbaycan’da yetkili muhasebe ve hukuk danışmanı** kullanılmalıdır.

---

## 1. Yönetici özeti

Gülora; müşteri vitrini (`frontend/store`), satıcı paneli (`frontend/saller`), ASP.NET Core 9 tabanlı API ve PostgreSQL ile **çok taraflı e-ticaret** yetenekleri sunar. Domain modeli **AZN** ve ülke kodu **AZ** varsayımlarıyla uyumludur (ör. adres ve para birimi değer nesneleri).

Stratejik öneri: İlk fazda **tek net ICP** (ör. belirli bir ürün dikeyi veya yerel üretici kümesi) seçerek **sipariş–stok–iade–satıcı operasyonu** “kahraman akışını” doğrulayın; ardından gelir modelini **bir ana + bir yardımcı** kaleme indirgenmiş şekilde ölçekleyin.

---

## 2. Ürün ve kod tabanı hizası

### 2.1 Müşteri tarafı

- Katalog, sepet, sipariş, teslimat türleri, ödeme yöntemleri (kart, kapıda ödeme, havale), kupon, istek listesi, bildirimler.  
- Sipariş yaşam döngüsü ve iade talebi (`Order` durum makinesi, `RequestReturn` / `CompleteReturn` çizgisi — bkz. domain README).  
- Değerlendirme moderasyonu (`Review`).

### 2.2 Satıcı tarafı

- Satıcı başvurusu (`SellerApplication`) ve vitrin (`SellerShop`).  
- Ürün, stok, sipariş listeleri ve satıcı dashboard okuma modelleri.  
- Kampanya / planlama sinyali (`SellerCampaignPlan`).

### 2.3 Teknik güven ve ölçek

- JWT kısa ömür + refresh (DB), BCrypt; ödemede kart/CVV saklama yok (`Payment`).  
- Idempotency anahtarı ile ödeme çağrıları; Clean Architecture ve CQRS ile evrilebilirlik.

**Sonuç:** Pazaryeri (B), D2C çok mağaza (A) veya satıcı SaaS (C) pozisyonlarının hepsi bu mimariyle desteklenebilir; pazar mesajını tek seçeneğe indirgemek satış ve ürün önceliği için kritiktir.

---

## 3. Azerbaycan’da kurulum çerçevesi

| Alan | Uygulama notları |
|------|-------------------|
| Tüzel kişi | Yerel limited şirket (MMC); faaliyet kodlarında e-ticaret, perakende veya yazılım faaliyetinin uyumu. |
| Banka | AZN işletme hesabı; yerel banka veya ödeme hizmeti sağlayıcı ile sanal POS / gateway. |
| Sözleşmeler | Kullanım şartları, gizlilik ve çerez; çok satıcılı modelde satıcı sözleşmesi ve komisyon / faturalama kuralları. |
| Tüketici | İade ve cayma politikası, müşteri hizmetleri SLA’sı, şikâyet kanalı. |
| Operasyon | İade toplama adresi, kargo anlaşmaları, kapıda ödeme tahsilat mutabakatı. |
| Dil | Azerbaycan Türkçesi birincil; gerektiğinde Rusça/İngilizce destek katmanı. |

---

## 4. ICP ve konumlandırma

Ayrıntılı ICP tablosu: [01-ICP-ve-deger-onerisi.md](./01-ICP-ve-deger-onerisi.md).

**Özet öneri:** “Her şeyi satan” pazar yeri yerine, **ölçülebilir acı** olan segment (yüksek iade, çok kanal, stok hatası, manuel WhatsApp/Excel) ile başlayın. İlk 6–12 ay tek dikey veya tek şehir kümesi (örn. Bakü ve çevresi) ile sınırlandırın.

---

## 5. Go-to-market: 0 → 1 → 10

### Faz 0 — Doğrulama (4–8 hafta)

- 10–20 yapılandırılmış görüşme — çerçeve: [02-pilot-dogrulama.md](./02-pilot-dogrulama.md).  
- Tek “kahraman” akışı: sipariş → kargo takibi → iade.  
- Başarı ölçütleri: en az **3 pilot taahhüdü** veya ön ödeme / LOI.

### Faz 1 — Pilot (8–12 hafta)

- 3–7 satıcı veya tek marka, çok depo senaryosu.  
- Haftalık operasyon görüşmesi; KPI: teslim süresi, iade oranı, sepet terki, GMV, destek başına sipariş.

### Faz 2 — Büyüme (12+ hafta)

- SEO/içerik, influencer, satıcı edinimi kanalları.  
- `SellerApplication` sürecini ölçülebilir SLA ile ölçekleme.

**Akış özeti (diyagram yerine metin):** Keşif görüşmeleri → Pilot (3–7) → Canlı GMV hedefi → Satıcı edinimi ve entegrasyonlar.

---

## 6. Gelir modeli ve birim ekonomi

### 6.1 Gelir akışları (1 ana + 1 yardımcı öneri)

1. **İşlem komisyonu** — GMV üzerinden yüzde; pazar yeri modeli.  
2. **SaaS abonelik** — ay/yıl; SKU, satıcı sayısı, API limitleri ile paketler.  
3. **Öne çıkarma / reklam** — şeffaf açık artırma veya sabit yerleştirme.  
4. **Hizmet** — kurulum, eğitim, özel entegrasyon (muhasebe, kargo API).

### 6.2 Maliyetler

Ödeme sağlayıcı komisyonu, iade lojistiği, destek iş gücü, bulut (PostgreSQL, CDN, e-posta/SMS OTP), pazarlama CAC, dolandırıcılık/chargeback.

### 6.3 Planlama göstergeleri

Her senaryo için tabloda hedef tutun: **take rate** veya **MRR**, **CAC geri ödeme süresi**, **brüt marj**, **GMV başına destek dakikası**.

---

## 7. Rekabet ve savunma

| Alternatif | Güçlü yön | Zayıf yön | Gülora açısı |
|------------|-----------|-----------|----------------|
| Genel e-ticaret SaaS | Hızlı kurulum | Yerel ödeme/kargo, dil | Kendi kodunuz, AZ iş akışlarına uyarlama |
| Büyük pazar yeri | Trafik | Komisyon, marka kontrolü | Satıcıya özel vitrin ve operasyon derinliği |
| Özel yazılım | Tam uyum | Pahalı, yavaş yenileme | Clean Architecture ile modüler gelişim |
| Hiçbir şey (manuel) | Düşük ilk maliyet | Hata, ölçek yok | Ölçülebilir KPI ile değiştirme |

---

## 8. Riskler ve azaltma

| Risk | Azaltma |
|------|---------|
| Ödeme dolandırıcılığı | Idempotency, `Payment` durum makinesi, denetim günlüğü |
| İade suistimali | Net politika, `Order` geçişleri, şablon yanıtlar |
| Satıcı kalitesi | `SellerApplication` incelemesi, `Review` moderasyonu |
| Teknik borç | `.cursor/rules` ve CI disiplini |

---

## 9. 12 aylık kilometre taşları (özet)

| Çeyrek | İş | Ürün/teknik |
|--------|-----|-------------|
| Q1 | ICP + 10 görüşme + 3 pilot | Kahraman akışı, AZ ödeme pilotu |
| Q2 | İlk ücretli müşteri | Satıcı dashboard KPI, kargo entegrasyonu |
| Q3 | 10–30 aktif satıcı veya GMV hedefi | Kampanya MVP, performans |
| Q4 | Tekrarlayan gelir + referans | Güvenlik/ölçek testleri, CI/CD |

---

---

# Bölüm 2 — ICP, değer önerisi ve rakip farkı

# Gülora — ICP, değer önerisi ve rakip farkı (tek sayfa)

Bu belge [AGENTS.md](../../AGENTS.md)’deki ürün yönünü (müşteri mağazası + satıcı paneli, React + ASP.NET Core) varsayar. Aşağıdaki tablolar **Azerbaycan (AZ) odaklı başlangıç varsayımları** ile doldurulmuştur; rakamları kendi görüşmelerinizle güncelleyin ([02-pilot-dogrulama.md](./02-pilot-dogrulama.md)).

## 1. İdeal müşteri profili (ICP)

| Alan | Tanım |
|------|--------|
| **Segment** | Çok kanallı KOBİ: Instagram/WhatsApp’tan satış yapan, ayrıca kendi sitesi veya vitrin ihtiyacı olan; veya tek marka D2C ile 2+ depo / kurye tipi yöneten perakendeci. |
| **Coğrafya** | **Azerbaycan** (öncelik Bakü ve metropol; sonra Gəncə, Sumqayıt). Başlangıçta tek ülke (AZ); para birimi **AZN**. |
| **Şirket büyüklüğü** | 1–25 çalışan; aylık **150–5000** sipariş bandı (doğrulama ile daraltın). |
| **Gelir / sipariş hacmi** | Aylık brüt **~50.000–500.000 AZN** GMV bandı (tahmini; sektöre göre değişir). |
| **Teknoloji olgunluğu** | Excel/Google Sheets stok; WhatsApp sipariş; mevcut banka POS veya kapıda nakit; bazen harici pazar yeri vitrini; entegrasyon ihtiyacı “sonra” değil “şimdi” hissi. |
| **Satın alma süreci** | Karar: genelde işveren veya operasyon müdürü; finans onayı 1–2 adım; tipik döngü **2–6 hafta** (KOBİ), pilot ile düşürülebilir. |
| **Dışlayıcılar (kim değil)** | Yalnız fiziksel mağaza ve online kanal istemeyenler; entegrasyon veya fatura disiplinine sıfır uyum; regülasyon açısından riskli ürün kategorileri (özel lisans gerektiren); yıllık sipariş sayısı çok düşük hobi satıcıları. |

## 2. Çözdüğünüz problem (acı)

- **Şu an nasıl yapıyorlar?** Sipariş ve stok WhatsApp/Excel’de; iade ve kargo takibi dağınık; müşteriye durum netliği yok; çok satıcılı modelde komisyon ve mutabakat manuel.
- **Maliyet:** Operasyon başına **saat** ve hata (yanlış stok, çift satış, iade gecikmesi), müşteri kaybı, kapıda ödeme tahsilat farkları, destek yükü.
- **Tetikleyici olay:** Kampanya veya sezonda sipariş patlaması; iade oranı artışı; yeni kurye / ödeme sağlayıcısı eklenmesi; pazar yeri komisyon baskısı nedeniyle **kendi vitrinine** geçme kararı.

## 3. Değer önerisi (tek cümle)

> **[ICP]** için **[ölçülebilir sonuç]** sağlıyoruz; **[Z süre / Z maliyet]** ile **[mevcut alternatif]**e kıyasla **[fark]**.

**Doldurulmuş öneri (doğrulamaya tabi):**

> Azerbaycan’da çok kanallı satan KOBİ ve tek marka D2C işletmeleri için **sipariş–stok–iade ve satıcı operasyonunu** tek panelde birleştiriyoruz; **90 gün içinde** operasyon ekranında geçirilen süreyi ve iade sürtünmesini ölçülebilir şekilde düşürmeyi hedefliyoruz; **Excel + WhatsApp + dağınık kargo takibi** yerine **Gülora müşteri mağazası + satıcı paneli** ile şeffaflık ve denetim izi sağlıyoruz.

## 4. Rakip ve alternatifler

| Alternatif | Güçlü yön | Zayıf yön | Sizin açı |
|------------|-----------|-----------|-----------|
| Genel e-ticaret SaaS | Hızlı vitrin, şablon | Yerel AZN ödeme/kapıda ödeme, yerel kargo SLA, dil/destek | Kendi stack; domain’de AZN ve AZ adres varsayımları; özelleştirilebilir akış |
| Pazar yeri komisyonu | Hazır trafik | Yüksek komisyon, marka kontrolü zayıf | Kendi GMV’iniz; take rate’i siz belirlersiniz veya SaaS ile sabitlersiniz |
| Özel yazılım | Tam uyum | Maliyet, bakım, zaman | Clean Architecture + CQRS; modül modül evrim |
| Hiçbir şey (manuel) | Düşük ilk yatırım | Ölçek yok, hata | Pilot KPI ile somut karşılaştırma |

## 5. Ölçülebilir fark (3 madde)

1. Uçtan uca müşteri akışı: `frontend/store` vitrin → sepet → ödeme (kart / kapıda / havale) → hesap ve **iadeler**.
2. Satıcı operasyonu: `frontend/saller` sipariş, stok, kampanya planı sinyali; başvuru ve vitrin (`SellerApplication`, `SellerShop`).
3. Mimari: JWT + refresh, ödemede hassas veri saklamama, idempotency; kurumsal güven ve sürdürülebilir evrim.

## 6. Kanıt planı (30 gün)

- [ ] 10 yapılandırılmış görüşme (bkz. `02-pilot-dogrulama.md`)
- [ ] En az 3 pilot taahhüdü veya ön ödeme niyeti (bkz. `05-pilot-taahhut-ozeti.md`)
- [ ] Bir referans müşteri hikâyesi (anonimleştirilmiş)

---

*Bu belge iş stratejisi taslağıdır; hukuki veya mali tavsiye değildir.*

---

# Bölüm 3 — Pilot doğrulama (görüşme ve takip)

# Pilot doğrulama — görüşme rehberi ve takip

Amaç: **10–20** hedef profil ile yapılandırılmış görüşme; sonunda **ödeme niyeti** veya **pilot taahhüdü** (tarih + kapsam + ücret veya indirimli pilot).

**İlgili:** [01-ICP-ve-deger-onerisi.md](./01-ICP-ve-deger-onerisi.md), [03-azerbaycan-strateji-kar-uclan-belge.md](./03-azerbaycan-strateji-kar-uclan-belge.md), pilot taahhüt yuvaları: [05-pilot-taahhut-ozeti.md](./05-pilot-taahhut-ozeti.md).

## 1. Aday havuzu

Ön koşul: `01-ICP-ve-deger-onerisi.md` ile uyumlu.

| # | Kurum / kişi | Rol | Kanal (nasıl bulundu) | Durum |
|---|--------------|-----|------------------------|--------|
| 1 | | | | Planlandı / Yapıldı |
| 2 | | | | Planlandı / Yapıldı |
| 3 | | | | Planlandı / Yapıldı |
| 4 | | | | Planlandı / Yapıldı |
| 5 | | | | Planlandı / Yapıldı |
| 6 | | | | Planlandı / Yapıldı |
| 7 | | | | Planlandı / Yapıldı |
| 8 | | | | Planlandı / Yapıldı |
| 9 | | | | Planlandı / Yapıldı |
| 10 | | | | Planlandı / Yapıldı |

Hedef: en az **10** tamamlanmış görüşme; ideal **15–20**. Ek görüşmeler için tabloya satır ekleyin.

## 2. 30 dakikalık görüşme akışı

1. **Isınma (2 dk):** Amaç (öğrenme, satış değil), kayıt izni.
2. **Bağlam (5 dk):** Rol, sipariş hacmi, kanallar, mevcut araçlar.
3. **Problem derinliği (10 dk):** Son 90 gündeki en kötü operasyon anı; iade/lojistik/ödeme/stok hangisi acıtıyor?
4. **Mevcut çözüm (5 dk):** Maliyet, entegrasyonlar, memnuniyet.
5. **Çözüm tasviri (5 dk):** Tek ekranlı demo veya wireframe; “Böyle olsaydı ne değişirdi?”
6. **Kapanış (3 dk):** Pilot teklifi (aşağıdaki metin) + takip tarihi.

### Kaçınılması gerekenler

- Evet/hayır anketleri yerine davranış ve son örnek olaylar.
- Fiyat sormadan önce değeri anlamadan fiyat vermek.
- Tüm özellik listesini anlatmak; tek ana akışa odaklanın.

## 3. Temel soru seti (işaretleyin)

- [ ] Haftalık sipariş / iade hacmi nedir?
- [ ] Karar verici kim, bütçe ne zaman açılır?
- [ ] “Hayır” dediğiniz son çözüm neydi ve neden?
- [ ] Başarıyı 90 günde nasıl ölçersiniz? (KPI)
- [ ] Veri ve KVKK/GDPR kısıtları var mı?

## 4. Pilot teklifi (kopyala-yapıştır — AZ bağlamı)

> **2026-__-__** itibarıyla **8 hafta** süreli sınırlı pilot: **sipariş → kargo takibi → iade talebi** ana akışı; müşteri tarafında AZN fiyat ve teslimat seçenekleri, satıcıda sipariş/stok listesi.  
> Pilot ücreti: **[ücretsiz / indirimli / ücretli — tutarı yazın]** AZN veya eşdeğeri. Karşılığında: haftalık 30 dk geri bildirim, anonim kullanım metrikleri, isteğe bağlı vaka izni.  
> Go-live kriteri: **90 gün içinde** operasyon ekranında geçirilen süre veya iade çözüm süresinde **%__** iyileşme (birlikte tanımlanacak KPI).

## 5. Ödeme niyeti sinyalleri (puanlama)

| Sinyal | Puan |
|--------|------|
| Toplantı sonrası “bir sonraki adım” tarihi verdi | +2 |
| Bütçe kalemi veya satın alma sürecini anlattı | +2 |
| Pilot için veri/API erişimi taahhüdü | +3 |
| Ön ödeme veya imzalı LOI | +5 |

**Eşik:** Toplam **≥5** ise “sıcak pilot”; **≥8** ise önceliklendir.

### Görüşme puan özeti (doldurun)

| Görüşme # | Tarih | Puan (Bölüm 5 tablosu) | Sıcak pilot? |
|-----------|-------|-----------|----------------|
| 1 | | | |
| 2 | | | |
| 3 | | | |
| 4 | | | |
| 5 | | | |
| 6 | | | |
| 7 | | | |
| 8 | | | |
| 9 | | | |
| 10 | | | |

## 6. Görüşme notları şablonu

```
Tarih:
Katılımcı / şirket:
Rol:
Özet (3 cümle):
Acı (verbatim alıntı):
Mevcut stack:
Itirazlar:
Sonraki adım + tarih:
Puan (tablo 5):
```

## 7. Pilot taahhüt operasyonu

1. Sıcak adayları `05-pilot-taahhut-ozeti.md` tablosuna taşıyın (en fazla 3 eşzamanlı pilot yuvası önerilir).  
2. Her pilot için başlangıç tarihi, ücret/indirim ve go-live KPI’sını yazılı teyit edin (yerel hukuka uygun sözleşme/ek protokol).  
3. Haftalık geri bildirim notlarını dahili CRM’de saklayın; tamamlandığında `06-pilot-vaka-ozeti.md` ile anonim özet oluşturun.

## 8. Tamamlama kriteri

Aşağıdakiler tamamlandığında bu çerçeve **operasyonel olarak tamamlanmış** sayılır:

- [ ] Tabloda **10** “Yapıldı” görüşme satırı (kurum/kişi ve rol dolu).  
- [ ] `05-pilot-taahhut-ozeti.md` içinde **3** pilot satırı “Teyit / İmzalı” durumuna getirildi.  
- [ ] En az bir anonimleştirilmiş vaka özeti (bkz. `06-pilot-vaka-ozeti.md`).

---

*Bu belge hukuki taahhüt değildir; pilot sözleşmesi için yerel hukuk danışmanı kullanın.*

---

# Bölüm 4 — Pilot taahhüt özeti

# Pilot taahhüt özeti

Bu dosya [02-pilot-dogrulama.md](./02-pilot-dogrulama.md) ile birlikte kullanılır. Hedef: en az **3** imzalı veya yazılı pilot taahhüdü / LOI / ön ödeme kaydı.

**Feragatname:** Hukuki taahhüt değildir; sözleşme için yerel hukuk danışmanı kullanın.

## Taahhüt kayıtları (3 yuva)

| # | Kurum / kişi | Tarih | Kapsam (kahraman akış) | Ücret / indirim | Go-live kriteri | Durum |
|---|--------------|-------|-------------------------|-----------------|-----------------|--------|
| 1 | | | Sipariş → kargo → iade | | | Taslak |
| 2 | | | Sipariş → kargo → iade | | | Taslak |
| 3 | | | Sipariş → kargo → iade | | | Taslak |

## Sonraki adımlar (kontrol listesi)

- [ ] Her taahhüt için karşı tarafta imza yetkisi doğrulandı
- [ ] Veri işleme ve NDA ihtiyacı netleştirildi
- [ ] Pilot başlangıç tarihi ve haftalık geri bildirim slotu takvime alındı

---

# Bölüm 5 — Pilot vaka özeti (şablon)

# Pilot vaka özeti (anonimleştirilmiş)

[02-pilot-dogrulama.md](./02-pilot-dogrulama.md) Bölüm 8 tamamlama kriteri için kullanın. Gerçek isim/marka yerine kod adları kullanın.

## Vaka kodu: ___________

| Alan | İçerik |
|------|--------|
| Segment (ICP) | |
| Coğrafya | |
| Önceki durum (araçlar) | |
| Pilot kapsamı | |
| KPI (başlangıç → pilot sonu) | |
| Alıntı (izinli, anonim) | |
| Sonraki adım (ücretli sözleşme / genişletme) | |

---

*Hukuki taahhüt değildir; müşteri izni olmadan paylaşmayın.*

---

*Bu birleşik belge iş stratejisi içindir; hukuki veya mali tavsiye değildir.*