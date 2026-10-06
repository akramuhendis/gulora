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

## 10. PDF üretimi (teknik)

Bu dosya kaynaktır. PDF oluşturmak için aşağıdakilerden biri kullanılabilir:

- **Microsoft Edge (headless, Windows):** [scripts/export-strategy-pdf.ps1](../../scripts/export-strategy-pdf.ps1) betiği Markdown’ı geçici HTML’e sarar ve `msedge --print-to-pdf` ile `docs/business/03-azerbaycan-strateji-kar-uclan-belge.pdf` üretir. Feragatname kaynak `.md` dosyasının başındadır ve PDF’e taşınır.
- **Pandoc + XeLaTeX:** `pandoc 03-azerbaycan-strateji-kar-uclan-belge.md -o 03-azerbaycan-strateji-kar-uclan-belge.pdf --pdf-engine=xelatex -V lang=tr`
- **npx md-to-pdf:** `npx --yes md-to-pdf 03-azerbaycan-strateji-kar-uclan-belge.md` (ilk çalıştırmada Chromium indirilebilir).
- **Düzenleyici:** Markdown önizleme → Yazdır → PDF olarak kaydet.

Çıktı dosyası: [03-azerbaycan-strateji-kar-uclan-belge.pdf](./03-azerbaycan-strateji-kar-uclan-belge.pdf) (üretildiyse).

---

## 11. İlgili belgeler

- [01-ICP-ve-deger-onerisi.md](./01-ICP-ve-deger-onerisi.md)  
- [02-pilot-dogrulama.md](./02-pilot-dogrulama.md)  
- [05-pilot-taahhut-ozeti.md](./05-pilot-taahhut-ozeti.md)  
- [06-pilot-vaka-ozeti.md](./06-pilot-vaka-ozeti.md)
