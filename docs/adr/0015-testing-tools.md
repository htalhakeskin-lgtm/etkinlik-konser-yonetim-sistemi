# ADR-0015: Test araçları

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [03-business-rules.md §3](../03-business-rules.md#3-biçim-ve-kullanım), [04-state-machines.md](../04-state-machines.md), [ADR-0002](0002-module-boundaries-and-layers.md), [ADR-0005](0005-dependency-license-policy.md)

## Bağlam

- Test önceliği iş kurallarındadır: ihtiyaç hesabı, müsaitlik, çakışma motoru ve durum geçişleri.
- Veritabanı davranışı gerçek PostgreSQL'de sınanmalıdır; sahte veritabanları SQL farklılıklarını gizler.
- Modül sınırları her derlemede otomatik doğrulanmalıdır.
- Her kural ve geçiş, numarasıyla etiketlenmiş testlerle izlenebilmelidir.

Test stratejisi ve "bitti" tanımı Faz 0 D bölümünde yazılacak; bu ADR yalnızca araçları seçer.

## Karar

| Katman | Araç |
|---|---|
| Birim ve entegrasyon testleri | xUnit v3 |
| Doğrulama ifadeleri | Shouldly |
| Sahte nesneler | NSubstitute |
| Gerçek veritabanı | Testcontainers ile her test çalıştırmasında bir PostgreSQL 18 konteyneri; testler arası temizlik için Respawn |
| Zaman | `FakeTimeProvider`; otomatik geçişler, son tarihler ve zamanlanmış işler zaman ilerletilerek sınanır |
| Mimari testler | ArchUnitNET: katman kuralı, yalnızca `Contracts` projelerine referans ve modül içi katman yönleri |
| Ön yüz birim ve bileşen testleri | Vitest + Testing Library; API sahteleme için MSW |
| Uçtan uca testler | Playwright; öncelikle MVP demo senaryosu ([00 §6.1](../00-scope.md#61-bitti-kriteri-mvp-demo-senaryosu)) |

**İzlenebilirlik:** Testler kural ve geçiş numaralarıyla etiketlenir (ör. `[Trait("Rule", "BR-MRP-002")]`, `[Trait("Transition", "T-EVT-05")]`). Böylece "bu kuralı hangi testler doğruluyor?" sorusu tek aramayla cevaplanır.

## Sonuçlar

**Olumlu:**
- Tüm araçlar serbest lisanslıdır ve kendi alanlarında yaygındır.
- Veritabanı testleri üretimle aynı veritabanı sürümünde çalışır.
- Mimari kurallar yazılı bir belge olarak kalmaz, derlemeyi durduran bir test olur.

**Olumsuz / bedeli:**
- Entegrasyon testleri için Docker gerekir; testler birim testlerine göre daha yavaştır. Hızlı birim testleri ile daha yavaş entegrasyon testleri ayrı çalıştırılabilir.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| NUnit, MSTest | İkisi de yeterli; xUnit .NET topluluğunda en yaygın olanı. |
| FluentAssertions | v8 ile ücretli lisansa geçti. |
| Moq | 2023'teki gizli veri toplama tartışması nedeniyle güven kaybı. |
| EF Core InMemory sağlayıcısı ya da SQLite | Gerçek SQL davranışını, kilitleri ve PostgreSQL'e özgü özellikleri sınamaz; yanıltıcı yeşil testler üretir. |
| NetArchTest | Daha basit; ArchUnitNET kuralları daha ifade gücüyle yazmaya izin verir ve bakımı daha aktif. |
