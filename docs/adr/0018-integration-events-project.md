# ADR-0018: Entegrasyon olaylarının ayrı projede tutulması

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [ADR-0002](0002-module-boundaries-and-layers.md) (bu ADR onu genişletir), [ADR-0003](0003-integration-events-and-outbox.md), [08-architecture.md §3](../08-architecture.md#3-bir-modülün-yapısı)

## Bağlam

[ADR-0002](0002-module-boundaries-and-layers.md), bir modülün başka bir modülün yalnızca `Contracts` projesine ve yalnızca izin verilen yönde (aşağı katmana) referans verebileceğini söyler. Entegrasyon olayları ise her yöne akar; örneğin Planning (katman 2), Booking'in (katman 4) olaylarını dinler.

Olay tipleri `Contracts` projesinde durursa Planning'in Booking'in olaylarını dinleyebilmesi için Booking'in `Contracts` projesine referans vermesi gerekir. Bu da Planning'e Booking'in senkron sözleşmesini çağırma imkanı verir ve katman kuralını fiilen deler.

## Karar

- Her modülün `Contracts` projesinin yanında ayrı bir `IntegrationEvents` projesi vardır; modül beş projeden oluşur.
- `Contracts` yalnızca senkron sözleşmeyi (arayüz ve veri tipleri) içerir ve yalnızca izin verilen yönde referans alabilir.
- `IntegrationEvents` yalnızca olay tiplerini içerir ve her modül tarafından referans alınabilir.
- İki proje de yalnızca `BuildingBlocks.Contracts`'a bağımlıdır.
- Kurallar mimari testlerle (AT-03, AT-05) derleme sırasında doğrulanır.

## Sonuçlar

**Olumlu:**
- "Kimi çağırabilirim" ile "kimi dinleyebilirim" kuralları derleyici seviyesinde ayrılır. Katman kuralı olay dinleme ihtiyacı yüzünden delinmez.
- Bir modülün olaylarını dinlemek, o modülün sözleşmesine erişim vermez.

**Olumsuz / bedeli:**
- Her modülde bir proje daha vardır (S1'de toplam 10 proje).
- Olay yayınlamayan modüllerin (S1'de Parties, Catalog, Audit) `IntegrationEvents` projesi boş kalır. Bu, tüm modüllerin aynı düzende olması için bilinçli olarak kabul edilir.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Olayları `Contracts` içinde ayrı bir isim alanında tutup kuralı yalnızca mimari testle zorlamak | Kural derleyici seviyesinde değil, yalnızca testte korunur. Proje referansı yine de sözleşmeye erişim verir. |
| Tüm modüllerin olaylarını tek ortak projede toplamak | Tüm modüller tek bir projeye bağlanır; her olay değişikliği tüm modülleri etkiler ve olayların sahipliği bulanıklaşır. |
| Dinleyen modülün olay tipinin kendi kopyasını tanımlaması | Referans gerekmez, ama olayın biçimi yayınlayan ile dinleyen arasında sessizce ayrışabilir. |
