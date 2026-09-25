# 07 — Teknoloji Yığını

> **Durum:** v1.6 · **Son güncelleme:** 2026-09-25
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
| API belgesi | ASP.NET Core'un dahili OpenAPI üretimi + Scalar arayüzü; belge derleme sırasında dosyaya da üretilir (Microsoft.Extensions.ApiDescription.Server) | — | MIT | [0006](adr/0006-backend-platform.md) |
| Kod biçimi | CSharpier | 1.x | MIT | [0019](adr/0019-code-style-and-static-analysis-tools.md) |
| Derleme analizörleri | .NET analizörleri (kültür kuralları hata seviyesinde) + Meziantou.Analyzer + BannedApiAnalyzers (yasak API listesi) | — | MIT | [0019](adr/0019-code-style-and-static-analysis-tools.md) |
| Komut / sorgu işleyicileri | Kendi basit arayüzlerimiz; doğrulama, işlem birimi ve loglama DI dekoratörleriyle (Scrutor) | — | MIT | [0006](adr/0006-backend-platform.md) |
| Doğrulama | FluentValidation | 12.x | Apache 2.0 | [0006](adr/0006-backend-platform.md) |
| Nesne eşleme | Elle yazılır; gerekirse Mapperly (kaynak kod üretimi) | — | Apache 2.0 | [0006](adr/0006-backend-platform.md) |
| Anlık güncelleme | ASP.NET Core SignalR | .NET 10 ile | MIT | [0012](adr/0012-realtime-signalr.md) |
| Kimlik doğrulama | ASP.NET Core çerez kimlik doğrulaması, sunucu tarafında saklanan oturumlar, ASP.NET Core Identity parola özetleme | .NET 10 ile | MIT | [0011](adr/0011-authentication.md) |
| Veri koruma anahtarları | ASP.NET Core Data Protection, anahtarlar veritabanında (EF Core sağlayıcısı) | .NET 10 ile | MIT | [0027](adr/0027-session-and-password-security.md) |
| Yaygın şifre listesi | SecLists'ten derlenmiş liste, uygulamayla birlikte dağıtılır | — | MIT | [security §5.1](standards/security.md#51-şifre-politikası) |
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
| PostgreSQL eklentileri | `btree_gist` (zaman çakışması kısıtları), `pg_trgm` (metin araması); PostgreSQL ile birlikte gelir | 18 | PostgreSQL | [database §3](standards/database.md#3-veritabanı-ve-şemalar) |
| Veritabanı adları | EFCore.NamingConventions: C# adlarını snake_case'e çevirir (kültür açıkça `InvariantCulture`) | 10.x | Apache 2.0 | [naming §5](standards/naming.md#5-veritabanı-adları) |

### 3.3 Ön yüz (frontend)

| Konu | Seçim | Sürüm | Lisans | ADR |
|---|---|---|---|---|
| Uygulama türü | Tek sayfalı uygulama (SPA) | — | — | [0008](adr/0008-frontend-architecture.md) |
| Dil | TypeScript, katı (strict) mod | 6.x (7.x'e typescript-eslint desteğiyle geçilir) | Apache 2.0 | [0008](adr/0008-frontend-architecture.md), [0019](adr/0019-code-style-and-static-analysis-tools.md) |
| Kütüphane ve derleme | React + Vite | React 19 | MIT | [0008](adr/0008-frontend-architecture.md) |
| Otomatik önbellekleme | React Compiler | 1.x | MIT | [0019](adr/0019-code-style-and-static-analysis-tools.md) |
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
| Lint | ESLint 10 + typescript-eslint (tip bilgili) + React Hooks, TanStack, erişilebilirlik ve modül sınırı eklentileri | — | MIT | [0019](adr/0019-code-style-and-static-analysis-tools.md) |
| Kod biçimi | Prettier + Tailwind sınıf sıralama eklentisi | 3.x | MIT | [0019](adr/0019-code-style-and-static-analysis-tools.md) |
| Çalışma ortamı ve paket yöneticisi | Node.js LTS + pnpm | Node 24 (Ekim 2026'da LTS olunca 26) | MIT | — |

### 3.4 Test

| Konu | Seçim | Lisans | ADR |
|---|---|---|---|
| Birim ve entegrasyon test çatısı | xUnit v3 | Apache 2.0 | [0015](adr/0015-testing-tools.md) |
| Doğrulama ifadeleri | Shouldly | BSD | [0015](adr/0015-testing-tools.md) |
| Sahte nesneler | NSubstitute | BSD | [0015](adr/0015-testing-tools.md) |
| Gerçek veritabanıyla test | Testcontainers (PostgreSQL) + Respawn (testler arası temizlik) | MIT / Apache 2.0 | [0015](adr/0015-testing-tools.md) |
| Zamanı ilerletme | `FakeTimeProvider` | MIT | [0015](adr/0015-testing-tools.md) |
| Mimari testler | ArchUnitNET; tablo adlarının çoğul denetimi için yalnızca test projesinde Humanizer | Apache 2.0 / MIT | [0015](adr/0015-testing-tools.md) |
| Ön yüz testleri | Vitest + Testing Library; API sahteleme için MSW | MIT | [0015](adr/0015-testing-tools.md) |
| Uçtan uca testler | Playwright | Apache 2.0 | [0015](adr/0015-testing-tools.md) |
| Özellik tabanlı testler | CsCheck (yalnızca hesaplama motorlarında) | Apache 2.0 | [0029](adr/0029-test-strategy-and-rule-traceability.md) |
| Mutasyon testi | Stryker.NET (sürüm öncesi, elle) | Apache 2.0 | [0029](adr/0029-test-strategy-and-rule-traceability.md) |
| Kod kapsamı | coverlet (Microsoft Testing Platform eklentisi) + ReportGenerator | MIT / Apache 2.0 | [0029](adr/0029-test-strategy-and-rule-traceability.md) |
| Erişilebilirlik testi | @axe-core/playwright | MPL 2.0 (değiştirilmeden) | [0029](adr/0029-test-strategy-and-rule-traceability.md) |

### 3.5 Gözlemlenebilirlik ve yerel geliştirme

| Konu | Seçim | Lisans | ADR |
|---|---|---|---|
| Log, iz (trace) ve ölçüm | .NET'in yerleşik loglaması + OpenTelemetry | Apache 2.0 | [0016](adr/0016-observability-and-local-dev.md) |
| Yerel geliştirme | Aspire: PostgreSQL konteynerini, API'yi ve ön yüzü tek komutla başlatır; logları, izleri ve ölçümleri tek panelde gösterir | MIT | [0016](adr/0016-observability-and-local-dev.md) |
| Konteyner | Docker (Aspire ve Testcontainers için); demo ortamında Docker Compose | — | — |

### 3.6 Geliştirme süreci

| Konu | Seçim | Lisans | ADR |
|---|---|---|---|
| Kod barındırma, görev takibi, sürekli entegrasyon | GitHub (Issues, Projects, Actions) | Ücretsiz plan | [0028](adr/0028-development-workflow.md) |
| Commit kancaları | Lefthook | MIT | [0028](adr/0028-development-workflow.md) |
| Commit mesajı ve PR başlığı denetimi | commitlint; action-semantic-pull-request | MIT | [0028](adr/0028-development-workflow.md) |
| Sürüm numarası | MinVer (git etiketlerinden) | Apache 2.0 | [0028](adr/0028-development-workflow.md) |
| Sürüm PR'ı ve değişiklik günlüğü | release-please | Apache 2.0 | [0028](adr/0028-development-workflow.md) |
| Bağımlılık güncellemeleri | Dependabot | GitHub özelliği | [0028](adr/0028-development-workflow.md) |
| Gizli bilgi taraması | gitleaks (CLI) | MIT | [0028](adr/0028-development-workflow.md) |
| İş akışı denetimi | actionlint; zizmor | MIT | [ci](standards/ci.md) |
| Lisans denetimi | nuget-license; `pnpm licenses` | Apache 2.0 | [ci](standards/ci.md) |
| İmaj güvenlik taraması | Grype | Apache 2.0 | [ci](standards/ci.md) |

### 3.7 Ortamlar ve yayın

| Konu | Seçim | Lisans | ADR |
|---|---|---|---|
| Konteyner imajı | .NET SDK'nın yerleşik konteyner üretimi; `aspnet:10.0-noble-chiseled-extra` temel imajı; x64 + ARM64 | MIT | [0030](adr/0030-demo-environment-and-deployment.md) |
| İmaj kaydı | GitHub Container Registry (`ghcr.io`) | GitHub özelliği | [0030](adr/0030-demo-environment-and-deployment.md) |
| Ters proxy ve HTTPS | Caddy | Apache 2.0 | [0030](adr/0030-demo-environment-and-deployment.md) |
| Yedekleme ve zamana göre geri dönüş | WAL-G | Apache 2.0 | [0031](adr/0031-backup-and-point-in-time-recovery.md) |
| Telemetri toplayıcı | Grafana Alloy | Apache 2.0 | [0032](adr/0032-production-telemetry-and-alerts.md) |
| Telemetri ve uyarı hizmeti | Grafana Cloud (ücretsiz plan, AB bölgesi) | Hizmet | [0032](adr/0032-production-telemetry-and-alerts.md) |
| Demo verisi üretimi | Bogus (sabit tohumla) | MIT | [0030](adr/0030-demo-environment-and-deployment.md) |
| Demo sunucusu | Oracle Cloud "Always Free" ARM sunucusu ve nesne deposu | Ücretsiz plan | [0030](adr/0030-demo-environment-and-deployment.md) |
| Demo alan adı | deSEC, ücretsiz `dedyn.io` alt alan adı | Ücretsiz hizmet | [0030](adr/0030-demo-environment-and-deployment.md) |

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
| TypeScript | 6.x'e sabit. TypeScript 7.x'e, programatik API'yi sunan sürüm (7.1 bekleniyor) çıkıp typescript-eslint onu desteklediğinde geçilir ([0019](adr/0019-code-style-and-static-analysis-tools.md)). |
| PostgreSQL | Ana sürüm 18. Yeni ana sürüme, desteği bitmeden önce planlı olarak geçilir. |
| Kütüphaneler | Sürümler merkezi olarak sabitlenir (.NET'te merkezi paket yönetimi, ön yüzde kilit dosyası). Güncellemeler Dependabot ile haftalık PR olarak gelir; yeni yayımlanan sürümler 3 gün, büyük sürümler 7 gün bekletilir ([git §9](standards/git.md#9-bağımlılık-güncellemeleri)). |

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
| 2026-09-25 | v1.1 | Derleme analizörleri ve derleme sırasında OpenAPI üretimi eklendi (C.2). |
| 2026-09-25 | v1.2 | C.3: kod biçimi ve statik analiz araçları (CSharpier, Meziantou.Analyzer, ESLint 10, Prettier), React Compiler ve EFCore.NamingConventions eklendi; TypeScript 6.x'e sabitlendi (ADR-0019). |
| 2026-09-25 | v1.3 | C.4: PostgreSQL eklentileri eklendi. |
| 2026-09-25 | v1.4 | C.6: veri koruma anahtarlarının saklanması ve yaygın şifre listesi eklendi. |
| 2026-09-25 | v1.5 | D.1–D.2: geliştirme süreci araçları (§3.6) ve test stratejisinin araçları (CsCheck, Stryker.NET, coverlet, axe-core) eklendi; MSW tabloya işlendi; bağımlılık güncelleme politikası bağlandı (ADR-0028, ADR-0029). |
| 2026-09-25 | v1.6 | D.3: ortamlar ve yayın araçları (§3.7), CI denetim araçları eklendi (ADR-0030…0032). |
