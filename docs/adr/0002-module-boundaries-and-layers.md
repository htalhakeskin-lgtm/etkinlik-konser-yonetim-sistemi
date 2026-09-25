# ADR-0002: Modül sınırları ve katman kuralı

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [05-module-map.md](../05-module-map.md), [06-erd-conceptual.md](../06-erd-conceptual.md), [ADR-0001](0001-modular-monolith.md)
- **Genişletildi:** [ADR-0018](0018-integration-events-project.md) — olay tipleri ayrı projede; her yönde referans alınabilir

## Bağlam

Modüler monolitte sınırlar tanımlanmazsa modüller birbirini serbestçe çağırır. Zamanla döngüsel bağımlılıklar oluşur ve modüller ayrılamaz hale gelir. Hangi verinin hangi modüle ait olduğu ve hangi modülün hangisini çağırabileceği baştan belirlenmelidir.

## Karar

- S1'de 10 modül vardır; her verinin tek sahibi bir modüldür ([05 §3](../05-module-map.md#3-modüller), [05 §6](../05-module-map.md#6-tartışmalı-sahiplik-kararları)).
- Modüller beş katmana ayrılır. Bir modül yalnızca **kendinden aşağıdaki katmanlardaki** modüllerin sözleşmelerini senkron çağırabilir ([05 §4](../05-module-map.md#4-bağımlılıklar)).
- Ters yöndeki ihtiyaçlar, entegrasyon olayları ve yerel kopyalarla karşılanır.
- Modüller arası yabancı anahtar yoktur; başka modüldeki kayda yalnızca kimliğiyle referans verilir.
- Kurallar mimari testlerle her derlemede doğrulanır. Bir modülün uygulama projesi başka bir modülün yalnızca `Contracts` projesine, o da yalnızca izin verilen yönde referans verebilir.

## Sonuçlar

**Olumlu:**
- Bağımlılık döngüsü yapısal olarak imkansızdır.
- Her modülün neye dokunduğu ve kimden ne istediği tek tablodan okunur.
- Katman kuralı tasarım kararlarını da yönlendirir; örneğin mekan modülünün etkinlik modülünden ayrı olması bu kuraldan çıkmıştır.

**Olumsuz / bedeli:**
- Ters yön ihtiyaçları için yerel kopyalar tutulur ve olaylarla güncellenir. Bu, bazı verilerin birkaç yüz milisaniye gecikmeli olması demektir ([05 §9.5](../05-module-map.md#95-kabul-edilen-yarış-durumları)).
- Yeni bir modül eklenirken katmanı ve bağımlılıkları bilinçli olarak belirlenmelidir.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Modüller birbirini serbestçe çağırabilir | Döngüsel bağımlılıklar oluşur, modüller birbirine kilitlenir. |
| Modüller yalnızca olaylarla haberleşir, hiç senkron çağrı yoktur | Bir geçişin koşulu başka modülün güncel verisine bağlı olduğunda (ör. dönmemiş ekipman varken etkinliği kapatmamak) güvenilir kontrol yapılamaz. |
