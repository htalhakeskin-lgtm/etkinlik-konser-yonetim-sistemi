# 07 — Teknoloji Yığını

> **Durum:** v1.0 · **Son güncelleme:** 2026-09-25
> **Kararlar:** [Bölüm 8](#8-kararlar)

## 1. Bu belge ne işe yarar

Projede kullanılacak dilleri, çatıları (framework), kütüphaneleri ve araçları tek tabloda toplar. Her önemli seçimin gerekçesi, alternatifleriyle birlikte bir ADR'de yazılıdır ([adr/](adr/README.md)). Bu belge özettir; bir seçimin nedenini merak eden ilgili ADR'yi okur.

Yığına yeni bir kütüphane eklemek ya da bir kütüphaneyi değiştirmek için önce bu belge ve gerekiyorsa yeni bir ADR güncellenir.

## 2. Seçim ilkeleri

1. **Uzun destekli sürümler.** Çalışma ortamlarında (.NET, Node.js, PostgreSQL) yalnızca uzun süre desteklenen sürümler kullanılır.
2. **Serbest lisans.** Yalnızca açık kaynak ve ticari kullanımda da ücretsiz lisanslı bağımlılıklar seçilir. Ücretli lisansa geçmiş kütüphaneler kullanılmaz ([ADR-0005](adr/0005-dependency-license-policy.md)).
3. **Az ve olgun bağımlılık.** Platformun kendi sunduğu çözüm yeterliyse ek kütüphane eklenmez. Eklenen her kütüphane yaygın kullanılan ve bakımı süren bir kütüphane olmalıdır.
4. **Tek kişinin yönetebileceği sadelik.** Aynı işi yapan iki araç yerine biri seçilir. İşletim yükü getiren araçlardan (ayrı mesaj kuyruğu sunucusu, ayrı servisler) S1'de kaçınılır.
5. **Öğrenme ve portfolyo değeri.** Projenin öğretmeyi amaçladığı konuların (outbox, modül sınırları, müsaitlik hesabı) çekirdeği hazır bir çatıya bırakılmaz, kod olarak yazılır.

## 3. Yığın

### 3.1 Sunucu (backend)

| Konu | Seçim | Sürüm | Lisans | ADR |
|---|---|---|---|---|
| Dil ve platform | C# / .NET | .NET 10 LTS (C# 14) | MIT | [0006](adr/0006-backend-platform.md) |
| Web çatısı | ASP.NET Core Minimal API; her modül kendi uç nokta grubunu tanımlar | .NET 10 ile | MIT | [0006](adr/0006-backend-platform.md) |
| API belgesi | ASP.NET Core'un dahili OpenAPI üretimi + Scalar arayüzü | — | MIT | [0006](adr/0006-backend-platform.md) |
| Komut / sorgu işleyicileri | Kendi basit arayüzlerimiz; doğrulama, işlem birimi ve loglama DI dekoratörleriyle (Scrutor) | — | MIT | [0006](adr/0006-backend-platform.md) |
| Doğrulama | FluentValidation | 12.x | Apache 2.0 | [0006](adr/0006-backend-platform.md) |
| Nesne eşleme | Elle yazılır; gerekirse Mapperly (kaynak kod üretimi) | — | Apache 2.0 | [0006](adr/0006-backend-platform.md) |
| Anlık güncelleme | ASP.NET Core SignalR | .NET 10 ile | MIT | [0012](adr/0012-realtime-signalr.md) |
| Kimlik doğrulama | ASP.NET Core çerez kimlik doğrulaması, sunucu tarafında saklanan oturumlar, ASP.NET Core Identity parola özetleme | .NET 10 ile | MIT | [0011](adr/0011-authentication.md) |
| Olaylar, outbox, inbox | Kendi yazdığımız, süreç içi olay yolu | — | — | [0010](adr/0010-messaging-infrastructure.md) |
| Zamanlanmış işler | .NET hosted service + Cronos (cron ifadesi) + PostgreSQL advisory lock | — | MIT | [0013](adr/0013-scheduled-jobs.md) |
| PDF | QuestPDF | 2026.x | Topluluk lisansı* | [0014](adr/0014-documents-and-qr.md) |
| QR üretimi | QRCoder | — | MIT | [0014](adr/0014-documents-and-qr.md) |
| Zaman ve para | .NET tipleri (`DateTimeOffset`, `DateOnly`, `TimeOnly`, `TimeSpan`), `TimeProvider`; kendi `Money` değer tipimiz | — | — | [0017](adr/0017-time-and-money-types.md) |

\* QuestPDF; bireyler, açık kaynak projeler ve yıllık geliri 1 milyon ABD dolarının altındaki şirketler için ücretsizdir. Bu projede ücret ödenmez. Eşik aşılırsa lisans satın alınmaz, PDF üretimi MIT lisanslı PDFsharp / MigraDoc'a taşınır (T-03).

### 3.2 Veri

| Konu | Seçim | Sürüm | Lisans | ADR |
|---|---|---|---|---|
| Veritabanı | PostgreSQL | 18 | PostgreSQL | [0007](adr/0007-data-access.md) |
| ORM | Entity Framework Core + Npgsql sağlayıcısı | 10 | MIT / PostgreSQL | [0007](adr/0007-data-access.md) |
| Şema yönetimi | EF Core migration'ları; modül başına ayrı bağlam ve migration seti | — | — | [0007](adr/0007-data-access.md) |

### 3.3 Ön yüz (frontend)

| Konu | Seçim | Sürüm | Lisans | ADR |
|---|---|---|---|---|
| Uygulama türü | Tek sayfalı uygulama (SPA) | — | — | [0008](adr/0008-frontend-architecture.md) |
| Dil | TypeScript, katı (strict) mod | güncel kararlı | Apache 2.0 | [0008](adr/0008-frontend-architecture.md) |
| Kütüphane ve derleme | React + Vite | React 19 | MIT | [0008](adr/0008-frontend-architecture.md) |
| Yönlendirme | TanStack Router (tip güvenli) | 1.x | MIT | [0008](adr/0008-frontend-architecture.md) |
| Sunucu verisi | TanStack Query | 5.x | MIT | [0008](adr/0008-frontend-architecture.md) |
| API istemcisi | Orval: OpenAPI belgesinden tipler ve TanStack Query kancaları üretir | — | MIT | [0008](adr/0008-frontend-architecture.md) |
| Arayüz bileşenleri | shadcn/ui + Tailwind CSS | Tailwind 4 | MIT | [0009](adr/0009-ui-components.md) |
| Tablolar | TanStack Table | 8.x | MIT | [0009](adr/0009-ui-components.md) |
| Formlar | React Hook Form + Zod | — | MIT | [0009](adr/0009-ui-components.md) |
| Çok dillilik altyapısı | react-i18next (S1'de yalnızca Türkçe, K-02) | — | MIT | [0008](adr/0008-frontend-architecture.md) |
| Tarih ve saat | date-fns + @date-fns/tz | 4.x | MIT | [0008](adr/0008-frontend-architecture.md) |
| Anlık güncelleme istemcisi | @microsoft/signalr | — | MIT | [0012](adr/0012-realtime-signalr.md) |
| QR okuma | Tarayıcının `BarcodeDetector` arayüzü + desteklemeyen tarayıcılar için barcode-detector yedeği | — | MIT | [0014](adr/0014-documents-and-qr.md) |
| Çalışma ortamı ve paket yöneticisi | Node.js LTS + pnpm | Node 24 (Ekim 2026'da LTS olunca 26) | MIT | — |

### 3.4 Test

| Konu | Seçim | Lisans | ADR |
|---|---|---|---|
| Birim ve entegrasyon test çatısı | xUnit v3 | Apache 2.0 | [0015](adr/0015-testing-tools.md) |
| Doğrulama ifadeleri | Shouldly | BSD | [0015](adr/0015-testing-tools.md) |
| Sahte nesneler | NSubstitute | BSD | [0015](adr/0015-testing-tools.md) |
| Gerçek veritabanıyla test | Testcontainers (PostgreSQL) + Respawn (testler arası temizlik) | MIT / Apache 2.0 | [0015](adr/0015-testing-tools.md) |
| Zamanı ilerletme | `FakeTimeProvider` | MIT | [0015](adr/0015-testing-tools.md) |
| Mimari testler | ArchUnitNET | Apache 2.0 | [0015](adr/0015-testing-tools.md) |
| Ön yüz testleri | Vitest + Testing Library | MIT | [0015](adr/0015-testing-tools.md) |
| Uçtan uca testler | Playwright | Apache 2.0 | [0015](adr/0015-testing-tools.md) |

### 3.5 Gözlemlenebilirlik ve yerel geliştirme

| Konu | Seçim | Lisans | ADR |
|---|---|---|---|
| Log, iz (trace) ve ölçüm | .NET'in yerleşik loglaması + OpenTelemetry | Apache 2.0 | [0016](adr/0016-observability-and-local-dev.md) |
| Yerel geliştirme | Aspire: PostgreSQL konteynerini, API'yi ve ön yüzü tek komutla başlatır; logları, izleri ve ölçümleri tek panelde gösterir | MIT | [0016](adr/0016-observability-and-local-dev.md) |
| Konteyner | Docker (Aspire ve Testcontainers için) | — | — |

## 4. Mimari kararlar (Faz 0 B bölümünden)

Modül haritasında verilen ve kodun yapısını belirleyen kararlar da ADR olarak kaydedildi:

| ADR | Karar |
|---|---|
| [0001](adr/0001-modular-monolith.md) | Modüler monolit |
| [0002](adr/0002-module-boundaries-and-layers.md) | Modül sınırları ve katman kuralı |
| [0003](adr/0003-integration-events-and-outbox.md) | Modüller arası iletişim: entegrasyon olayları, outbox ve inbox |
| [0004](adr/0004-commitments-vs-facts.md) | Taahhüt ile olgunun ayrılması (Planning ve Inventory) |

## 5. Kullanılmayan kütüphaneler

Yaygın oldukları halde bilinçli olarak kullanılmayanlar:

| Kütüphane | Neden | Yerine |
|---|---|---|
| MediatR | 2025'te ücretli lisansa geçti | Kendi komut ve sorgu işleyici arayüzlerimiz |
| AutoMapper | 2025'te ücretli lisansa geçti | Elle eşleme; gerekirse Mapperly |
| MassTransit | v9 ile ücretli lisansa geçti; ayrıca S1'de mesaj kuyruğu sunucusu yok | Kendi outbox ve inbox'ımız ([0010](adr/0010-messaging-infrastructure.md)) |
| FluentAssertions | v8 ile ücretli lisansa geçti | Shouldly |
| Moq | 2023'teki gizli veri toplama tartışması nedeniyle güven kaybı | NSubstitute |
| Swashbuckle | .NET 9'dan beri şablonlardan çıkarıldı; yerini dahili OpenAPI aldı | Dahili OpenAPI + Scalar |
| Next.js | İç kullanıma yönelik, giriş gerektiren bir iş uygulamasında sunucu tarafı üretim gerekmiyor | React + Vite SPA ([ADR-0008](adr/0008-frontend-architecture.md)) |

## 6. Sonraki sürümlerde verilecek teknoloji kararları

| Sürüm | İhtiyaç | Adaylar |
|---|---|---|
| S2 | Kaynak takvimi (Gantt görünümü) | vis-timeline, açık kaynak zaman çizelgesi bileşenleri |
| S2 | Excel / CSV dışa aktarım | ClosedXML |
| S2 | Dosya saklama | Yerel disk ya da S3 uyumlu nesne deposu |
| S5 | Harita | Leaflet + OpenStreetMap |
| S5 | Mesafe ve yol süresi | Açık kaynak yönlendirme servisi (OSRM) ya da ticari API |
| S6 | E-posta gönderimi | SMTP sağlayıcısı |

## 7. Sürüm politikası

| Bileşen | Politika |
|---|---|
| .NET | Yalnızca LTS. Sıradaki geçiş .NET 12 LTS'e (Kasım 2027). .NET 11 atlanır, çünkü desteği .NET 10 ile aynı tarihte (Kasım 2028) biter. |
| Node.js | Güncel LTS. Node 26, Ekim 2026'da LTS olduğunda ona geçilir. |
| PostgreSQL | Ana sürüm 18. Yeni ana sürüme, desteği bitmeden önce planlı olarak geçilir. |
| Kütüphaneler | Sürümler merkezi olarak sabitlenir (.NET'te merkezi paket yönetimi, ön yüzde kilit dosyası). Güncellemeler otomatik araçla PR olarak gelir (Faz 0 D bölümü). |

## 8. Kararlar

| No | Soru | Karar |
|---|---|---|
| T-01 | Ön yüz türü | React + Vite ile tek sayfalı uygulama ([ADR-0008](adr/0008-frontend-architecture.md)). |
| T-02 | Arayüz bileşen kütüphanesi | shadcn/ui + Tailwind CSS ([ADR-0009](adr/0009-ui-components.md)). |
| T-03 | QuestPDF'in topluluk lisansı | Kabul edildi; bu projede ücret ödenmez. Eşik aşılırsa lisans satın alınmaz, PDF üretimi MIT lisanslı bir kütüphaneye taşınır ([ADR-0014](adr/0014-documents-and-qr.md)). |

## 9. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-09-25 | v0.1 | İlk taslak |
| 2026-09-25 | v1.0 | Kararlar: React + Vite SPA, shadcn/ui, ücret ödenmeyecek koşuluyla QuestPDF. ADR-0005…0017 kabul edildi. |
