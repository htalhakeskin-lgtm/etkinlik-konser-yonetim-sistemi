# ADR-0017: Zaman ve para tipleri

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **Genişletildi:** [ADR-0022](0022-future-wall-clock-times.md) — gelecekteki duvar saati zamanları yerel saat + saat dilimiyle de saklanır
- **İlgili:** [03-business-rules.md §3](../03-business-rules.md#3-biçim-ve-kullanım) (Zaman), BR-MRP-001, BR-EVT-002, BR-EVT-008, [06-erd-conceptual.md karar 15](../06-erd-conceptual.md#4-önemli-modelleme-kararları)

## Bağlam

Alan dört farklı zaman kavramı kullanıyor:

| Kavram | Örnek |
|---|---|
| An | Etkinliğin başlangıcı, okutma zamanı |
| Takvim günü | Opsiyon günü, opsiyon son tarihi |
| Günün saati | Mekanın sessizlik saati |
| Süre | Hazırlık ve dönüş payı |

Tüm anlar UTC saklanır, Europe/Istanbul'a göre gösterilir. Opsiyon günü gibi gün bazlı kurallar İstanbul takvim gününe göre işler. Zamana bağlı kurallar (otomatik geçişler, son tarihler) testlerde zaman ilerletilerek sınanabilmelidir.

Tutarlar S1'de yok, ama S3–S4'te gelecek ve her zaman para birimiyle birlikte tutulmalıdır.

## Karar

- **An:** `DateTimeOffset`, her zaman UTC. Veritabanında saat dilimli zaman damgası olarak saklanır.
- **Takvim günü:** `DateOnly`.
- **Günün saati:** `TimeOnly`.
- **Süre:** `TimeSpan`; dakika hassasiyetinde kullanılır.
- **Saat dilimi:** `TimeZoneInfo` ile IANA kimliği `Europe/Istanbul`. Dönüşümler tek bir yardımcıdan geçer; kod içinde dağınık dönüşüm yapılmaz.
- **Şimdiki zaman:** Hiçbir kod `DateTime.Now` ya da `DateTimeOffset.UtcNow` okumaz; zaman her yerde `TimeProvider` üzerinden alınır.
- **Para:** `BuildingBlocks` içinde bir `Money` değer tipi: tutar (`decimal`) ve ISO 4217 para birimi kodu. Farklı para birimlerindeki tutarlar birbirine eklenemez. Saklama ve yuvarlama kuralları [standards/database.md §8](../standards/database.md#8-para-oran-ve-ölçüler)'dedir.

## Sonuçlar

**Olumlu:**
- Ek bağımlılık yoktur; bu tiplerin hepsi .NET ve Npgsql tarafından doğrudan desteklenir.
- Zaman kavramları tipler üzerinden ayrışır; örneğin "gün" ile "an" karıştırılamaz.
- Zamana bağlı tüm kurallar testlerde belirleyici (deterministik) biçimde sınanır.

**Olumsuz / bedeli:**
- `DateTimeOffset`'in UTC olması tip sistemiyle garanti edilmez; bu kural kod incelemesi ve bir analiz kuralıyla korunur.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| NodaTime | Zaman kavramlarını en doğru ayıran kütüphane (`Instant`, `LocalDate`, `Duration`). Ama güncel .NET'in yerleşik tipleri (`DateOnly`, `TimeOnly`, `TimeProvider`) bu projenin ihtiyacını karşılıyor; ek bağımlılık ve öğrenme maliyetine değmez. |
| `DateTime` | Saat dilimi bilgisi taşımaz; UTC mi yerel mi olduğu belirsizleşir. |
| Hazır para kütüphanesi | S1'de para alanı yok; ihtiyaç basit ve kendi değer tipimiz yeterli. |
