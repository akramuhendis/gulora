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
