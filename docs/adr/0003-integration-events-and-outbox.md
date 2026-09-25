# ADR-0003: Modüller arası iletişim: entegrasyon olayları, outbox ve inbox

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [05-module-map.md §7, §9](../05-module-map.md#9-tutarlılık-ve-işlem-kuralları), [ADR-0010](0010-messaging-infrastructure.md)

## Bağlam

Bir modüldeki işlemin başka modüllerde sonuçları olur. Örneğin etkinlik iptal edilince rezervasyonlar serbest kalmalıdır. Bu sonuçlar:
- kaybolmamalıdır,
- asıl işlem geri alınırsa gerçekleşmemelidir,
- kullanıcının istediği gibi olabildiğince anlık yansımalıdır (%95'te 300 ms, en geç 1 s).

Veritabanına yazıp ardından olayı ayrıca yayınlamak ("çift yazma") güvenilir değildir: ikisinden biri başarısız olursa sistem tutarsız kalır.

## Karar

- **Modül içi olaylar** (`…DomainEvent`) modül dışına çıkmaz.
- **Modüller arası olaylar** (`…IntegrationEvent`) modülün `Contracts` projesinde tanımlanır.
- **Outbox:** Entegrasyon olayı, onu doğuran değişiklikle aynı işlem biriminde modülün `outbox` tablosuna yazılır.
- **Anında gönderim:** İşlem tamamlanır tamamlanmaz dağıtıcı bir sinyalle uyarılır ve olayı hemen iletir. Düzenli aralıklı tarama yalnızca yedektir.
- **Teslim garantisi:** Olay en az bir kez teslim edilir. Dinleyen modül işlediği olayları `inbox` tablosunda tutar ve aynı olayı ikinci kez işlemez.
- **Sıra:** Aynı kaydın olayları sırayla işlenir.
- **Hata:** Hatalı olaylar artan aralıklarla yeniden denenir; denemeler tükenirse hatalı olaylar listesine alınır. Olay kaybolmaz.

## Sonuçlar

**Olumlu:**
- Bir işlem ya tüm olaylarıyla birlikte kalıcı olur ya da hiçbiriyle olmaz.
- Modüller birbirinin hatalarından etkilenmez. Dinleyen bir modüldeki hata, yayınlayan modülün işlemini geri almaz.
- Olay yolu ileride bir mesaj kuyruğuna taşınabilir.

**Olumsuz / bedeli:**
- Modüller arası etkiler anlık değil, çok kısa bir gecikmeyle gerçekleşir. Bu durumun yol açtığı yarış durumları ve telafileri [05 §9.5](../05-module-map.md#95-kabul-edilen-yarış-durumları)'te tanımlıdır.
- Her dinleyici tekrarlanan olaya dayanıklı yazılmalıdır.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Modüller arası etkileri aynı işlem biriminde, senkron bildirimle yapmak | Bir modüldeki hata diğer modüllerin işlemini de geri alır; sınırlar kodda görünmez olur. |
| İşlem sonrası olayı doğrudan yayınlamak (outbox olmadan) | Uygulama olay yayınlanmadan çökerse olay kaybolur. |
| Yalnızca aralıklı tarama | Tarama aralığı kadar gecikme ekler; anlık güncelleme hedefini karşılamaz. |
