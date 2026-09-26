# İsimlendirme Standardı

> **Durum:** v1.5 · **Son güncelleme:** 2026-09-26
> **Kararlar:** [Bölüm 12](#12-kararlar)

## 1. Bu belge ne işe yarar

Koddaki, veritabanındaki, API'deki ve ön yüzdeki her adın nasıl yazılacağını tanımlar. Amaç:
- Aynı kavramın her katmanda tahmin edilebilir bir adı olsun. `VenueHold` sınıfını bilen biri tablonun, JSON alanının, adresin ve ön yüz dosyasının adını bakmadan bilebilsin.
- Adlar elle "uydurulmasın"; sözlükteki kod adından mekanik olarak türetilsin.
- Sonradan değiştirmesi pahalı olan adlar (tablo, kolon, API alanı, yetki kodu) baştan doğru konsun.

Kod biçimi ve yazım kuralları [code-style.md](code-style.md)'dedir. Veritabanının fiziksel kuralları (kimlik tipi, ortak kolonlar, para hassasiyeti) C.4'te, API'nin adres yapısı ve sürümleme C.5'te tanımlanacak. Bu belge yalnızca **adları** belirler.

## 2. Temel ilkeler

1. **Tek kaynak sözlüktür.** Her varlığın, durumun ve kavramın kod adı [01-glossary.md](../01-glossary.md)'den gelir. Diğer tüm biçimler (tablo, kolon, JSON, adres, dosya adı) bu addan [Bölüm 3](#3-dönüşüm-tablosu)'teki kurallarla türetilir. Sözlükte olmayan bir kavram koda girmeden önce sözlüğe eklenir (sözlük kuralı 4).
2. **Kod adları İngilizce ve yalnızca ASCII harflerdir.** Türkçe karakter yalnızca kullanıcıya gösterilen metinlerde (çeviri dosyaları) ve belgelerde bulunur. Böylece Türkçe büyük / küçük harf dönüşümü hiçbir adı bozamaz ([08 §9](../08-architecture.md#9-türkçe-karakter-güvenliği)). C# tarafında mimari test (AT-12), ön yüzde lint kuralı bunu zorlar.
3. **Açıklık kısalıktan önemlidir.** Kısaltma kullanılmaz; yalnızca [Bölüm 2.1](#21-kullanılabilen-kısaltmalar)'deki liste serbesttir. `qty`, `amt`, `desc`, `mgr`, `cnt` gibi kısaltmalar yasaktır.
4. **Kısaltmalar kelime gibi yazılır.** `QrLabel`, `PdfDocument`, `ApiClient`, `UiState` (`QRLabel`, `PDFDocument` değil). Böylece C#, TypeScript, JSON ve veritabanı biçimleri arasında dönüşüm tek anlamlı olur: `QrLabel` → `qrLabel` → `qr_label`. Ürün adı **FestOS** özel addır, bu kurala tabi değildir. Dış kütüphanelerin tip adları (`QRCodeGenerator`) olduğu gibi kullanılır.
5. **Olumlu ad.** Mantıksal (boolean) adlar olumlu kurulur: `IsActive` (`IsNotActive` ya da `IsDisabled` değil). `!isDisabled` gibi çift olumsuzluk okunmaz.
6. **Birim ada yazılır.** Bir sayı bir süreyi, miktarı ya da ölçüyü tutuyorsa birimi adında, kısaltmasız yer alır: `TimeoutSeconds`, `WeightKilograms`, `PowerWatts`. Süre `TimeSpan` tipindeyse birim gerekmez (`PrepBuffer`).

### 2.1 Kullanılabilen kısaltmalar

| Kısaltma | Anlamı | Örnek |
|---|---|---|
| `Id` | kimlik | `EventId`, `event_id` |
| `Api` | uygulama programlama arayüzü | `ApiClient` |
| `Url`, `Uri` | adres | `CallbackUrl` |
| `Qr` | QR kodu | `QrLabel` |
| `Pdf` | PDF belgesi | `PdfRenderer` |
| `Ui` | kullanıcı arayüzü | `UiState` |
| `Db` | veritabanı | `BookingDbContext` |
| `Utc` | eş güdümlü evrensel zaman | `ToUtc()` |
| `Iso` | ISO standardı | `IsoCurrencyCode` |

Listeye yeni kısaltma bu belge güncellenerek eklenir.

## 3. Dönüşüm tablosu

Sözlükteki kod adı `VenueHold` (opsiyon) örneğiyle:

| Nerede | Biçim | Örnek |
|---|---|---|
| C# sınıfı | PascalCase, tekil | `VenueHold` |
| C# koleksiyon özelliği | PascalCase, çoğul | `VenueHolds` |
| C# kimlik özelliği | PascalCase + `Id` | `VenueHoldId` |
| Veritabanı şeması | snake_case, modül adı | `booking` |
| Veritabanı tablosu | snake_case, **çoğul** | `booking.venue_holds` |
| Veritabanı kolonu | snake_case | `venue_hold_id` |
| JSON alanı | camelCase | `venueHoldId` |
| Adres bölümü | kebab-case, çoğul | `/api/v1/venue-holds/{venueHoldId}` |
| OpenAPI işlem adı (operationId) | fiil + ad, PascalCase | `PlaceVenueHold` |
| Üretilen ön yüz kancası (Orval) | `use` + işlem adı | `usePlaceVenueHold` |
| TypeScript tipi | PascalCase | `VenueHold` |
| Ön yüz dosyası | kebab-case | `venue-hold-card.tsx` |
| Çeviri anahtarı | ad alanı + camelCase yol | `booking:venueHold.expiresAt` |
| Yetki kodu | Modül.KaynakÇoğul.Eylem | `Booking.VenueHolds.Place` |
| Telemetri özniteliği | küçük harf, noktalı | `festos.venue_hold.id` |

**Çoğul biçim:** İngilizce çoğul kuralı uygulanır (`EventStatusHistory` → `event_status_histories`, `Person` → `people`; bkz. [N-03](#12-kararlar)). Mimari test, tablo adlarını bir çoğullaştırma kütüphanesiyle üretilen beklenen adla karşılaştırır; kütüphanenin yanlış çoğullaştırdığı sözcükler için testte istisna listesi tutulur.

## 4. C# adları

### 4.1 Genel kurallar

Microsoft'un .NET ekibinin kullandığı kurallar temel alınır ([kaynak](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names)). Kurallar `.editorconfig`'deki adlandırma kurallarıyla derlemede denetlenir.

| Öğe | Biçim | Örnek |
|---|---|---|
| Ad alanı (namespace) | Proje adı + klasör yolu | `FestOS.Modules.Booking.Application.Holds.PlaceVenueHold` |
| Sınıf, kayıt (record), yapı, enum, temsilci | PascalCase | `HoldQueue` |
| Arayüz | `I` + PascalCase | `IBookingModule` |
| Genel tip parametresi | `T` ya da `T` + açıklayıcı ad | `T`, `TResult`, `TCommand` |
| Metot, özellik, olay (C# event) | PascalCase | `Confirm()`, `ExpiresAt` |
| Yerel değişken, parametre | camelCase | `venueHold`, `cancellationToken` |
| Özel (private) ve iç (internal) alan | `_` + camelCase | `_timeProvider` |
| Sabit (`const`) ve `static readonly` | PascalCase | `MaxHoldRank`, `DefaultPrepBuffer` |
| Pozisyonel kayıt parametresi | PascalCase (özelliğe dönüşür) | `record Money(decimal Amount, string Currency)` |
| Birincil kurucu parametresi (sınıf) | camelCase | `class PlaceVenueHoldHandler(BookingDbContext dbContext)` |

**Ek kurallar:**
- Değiştirilebilir statik alan yoktur (global durum yasağı). Statik alanlar yalnızca `const` ya da `static readonly` olur.
- Enum adı tekildir (`HoldStatus`); `[Flags]` enum'ları çoğuldur. Enum üyeleri PascalCase'dir ve sözlükteki durum adlarıyla aynıdır (`EventStatus.LoadIn`).
- Mantıksal adlar `Is`, `Has`, `Can`, `Should`, `Was` ile başlar: `IsExpired`, `HasOpenConflicts`, `CanBeCancelled`.
- Koleksiyonların adı çoğuldur; türünü söylemez: `holds` (`holdList` ya da `holdArray` değil).
- Asenkron metotlar (`Task` / `ValueTask` dönen) `Async` ile biter: `GetAvailabilityAsync`. İstisna: test metotları.
- `CancellationToken` parametresi her zaman `cancellationToken` adını taşır ve son parametredir.
- Her dosyada tek bir üst düzey tip bulunur ve dosya adı tip adıyla aynıdır (`VenueHold.cs`). Meziantou analizörü (MA0048) bunu derlemede denetler.
- Bir tip adı içinde bulunduğu ad alanının herhangi bir bölümüyle aynı olamaz (ör. `Booking` ad alanında `Booking` sınıfı olmaz).

### 4.2 Mimari yapı taşlarının adları

[08-architecture.md](../08-architecture.md)'deki yapı taşları için sabit ad kalıpları:

| Yapı taşı | Kalıp | Örnek |
|---|---|---|
| Komut | Emir kipi fiil + ad + `Command` | `PlaceVenueHoldCommand`, `ConfirmEventCommand` |
| Tek kayıt sorgusu | `Get` + ad + `Query` | `GetEventQuery` |
| Liste sorgusu | `List` + çoğul ad + `Query` | `ListEventsQuery` |
| Alana özgü sorgu | Fiil + ad + `Query` | `CheckAvailabilityQuery`, `CalculateRequirementsQuery` |
| İşleyici | Komut / sorgu adı − sonek + `Handler` | `PlaceVenueHoldHandler`, `ListEventsHandler` |
| Doğrulayıcı | Komut / sorgu adı − sonek + `Validator` | `PlaceVenueHoldValidator` |
| Komut sonucu (kimlikten fazlası dönüyorsa) | Komut adı − sonek + `Result` | `PlaceVenueHoldResult` |
| Tek kayıt okuma modeli | Ad + `Details` | `EventDetails` |
| Liste satırı okuma modeli | Ad + `ListItem` | `EventListItem` |
| Seçim listesi seçeneği | Ad + `Option` | `WarehouseOption` |
| HTTP'ye özgü istek modeli | İşlem adı + `Request` | `PlaceVenueHoldRequest` |
| Modül içi olay | Özne + geçmiş zaman fiil + `DomainEvent` | `EventConfirmedDomainEvent` |
| Entegrasyon olayı | Özne + geçmiş zaman fiil + `IntegrationEvent` | `EventStatusChangedIntegrationEvent` |
| Entegrasyon olayı işleyicisi | Etki + `On` + olay adı − sonek + `Handler` | `ReleaseReservationsOnEventCancelledHandler` |
| Modül sözleşmesi (senkron) | `I` + modül + `Module` | `IInventoryModule` |
| Modül sözleşmesinin uygulaması | Modül + `Module` | `InventoryModule` |
| Modül tanımı (Host kaydı) | Modül + `ModuleDefinition` | `BookingModuleDefinition` |
| Veritabanı bağlamı | Modül + `DbContext` | `BookingDbContext` |
| Tablo eşlemesi | Varlık + `Configuration` | `VenueHoldConfiguration` |
| Uç nokta grubu | Kaynak + `Endpoints`, metodu `Map` + kaynak + `Endpoints` | `VenueHoldEndpoints.MapVenueHoldEndpoints()` |
| Uç nokta işleyici metodu | İşlem adı (operationId ile aynı) | `VenueHoldEndpoints.PlaceVenueHold(…)` |
| Zamanlanmış iş | Ad + `Job` | `AutomaticTransitionsJob` |
| Ayar sınıfı | Modül ya da özellik + `Options` | `BookingOptions` |
| İstisna | Ad + `Exception` | `BusinessRuleViolationException` |
| Genişletme metodu sınıfı | Hedef + `Extensions` | `MoneyExtensions` |
| Kural kodu sabitleri | Modül + `RuleCodes` | `BookingRuleCodes` |
| Yetki kodu sabitleri | Modül + `Permissions` | `BookingPermissions` |

**Neden `Get` / `List` ayrımı:** `GetEventQuery` ile `GetEventsQuery` tek harf farkla ayrılır; kodda ve üretilen ön yüz kancalarında (`useGetEvent` / `useGetEvents`) kolayca karışır. Farklı fiiller bu hatayı yapısal olarak önler.

**Neden `Dto` soneki yok:** Sonek, tipin ne olduğunu değil nasıl taşındığını söyler. `EventDetails` ile `EventListItem` hangi ekranda ne kadar veri olduğunu anlatır; `EventDto` anlatmaz ve zamanla her şeyi taşıyan tek bir tipe dönüşür.

**Neden olay işleyicileri etkiyle adlandırılır:** Aynı olayı bir modülde birden fazla işleyici dinleyebilir. `EventStatusChangedHandler` adı ne yaptığını söylemez; `ReleaseReservationsOnEventCancelledHandler` söyler ve loglarda doğrudan okunur.

**Kural kodları:** İş kuralı ihlalleri, [03-business-rules.md](../03-business-rules.md)'deki numarayı taşır. Numara kodda açıklayıcı adlı bir sabitin değeridir; böylece hem okunur hem de numara ile aranabilir:

```csharp
public static class BookingRuleCodes
{
    public const string ConfirmationRequiresApproval = "BR-EVT-009";
}
```

İleride `tools/docs` betiği, 03'teki her kuralın kodda bir sabiti olup olmadığını kontrol edebilir.

### 4.3 Bilinen çakışmalar

Bazı sözlük adları sık kullanılan .NET tipleri ya da projedeki başka kavramlarla aynıdır:

| Sözlük adı | Çakıştığı | Kural |
|---|---|---|
| `Event` | "olay" anlamındaki event kelimesi | Sözlük kuralı 6: olaylar her zaman `DomainEvent` / `IntegrationEvent` sonekiyle adlandırılır. |
| `Session` | ASP.NET Core `ISession` | Identity modülünde alan tipi `Session` kalır; çerçeve tipi gerekirse tam adıyla yazılır. |
| `Production` (S2) | Ortam adı "Production" (`IHostEnvironment.IsProduction()`) | Ortam adları kodda yalnızca çerçeve API'leriyle kullanılır; alan tipi `Production` kalır. |
| `Activity` (S4) | `System.Diagnostics.Activity` (izleme / telemetri) | S4'te sözlükte yeniden değerlendirilecek. Aynı dosyada ikisi gerekirse telemetri tipi takma adla alınır. |
| `Contract` (S4) | Modüllerin `Contracts` projeleri (senkron sözleşme) | S4'te sözlükte yeniden değerlendirilecek. |

Çakışan bir ad kullanılacaksa, modül içinde alan (domain) adı önceliklidir. Çerçeve tipi takma adla (`using DiagnosticsActivity = System.Diagnostics.Activity;`) alınır.

## 5. Veritabanı adları

PostgreSQL, tırnaksız yazılan adları küçük harfe çevirir. PascalCase adlar (`"CreatedBy"`) her SQL sorgusunda çift tırnak gerektirir; tırnak unutulursa sorgu "kolon bulunamadı" hatası verir. Bu yüzden tüm veritabanı adları **küçük harf snake_case** yazılır ve tırnak hiç gerekmez.

Dönüşüm elle yapılmaz. EF Core'a [EFCore.NamingConventions](https://github.com/efcore/EFCore.NamingConventions) (Apache 2.0) paketi eklenir ve C# adları otomatik olarak snake_case'e çevrilir.

> **Türkçe bilgisayar tuzağı.** Bu paket geçmişte, Türkçe bölge ayarlı bilgisayarlarda migration üretirken `customer_id` kolonunu `customer_ıd` yapmıştır ([issue #49](https://github.com/efcore/EFCore.NamingConventions/issues/49)). Migration'lar geliştiricinin kendi bilgisayarında, onun kültürüyle üretildiği için sunucunun sabit kültürle çalışması bu hatayı önlemez. Güncel sürüm varsayılan olarak `InvariantCulture` kullanır. Yine de kültür **her zaman açıkça** verilir: `UseSnakeCaseNamingConvention(CultureInfo.InvariantCulture)`. Mimari test (AT-13) tüm adların ASCII olduğunu ayrıca doğrular.

### 5.1 Adlar

| Nesne | Kalıp | Örnek |
|---|---|---|
| Şema | Modül adı, snake_case | `booking`, `planning`, `identity` |
| Tablo | Varlık adı, snake_case, çoğul | `venue_holds`, `event_status_histories` |
| Kolon | Özellik adı, snake_case | `expires_at`, `hold_rank` |
| Kimlik kolonu | `id` | `id` |
| Referans kolonu (aynı ya da başka modül) | Hedef varlık + `_id` | `event_id`, `venue_id` |
| Aynı hedefe birden fazla referans | Rol + `_id` | `source_warehouse_id`, `target_warehouse_id` |
| Değer tipi kolonları (ör. `Money`) | Özellik + alt alan | `fee_amount`, `fee_currency` |
| Zaman aralığı kolonları | Özellik + `_start` / `_end` | `window_start`, `window_end` |
| Birincil anahtar | `pk_` + tablo | `pk_venue_holds` |
| Yabancı anahtar (yalnızca modül içi) | `fk_` + tablo + hedef tablo + kolon | `fk_venue_holds_hold_queues_hold_queue_id` |
| İndeks | `ix_` + tablo + kolonlar | `ix_venue_holds_event_id` |
| Benzersiz indeks | `ux_` + tablo + kolonlar | `ux_equipment_units_qr_code` |
| Kontrol kısıtı | `ck_` + tablo + kural | `ck_equipment_reservations_quantity_positive` |
| Dışlama kısıtı (zaman çakışması) | `ex_` + tablo + kural | `ex_venue_holds_active_overlap` |
| Migration geçmişi tablosu | Her şemada `__ef_migrations_history` | `booking.__ef_migrations_history` |
| Outbox / inbox tabloları | Her şemada | `booking.outbox_messages`, `booking.inbox_messages` |
| Tekrar güvenliği tablosu | Her şemada | `booking.idempotency_keys` |
| Migration dosyası | Tarih damgası (EF üretir) + PascalCase açıklama | `20261102143000_AddHoldQueue` |

**Kurallar:**
- **Uzunluk sınırı:** PostgreSQL adları en fazla 63 bayttır ve daha uzun adları **hata vermeden keser** ([kaynak](https://til.hashrocket.com/posts/8f87c65a0a-postgresqls-max-identifier-length-is-63-bytes)). EF'in ürettiği uzun indeks ya da kısıt adları kesilince iki farklı ad aynı hale gelebilir ve migration beklenmedik biçimde bozulur. Mimari test (AT-13) her adın 63 baytı aşmadığını doğrular. Aşan ad elle kısaltılır (`HasDatabaseName`).
- **Ayrılmış kelimeler:** Hiçbir tablo ya da kolon adı PostgreSQL'in ayrılmış kelimelerinden biri olamaz (`user`, `order`, `group`, `end`, `case`, `check`, `default`, `limit`, `offset`, `table`, `column`, `window` vb.). Tablo adlarının çoğul olması bu çakışmaların çoğunu kendiliğinden önler: `User` → `users`, `Case` → `cases`. Kolonlarda bir önek eklenir: `End` değil `EndsAt`, `Order` değil `SortOrder`.
- **Tablo adı açıkça yazılır.** EF Core, `DbSet` özelliği olmayan varlıklarda tablo adını tekil sınıf adından üretir; bu da çoğul ve tekil tabloların karışmasına yol açar. Bu yüzden her tablo eşlemesinde (`…Configuration`) tablo adı `ToTable("venue_holds")` ile açıkça verilir.
- **Benzersiz indeksler** `ux_` önekini almak için `HasDatabaseName` ile açıkça adlandırılır. EF bunları varsayılan olarak `ix_` ile adlandırır.
- Enum'ların veritabanında nasıl saklanacağı (metin mi sayı mı) C.4'te belirlenecek.

### 5.2 Zaman, tarih ve süre sonekleri

Adın soneki, değerin tipini ve anlamını belirler. "Bu tarih mi, zaman damgası mı, hangi saat diliminde?" sorusu adla cevaplanır.

| Sonek | Anlamı | C# tipi | PostgreSQL tipi | Örnek |
|---|---|---|---|---|
| `…At` / `_at` | Zaman çizgisinde tek bir an | `DateTimeOffset` | `timestamptz` | `ConfirmedAt`, `expires_at` |
| `…Date` / `_date` | Takvim günü (saat yok) | `DateOnly` | `date` | `CountDate` |
| `…Time` / `_time` | Günün saati (tarih yok) | `TimeOnly` | `time` | `CurfewTime` |
| `…By` / `_by` | İşlemi yapan kullanıcı | kullanıcı kimliği | kimlik tipi | `ConfirmedBy` |
| Süre (birim yok) | Süre | `TimeSpan` | `interval` | `PrepBuffer` |
| Süre (sayı olarak) | Birim sonekli | `int` | `integer` | `TimeoutSeconds` |

İşlemi yapan kullanıcıya verilen referanslar `…By` ile adlandırılır (`confirmed_by`); bu, referans kolonlarındaki `_id` kuralının tek istisnasıdır, çünkü ad zaman damgasıyla (`confirmed_at`) çift oluşturur.

Sözlükteki zaman adları bu kurala zaten uyar: `StartsAt`, `DoorsAt`, `LoadOutAt`, `HoldExpiresAt`.

Gelecekteki duvar saati zamanlarının kaynak değeri `…AtLocal` / `_at_local` (saat dilimsiz), saat dilimi kimliği `TimeZone` / `time_zone` kolonundadır; türetilen UTC değeri `…At` / `_at` adını taşır ([database §7.2](database.md#72-gelecekteki-duvar-saati-zamanları)).

### 5.3 Ortak ve özel amaçlı kolonlar

| Kolon | Anlamı | Kaynak |
|---|---|---|
| `created_at`, `created_by`, `updated_at`, `updated_by` | Oluşturma ve son değişiklik | [database §9](database.md#9-ortak-kolonlar) |
| `version` | İyimser kilit sürüm numarası (toplu kök) | [database §11.1](database.md#111-sürüm-numarasıyla-iyimser-kilit) |
| `deactivated_at`, `deactivated_by` | Pasifleştirme; boşsa kayıt aktiftir | [database §10.1](database.md#101-pasifleştirme) |
| `…_percent` | Yüzde, 0–100 | [database §8.3](database.md#83-oranlar-ve-ölçüler) |
| `…_search` | Büyük / küçük harf ve Türkçe karakter duyarsız arama anahtarı | [database §13](database.md#13-metin-sıralama-ve-arama) |
| `…_normalized` | Duyarsız benzersizlik için normalleştirilmiş değer (ör. `email_normalized`) | [database §13](database.md#13-metin-sıralama-ve-arama) |

## 6. API adları

Adres yapısı, sürümleme, sayfalama ve hata biçimi [api.md](api.md)'dedir. Bu bölüm yalnızca adların yazımını sabitler.

| Öğe | Biçim | Örnek |
|---|---|---|
| Adres kökü | `/api/v1`; modül adı adreste yer almaz | `/api/v1/events` |
| Adres bölümü | kebab-case, kaynaklar çoğul | `/venue-holds`, `/equipment-units` |
| Adres parametresi | camelCase + `Id` | `/events/{eventId}/holds` |
| Durum geçişi eylemi | Kaynağın altında fiil, kebab-case | `POST /events/{eventId}/confirm` |
| Süre alanı | Dakika cinsinden tamsayı, `…Minutes` | `prepBufferMinutes` |
| Sorgu parametresi | camelCase | `?status=confirmed&pageSize=20` |
| JSON alanı | camelCase | `"startsAt": "…"` |
| JSON enum değeri | camelCase metin | `"status": "holdPlaced"` |
| OpenAPI işlem adı (operationId) | Komut / sorgu adı − sonek; tüm API'de benzersiz | `ConfirmEvent`, `ListEvents`, `GetEvent` |
| OpenAPI etiketi (tag) | Modül adı | `Booking` |

**İşlem adı neden önemli:** Ön yüzün API istemcisi (Orval) OpenAPI belgesinden üretilir ve kanca adlarını işlem adından türetir: `ConfirmEvent` → `useConfirmEvent`. İşlem adı verilmezse Orval adres ve metottan uzun, kararsız adlar üretir; bir adres değişince ön yüzdeki tüm kullanımlar da değişir. Bu yüzden her uç nokta `WithName("ConfirmEvent")` ile adlandırılır ([kaynak](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/include-metadata?view=aspnetcore-10.0)). Etiketin modül adı olması, Orval'ın üretilen kodu modül modül klasörlemesini sağlar. Mimari test (AT-14) her uç noktanın benzersiz bir işlem adı, modül etiketi ve açıklaması (summary) olduğunu doğrular.

**Enum değerleri neden camelCase metin:** Sayı olarak gönderilen enum'lar okunmaz ve sıraları değişince sessizce anlam değiştirir. camelCase metin, JSON alan adlarıyla aynı biçimdedir ve doğrudan çeviri anahtarına dönüşür (`holdPlaced` → `booking:eventStatus.holdPlaced`). .NET'in OpenAPI üretimi, metin enum'larda şemaya `type: string` bilgisini her durumda eklemeyebilir ([kaynak](https://github.com/dotnet/aspnetcore/issues/62022)). Bu, C.5'te bir şema dönüştürücüsüyle garanti altına alınacak.

## 7. Ön yüz adları

| Öğe | Biçim | Örnek |
|---|---|---|
| Dosya ve klasör | kebab-case | `event-list-page.tsx`, `use-event-filters.ts` |
| Bileşen | PascalCase | `EventListPage` |
| Sayfa bileşeni | Ad + `Page` | `EventDetailsPage` |
| Diyalog, form | Ad + `Dialog` / `Form` | `PlaceHoldDialog`, `EventForm` |
| Kanca (hook) | `use` + PascalCase | `useEventFilters` |
| Bileşen özellikleri tipi | Bileşen + `Props` | `EventListPageProps` |
| Tip | PascalCase; `I` ya da `T` öneki yok | `EventFilters` |
| Değişken, fonksiyon | camelCase | `selectedWarehouse`, `formatMoney()` |
| Değişmez ilkel sabit | UPPER_SNAKE_CASE | `SCAN_DEBOUNCE_MILLISECONDS` |
| Mantıksal değişken | `is` / `has` / `can` / `should` + ad | `isScanning` |
| Olay özelliği / işleyicisi | `on` + olay / `handle` + olay | `onConfirm` / `handleConfirm` |
| Zod şeması / form değerleri tipi | Ad + `Schema` / Ad + `FormValues` | `eventFormSchema` / `EventFormValues` |
| Birim testi dosyası | Kaynak dosya + `.test` | `event-list-page.test.tsx` |
| Uçtan uca test dosyası | Senaryo + `.spec` | `mvp-demo.spec.ts` |
| Tasarım sistemi örneği | Kaynak dosya + `.stories` | `status-badge.stories.tsx` |
| CSS tasarım değişkeni | kebab-case, anlamı söyleyen ad; ham değerler `palette-` önekli ve OKLCH açıklığıyla numaralı ([ui §3.1](ui.md#31-yapı)) | `--status-warning-surface`, `--palette-violet-540` |

**Kurallar:**
- **Yalnızca adlı dışa aktarım.** `export default` kullanılmaz. Adlı dışa aktarım yeniden adlandırmada güvenlidir, aramada tek adla bulunur ve otomatik içe aktarmayı tutarlı yapar. İstisna: aracın zorunlu kıldığı yapılandırma dosyaları (`vite.config.ts`, `eslint.config.js`, `.storybook/`) ve Storybook örnek dosyaları (`*.stories.tsx`).
- **Tipler için `type`.** `interface` yalnızca birleştirme (declaration merging) gerektiğinde, örneğin çeviri tiplerinde kullanılır.
- **TypeScript `enum` yok.** Orval, enum'ları sabit nesne (`as const`) ve birleşim tipi olarak üretir. Elle yazılan kodda da aynı yöntem kullanılır. `erasableSyntaxOnly` derleyici ayarı `enum`'u zaten yasaklar ([code-style §5](code-style.md#5-typescript-ve-react)).
- **Yönlendirme dosyaları** TanStack Router'ın kurallarına uyar ([kaynak](https://tanstack.com/router/latest/docs/routing/file-naming-conventions)). Adres bölümleri kebab-case'tir. Parametreler `$` ile başlar (`$eventId.tsx`), dizin sayfası `index.tsx`'tir. Yönlendirme klasöründe yönlendirme olmayan dosyalar `-` önekli klasörde durur (`-components/`).
- Ön yüz modül klasörleri sunucudaki modül adlarının küçük harflisidir (`booking/`, `inventory/`).

### 7.1 Çeviri anahtarları

react-i18next ad alanları (namespace) ve anahtarlar:

| Öğe | Kural | Örnek |
|---|---|---|
| Ad alanı | Modül adı küçük harf; ortaklar için `common`, `validation`, `errors` | `booking`, `common` |
| Anahtar | camelCase, noktayla iç içe; ekran ya da özellik adıyla başlar | `eventList.title`, `placeHoldDialog.submit` |
| Alan etiketi | Varlık + alan (camelCase) | `booking:venueHold.expiresAt` |
| Enum etiketi | Enum adı (camelCase) + değer (JSON'daki gibi) | `booking:eventStatus.holdPlaced` |
| İş kuralı hata mesajı | `errors` ad alanında, anahtar kural numarasının kendisi | `errors:BR-EVT-009` |
| Doğrulama mesajı | `validation` ad alanında, kural türü | `validation:required`, `validation:maxLength` |

Kural numarası anahtar olarak aynen kullanılır (camelCase istisnası). Böylece API'nin döndüğü hata kodu çeviri dosyasında doğrudan bulunur; arada eşleme tablosu gerekmez. Anahtarlar TypeScript tiplerine bağlanır ([kaynak](https://react.i18next.com/latest/typescript)); yanlış yazılmış bir anahtar derlemede hata verir.

## 8. Yetki, ayar ve anlık bildirim adları

### 8.1 Yetki kodları

Biçim: `{Modül}.{KaynakÇoğul}.{Eylem}`, örneğin `Booking.Events.Confirm`. Kodlar modülün `…Permissions` sınıfında sabit olarak tanımlanır ve modül tanımıyla Host'a bildirilir ([08 §5](../08-architecture.md#5-ana-uygulama-host-ve-modüllerin-kaydı)).

Eylemler ortak bir sözlükten seçilir, eş anlamlı kullanılmaz:

| Eylem | Anlamı | Kullanılmayan eş anlamlılar |
|---|---|---|
| `View` | Listeleme ve görüntüleme | `Read`, `List`, `Get` |
| `Create` | Yeni kayıt | `Add`, `New`, `Insert` |
| `Edit` | Var olan kaydı değiştirme | `Update`, `Modify` |
| `Deactivate` | Pasifleştirme (silme yerine) | `Delete`, `Remove` |
| Alana özgü fiil | Durum geçişleri ve özel işlemler; fiil sözlükteki işlemin adıdır | `Confirm`, `Cancel`, `Place`, `Release`, `CheckOut`, `CheckIn` |

Kalıcı silme yetkisi gerekirse C.6'daki yetki modeliyle birlikte eklenir.

### 8.2 Ayar anahtarları

- Her modülün ayarları `Modules:{Modül}` bölümündedir. Anahtarlar PascalCase'dir: `Modules:Booking:HoldExpiryWarningThreshold`.
- Ortam değişkeni biçimi .NET kuralıdır: `Modules__Booking__HoldExpiryWarningThreshold`.
- Süreler `TimeSpan` olarak yazılır (`"3.00:00:00"`). Sayıysa birim ada eklenir (`TimeoutSeconds`).
- İş kuralı parametreleri (P-01…P-15) ile ayar anahtarlarının eşlemesi, modül tasarım belgelerinde (`docs/modules/`) tablo halinde tutulur.

### 8.3 Anlık bildirim (SignalR)

| Öğe | Kalıp | Örnek |
|---|---|---|
| Hub adresi | `/hubs/{ad}` | `/hubs/notifications` |
| Kayıt grubu | `{kaynakÇoğul}:{kimlik}` | `events:{eventId}`, `warehouses:{warehouseId}` |
| Liste grubu | `{kaynakÇoğul}` | `equipment-units` |
| Kullanıcı grubu | `users:{kimlik}` | `users:{userId}` |
| İstemci metodu | camelCase fiil | `resourceChanged` |

Grup adları, ön yüzün hangi sorguyu yenileyeceğini belirler ([ADR-0012](../adr/0012-realtime-signalr.md), [api §13](api.md#13-anlık-bildirimler)). Bu yüzden grup adının kaynak bölümü, adresin kaynak bölümüyle aynıdır. Kaynak adları sözlükten geldiği ve tekil olduğu için grup adında modül yer almaz.

## 9. Telemetri ve log adları

OpenTelemetry'nin adlandırma kuralları uygulanır ([kaynak](https://opentelemetry.io/docs/specs/semconv/general/naming/)):

| Öğe | Kalıp | Örnek |
|---|---|---|
| İzleme kaynağı (ActivitySource) ve ölçüm kaynağı (Meter) | `FestOS.{Modül}` | `FestOS.Booking` |
| Özel öznitelik | `festos.` + küçük harf, noktalı; kelimeler `_` ile | `festos.event.id`, `festos.rule.code` |
| Ölçüm adı | `festos.{modül}.{ölçüm}` | `festos.messaging.outbox.pending` |
| Log şablon alanı | PascalCase, nitelikli | `{EventId}`, `{WarehouseId}`, `{RuleCode}` |

**Kurallar:**
- Özel öznitelikler OpenTelemetry'nin kendi ad alanlarıyla (`http.`, `db.`, `otel.` gibi) başlamaz. Böylece ileride standart bir adla çakışmaz.
- Log şablonlarında tek başına `{Id}` ya da `{Name}` kullanılmaz. .NET'in OpenTelemetry log aktarıcısı, logun kendi olay numarasını (`EventId`) `Id` ve `Name` alanlarına açar; aynı adlı şablon alanları bunlarla çakışır ([kaynak](https://github.com/open-telemetry/opentelemetry-dotnet/issues/4404)). Alan her zaman nitelikli yazılır: `{EventId}`, `{PartyName}`.
- Log mesajları İngilizcedir; kullanıcıya gösterilmez.

## 10. Test adları

| Öğe | Kalıp | Örnek |
|---|---|---|
| Test sınıfı | Test edilen tip + `Tests` | `VenueHoldTests`, `PlaceVenueHoldHandlerTests` |
| Test metodu | `Birim_Durum_BeklenenSonuç` | `Place_WhenQueueHasActiveHold_AssignsNextRank` |
| Kural ve geçiş etiketleri | xUnit trait'leri | `[Trait("Rule", "BR-EVT-003")]`, `[Trait("Transition", "T-HLD-01")]` |
| Veri kurucusu | Varlık + `Builder` | `EventBuilder` |
| Elle yazılmış sahte | `Fake` + tip | `FakeInventoryModule` |
| Ön yüz testi | `describe('Bileşen')` + `it('davranış')` | `describe('PlaceHoldDialog')`, `it('disables submit while saving')` |

Test metodu kalıbı Microsoft'un önerdiği biçimdir ([kaynak](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)). Test adları İngilizcedir. Alt çizgiler bu kalıpta bilerek kullanılır; adlandırma analizörü test projelerinde bu kural için susturulur.

## 11. Nasıl denetlenir

Kurallar mümkün olduğunca araçla denetlenir; kod incelemesine kalan kısım küçük tutulur.

| Kural | Denetleyen | Ne zaman |
|---|---|---|
| C# büyük / küçük harf ve önek kuralları | `.editorconfig` adlandırma kuralları | Derleme |
| Dosya adı = tip adı | Meziantou MA0048 | Derleme |
| C# adları yalnızca ASCII | Mimari test AT-12 | Test |
| Mimari yapı taşı sonekleri | Mimari testler AT-06, AT-07 | Test |
| Veritabanı adları (snake_case, çoğul tablo, 63 bayt, ayrılmış kelime, indeks önekleri) | Mimari test AT-13 | Test |
| Uç nokta işlem adı, etiketi ve açıklaması | Mimari test AT-14 | Test |
| TypeScript adları, boolean önekleri | typescript-eslint `naming-convention` | Lint |
| TypeScript adları yalnızca ASCII | ESLint `id-match` | Lint |
| Varsayılan dışa aktarım yasağı | ESLint `no-restricted-exports` | Lint |
| Çeviri anahtarları | react-i18next tipleri | Tip denetimi |
| Kısaltma yasağı, anlamlı ad, yetki eylem sözlüğü | Kod incelemesi | PR |

Araçların kurulumu ve kuralların tam listesi [code-style.md](code-style.md)'dedir. Mimari testlerin tam listesi [08 §12.2](../08-architecture.md#122-mimari-testler)'dedir.

## 12. Kararlar

| No | Konu | Karar | Gerekçe |
|---|---|---|---|
| N-01 | Veritabanı ad biçimi | Küçük harf snake_case, EFCore.NamingConventions ile otomatik; kültür açıkça `InvariantCulture` | PostgreSQL tırnaksız adları küçük harfe çevirir; PascalCase her sorguda tırnak ister. Paket Türkçe bilgisayarda `ıd` hatası yapmıştı; açık kültür bunu kesin olarak önler. |
| N-02 | Tablo adı tekil mi çoğul mu | Çoğul, her eşlemede açıkça yazılır | `user`, `order`, `group`, `case` gibi ayrılmış kelimelerle çakışmayı önler. SQL'de doğal okunur. EF'in DbSet'e bağlı karışık tekil / çoğul davranışı açık yazımla ortadan kalkar. |
| N-03 | Düzensiz çoğullar | Çoğullaştırma kütüphanesinin (Humanizer, MIT; yalnızca test projesinde) sonucu esas alınır; istisnalar testte listelenir | Kuralın tek anlamlı ve otomatik denetlenebilir olması, dilbilgisi tartışmasından önemlidir. |
| N-04 | Kısaltmaların yazımı | Kelime gibi (`QrLabel`, `PdfDocument`) | Dört katman (C#, TS, JSON, veritabanı) arasında dönüşüm tek anlamlı olur. .NET de `DbContext`, `Xml` gibi adlarda bu yolu izler. |
| N-05 | Sorgu adları | Tek kayıt `Get…Query`, liste `List…Query` | Tek harf farkla ayrılan adlar kodda ve üretilen kancalarda karışır. |
| N-06 | Okuma modelleri | `…Details`, `…ListItem`, `…Option`; `Dto` soneki yok | Tipin içeriğini anlatan ad, her şeyi taşıyan tek tipe dönüşmeyi önler. |
| N-07 | JSON enum değerleri | camelCase metin | Okunur, sıra değişince anlam kaymaz, doğrudan çeviri anahtarı olur. |
| N-08 | OpenAPI işlem adı | Her uç noktada zorunlu, komut / sorgu adıyla aynı | Üretilen ön yüz kancalarının adı kararlı olur; adres değişse de ön yüz kodu değişmez. |
| N-09 | Zaman sonekleri | `…At` an, `…Date` gün, `…Time` saat | Tarih ile zaman damgasının karışması, saat dilimi hatalarının en yaygın kaynağıdır. |
| N-10 | Yetki eylemleri | Sabit eylem sözlüğü (`View`, `Create`, `Edit`, `Deactivate` + alana özgü fiiller) | `Read` / `View` / `List` gibi eş anlamlılar yetki listesini bölmez. |
| N-11 | Log alanları | Nitelikli ad; tek başına `{Id}` ve `{Name}` yasak | OpenTelemetry log aktarıcısıyla alan çakışmasını önler. |


## 13. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-09-25 | v0.1 | İlk taslak |
| 2026-09-25 | v1.0 | Kararlar kesinleşti. |
| 2026-09-25 | v1.1 | Veritabanı standardıyla uyum: ortak ve özel amaçlı kolon adları, duvar saati kolonları (C.4). |
| 2026-09-25 | v1.2 | API standardıyla uyum: `/api/v1` kökü, süre alanları, tekrar güvenliği tablosu, SignalR grup adlarından modül çıkarıldı (C.5). |
| 2026-09-25 | v1.3 | Tasarım sistemi örneği ve CSS değişkeni adları (D.4). |
| 2026-09-26 | v1.4 | Palet değişkenlerinin numaralandırması (Faz 1.0). |
| 2026-09-26 | v1.5 | Varsayılan dışa aktarım istisnasına Storybook dosyaları eklendi (Faz 1.0). |
