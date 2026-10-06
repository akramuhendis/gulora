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
