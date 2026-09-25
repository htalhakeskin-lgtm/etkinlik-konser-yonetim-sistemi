# ADR-0022: Gelecekteki duvar saati zamanları

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [ADR-0017](0017-time-and-money-types.md) (bu ADR onu genişletir), [standards/database.md §7.2](../standards/database.md#72-gelecekteki-duvar-saati-zamanları)

## Bağlam

[ADR-0017](0017-time-and-money-types.md) tüm anların UTC saklanmasını kararlaştırdı. Bu, olmuş olaylar (okutma, onay) için doğrudur. Ama kullanıcının gelecekteki bir etkinlik için girdiği "kapılar 19:00'da açılır" bilgisi bir UTC anı değil, **bir yerin duvar saatidir.**

Saat dilimi kuralları siyasi kararlarla değişir:
- Türkiye 2015'te seçim nedeniyle kış saatine geçişi iki hafta erteledi.
- Eylül 2016'da da bir ay önceden duyurarak kalıcı olarak UTC+3'e geçti ([kaynak](https://www.timeanddate.com/news/time/turkey-permanent-trt.html)).

Yalnızca UTC saklanırsa, kural değişince gelecekteki etkinliklerin saatleri ekranda sessizce bir saat kayar. Özgün "19:00" bilgisi kaybolduğu için doğrusu da hesaplanamaz ([kaynak](https://codeblog.jonskeet.uk/2019/03/27/storing-utc-is-not-a-silver-bullet/)).

Ayrıca S5'teki turnelerde yurt dışı mekanlar olabilir. O mekanın saati İstanbul saatiyle aynı değildir.

## Karar

- Mekanın bir saat dilimi vardır (IANA kimliği; varsayılanı `Europe/Istanbul`).
- Etkinliğin zaman noktaları (başlangıç, soundcheck, kapı açılışı, set, söküm, bitiş) ve S2'deki crew çağrı saatleri iki biçimde saklanır:
  - **Kaynak:** yerel duvar saati (`timestamp`, saat dilimsiz) + saat dilimi kimliği.
  - **Türetilen:** aynı anın UTC karşılığı (`timestamptz`). Müsaitlik, çakışma ve sıralama hesapları bununla yapılır.
- Saat dilimi veritabanı güncellendiğinde bir bakım komutu, gelecekteki kayıtların türetilen UTC değerlerini kaynaktan yeniden hesaplar.
- Olmuş olayların zamanı (okutma, onay, oluşturma, işlem geçmişi) ADR-0017'deki gibi yalnızca UTC saklanır.

## Sonuçlar

**Olumlu:**
- Kullanıcının girdiği bilgi hiçbir zaman kaybolmaz; saat dilimi kuralı değişse bile etkinlik doğru saatte görünür.
- Hesaplar yine tek tip UTC anlarıyla yapılır; müsaitlik sorguları karmaşıklaşmaz.
- Yurt dışı mekanlar ileride yapı değişmeden desteklenir.

**Olumsuz / bedeli:**
- Etkinlik zaman noktası başına iki kolon vardır ve ikisinin tutarlılığı alan katmanında korunur: kaynak değişince türetilen değer aynı işlemde yeniden hesaplanır.
- Yaz saati uygulanan bir saat diliminde "olmayan" (ileri alınan saat) ya da "iki kez yaşanan" (geri alınan saat) yerel saatler için bir kural gerekir. Türkiye'de yaz saati uygulaması yok; kural yurt dışı mekanlar geldiğinde (S5) belirlenir.
- Bakım komutu, saat dilimi veritabanı güncellemesinden sonra çalıştırılmalıdır. Bu yüzden her yayından sonra otomatik çalışır ([10 §7](../10-operations.md#7-saat-dilimi-verisi-güncellemesi)).

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Yalnızca UTC (ADR-0017'nin ilk hali) | En basit yol; ama saat dilimi kuralı değişince gelecekteki etkinlik saatleri sessizce kayar ve doğrusu bulunamaz. |
| Yalnızca yerel saat + saat dilimi | Bilgi kaybolmaz; ama her müsaitlik ve çakışma sorgusunda anlık dönüşüm gerekir. SQL'de aralık kısıtları ve indeksler kullanılamaz. |
| Tüm zamanlar için iki biçim | Olmuş olayların zamanı kural değişikliğinden etkilenmez; iki biçim yalnızca gereksiz karmaşıklık olur. |
