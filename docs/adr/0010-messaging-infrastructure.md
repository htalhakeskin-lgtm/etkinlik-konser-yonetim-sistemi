# ADR-0010: Olay altyapısının kendimiz tarafından yazılması

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [ADR-0003](0003-integration-events-and-outbox.md), [05-module-map.md §9.2](../05-module-map.md#92-olayların-teslimi), [ADR-0005](0005-dependency-license-policy.md)

## Bağlam

[ADR-0003](0003-integration-events-and-outbox.md) outbox, inbox, anında gönderim ve aynı kayıt için sıra garantisi gerektiriyor. Bunlar hazır bir kütüphaneyle ya da kendi kodumuzla sağlanabilir. S1'de tek uygulama örneği ve süreç içi olay yolu yeterli; ayrı bir mesaj kuyruğu sunucusu gerekmiyor. Outbox ve modüller arası tutarlılık, projenin öğretmeyi amaçladığı konuların başında geliyor.

## Karar

Olay altyapısı `BuildingBlocks` içinde kendimiz tarafından yazılır:

- **Outbox:** Her modül şemasında bir `outbox` tablosu. Olaylar, işlem birimi kaydedilirken aynı işlemle yazılır.
- **Dağıtıcı:** İşlem tamamlanınca süreç içi bir sinyalle hemen uyanır. Yedek olarak outbox'ı düzenli aralıklarla tarar; aralık yapılandırılabilir, varsayılanı 5 saniyedir.
- **Olay yolu:** Olayı, abone olan modüllerin dinleyicilerine süreç içinde iletir. Farklı modüller aynı olayı paralel işler.
- **Inbox:** Her modül şemasında bir `inbox` tablosu. Dinleyici, olay kimliğini kendi işlem biriminde kaydeder; aynı olay ikinci kez işlenmez.
- **Sıra:** Aynı kaydın olayları sırayla işlenir.
- **Hata:** Artan aralıklarla yeniden deneme. Denemeler tükenirse olay hatalı olaylar listesine alınır ve sistem yöneticisine görünür.
- **Ölçüm:** Her olayın oluşma ve işlenme zamanı kaydedilir ve gecikme ölçüm olarak yayınlanır ([ADR-0016](0016-observability-and-local-dev.md)).
- **Geleceğe hazırlık:** Olay yolu bir arayüz arkasındadır. Birden fazla uygulama örneği gerekirse PostgreSQL'in LISTEN/NOTIFY özelliği ya da bir mesaj kuyruğu takılabilir.

## Sonuçlar

**Olumlu:**
- Altyapının her satırı bilinir ve açıklanabilir; bu, portfolyodaki en güçlü anlatılardan biridir.
- Lisans riski ve çatı bağımlılığı yoktur.
- Gereksinimlere (anında gönderim, modül başına şema) tam uyar.

**Olumsuz / bedeli:**
- Birkaç yüz satırlık altyapı kodu yazılır ve test edilir. Hatalı yazılırsa olay kaybı ya da tekrar riski doğar. Bu yüzden altyapı, gerçek veritabanıyla çalışan entegrasyon testleriyle kapsamlı şekilde doğrulanır.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Wolverine | Outbox, inbox, süreç içi ve dayanıklı mesajlaşmayı hazır sunar (MIT). Ama öğrenilmesi amaçlanan mekanizmayı gizler ve komut işlemeden kalıcılığa kadar projeyi tek bir çatının kurallarına bağlar. |
| MassTransit | v9 ile ücretli lisansa geçti; ayrıca bir mesaj kuyruğu sunucusu varsayar. |
| CAP | .NET için outbox sunan açık kaynak bir kütüphane; ama topluluğu daha küçük ve modül başına şema düzenine uyarlanması gerekir. |
| Rebus | Bir taşıma katmanı (kuyruk) gerektirir; S1'de gereksiz işletim yükü. |
