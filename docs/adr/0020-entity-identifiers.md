# ADR-0020: Varlık kimlikleri — UUIDv7

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [standards/database.md §5](../standards/database.md#5-kimlik), [ADR-0003](0003-integration-events-and-outbox.md), [ADR-0007](0007-data-access.md)

## Bağlam

Her varlığın bir birincil anahtarı olacak ve bu seçim sonradan değiştirilemeyecek kadar yaygın etki eder: tablolar, referanslar, olaylar, API adresleri ve ön yüz tipleri. Projenin özellikleri:
- Modüller birbirine yalnızca kimlikle referans verir; yabancı anahtar yoktur ([05 ilke 7](../05-module-map.md#2-temel-ilkeler)).
- Bir komut, varlığı oluşturduğu işlem biriminde olayını da outbox'a yazar; olay varlığın kimliğini taşır ([ADR-0003](0003-integration-events-and-outbox.md)).
- Kimlikler API adreslerinde görünür (`/events/{eventId}`).
- Veri hacmi küçüktür (tek şirket).

## Karar

- Tüm varlıkların birincil anahtarı `uuid` tipinde, **sürüm 7** (zaman sıralı) UUID'dir.
- Kimlik, varlık oluşturulurken uygulamada `Guid.CreateVersion7()` ile üretilir; EF'te anahtarlar `ValueGeneratedNever()`.
- `Guid.NewGuid()` (sürüm 4) yasak API listesine eklenir.
- Kimlik sıralama amacıyla kullanılmaz; sıra gerektiğinde zaman kolonu ya da ayrı sıra numarası kullanılır.
- Teknik tablolarda sıra gerektiğinde `bigint` kimlik kolonu kullanılır.

## Sonuçlar

**Olumlu:**
- Kimlik kayıttan önce bellidir; olay, referans ve yanıt aynı işlem biriminde kurulabilir.
- Tüm modüllerde tek kimlik tipi vardır.
- API'deki kimlikler tahmin edilemez; ardışık sayılarla kayıt taramak mümkün olmaz.
- Sürüm 7'nin baş kısmı zaman olduğu için indeks, rastgele UUID'deki gibi dağılmaz.
- PostgreSQL 18 aynı biçimi veritabanında da üretebilir (`uuidv7()`); gerekirse SQL'de de kullanılabilir.

**Olumsuz / bedeli:**
- Kimlik 16 bayttır (`bigint` 8 bayt); indeksler yaklaşık 1,4 kat büyüktür. Bu ölçekte önemsizdir.
- Kimlik uzun ve okunmazdır. İnsanın okuyacağı yerde ayrı kod kullanılır (ör. etiket kodu).
- .NET'in ürettiği sürüm 7 kimlikler aynı milisaniye içinde sıralı değildir; bu yüzden kimlik sıra olarak kullanılamaz.
- Kimlik oluşturma zamanını açığa vurur; kurum içi uygulamada risk değildir.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| `bigint` kimlik kolonu (veritabanı üretir) | En küçük ve hızlı seçenek. Ama kimlik ancak kayıttan sonra belli olur; olay ve referansların aynı işlem biriminde kurulması zorlaşır. API'de ardışık, tahmin edilebilir sayılar görünür. |
| UUID sürüm 4 (rastgele) | İndeks dağılır; ölçümlerde ekleme çok daha yavaştır. |
| ULID | Sürüm 7 ile aynı fikir; ama PostgreSQL ve .NET'te yerleşik tip değildir, metin olarak saklanması gerekir. |
| İkili anahtar: içte `bigint`, dışta UUID | İki kimlik yönetmek her tabloda karmaşıklık getirir; bu veri hacminde kazancı yoktur. |
