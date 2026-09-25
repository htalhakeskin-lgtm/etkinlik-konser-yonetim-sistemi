# ADR-0013: Zamanlanmış işler

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** BR-EVT-018 (P-13), BR-MRP-017 (P-12), BR-WHS-011, [ADR-0010](0010-messaging-infrastructure.md)

## Bağlam

S1'deki zamanlanmış işler:

| İş | Modül | Sıklık |
|---|---|---|
| Otomatik operasyon geçişleri | Booking | Her 5 dakika (P-13) |
| Günlük aşırı rezervasyon kontrolü | Planning | Her gün 03:00, Europe/Istanbul (P-12) |
| Gecikmiş transfer tespiti | Planning | Her 5 dakika |
| Outbox yedek taraması | BuildingBlocks | Her 5 saniye |
| Süresi dolmuş oturumların temizliği | Identity | Her gün |

Bu işlerin hepsi kısa süren ve tekrar çalıştırılabilen taramalardır. Kalıcı bir iş kuyruğu gerektirmezler. Ama birden fazla uygulama örneği çalışırsa aynı işin iki kez aynı anda çalışmaması gerekir.

## Karar

- **Çalıştırma:** Her iş bir .NET hosted service (`BackgroundService`) olarak yazılır ve sahibi olan modülde bulunur.
- **Sıklık:** Aralıklı işler `PeriodicTimer`, saatli işler Cronos kütüphanesiyle hesaplanan cron ifadesiyle çalışır. Saatler Europe/Istanbul'a göredir.
- **Tek çalışma:** İş başlamadan önce PostgreSQL advisory lock alır. Kilidi alamayan örnek o turu atlar.
- **Tekrar güvenliği:** Her iş, iki kez çalışsa bile aynı sonucu üretecek şekilde yazılır.
- **Test:** İşler zamanı `TimeProvider` üzerinden okur; testlerde zaman ilerletilerek sınanır ([ADR-0017](0017-time-and-money-types.md)).

## Sonuçlar

**Olumlu:**
- Ek bir altyapı ya da panel gerekmez. İşler koda ve modüle aittir.
- Birden fazla örnekte de güvenli çalışır.

**Olumsuz / bedeli:**
- Kalıcı iş geçmişi ya da hazır bir izleme paneli yoktur. İşlerin çalışması log ve ölçümlerle izlenir ([ADR-0016](0016-observability-and-local-dev.md)).
- İleride ertelenmiş ya da kuyruklanmış işler gerekirse (ör. S6'da e-posta gönderimi) bu karar yeniden değerlendirilir.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Hangfire | Kalıcı iş kuyruğu ve panel sunar; ama S1'deki işler için gereksiz bir altyapı. Gelişmiş özellikleri ücretli sürümde. |
| Quartz.NET | Güçlü bir zamanlayıcı ve kümeleme desteği var; ama yapılandırması bu kadar basit işler için ağır. |
