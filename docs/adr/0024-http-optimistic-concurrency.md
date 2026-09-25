# ADR-0024: HTTP üzerinden iyimser kilit — ETag ve If-Match

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [standards/api.md §9](../standards/api.md#9-eşzamanlı-düzenleme), [standards/database.md §11.1](../standards/database.md#111-sürüm-numarasıyla-iyimser-kilit), BR-SYS-011

## Bağlam

BR-SYS-011: kullanıcı bir kaydı açtıktan sonra kayıt başkası tarafından değiştirildiyse kaydetme reddedilir; hiçbir değişiklik sessizce ezilmez. Veritabanında her toplu kökün bir sürüm numarası var. Ama kullanıcının ekranı açması ile kaydetmesi arasında dakikalar geçebilir. Çakışmanın yakalanması için istemcinin **gördüğü** sürümü sunucuya geri göndermesi gerekir.

Durum geçişi gibi bazı isteklerin gövdesi boştur (`POST /events/{id}/confirm`); sürüm yine de gönderilmelidir.

## Karar

- Toplu kökü döndüren `GET` yanıtları `ETag: "{sürüm}"` başlığı ve gövdede `version` alanı taşır.
- Toplu kökü değiştiren her istek (`PUT`, işlem `POST`'u, `DELETE`) `If-Match: "{sürüm}"` gönderir.
- Sürüm tutmazsa `412 Precondition Failed` (`concurrencyConflict`); başlık yoksa `428 Precondition Required` (`versionRequired`).
- Bu uç noktalar OpenAPI'de `If-Match`'i zorunlu parametre olarak bildirir; üretilen istemci onu unutmaya izin vermez. AT-15 bildirimi denetler.

## Sonuçlar

**Olumlu:**
- HTTP'nin kendi mekanizmasıdır; gövdesi olan ve olmayan tüm isteklerde aynı biçimde çalışır.
- Sürüm unutulamaz: zorunlu parametre derleme zamanında, `428` çalışma zamanında yakalar.
- Aradaki katmanlar (proxy) bu başlıkları tanır.

**Olumsuz / bedeli:**
- Ön yüz her değiştiren istekte bir başlık gönderir. Bu, üretilen istemcinin parametresi olduğu için ek kod gerektirmez.
- Cevabı kaybolup yeniden gönderilen başarılı bir istek sürümü tutmaz ve yanlış bir çakışma verir. Bu durum tekrar güvenliği anahtarıyla çözülür ([ADR-0025](0025-idempotency-keys.md)).

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Sürümü gövdede göndermek (`expectedVersion`) | Gövdesi boş işlem isteklerine yapay bir gövde eklemek gerekir; standart HTTP araçları tanımaz. |
| Son yazan kazanır | BR-SYS-011'e doğrudan aykırı. |
| Kayıt kilitleme (kullanıcı düzenlerken kaydı kilitlemek) | Terk edilen ekranlar kilitli kayıt bırakır; kilit süreleri ve serbest bırakma ayrı bir sorun yaratır. |
