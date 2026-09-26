# FestOS

**Konser ve canlı etkinlik prodüksiyon şirketleri için ERP** · *An ERP for concert and live-event production companies*

[English](#english) · [Türkçe](#türkçe)

> **Durum / Status:** Faz 0 (belge öncelikli planlama) tamamlandı; Faz 1'de S1'in (MVP) geliştirilmesi başladı ([plan](docs/12-implementation-plan.md)).
> Phase 0 (documentation-first planning) is complete; Phase 1, the implementation of the S1 MVP, has started.

---

## English

### What it is

FestOS is an ERP for a company that both **promotes its own concerts** and **provides technical production services** (sound, light, stage, crew) to other events. It follows an event from the first inquiry through venue holds, confirmation, preparation, load-in, show and load-out to settlement, and plans the equipment, warehouses and people behind it.

### Highlights

- **MRP for live events.** Equipment needs are calculated from the artist's technical rider, which works like a versioned bill of materials. The gross need is netted against the venue's own equipment and allowed equivalents, then sourced from the home warehouse, other warehouses (as transfers) or external rental.
- **Multi-warehouse availability at minute precision**, including preparation and return buffers and stock in transit. A conflict engine detects competing demand and over-reservation.
- **Warehouse work on the phone.** QR check-out and check-in with the phone camera or hardware scanners; crate-level counting for bulk items such as cables; screens sized for gloved hands.
- **Real-time and consistent.** Changes reach other users' open screens within 300 ms for 95% of operations. The same unit can never be checked out twice, and edits are never silently overwritten (optimistic concurrency with ETag / If-Match).
- **A modular monolith with enforced boundaries.** 10 modules, each with its own database schema and database role; modules communicate through 32 integration events with an outbox and inbox; architecture tests keep the boundaries intact.
- **Traceable business rules.** 91 numbered business rules and 62 state transitions. CI will require a tagged test for every rule and transition implemented in code.

### Releases

| Release | Focus | Enterprise planning layer |
|---|---|---|
| **S1 (MVP)** | Event lifecycle, multiple warehouses and transfers, net requirements from riders, reservations, equipment conflicts, QR check-out / check-in | Core + MRP |
| S2 | Crew, skills and calls; conflicts across all resources; resource calendar; day and call sheets | MRP II |
| S3 | Deal pipeline, price lists, quotes, contracts, sponsorship | CRM |
| S4 | Budgets, income and expenses, ticket sales import, multi-currency, artist settlement | Finance |
| S5 | Vehicles, shipments, load calculation, inter-city transit checks, tour map | DRP |
| S6 | Dashboards, utilization, buy-versus-rent analysis, notifications | Reporting |

S1 is done when a scripted demo scenario runs end to end with automated tests: from a concert inquiry and venue holds to warehouse check-out, return and the audit trail.

### Status

Phase 0 produced the complete design before any code:
- scope, a domain glossary, 52 user stories with acceptance criteria, 91 business rules and state machines;
- a module map and a conceptual data model;
- 34 architecture decision records and 12 engineering standards (naming, code style, database, API, security, observability, configuration, git workflow, testing, definition of done, CI, UI);
- environments and deployment, an operations runbook, and screen templates.

The documentation is written in Turkish. Code, commit messages and the API are in English. A demo environment will be available when S1 is complete; demo accounts will be given on request.

### Tech stack

- **Backend:** .NET 10 (LTS), ASP.NET Core minimal APIs, EF Core 10, PostgreSQL 18, SignalR
- **Frontend:** React 19 single-page app with Vite, TypeScript, TanStack Router, Query and Table, shadcn/ui on Base UI, Tailwind CSS 4
- **Quality:** xUnit v3, Testcontainers, Playwright, architecture tests, Storybook
- **Operations:** OpenTelemetry, Aspire for local development, Docker Compose, Caddy, WAL-G, GitHub Actions

No paid dependency is used, and every dependency's license is checked against an explicit policy with documented exceptions ([ADR-0005](docs/adr/0005-dependency-license-policy.md)).

### How it is built

The project is planned documentation first: every decision is researched, sourced and recorded before it becomes code, and architecturally significant decisions are kept as [ADRs](docs/adr/README.md). The planning documents were prepared with AI assistance (Claude, by Anthropic); the scope, the decisions and their approval are the project owner's.

### License

Copyright © 2026 Hasan Talha Keskin. All rights reserved.

No open-source license is granted. Under GitHub's Terms of Service, other GitHub users may view and fork this repository on GitHub. Beyond that, no one may reproduce, distribute or create derivative works from it without written permission ([GitHub Docs](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/licensing-a-repository)). For permission, contact [@htalhakeskin-lgtm](https://github.com/htalhakeskin-lgtm) on GitHub.

---

## Türkçe

### Nedir

FestOS, hem **kendi konserlerini düzenleyen** hem de başka etkinliklere **teknik prodüksiyon hizmeti** (ses, ışık, sahne, ekip) veren bir şirket için ERP'dir. Bir etkinliği ilk talepten mekan opsiyonlarına, onaydan hazırlık, kurulum, gösteri ve sökümden hesaplaşmaya kadar izler; arkasındaki ekipmanı, depoları ve insanları planlar.

### Öne çıkanlar

- **Canlı etkinlikler için MRP.** Ekipman ihtiyacı, sanatçının teknik rider'ından hesaplanır; rider versiyonlu bir ürün ağacı gibi çalışır. Brüt ihtiyaçtan mekanın kendi ekipmanı ve izin verilen muadiller düşülür. Kalan ihtiyaç kaynak depodan, diğer depolardan (transferle) ya da dış kiralamayla karşılanır.
- **Dakika hassasiyetinde çoklu depo müsaitliği:** hazırlık ve dönüş payları, yoldaki stok dahil. Çakışma motoru rakip talepleri ve aşırı rezervasyonu yakalar.
- **Depo işleri telefondan.** Telefon kamerası ya da donanım okuyucuyla QR çıkış ve giriş; kablo gibi adetli kalemlerde kasa bazında sayım; eldivenle kullanılacak büyüklükte ekranlar.
- **Anlık ve tutarlı.** Değişiklikler diğer kullanıcıların açık ekranlarına işlemlerin %95'inde 300 milisaniye içinde yansır. Aynı birim iki kez çıkış yapılamaz; düzenlemeler sessizce ezilmez.
- **Sınırları korunan modüler monolit.** 10 modül; her birinin kendi veritabanı şeması ve rolü var; modüller 32 entegrasyon olayıyla, outbox ve inbox üzerinden haberleşir; mimari testler sınırları korur.
- **İzlenebilir iş kuralları.** 91 numaralı iş kuralı ve 62 durum geçişi. Kodda uygulanan her kuralın ve geçişin numarasıyla etiketli bir testi olması CI'da zorunlu olacak.

### Sürümler

| Sürüm | Odak | ERP katmanı |
|---|---|---|
| **S1 (MVP)** | Etkinlik yaşam döngüsü, çoklu depo ve transfer, rider'dan net ihtiyaç, rezervasyon, ekipman çakışması, QR ile çıkış ve giriş | Çekirdek + MRP |
| S2 | Ekip, yetkinlik ve çağrılar; tüm kaynaklarda çakışma; kaynak takvimi; day sheet ve call sheet | MRP II |
| S3 | Anlaşma hunisi, fiyat listesi, teklif, sözleşme, sponsorluk | CRM |
| S4 | Bütçe, gelir ve gider, bilet satışı aktarımı, çoklu para birimi, hesaplaşma | Finans |
| S5 | Araç, sevkiyat, yükleme hesabı, şehirler arası geçiş kontrolü, turne haritası | DRP |
| S6 | Panolar, kullanım oranı, satın al / kirala analizi, bildirimler | Raporlama |

S1, [MVP demo senaryosu](docs/00-scope.md#61-bitti-kriteri-mvp-demo-senaryosu) otomatik testlerle baştan sona geçtiğinde tamamlanmış sayılır. Senaryo, bir konser talebi ve mekan opsiyonlarından depo çıkışına, dönüşe ve işlem geçmişine kadar uzanır. Sıralamanın gerekçesi [kapsam belgesindedir](docs/00-scope.md#5-sürüm-planı).

### Durum ve belgeler

Faz 0'da kod yazılmadan önce tasarımın tamamı belgelendi. Belgeler Türkçedir; kod, commit mesajları ve API İngilizcedir. Demo ortamı S1 tamamlandığında açılacak; demo hesapları istek üzerine verilecek.

**Gereksinimler ve modelleme**

| Belge | Sürüm |
|---|---|
| [00 — Kapsam ve MVP sınırları](docs/00-scope.md) | v1.7 |
| [01 — Terimler sözlüğü](docs/01-glossary.md) | v1.9 |
| [02 — Kullanıcı hikayeleri](docs/02-user-stories/README.md) (52 hikaye) | v1.7 |
| [03 — İş kuralları](docs/03-business-rules.md) (91 kural) | v1.7 |
| [04 — Durum makineleri](docs/04-state-machines.md) (62 geçiş) | v1.1 |
| [05 — Modül haritası](docs/05-module-map.md) | v1.3 |
| [06 — Kavramsal veri modeli](docs/06-erd-conceptual.md) | v1.0 |

**Mimari ve teknoloji**

| Belge | Sürüm |
|---|---|
| [07 — Teknoloji yığını](docs/07-tech-stack.md) | v1.8 |
| [08 — Mimari ve klasör yapısı](docs/08-architecture.md) | v1.9 |
| [Mimari karar kayıtları (ADR)](docs/adr/README.md) | 34 kayıt |

**Standartlar** ([dizin](docs/standards/README.md))

| Belge | Sürüm |
|---|---|
| [İsimlendirme](docs/standards/naming.md) | v1.3 |
| [Kod stili ve statik analiz](docs/standards/code-style.md) | v1.7 |
| [Veritabanı](docs/standards/database.md) | v1.3 |
| [API](docs/standards/api.md) | v1.3 |
| [Güvenlik](docs/standards/security.md) | v1.3 |
| [Gözlemlenebilirlik](docs/standards/observability.md) | v1.1 |
| [Yapılandırma](docs/standards/configuration.md) | v1.2 |
| [Arayüz](docs/standards/ui.md) | v1.0 |

**Çalışma süreci ve işletim**

| Belge | Sürüm |
|---|---|
| [Git ve iş akışı](docs/standards/git.md) | v1.4 |
| [Test stratejisi](docs/standards/testing.md) | v1.2 |
| [Bitti tanımı](docs/standards/definition-of-done.md) | v1.2 |
| [Sürekli entegrasyon](docs/standards/ci.md) | v1.2 |
| [09 — Ortamlar ve yayın](docs/09-environments-and-deployment.md) | v1.1 |
| [10 — İşletim el kitabı](docs/10-operations.md) | v1.0 |
| [11 — Ekran şablonları ve envanteri](docs/11-screens.md) | v1.0 |
| [12 — Uygulama planı (Faz 1: S1)](docs/12-implementation-plan.md) | v1.0 |

### Teknoloji

- **Sunucu:** .NET 10 (LTS), ASP.NET Core minimal API, EF Core 10, PostgreSQL 18, SignalR
- **Ön yüz:** React 19 ve Vite ile tek sayfalı uygulama, TypeScript, TanStack Router, Query ve Table, Base UI temelli shadcn/ui, Tailwind CSS 4
- **Kalite:** xUnit v3, Testcontainers, Playwright, mimari testler, Storybook
- **İşletim:** OpenTelemetry, yerel geliştirmede Aspire, Docker Compose, Caddy, WAL-G, GitHub Actions

Ücretli hiçbir bağımlılık kullanılmaz; her bağımlılığın lisansı, istisnaları gerekçesiyle yazılı açık bir politikaya göre denetlenir ([ADR-0005](docs/adr/0005-dependency-license-policy.md)). Ayrıntı: [07 — Teknoloji yığını](docs/07-tech-stack.md).

### Nasıl hazırlanıyor

Proje belge öncelikli planlanıyor: her karar, koda dönüşmeden önce araştırılıyor, kaynağıyla yazılıyor ve kayda geçiyor. Mimariyi etkileyen kararlar [ADR](docs/adr/README.md) olarak tutuluyor. Planlama belgeleri yapay zekâ desteğiyle (Anthropic'in Claude modeli) hazırlandı; kapsam, kararlar ve onaylar proje sahibine aittir.

### Belge araçları

Belgelerdeki bağlantılar ve kural referansları [tools/docs](tools/docs/README.md) altındaki betiklerle doğrulanır.

### Lisans

Copyright © 2026 Hasan Talha Keskin. Tüm hakları saklıdır.

Açık kaynak lisansı verilmemiştir. GitHub'ın kullanım koşulları gereği diğer GitHub kullanıcıları bu repoyu GitHub üzerinde görüntüleyebilir ve çatallayabilir (fork). Bunun dışında yazılı izin olmadan çoğaltılamaz, dağıtılamaz ve türev çalışma yapılamaz ([GitHub belgesi](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/licensing-a-repository)). İzin için GitHub üzerinden [@htalhakeskin-lgtm](https://github.com/htalhakeskin-lgtm) ile iletişime geçin.
