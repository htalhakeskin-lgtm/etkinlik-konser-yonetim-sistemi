# FestOS — Etkinlik ve Konser Yönetim Sistemi

> **English summary:** An ERP for a live-events production company that both promotes its own concerts and rents out sound, light, stage and crew to other events. It covers the full stack of enterprise planning in one product: MRP (equipment needs netted from artist technical riders across multiple warehouses), MRP II (crew and resource scheduling with a conflict engine), CRM (deals, quotes, contracts, sponsorship), finance (budgets, multi-currency, artist settlement) and DRP (tour logistics). Built as a modular monolith with .NET, a React (Vite) single-page app and PostgreSQL.

Konser ve etkinlik sektöründe hem kendi etkinliklerini düzenleyen hem de teknik hizmet veren bir prodüksiyon şirketi için ERP. Bir etkinliğin talepten hesaplaşmaya kadar tüm yaşam döngüsünü yönetir.

## Durum

**Faz 0: Planlama.** Kod yazılmadan önce kapsam, terimler, iş kuralları, veri modeli ve standartlar belgeleniyor.

| Belge | Durum |
|---|---|
| [00 — Kapsam ve MVP sınırları](docs/00-scope.md) | v1.6 |
| [01 — Terimler sözlüğü](docs/01-glossary.md) | v1.9 |
| [02 — Kullanıcı hikayeleri](docs/02-user-stories/README.md) | v1.7 |
| [03 — İş kuralları](docs/03-business-rules.md) | v1.7 |
| [04 — Durum makineleri](docs/04-state-machines.md) | v1.1 |
| [05 — Modül haritası](docs/05-module-map.md) | v1.3 |
| [06 — Kavramsal veri modeli](docs/06-erd-conceptual.md) | v1.0 |
| [07 — Teknoloji yığını](docs/07-tech-stack.md) ve [ADR'ler](docs/adr/README.md) | v1.5 |
| [08 — Mimari ve klasör yapısı](docs/08-architecture.md) | v1.5 |
| [Standartlar](docs/standards/README.md): [isimlendirme](docs/standards/naming.md) · [kod stili](docs/standards/code-style.md) · [veritabanı](docs/standards/database.md) · [API](docs/standards/api.md) · [güvenlik](docs/standards/security.md) · [gözlemlenebilirlik](docs/standards/observability.md) · [yapılandırma](docs/standards/configuration.md) | Tamamlandı |
| Çalışma süreci: [git ve iş akışı](docs/standards/git.md) · [test stratejisi](docs/standards/testing.md) · [bitti tanımı](docs/standards/definition-of-done.md) | Tamamlandı |
| Çalışma süreci: ortamlar ve sürekli entegrasyon | Sırada |
| Çalışma süreci: arayüz standartları | Sırada |

## Teknoloji

.NET 10 · ASP.NET Core · PostgreSQL 18 · EF Core · React + Vite · TypeScript · SignalR · Modüler monolit

Ayrıntı: [07 — Teknoloji yığını](docs/07-tech-stack.md)

## Belge araçları

Belgelerdeki linkler ve kural referansları [tools/docs](tools/docs/README.md) altındaki betiklerle doğrulanır.
