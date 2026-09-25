# FestOS — Etkinlik ve Konser Yönetim Sistemi

> **English summary:** An ERP for a live-events production company that both promotes its own concerts and rents out sound, light, stage and crew to other events. It covers the full stack of enterprise planning in one product: MRP (equipment needs netted from artist technical riders across multiple warehouses), MRP II (crew and resource scheduling with a conflict engine), CRM (deals, quotes, contracts, sponsorship), finance (budgets, multi-currency, artist settlement) and DRP (tour logistics). Built as a modular monolith with .NET, React/Next.js and PostgreSQL.

Konser ve etkinlik sektöründe hem kendi etkinliklerini düzenleyen hem de teknik hizmet veren bir prodüksiyon şirketi için ERP. Bir etkinliğin talepten hesaplaşmaya kadar tüm yaşam döngüsünü yönetir.

## Durum

**Faz 0: Planlama.** Kod yazılmadan önce kapsam, terimler, iş kuralları, veri modeli ve standartlar belgeleniyor.

| Belge | Durum |
|---|---|
| [00 — Kapsam ve MVP sınırları](docs/00-scope.md) | v1.6 |
| [01 — Terimler sözlüğü](docs/01-glossary.md) | v1.8 |
| [02 — Kullanıcı hikayeleri](docs/02-user-stories/README.md) | v1.6 |
| [03 — İş kuralları](docs/03-business-rules.md) | v1.6 |
| [04 — Durum makineleri](docs/04-state-machines.md) | v1.1 |
| [05 — Modül haritası](docs/05-module-map.md) | v1.2 |
| [06 — Kavramsal veri modeli](docs/06-erd-conceptual.md) | v1.0 |
| [07 — Teknoloji yığını](docs/07-tech-stack.md) ve [ADR'ler](docs/adr/README.md) | v1.0 |
| [08 — Mimari ve klasör yapısı](docs/08-architecture.md) | v1.0 |
| C.3 — İsimlendirme ve kod standartları | Sırada |

## Teknoloji

.NET 10 · ASP.NET Core · PostgreSQL 18 · EF Core · React + Vite · TypeScript · SignalR · Modüler monolit

Ayrıntı: [07 — Teknoloji yığını](docs/07-tech-stack.md)

## Belge araçları

Belgelerdeki linkler ve kural referansları [tools/docs](tools/docs/README.md) altındaki betiklerle doğrulanır.
