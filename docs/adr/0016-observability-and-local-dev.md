# ADR-0016: Gözlemlenebilirlik ve yerel geliştirme ortamı

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **Genişletildi:** [ADR-0032](0032-production-telemetry-and-alerts.md) — yayın ortamında telemetri Grafana Cloud'a, sunucudaki Alloy toplayıcı üzerinden; uyarılar
- **İlgili:** [05-module-map.md §9.2](../05-module-map.md#92-olayların-teslimi), P-08, P-09, P-15, [ADR-0010](0010-messaging-infrastructure.md)

## Bağlam

- Gecikme hedefleri ölçülebilir olmalıdır: olay gecikmesi (P-08, P-15) ve ihtiyaç hesabı süresi (P-09).
- Bir kullanıcı işleminin modüller arası olaylarla yol açtığı zincir uçtan uca izlenebilmelidir.
- Yerel geliştirmede veritabanı, API ve ön yüz birlikte, tek komutla ayağa kalkmalıdır.
- Proje başlangıcında yerel ortam için Docker Compose düşünülmüştü.

## Karar

- **Telemetri:** .NET'in yerleşik loglama altyapısı ve OpenTelemetry ile loglar, izler (trace) ve ölçümler (metric) tek bir standartla üretilir. Loglar yapılandırılmış biçimdedir.
- **İlişki kimliği:** Bir isteğin iz kimliği, doğurduğu entegrasyon olaylarına ve işlem geçmişi kayıtlarına taşınır. Böylece bir işlemin tüm modüllerdeki etkileri tek iz altında görülür.
- **Özel ölçümler:**
  - olay gecikmesi (oluşma → işlenme),
  - ihtiyaç hesabı süresi,
  - başarısız olay sayısı,
  - SignalR bağlantı sayısı.
- **Yerel geliştirme:** Aspire. Tek bir başlatıcı proje (AppHost) PostgreSQL konteynerini, API'yi ve Vite ön yüzünü birlikte başlatır. Aspire paneli logları, izleri ve ölçümleri gösterir. Sağlık kontrolleri ve telemetri ayarları modüllerde ortak bir yapılandırmayla tanımlanır.
- **Yayın ortamı:** Telemetri sunucudaki Grafana Alloy toplayıcısı üzerinden Grafana Cloud'a gider ([ADR-0032](0032-production-telemetry-and-alerts.md)).

## Sonuçlar

**Olumlu:**
- Tek standart; telemetrinin gönderildiği hedef sonradan kod değişmeden değiştirilebilir.
- Yerel ortam tek komutla kalkar; yeni bir geliştiricinin (ya da portfolyoyu inceleyen birinin) projeyi çalıştırması kolaylaşır.
- Gecikme hedefleri gerçekten ölçülür ve aşıldığında görülür.

**Olumsuz / bedeli:**
- Aspire, Docker Compose'a göre daha yeni bir araçtır ve kendi öğrenme eğrisi vardır.
- Yerel geliştirme için Docker gerekir.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Serilog + Seq | Yaygın ve güçlü; ama OpenTelemetry artık platformun standardı ve iz ile ölçümü de aynı altyapıda sunuyor. Serilog gerekirse sonradan bir log hedefi olarak eklenebilir. |
| Yalnızca Docker Compose | Veritabanını ayağa kaldırır ama telemetri paneli ve servislerin birlikte başlatılması için ek araçlar gerekir. Demo ortamının yayını için Compose yine kullanılabilir (Faz 0 D). |
