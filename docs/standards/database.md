# Veritabanı Standardı

> **Durum:** v1.3 · **Son güncelleme:** 2026-09-25
> **Kararlar:** [Bölüm 18](#18-kararlar)

## 1. Bu belge ne işe yarar

PostgreSQL veritabanının fiziksel kurallarını tanımlar:
- roller ve yetkiler,
- kimlik tipi,
- kolon tipleri,
- zaman, para ve metin,
- ortak kolonlar, pasifleştirme ve silme,
- eşzamanlılık ve kilitler,
- kısıtlar ve indeksler,
- işlem geçmişi, outbox ve inbox tabloları,
- migration'lar, bağlantı ayarları ve veritabanı testleri.

Adlar (tablo, kolon, indeks, kısıt) [naming.md](naming.md#5-veritabanı-adları)'dedir. Veri modeli [06-erd-conceptual.md](../06-erd-conceptual.md)'dedir. Bu belge ikisini fiziksel şemaya çevirmenin kurallarını verir. Kararların gerekçeleri araştırma kaynaklarıyla birlikte ilgili bölümlerde ve ADR'lerde yazılıdır.

## 2. Temel ilkeler

1. **Veritabanı son savunma hattıdır.** Uygulama kuralları denetler ve kullanıcıya anlaşılır mesaj verir. Ama yarış durumları, hatalar ve ham SQL uygulama denetimini atlayabilir. Bu yüzden ucuz ifade edilebilen her değişmez (benzersizlik, zorunluluk, aralık, çakışmazlık, değiştirilemezlik) veritabanında da bir kısıtla korunur.
2. **Modül sınırı veritabanında da geçerlidir.** Her modül kendi şemasına kendi rolüyle bağlanır; başka modülün tablosunu ham SQL ile bile okuyamaz ([V-02](#18-kararlar), [ADR-0021](../adr/0021-database-roles-per-module.md)).
3. **İş mantığı veritabanında değil, kodda.** Tetikleyici (trigger) ve saklı yordam (stored procedure) ile iş kuralı yazılmaz; kurallar tek yerde, test edilebilir biçimde alan katmanındadır. Veritabanı yalnızca kısıtlarla korur.
4. **Az ve kararlı PostgreSQL özelliği.** Her eklenti ve özel özellik gerekçesiyle bu belgeye yazılır.
5. **Veri hacmi küçüktür; güvenilirlikten taviz verilmez.** Tek şirket, binlerce birim ve etkinlik. Mikro optimizasyon yerine doğruluk, sadelik ve öngörülebilirlik seçilir.

## 3. Veritabanı ve şemalar

| Konu | Kural | Neden |
|---|---|---|
| Sürüm | PostgreSQL 18 | [ADR-0007](../adr/0007-data-access.md) |
| Veritabanı | Tek veritabanı: `festos`; kodlama UTF-8 | [05 §13](../05-module-map.md#13-kod-ve-veritabanına-yansıması) |
| Varsayılan sıralama kuralı (collation) | PostgreSQL'in yerleşik sağlayıcısı: `C.UTF-8` | İşletim sisteminin sıralama kütüphanesi (glibc) güncellendiğinde indeksler sessizce bozulabiliyor ([kaynak](https://www.crunchydata.com/blog/glibc-collations-and-data-corruption)). Yerleşik sağlayıcı PostgreSQL'in içindedir ve sürümler arasında sabittir ([kaynak](https://www.pgedge.com/blog/what-is-a-collation-and-why-is-my-data-corrupt)). Türkçe sıralama sorgu anında yapılır ([§13](#13-metin-sıralama-ve-arama)). |
| Veri sağlama toplamı (checksum) | Açık (PostgreSQL 18'de yeni kurulan veritabanı kümeleri için varsayılan) | Disk bozulmaları sessiz kalmaz. |
| Şemalar | Her modülün bir şeması; adı modül adı | [naming §5.1](naming.md#51-adlar) |
| `public` şeması | Yalnızca eklentiler içindir; uygulama nesnesi konmaz | Modül sınırı dışında nesne birikmez. |
| Eklentiler | `btree_gist` (zaman çakışması kısıtları, [§12](#12-kısıtlar-ve-indeksler)), `pg_trgm` (metin araması, [§13](#13-metin-sıralama-ve-arama)) | Başka eklenti bu belge güncellenmeden eklenmez. |

## 4. Roller ve yetkiler

Her modül veritabanına **kendi rolüyle** bağlanır ([V-02](#18-kararlar)). Rol yalnızca kendi şemasına erişebilir. Böylece mimari testlerin (AT-08) kod tarafında koruduğu sınır veritabanında da korunur: bir modülün ham SQL'i başka modülün tablosuna ulaşamaz ([kaynak](https://www.milanjovanovic.tech/blog/how-to-keep-your-data-boundaries-intact-in-a-modular-monolith)).

| Rol | Kim kullanır | Yetkiler |
|---|---|---|
| `festos_owner` | Hiç kimse doğrudan bağlanmaz (giriş yapamaz) | Tüm şemaların ve nesnelerin sahibi |
| `festos_migrator` | Migration aracı | `festos_owner`'ın üyesi; şema değiştirir |
| `festos_{modül}` (ör. `festos_booking`) | Modülün veritabanı bağlamı | Kendi şemasında `USAGE`; tablolarında `SELECT`, `INSERT`, `UPDATE`, `DELETE` (aşağıdaki istisnalarla); `audit.audit_entries` tablosunda yalnızca `INSERT` |
| `festos_audit` | Audit modülü | `audit` şemasında `SELECT` |
| `festos_readonly` | Elle inceleme, ileride raporlama (S6) | Tüm şemalarda `SELECT` |
| `festos_monitor` | Telemetri toplayıcısı (Alloy) | PostgreSQL'in yerleşik `pg_monitor` rolünün üyesi: sunucu istatistikleri (bağlantılar, WAL arşivleme, boyut). Hiçbir tabloda yetkisi yoktur ([09 §8.1](../09-environments-and-deployment.md#81-kurulum)). |

**Yetki istisnaları, yani kuralların veritabanında zorlanması:**
- **Değişmez tablolar** ([06 §8](../06-erd-conceptual.md#8-değişmez-kayıtlar)): modül rolüne `UPDATE` ve `DELETE` verilmez. İşlem geçmişi, stok hareketi ve rider versiyonu gibi kayıtlar koddaki bir hatayla bile değiştirilemez (BR-SYS-010).
- **Silinmeyen ana veriler** (BR-SYS-001: kullanıcı, taraf, mekan, depo, kategori, model, kit, prodüksiyon): bu tablolarda `DELETE` yetkisi verilmez.
- Modül rolü şema değiştiremez (`CREATE` yok). Şema yalnızca migration aracıyla değişir.

**Diğer kurallar:**
- Parolalar SCRAM ile doğrulanır. PostgreSQL 18'de MD5 parola doğrulaması kullanımdan kaldırılma sürecindedir.
- Her rolün `search_path` ayarı kendi şeması ve `public`'tir.
- Rollerin oluşturulması ve yetkilerin verilmesi migration'larla yapılır ve sürüm kontrolündedir. Parolalar ortam gizlilerinden gelir (C.6).
- Yetkilerin doğruluğunu bir veritabanı testi (DT-02) denetler.

## 5. Kimlik

### 5.1 Varlık kimlikleri

| Kural | Açıklama |
|---|---|
| Tip | `uuid`, **sürüm 7** (zaman sıralı) ([V-01](#18-kararlar), [ADR-0020](../adr/0020-entity-identifiers.md)) |
| Üretim | Uygulamada, varlık oluşturulurken: `Guid.CreateVersion7()`. Kimlik, kayıttan önce bellidir. |
| EF ayarı | Anahtarlar `ValueGeneratedNever()`; kimliği alan katmanı verir. |
| Yasak | `Guid.NewGuid()` (sürüm 4, rastgele); yasak API listesine eklenir. |

**Neden UUIDv7:**
- Kimlik kayıttan önce bellidir. Olay ve outbox kaydı, aynı işlem biriminde varlığın kimliğini taşıyabilir; veritabanına sormaya gerek kalmaz.
- Modüller birbirine kimlikle referans verir ([05 ilke 7](../05-module-map.md#2-temel-ilkeler)). Tüm modüllerde tek tip kimlik, referansları basitleştirir.
- Ardışık sayılar API'de tahmin edilebilir; bir sonraki kaydın adresi kolayca denenebilir. UUID'de bu yoktur.
- Sürüm 4 (rastgele) UUID indeksi dağıtır ve yavaşlatır. Sürüm 7'nin baş kısmı zamandır; indekse sona yakın eklenir. Bir ölçümde sürüm 7 indeksi `bigint` indeksinin 1,41 katı, sürüm 4 ise 1,85 katı büyüklüktedir ([kaynak](https://dev.to/libme/uuidv7-vs-ulid-vs-bigint-which-primary-key-holds-up-when-the-table-gets-big-4mkg)). Bu ölçekte fark önemsizdir.

**Bilinen sınırlar:**
- .NET'in `Guid.CreateVersion7()` metodu aynı milisaniye içinde üretilen kimlikleri sıralı üretmez ([kaynak](https://gist.github.com/sdrapkin/03b13a9f7ba80afe62c3308b91c943ed)). Bu yüzden **kimlik sıralama için kullanılmaz.** Sıra gerekiyorsa `created_at` ya da ayrı bir sıra numarası kullanılır (outbox, [§15](#15-outbox-ve-inbox-tabloları)).
- Kimlik oluşturma zamanını açığa vurur. Kayıtlar zaten oluşturma zamanını gösteren kurum içi bir uygulamada bu risk değildir.

### 5.2 Kod içinde tip güvenli kimlikler

Modülün kendi varlıklarının kimlikleri kod içinde ayrı tiplerdir: `readonly record struct EventId(Guid Value)`. Böylece bir `VenueId` yanlışlıkla `EventId` bekleyen yere verilemez; derleyici yakalar.
- Tip güvenli kimlik yalnızca modülün **Domain ve Application** katmanlarındadır.
- Başka modüldeki kayda verilen referanslar ile Contracts, IntegrationEvents ve API tiplerinde kimlik `Guid`'dir. Böylece modüller birbirinin kimlik tiplerine bağımlı olmaz.
- EF dönüştürücüsü ve JSON dönüştürücüsü, BuildingBlocks'taki tek bir genel kural (convention) ile tüm kimlik tiplerine uygulanır. Kimlik başına elle kod yazılmaz.

### 5.3 İnsan tarafından okunan numaralar

| İhtiyaç | Yöntem | Örnek |
|---|---|---|
| Tekil, tekrar kullanılmayan kod (boşluk olabilir) | PostgreSQL dizisi (sequence) + önek | Etiket kodu (BR-EQP-004): birim ve kasa için ayrı dizi |
| Bir kayıt içinde sıra numarası | Toplu kök içinde `max + 1`; (üst kayıt, numara) benzersiz | Rider versiyon numarası (BR-RDR-003) |
| Yasal olarak boşluksuz numara | Aynı işlem biriminde satır kilitli sayaç tablosu | S1'de yok. S4'te gerekirse bu yöntem kullanılır ([kaynak](https://www.cybertec-postgresql.com/en/postgresql-sequences-vs-invoice-numbers/)). |

PostgreSQL dizileri işlem geri alınsa da geri sarılmaz; boşluk oluşur. Etiket kodunda boşluk sorun değildir; önemli olan tekrar kullanılmamasıdır.

### 5.4 Teknik tablolarda kimlik

Outbox ve inbox gibi teknik tablolarda sıra gerektiğinde `bigint GENERATED BY DEFAULT AS IDENTITY` kullanılır. `serial` kullanılmaz.

## 6. Kolon tipleri

| C# tipi | PostgreSQL tipi | Kural |
|---|---|---|
| `Guid`, tip güvenli kimlik | `uuid` | [§5](#5-kimlik) |
| `string` | `varchar(n)` | Her metin alanının uzunluk sınırı açıkça verilir ([§6.1](#61-metin-uzunlukları)). |
| `string` (uzun serbest metin) | `text` | Yalnızca açıkça işaretlenmiş alanlar |
| `bool` | `boolean` | Boş olabilen (`bool?`) mantıksal alan yalnızca "bilinmiyor" anlamı belgelenmişse kullanılır. |
| `int` | `integer` | Adet ve sayılar |
| `long` | `bigint` | Teknik sıra numaraları |
| `decimal` (tutar) | `numeric(19,4)` | [§8](#8-para-oran-ve-ölçüler) |
| `decimal` (kur) | `numeric(18,8)` | [§8](#8-para-oran-ve-ölçüler) |
| `decimal` (yüzde) | `numeric(7,4)` | Değer 0–100; ad `…Percent` ile biter |
| `decimal` (fiziksel ölçü) | `numeric(p,s)`; ad birimi taşır | `WeightKilograms numeric(10,3)` |
| `DateTimeOffset` | `timestamptz` | Her zaman UTC ([§7](#7-zaman)) |
| `DateTime` (`Kind = Unspecified`) | `timestamp` | Yalnızca gelecekteki duvar saati zamanlarının kaynak değeri (`…AtLocal`, [§7.2](#72-gelecekteki-duvar-saati-zamanları)); başka yerde `DateTime` kullanılmaz. |
| `DateOnly` | `date` | Takvim günü |
| `TimeOnly` | `time` | Günün saati |
| `TimeSpan` | `interval` | Süreler; SQL'de doğrudan tarihlerle toplanabilir |
| `enum` | `varchar` + `CHECK` | [§6.2](#62-enumlar) |
| Değer tipi (`Money`, zaman aralığı) | Birden fazla kolon | EF 10 karmaşık tipi (complex type); sahip olunan varlık (owned entity) kullanılmaz ([kaynak](https://www.npgsql.org/efcore/mapping/json.html)) |
| Değişken yapılı veri | `jsonb` | Yalnızca [§6.3](#63-jsonb-kullanımı)'teki durumlarda |

**Kullanılmayan tipler:**
- `money`: çıktısı sunucunun yerel ayarına bağlıdır ([kaynak](https://www.postgresql.org/docs/current/datatype-money.html)).
- `real`, `double precision` ve C# `float` / `double`: tutar, adet ve ölçüde yuvarlama hatası üretir.
- `char(n)`: değeri boşlukla doldurur ([kaynak](https://wiki.postgresql.org/wiki/Don%27t_Do_This)).
- `timestamp` (saat dilimsiz): yalnızca [§7.2](#72-gelecekteki-duvar-saati-zamanları)'deki yerel duvar saati için kullanılır.
- PostgreSQL'e özgü `enum` tipi ([§6.2](#62-enumlar)).
- Diziler (array): yalnızca teknik tablolarda. İş verisinde dizi yerine alt tablo kullanılır.

**Zorunluluk:** Kolonlar varsayılan olarak `NOT NULL`'dır. Boş değer yalnızca "yok" anlamı taşıdığında izinlidir ve C# tipinde `?` ile görünür. EF bunu C#'ın boş olabilirlik bilgisinden otomatik türetir.

**Varsayılan değerler:** İş alanlarının varsayılanı alan katmanında verilir; veritabanı varsayılanı (`DEFAULT`) yalnızca teknik kolonlarda kullanılır. Böylece nesne, kaydedilmeden önce de eksiksizdir.

### 6.1 Metin uzunlukları

| Alan türü | Uzunluk | Örnek |
|---|---|---|
| Kısa kod | 32 | Etiket kodu, para birimi kodu dışındaki kodlar |
| Ad, başlık | 200 | Mekan adı, model adı |
| E-posta | 254 | İletişim adresi |
| Telefon | 32 | — |
| Adres (URL) | 2048 | — |
| Açıklama, not | 2000 | Pasifleştirme nedeni, hasar açıklaması |
| Uzun serbest metin | `text` | Rider notları |

`varchar(n)` sınırını büyütmek PostgreSQL'de tabloyu yeniden yazmaz; sınır ihtiyaç doğunca güvenle genişletilir. Uzunluk sınırı olmayan metin alanını mimari test (AT-13) yakalar.

### 6.2 Enum'lar

Enum'lar **metin olarak** saklanır. Değer, API'deki biçimle aynıdır (camelCase, ör. `holdPlaced`; [naming §6](naming.md#6-api-adları)). Kolona, izin verilen değerleri listeleyen bir `CHECK` kısıtı eklenir.
- Dönüştürücü ve `CHECK` kısıtı BuildingBlocks'taki genel kuralla (convention) otomatik üretilir. Enum'a değer eklenince bir sonraki migration kısıtı kendiliğinden günceller.
- Bir enum üyesinin adı değişirse bu, veri migration'ı gerektirir (`UPDATE … SET status = 'yeniAd'`). Üye adları API ve veritabanı sözleşmesinin parçasıdır.

**Neden PostgreSQL enum tipi değil:** PostgreSQL enum'undan değer silinemez. Aynı işlemde eklenen değer kullanılamaz; bu da değer ekleyip veriyi güncelleyen migration'ları bozar. Metin ve `CHECK` standart SQL'dir, okunur ve serbestçe değişir ([kaynak](https://atomiccoding.substack.com/p/postgresql-domain-types-and-enums)). Sayı olarak saklamak ise SQL'de okunmaz ve sıra değişince anlam kayar.

### 6.3 jsonb kullanımı

`jsonb` yalnızca şu durumlarda kullanılır:
- işlem geçmişindeki eski ve yeni değerler ([§14](#14-değişmez-kayıtlar-ve-işlem-geçmişi)),
- outbox ve inbox'taki olay içeriği ([§15](#15-outbox-ve-inbox-tabloları)),
- yapısı gerçekten kayıttan kayda değişen ve üzerinde filtre ya da referans kurulmayan veri. Her yeni kullanım modül tasarımında gerekçelendirilir.

Sorgulanan, filtrelenen ya da başka kayda referans veren veri `jsonb`'ye konmaz; kolon ya da alt tablo olur.

## 7. Zaman

### 7.1 Anlar

- Tüm anlar `timestamptz` kolonunda **UTC** saklanır ([ADR-0017](../adr/0017-time-and-money-types.md)). Kolon adı `…_at` ile biter ([naming §5.2](naming.md#52-zaman-tarih-ve-süre-sonekleri)).
- Npgsql, UTC olmayan bir `DateTimeOffset`'i yazmayı reddeder ([kaynak](https://www.npgsql.org/doc/types/datetime.html)). Bu bilinçli bir güvenliktir; eski davranışa dönen ayar (`EnableLegacyTimestampBehavior`) açılmaz. Zaman her yerde `TimeProvider.GetUtcNow()`'dan gelir.
- PostgreSQL mikrosaniye, .NET 100 nanosaniye hassasiyetindedir. Kaydedilip okunan bir değer son hanelerini kaybeder ([kaynak](https://www.roji.org/postgresql-dotnet-timestamp-mapping)). Bu yüzden testlerde sahte saat mikrosaniyeye hizalı bir değerden başlar ve tam birimlerle ilerletilir.

### 7.2 Gelecekteki duvar saati zamanları

Kullanıcının bir mekan için **yerel saatle** girdiği gelecekteki zamanlar ([V-03](#18-kararlar), [ADR-0022](../adr/0022-future-wall-clock-times.md)):
- **Kaynak değer:** yerel duvar saati (`timestamp`, saat dilimsiz; kolon `…_at_local`) ve saat dilimi kimliği (IANA, ör. `Europe/Istanbul`; kolon `time_zone`). Saat dilimi mekandadır; varsayılanı `Europe/Istanbul`'dur. Mekan başka modülde olduğu için etkinlik, saat dilimini kendi üzerinde de tutar.
- **Türetilen değer:** aynı anın UTC karşılığı (`timestamptz`; kolon `…_at`). Müsaitlik, çakışma ve sıralama hesapları bu kolonla yapılır. Örnek: `doors_at_local` + `time_zone` → `doors_at`.
- **Saat dilimi kuralları değişirse:** güncel saat dilimi veritabanıyla yayınlanan bir bakım komutu, gelecekteki kayıtların UTC değerlerini kaynak değerden yeniden hesaplar.

**Neden:** Saat dilimi kuralları siyasi kararlarla, bazen haftalar önceden değişir. Türkiye 2015'te seçim nedeniyle kış saatine geçişi iki hafta erteledi. Eylül 2016'da da bir ay önceden duyurarak kalıcı olarak UTC+3'e geçti ([kaynak](https://www.timeanddate.com/news/time/turkey-permanent-trt.html)). Yalnızca UTC saklanırsa, kural değişince gelecekteki bir etkinliğin kapı açılışı sessizce bir saat kayar. "Saat 19:00" bilgisi kaybolduğu için doğrusu da bulunamaz ([kaynak](https://codeblog.jonskeet.uk/2019/03/27/storing-utc-is-not-a-silver-bullet/)).

**Kapsam:** Bu kural yalnızca etkinliğin zaman noktalarına (başlangıç, soundcheck, kapı açılışı, set, söküm, bitiş) ve S2'deki crew çağrı saatlerine uygulanır. Olmuş olayların zamanı (okutma, onay, oluşturma) yalnızca UTC saklanır; geçmiş zamanlar kural değişikliğinden etkilenmez.

### 7.3 Zaman aralıkları

- Aralıklar iki kolonla saklanır: `…_start`, `…_end` ([naming §5.1](naming.md#51-adlar)).
- Aralıklar **yarı açıktır**: `[başlangıç, bitiş)`. Başlangıç dahil, bitiş hariçtir. Böylece art arda iki rezervasyon (biri 18:00'de biter, diğeri 18:00'de başlar) çakışmış sayılmaz.
- `CHECK (…_start < …_end)` her aralıkta zorunludur (ör. BR-EVT-020).
- Çakışma kısıtlarında ve sorgularda aralık, `tstzrange(…_start, …_end, '[)')` ifadesiyle kurulur.

### 7.4 Takvim günü

Opsiyon günü gibi gün bazlı kurallar Europe/Istanbul takvim gününe göre `date` kolonunda tutulur ([ADR-0017](../adr/0017-time-and-money-types.md)).

## 8. Para, oran ve ölçüler

### 8.1 Tutarlar

- Tutar, `Money` değer tipiyle iki kolonda saklanır: `…_amount numeric(19,4)` ve `…_currency varchar(3)`. Para birimi kodu `CHECK (… ~ '^[A-Z]{3}$')` ile denetlenir (ISO 4217).
- Ölçek 4'tür, çünkü birim fiyatlar ve kur çevrimleri ara hesaplarda kuruştan daha ince değer üretebilir. Tüm ISO 4217 para birimlerinin küsurat hanesi 4'ü geçmez.
- Farklı para birimindeki tutarlar toplanamaz ([ADR-0017](../adr/0017-time-and-money-types.md)).

### 8.2 Yuvarlama

| Kural | Karar |
|---|---|
| Hedef hassasiyet | Para biriminin küsurat hanesi (TRY, EUR, USD için 2) |
| Yöntem | **Yarım değer sıfırdan uzağa** (`MidpointRounding.AwayFromZero`): 2,345 → 2,35 |
| Ne zaman | Belirli noktalarda: satır tutarı (miktar × birim fiyat), satır vergisi, kur çevrimi sonucu. Belge toplamı, yuvarlanmış satırların toplamıdır. |
| Ara hesap | Ara hesaplar yuvarlanmaz; yuvarlama yalnızca tanımlı noktada ve bir kez yapılır. |

**Neden sıfırdan uzağa:** .NET'in varsayılanı "yarım değer çifte" (bankacı yuvarlaması: 2,345 → 2,34) iken ([kaynak](https://learn.microsoft.com/en-us/dotnet/api/system.math.round?view=net-10.0)):
- PostgreSQL'in `numeric` tipindeki `round()` fonksiyonu yarım değeri sıfırdan uzağa yuvarlar ([kaynak](https://www.postgresql.org/docs/current/datatype-numeric.html)). Aynı yöntem seçilince SQL ile C#'ın hesapları aynı sonucu verir.
- Excel'in `ROUND` fonksiyonu ve kullanıcıların alışkın olduğu yuvarlama da budur. Kullanıcı sonucu hesap makinesiyle ya da tabloyla karşılaştırdığında aynı kuruşu görür.
- Türkiye'de fatura yazılımları kuruş hassasiyetinde, satır bazında yuvarlar ([kaynak](https://www.faturaniyazdir.com/Blog/Faturada_Kurus_Yuvarlama_Nasil_Yapilir)).

Yuvarlama yalnızca `Money` tipinin yöntemiyle yapılır. Yöntem belirtmeyen `Math.Round(decimal)` ve `decimal.Round(…)` çağrıları yasak API listesine eklenir.

### 8.3 Oranlar ve ölçüler

- **Kur:** `numeric(18,8)`; kaynağı ve tarihiyle saklanır (S4).
- **Yüzde:** `numeric(7,4)`, 0–100 aralığında (`CHECK`); ad `…Percent` ile biter (`DiscountPercent`). Kesir (0,2) ile yüzde (20) karışmasın diye oranlar her yerde yüzde olarak tutulur.
- **Adet:** `integer`; eksi olamayan adetlerde `CHECK (… >= 0)` bulunur. Adetli stok adetle izlenir (K-03).
- **Fiziksel ölçü:** Birim adda yazılır ([naming §2](naming.md#2-temel-ilkeler)): `weight_kilograms numeric(10,3)`, `power_watts integer`, `current_amperes numeric(7,2)`.

## 9. Ortak kolonlar

| Varlık türü | Kolonlar |
|---|---|
| Toplu kök | `id`, `version`, `created_at`, `created_by`, `updated_at`, `updated_by` |
| Toplu köke bağlı varlık | `id` (bilgileri kökün kolonlarından ve işlem geçmişinden izlenir) |
| Değişmez kayıt | `id`, `created_at`, `created_by` (`updated_*` ve `version` yok) |
| Pasifleştirilebilir ana veri | Yukarıdakilere ek olarak `deactivated_at`, `deactivated_by` (ikisi de boş olabilir) |

- `created_*` ve `updated_*` kolonlarını işlem birimi, oturumdaki kullanıcıdan ve `TimeProvider`'dan otomatik doldurur; kodda elle atanmaz.
- **Sistem kullanıcısı:** Zamanlanmış işler ve olay dinleyicileri gibi bir kullanıcının başlatmadığı işlemler, sabit kimlikli bir sistem kullanıcısıyla kaydedilir. Kimlik BuildingBlocks'ta sabit olarak tanımlanır; Identity modülünde bu kimlikle, giriş yapamayan bir kullanıcı kaydı bulunur. Böylece `…_by` kolonları hiçbir zaman boş kalmaz.
- **İşlem geçmişi varken neden bu kolonlar:** "Son değişiklik" bilgisi listelerde sıralama ve filtre için gerekir. Bunu `audit` şemasından okumak, modüller arası sorgu demektir. Kolonlar bu bilgiyi kayıt üzerinde, ucuza sunar. Tam geçmiş işlem geçmişindedir.

## 10. Pasifleştirme ve silme

### 10.1 Pasifleştirme

- Başka modüllerin referans verebildiği ana veriler (BR-SYS-001) silinmez, pasifleştirilir: `deactivated_at` doldurulur. Alan tipinde `IsActive => DeactivatedAt is null`. Yeniden etkinleştirmede kolonlar boşaltılır; geçmiş işlem geçmişindedir.
- **Genel sorgu filtresi (global query filter) kullanılmaz.** Yaygın "soft delete" uygulamaları pasif kayıtları her sorgudan otomatik gizler ([kaynak](https://learn.microsoft.com/en-us/ef/core/querying/filters)). Ama BR-SYS-001'e göre pasif kayıt, bağlı olduğu mevcut kayıtlarda **görünmeye devam eder**: pasifleştirilen bir model, geçmiş etkinliğin ekipman listesinde yine görünmelidir. Otomatik filtre bu kaydı gizler ve hatalı sonuç verir. Bunun yerine **yalnızca seçim listeleri** (`…Option` sorguları) pasif kayıtları açıkça dışarıda bırakır.
- Benzersizlik kısıtlarının pasif kayıtları kapsayıp kapsamayacağı alanın kuralına göre seçilir. Örneğin pasif bir deponun adı yeniden kullanılabilecekse kısıt `WHERE deactivated_at IS NULL` koşullu indeksle kurulur. Etiket kodu gibi hiçbir zaman tekrar kullanılmayan değerlerin kısıtı koşulsuzdur.

### 10.2 Silme

| Kayıt | Silinebilir mi | Nasıl korunur |
|---|---|---|
| Ana veri (BR-SYS-001) | Hayır | Modül rolüne `DELETE` yetkisi verilmez ([§4](#4-roller-ve-yetkiler)). |
| Değişmez kayıt | Hayır | Modül rolüne `UPDATE` ve `DELETE` yetkisi verilmez. |
| Modül içi kayıt, kendisine bağlı kayıt yoksa | Evet, gerçekten silinir | Yabancı anahtar `RESTRICT` bağlı kayıt varken silmeyi engeller. |
| Toplu köke bağlı varlık | Kökle birlikte ya da kökün metoduyla | Yabancı anahtar `CASCADE` |

**Yabancı anahtar silme davranışı:** EF Core, zorunlu ilişkilerde varsayılan olarak **basamaklı silme** (`CASCADE`) kurar. Bu yüzden BuildingBlocks'taki genel kural (convention) varsayılanı `RESTRICT` yapar; `CASCADE` yalnızca toplu kökün kendi alt varlıklarına açıkça verilir. Aksi halde bir kaydın silinmesi, fark edilmeden başka kayıtları da silebilir.

## 11. Eşzamanlılık ve kilitler

### 11.1 Sürüm numarasıyla iyimser kilit

- Her toplu kökün `version integer` kolonu vardır ve EF'te eşzamanlılık belirteci (concurrency token) olarak işaretlidir.
- İşlem birimi, **toplu köke ya da ona bağlı herhangi bir varlığa** değişiklik yapıldığında kökün sürümünü bir artırır. Kaydederken sürüm değişmişse işlem reddedilir (BR-SYS-011).
- Çakışma kullanıcıya güncel kayıtla birlikte bildirilir (HTTP 409, C.5). **Otomatik yeniden denenmez**, çünkü bu sessizce ezme demektir.
- Sürüm, API'de kayıtla birlikte döner ve güncelleme isteğinde geri gönderilir. Böylece kullanıcının ekranı açtığı andan kaydettiği ana kadar geçen sürede yapılan değişiklikler de yakalanır (ayrıntı C.5).

**Neden PostgreSQL'in `xmin` kolonu değil:** Npgsql, her satırın son değiştiren işlemini tutan `xmin` sistem kolonunu hazır bir sürüm numarası olarak kullanmayı önerir ([kaynak](https://www.npgsql.org/efcore/modeling/concurrency.html)). Ama `xmin` yalnızca **o satır** değişince değişir. Bir toplu kökün yalnızca alt varlığı değiştiğinde (ör. etkinliğe yeni bir onay eklendiğinde) kökün `xmin`'i aynı kalır ve iki kullanıcının çakışan değişikliği yakalanmaz ([kaynak](https://www.kamilgrzybek.com/blog/posts/handling-concurrency-aggregate-pattern-ef-core)). Açık sürüm numarası toplu kökün tamamını korur, API'de anlamlı bir değer olarak taşınabilir ve veritabanından bağımsızdır.

### 11.2 Sıcak satırlarda satır kilidi

Aynı satırı kısa aralıklarla çok sayıda işlemin değiştirdiği durumlarda iyimser kilit, kullanıcılara sürekli "kayıt değişti" hatası gösterir. Örneğin yoğun çıkışta aynı kablo stoğunu okutan iki depo çalışanı. Bu satırlar için:
- Satır, işlem birimi içinde `SELECT … FOR UPDATE` ile kilitlenerek okunur, değiştirilir ve kaydedilir. İkinci işlem hata almaz; birkaç milisaniye bekler.
- Eksiye düşmeme gibi değişmezler ayrıca `CHECK (quantity >= 0)` ile korunur.
- Hangi tabloların sıcak satır olduğu modül tasarımında belirlenir. S1'de adetli stok (BR-EQP-005, BR-WHS-008) bu türdendir.

### 11.3 Müsaitlik kilidi

Taahhüt yazılırken alınan (model, depo) kilidi ([ADR-0004](../adr/0004-commitments-vs-facts.md), BR-MRP-013) PostgreSQL'in işlem sonunda kendiliğinden çözülen danışma kilidiyle alınır:
- `SELECT pg_advisory_xact_lock(hashtextextended(@key, 0))`; anahtar `planning:availability:{modelId}:{warehouseId}` biçimindedir.
- Birden fazla kilit gerekiyorsa anahtarlar **sıralanarak** alınır. Aynı kilitleri farklı sırayla alan iki işlem birbirini sonsuza kadar bekleyebilir (kilitlenme, deadlock); sabit sıra bunu önler.
- İstek işlerken yalnızca işlem düzeyindeki danışma kilidi kullanılır. Oturum düzeyindeki kilit (`pg_advisory_lock`) bağlantı havuzunda serbest bırakılmadan kalabilir ([kaynak](https://www.snowinch.com/en/blog/postgres-advisory-lock-connection-pool-leak)). Oturum düzeyi kilit yalnızca zamanlanmış işlerin tek çalışması için ([ADR-0013](../adr/0013-scheduled-jobs.md)), ayrı bir bağlantıda ve `finally` bloğunda açıkça bırakılarak kullanılır.
- Danışma kilidi anahtarları tüm veritabanında ortaktır. Bu yüzden anahtar her zaman `{modül}:{amaç}:` önekiyle başlar.

### 11.4 İşlem yalıtımı ve yeniden deneme

- İşlem yalıtım düzeyi PostgreSQL'in varsayılanıdır (`READ COMMITTED`). Daha yüksek düzey gereken bir durum olursa modül tasarımında gerekçesiyle yazılır.
- Geçici hatalarda (bağlantı kopması, kilitlenme, serileştirme hatası) Npgsql'in yeniden deneme stratejisi açıktır. Kendi işlem birimimizle birlikte kullanıldığında strateji, **tüm işlem birimini** tek bir yeniden denenebilir blok olarak çalıştırmalıdır; aksi halde EF hata verir ([kaynak](https://learn.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency)). Yeniden denemeden önce değişiklik takibi temizlenir ve işleyici baştan çalışır.
- İyimser kilit çakışması (§11.1) geçici hata değildir; yeniden denenmez.

## 12. Kısıtlar ve indeksler

### 12.1 Kısıtlar

| Değişmez | Veritabanında nasıl | Örnek |
|---|---|---|
| Zorunlu alan | `NOT NULL` | — |
| Doğal anahtar tekilliği | `UNIQUE` ya da benzersiz indeks | Seri no, model içinde tekil (US-EQP-003) |
| Modül içi ilişki | Yabancı anahtar | Opsiyon → opsiyon kuyruğu |
| Aralık ve sıra | `CHECK` | Başlangıç < bitiş (BR-EVT-020) |
| Enum değeri | `CHECK` (otomatik, §6.2) | — |
| "Tam olarak biri dolu" | `CHECK (num_nonnulls(a, b, c) = 1)` | Rider satırının hedefi: model, kategori ya da kit (BR-RDR-001, [06 karar 3](../06-erd-conceptual.md#4-önemli-modelleme-kararları)) |
| Aynı anahtar için zaman çakışmaması | Dışlama kısıtı (`EXCLUDE USING gist`) | Mekan ekipmanının kullanılamama dönemleri (BR-VEN-002) |
| Sıra değiştirme sırasında geçici çakışma | Ertelenmiş benzersizlik (`DEFERRABLE INITIALLY DEFERRED`) | Opsiyon kuyruğunda sıra numaraları (BR-EVT-002) |

**Özel kısıtlar hakkında notlar:**
- EF Core; dışlama, ertelenmiş ve PostgreSQL 18'in zaman kısıtlarını (`WITHOUT OVERLAPS`) modelde tanımlayamaz. Bu kısıtlar migration içinde SQL ile yazılır ve geri alma (`Down`) adımları da yazılır. Npgsql'in `WITHOUT OVERLAPS` desteği EF sağlayıcısının 11. sürümünde geliyor ([kaynak](https://www.npgsql.org/efcore/misc/temporal-constraints.html)); .NET LTS sürümünde kaldığımız sürece SQL ile yazılır.
- Ertelenmiş benzersizlik kısmi (koşullu) olamaz. Bu yüzden sıraya girmeyen kayıtların (ör. düşmüş opsiyon) sıra numarası `NULL` yapılır; benzersizlik kısıtı `NULL` değerleri zaten dikkate almaz.
- **Kısıt ihlali kullanıcıya kural numarasıyla döner.** Her kontrol, benzersizlik ve dışlama kısıtının adı, modülün altyapı katmanında bir kural koduna eşlenir. Böylece uygulama denetimini aşan bir yarış durumunda bile kullanıcı anlaşılır bir mesaj görür (hata biçimi C.5). Eşlemesi eksik kısıt bir veritabanı testiyle (DT-04) yakalanır.

### 12.2 İndeksler

- Modül içi yabancı anahtar kolonlarına EF otomatik indeks ekler.
- **Modüller arası referans kolonlarına indeks elle eklenir.** Bu kolonlarda yabancı anahtar olmadığı için EF indeks eklemez (ör. `planning.equipment_reservations.event_id`). Sorgulanan her referans kolonunun indeksli olduğunu DT-03 denetler.
- Belirli bir durumdaki kayıtları sık okuyan sorgular için koşullu indeks kullanılır (ör. gönderilmemiş outbox kayıtları).
- Dolu bir tabloya yeni indeks, yazmaları kilitlememek için `CREATE INDEX CONCURRENTLY` ile eklenir. Bu komut işlem içinde çalışamaz; migration'da işlemsiz SQL adımı olarak yazılır ([§16](#16-migrationlar)).
- Türkçe sıralama kuralı (ICU) ile indeks oluşturulmaz (§13).

## 13. Metin, sıralama ve arama

| İhtiyaç | Yöntem | Neden |
|---|---|---|
| Kullanıcıya gösterilen listelerde Türkçe sıralama | Sorguda `COLLATE "tr-x-icu"` (EF: `EF.Functions.Collate`) | Veritabanı varsayılanı (`C.UTF-8`) bayt sırasıyla sıralar; "Çanakkale", "Zonguldak"tan sonra gelir. ICU'nun Türkçe kuralı doğru sıralar. |
| ICU ile indeks | Oluşturulmaz | ICU kütüphanesi güncellenince bu indeksler yeniden oluşturulmak zorundadır ([kaynak](https://www.citusdata.com/blog/2020/12/12/dont-let-collation-versions-corrupt-your-postgresql-indexes/)). Bu veri hacminde indekssiz sıralama yeterince hızlıdır. |
| Büyük / küçük harf ve Türkçe karakter duyarsız arama | Uygulamada üretilen **arama anahtarı** kolonu + `pg_trgm` indeksi | Depo çalışanı telefondan "isik" yazdığında "Işık" bulunmalıdır. Anahtar tek kuralla, tek yerde (BuildingBlocks) üretilir ve ön yüzle aynı kuralı kullanır. |
| Büyük / küçük harf duyarsız benzersizlik (ör. e-posta) | Uygulamada üretilen **normalleştirilmiş kolon** (ör. `email_normalized`) üzerinde benzersiz indeks | Kural tek yerde ve Türkçe harf kurallarıyla tutarlıdır. `citext` eklentisi aksanları ele almaz ([kaynak](https://www.npgsql.org/efcore/misc/collations-and-case-sensitivity.html)). |

**Arama anahtarı nasıl üretilir:** Metin Unicode NFC biçimine getirilir, Türkçe kurallarıyla küçük harfe çevrilir (`İ` → `i`, `I` → `ı`), sonra Türkçe harfler aksansız karşılıklarına indirgenir (`ç→c, ğ→g, ı→i, ö→o, ş→s, ü→u`). Kolon adı `…_search` ile biter.

**Unicode normalleştirme:** API'den gelen tüm metinler kaydedilmeden önce kırpılır ve NFC biçimine getirilir (C.5). Aynı görünen iki metnin (ör. tek karakterli `İ` ile `I` + birleşen nokta) farklı saklanması böylece önlenir.

## 14. Değişmez kayıtlar ve işlem geçmişi

### 14.1 Değişmez kayıtlar

[06 §8](../06-erd-conceptual.md#8-değişmez-kayıtlar)'deki kayıtlar yalnızca eklenir. Koruma iki katmanlıdır: alan katmanında güncelleme metodu yoktur; veritabanında modül rolünün `UPDATE` ve `DELETE` yetkisi yoktur (§4).

### 14.2 Kayıtların işlem geçmişi

İşlem geçmişi (BR-SYS-010), her modülün işlem biriminde, değişiklikle **aynı işlemde** `audit.audit_entries` tablosuna yazılır ([05 §5.2](../05-module-map.md#52-audit--i̇şlem-geçmişi)). Yazan, EF'in kaydetme adımına bağlanan BuildingBlocks yazıcısıdır ([kaynak](https://milanjovanovic.tech/blog/audit-logging-ef-core)).

| Kolon | İçerik |
|---|---|
| `id` | UUIDv7 |
| `occurred_at` | İşlem zamanı (UTC) |
| `actor_id` | İşlemi yapan kullanıcı ya da sistem kullanıcısı |
| `module`, `entity_type`, `entity_id` | Değişen kayıt |
| `action` | `created`, `updated`, `deleted`, `statusChanged` |
| `changes` | `jsonb`: değişen her alanın eski ve yeni değeri |
| `trace_id` | OpenTelemetry iz kimliği; aynı isteğin loglarıyla bağlantı kurar |

**Kurallar:**
- Gizli alanlar (parola özeti, oturum anahtarı) işlem geçmişine yazılmaz; `[NotAudited]` özniteliğiyle işaretlenir.
- **Toplu güncelleme yasağı:** EF'in `ExecuteUpdate` / `ExecuteDelete` metotları ve ham SQL ile yazma, değişiklik takibini atladığı için işlem geçmişini de atlar ([kaynak](https://milanjovanovic.tech/blog/audit-logging-ef-core)). Bu yöntemler yalnızca işlem geçmişi gerekmeyen teknik tablolarda (outbox, inbox, yerel kopyalar) kullanılır. Domain ve Application projelerinde yasak API listesiyle engellenir.
- İşlem geçmişi hiç silinmez. Tablo büyürse zamana göre bölümleme (partitioning) değerlendirilir; S1 hacminde gerekmez.
- `(entity_type, entity_id, occurred_at)` üzerinde indeks bulunur; bir kaydın geçmişi bu indeksle okunur.

## 15. Outbox ve inbox tabloları

Yapı [ADR-0010](../adr/0010-messaging-infrastructure.md)'daki kararları fiziksel tabloya çevirir. Her modül şemasında bulunur.

**`outbox_messages`:**

| Kolon | İçerik |
|---|---|
| `id` | Olay kimliği (UUIDv7); dinleyicinin inbox'ında da bu kimlik tutulur |
| `sequence` | `bigint` kimlik kolonu; gönderim sırası |
| `type` | Olay tipinin adı |
| `ordering_key` | Sıra garantisi verilen kaydın kimliği (ör. etkinlik) |
| `payload` | `jsonb` olay içeriği |
| `occurred_at` | Olayın oluşma zamanı |
| `trace_parent` | Olayı doğuran isteğin iz bağlamı (W3C `traceparent`); dinleyicinin izi bununla ilişkilendirilir ([observability §4](observability.md#4-dağıtık-izleme)) |
| `dispatched_at` | Gönderildiği zaman; gönderilmediyse boş |
| `attempt_count`, `next_attempt_at`, `last_error` | Yeniden deneme bilgisi |

**`inbox_messages`:** `(message_id, handler)` birincil anahtar ve `processed_at`. Aynı olayı aynı dinleyici ikinci kez işlemez.

**Kurallar:**
- Gönderim sırası `sequence` ile belirlenir. UUIDv7 aynı milisaniye içinde sıralı olmadığı için sıra olarak kullanılmaz (§5.1).
- Dağıtıcı, gönderilecek kayıtları `FOR UPDATE SKIP LOCKED` ile alır. Böylece ileride birden fazla uygulama örneği aynı kaydı iki kez almaz ([kaynak](https://www.prisma.io/blog/you-dont-need-a-job-queue-postgres-already-has-skip-locked)).
- Gönderilmemiş kayıtlar için `sequence` üzerinde koşullu indeks bulunur (`WHERE dispatched_at IS NULL`).
- **Saklama süresi:** Gönderilmiş outbox kayıtları ve inbox kayıtları 30 gün sonra, küçük gruplar halinde silinir. Temizlenmeyen outbox tablosu zamanla veritabanının en yavaş tablosuna dönüşür ([kaynak](https://dev.to/nainikmehta/transactional-outbox-pattern-prevent-lost-events-in-eda-2e95)). Hatalı olaylar çözülene kadar silinmez.

**`idempotency_keys`:** Tekrar güvenliği anahtarları ([api §10](api.md#10-tekrar-güvenliği)). Birincil anahtar `(user_id, key)`; isteğin parmak izi, saklanan yanıtın durum kodu ve gövdesi (`jsonb`) ile oluşturma zamanı tutulur. Kayıt, komutun işlem biriminin ilk adımında eklenir; 24 saat sonra silinir.

## 16. Migration'lar

### 16.1 Üretme ve inceleme

- Her modülün kendi migration seti ve şemasında kendi geçmiş tablosu vardır ([naming §5.1](naming.md#51-adlar)).
- Migration EF ile üretilir ve **üretilen SQL incelenir**. PR'da migration'ın SQL çıktısı da görülür.
- Uygulanmış bir migration değiştirilmez ve silinmez. Hata yeni bir migration ile düzeltilir.
- Model ile migration'lar arasında fark kalmadığını sürekli entegrasyon denetler (`dotnet ef migrations has-pending-model-changes`, DT-01).
- EF'in modelde tanımlayamadığı nesneler (dışlama ve ertelenmiş kısıtlar, roller, yetkiler) migration'da SQL ile yazılır. Her SQL adımının geri alma (`Down`) karşılığı da yazılır.

### 16.2 Uygulama

| Ortam | Nasıl |
|---|---|
| Geliştirme | Host açılırken tüm modüllerin migration'larını uygular ([08 §5](../08-architecture.md#5-ana-uygulama-host-ve-modüllerin-kaydı)). |
| Demo ve yayın | Uygulama başlamadan önce yayın betiğinde ayrı bir adım: aynı imajdaki `migrate` komutu, `festos_migrator` rolüyle ([09 §5](../09-environments-and-deployment.md#5-konteyner-imajı)). |

Yayında uygulamanın açılışta migration çalıştırmaması önerilen yoldur: birden fazla örnek aynı anda migration uygulamaya çalışabilir, ayrıca uygulama rolünün şema değiştirme yetkisi olmamalıdır ([kaynak](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying)). EF 9'dan beri migration'lar veritabanı kilidi altında çalışır; yine de ayrı adım tercih edilir.

### 16.3 Güvenli şema değişikliği

- **Geri uyumluluk:** Bir migration, uygulamanın **bir önceki sürümüyle** de çalışabilmelidir. Böylece yeni sürümde sorun çıkarsa uygulama geri alınabilir, veritabanı geri alınmak zorunda kalmaz.
- **Kolon yeniden adlandırma ve silme** tek adımda yapılmaz. Önce yeni kolon eklenir ve iki kolon birlikte yazılır, veri taşınır, sonraki sürümde eski kolon silinir (genişlet / daralt, expand / contract; [kaynak](https://xata.io/blog/zero-downtime-schema-migrations-postgresql)).
- Migration rolünün kilit bekleme süresi 5 saniyedir (rol düzeyinde `lock_timeout`, [§17](#17-bağlantı-ve-işletim-ayarları)). Kilit bekleyen bir şema değişikliği, arkasındaki tüm okuma ve yazmaları da bekletir; zaman aşımı bunu önler. Zaman aşımına uğrayan migration, yük azaldığında yeniden çalıştırılır. Ayar role verildiği için hiçbir migration'da unutulamaz.
- Sabit varsayılanlı `NOT NULL` kolon eklemek PostgreSQL 11'den beri tabloyu yeniden yazmaz. Değişken varsayılan (ör. `uuidv7()`) ise tüm satırları yeniden yazar.
- Büyük veri taşımaları migration içinde değil, küçük gruplar halinde çalışan ayrı bir komutla yapılır.

### 16.4 Başlangıç verisi

- **Referans veriler** (para birimleri, sistem kullanıcısı, arayüzden değişen parametrelerin varsayılanları: P-06, P-07, P-14) migration'larla, sabit kimliklerle eklenir.
- Demo verisi migration'a girmez; ayrı bir yükleme komutuyla eklenir.

## 17. Bağlantı ve işletim ayarları

| Ayar | Değer | Neden |
|---|---|---|
| Bağlantı havuzu | Modül başına bir havuz (her rolün bağlantı dizesi ayrı); `Maximum Pool Size` modül başına ayardan | Toplam bağlantı sayısı, PostgreSQL'in `max_connections` sınırı içinde planlanır. |
| `Application Name` | `festos-{modül}` | Veritabanında hangi bağlantının hangi modüle ait olduğu `pg_stat_activity`'de görünür. |
| Komut zaman aşımı | 30 saniye (Npgsql) | — |
| Rol düzeyinde `statement_timeout` | Modül rolleri 30 saniye; migration rolü sınırsız | Kontrolden çıkan bir sorgu veritabanını kilitlemez. |
| Rol düzeyinde `idle_in_transaction_session_timeout` | 60 saniye | İşlemi açık unutulan bağlantı kilitleri tutmaz ([kaynak](https://www.postgresql.org/docs/current/runtime-config-client.html)). |
| Rol düzeyinde `lock_timeout` | Modül rolleri 10 saniye; migration rolü 5 saniye | Kilit bekleyen istek sonsuza kadar beklemez; şema değişikliği canlı trafiği bekletmez. |
| Bağlantı havuzlayıcı (PgBouncer) | S1'de yok | İleride eklenirse işlem modunda çalışabilmesi için istek işlerken yalnızca işlem düzeyinde danışma kilidi kullanılır (§11.3). |

**Ham SQL:** Yalnızca altyapı katmanında ve her zaman parametreli yazılır. EF'in `FromSql` / `ExecuteSql` gibi enterpolasyonlu metotları değerleri otomatik olarak parametreye çevirir. Metin birleştirmeye açık `FromSqlRaw`, `ExecuteSqlRaw` ve `SqlQueryRaw` yasak API listesine eklenir (SQL enjeksiyonu).

Yedekleme ve zamana göre geri dönüş (PITR) [09 §8.2](../09-environments-and-deployment.md#82-yedekleme-ve-zamana-göre-geri-dönüş)'de ve [ADR-0031](../adr/0031-backup-and-point-in-time-recovery.md)'dedir.

### 17.1 Veritabanı testleri

Gerçek PostgreSQL 18 üzerinde (Testcontainers) çalışan testler. Yer: `tests/Database/FestOS.DatabaseTests` ([08 §12](../08-architecture.md#12-testlerin-yeri-ve-mimari-testler)).

| No | Denetim |
|---|---|
| DT-01 | Tüm modüllerin migration'ları boş veritabanına sırayla uygulanır; modelde migration'a dökülmemiş değişiklik yoktur. |
| DT-02 | Rol yetkileri: modül rolü yalnızca kendi şemasına erişir; `audit` tablosuna yalnızca ekleme yapabilir; değişmez tablolarda güncelleme ve silme, ana veri tablolarında silme yetkisi yoktur. |
| DT-03 | Modüller arası referans kolonlarının (sonu `_id` ile biten, yabancı anahtarı olmayan kolonlar) indeksi vardır; istisnalar testte gerekçesiyle listelenir. |
| DT-04 | Her kontrol, benzersizlik ve dışlama kısıtı bir kural koduna eşlenmiştir. |
| DT-05 | Veritabanı varsayılan sıralama kuralı `C.UTF-8`'dir; `public` şemasında uygulama nesnesi yoktur. |

## 18. Kararlar

| No | Konu | Karar | Gerekçe |
|---|---|---|---|
| V-01 | Kimlik tipi | UUIDv7, uygulamada üretilir; sıralama için kullanılmaz ([ADR-0020](../adr/0020-entity-identifiers.md)) | §5.1 |
| V-02 | Modül başına veritabanı rolü | Evet; işlem geçmişine yalnızca ekleme, değişmez tablolarda güncelleme ve silme yok ([ADR-0021](../adr/0021-database-roles-per-module.md)) | §4 |
| V-03 | Gelecekteki duvar saati zamanları | Yerel saat + saat dilimi kaynak, UTC türetilmiş ([ADR-0022](../adr/0022-future-wall-clock-times.md)) | §7.2 |
| V-04 | Kod içinde tip güvenli kimlik | Modül içinde tip güvenli, sınırlarda `Guid` | §5.2 |
| V-05 | Varsayılan sıralama kuralı | Yerleşik `C.UTF-8`; Türkçe sıralama sorguda ICU ile, ICU indeksi yok | §3, §13 |
| V-06 | Enum saklama | Metin (camelCase) + otomatik `CHECK` | §6.2 |
| V-07 | Tutar hassasiyeti | `numeric(19,4)` + para birimi kodu | §8.1 |
| V-08 | Yuvarlama | Para biriminin küsuratına, yarım değer sıfırdan uzağa, satır bazında | §8.2 |
| V-09 | Eşzamanlılık belirteci | Toplu kökte açık `version`; `xmin` değil | §11.1 |
| V-10 | Pasifleştirme filtresi | Genel sorgu filtresi yok; yalnızca seçim listeleri filtreler | §10.1 |
| V-11 | Yabancı anahtar silme davranışı | Varsayılan `RESTRICT`; `CASCADE` yalnızca toplu kökün alt varlıklarında | §10.2 |
| V-12 | Aralık biçimi | Yarı açık `[başlangıç, bitiş)`, iki kolon | §7.3 |
| V-13 | Toplu güncelleme | Yalnızca teknik tablolarda; iş tablolarında yasak | §14.2 |
| V-14 | Migration uygulama | Geliştirmede açılışta; yayında ayrı adım, aynı imajdaki `migrate` komutuyla (migration paketi yerine; [09 E-08](../09-environments-and-deployment.md#13-kararlar)) | §16.2 |
| V-15 | İzleme rolü | `festos_monitor`, yalnızca `pg_monitor` üyeliği | §4 |

## 19. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-09-25 | v0.1 | İlk taslak |
| 2026-09-25 | v1.0 | V-01, V-02, V-03 kararlaştırıldı; ADR-0020, ADR-0021, ADR-0022 kabul edildi. |
| 2026-09-25 | v1.1 | Tekrar güvenliği tablosu eklendi (C.5). |
| 2026-09-25 | v1.2 | Outbox'a iz bağlamı kolonu eklendi (C.6). |
| 2026-09-25 | v1.3 | D.3 ile uyum: yayında migration aynı imajdaki `migrate` komutuyla (V-14); izleme rolü `festos_monitor` (V-15); yedekleme ve PITR bağlandı. |
