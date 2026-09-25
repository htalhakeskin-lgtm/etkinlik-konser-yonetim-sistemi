# ADR-0032: Yayın ortamında telemetri ve uyarılar

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [ADR-0016](0016-observability-and-local-dev.md) (bu ADR onu genişletir), [09 §11](../09-environments-and-deployment.md#11-telemetri-ve-uyarılar), [standards/observability.md](../standards/observability.md)

## Bağlam

- [ADR-0016](0016-observability-and-local-dev.md) telemetri standardını (OpenTelemetry) ve yerel geliştirme panelini (Aspire) seçti; yayın ortamında telemetrinin nereye gideceğini sonraya bıraktı.
- Gözlemlenebilirlik standardı bir dizi uyarı tanımlıyor (hatalı olaylar, olay gecikmesi, `500` oranı, sağlık, zamanlanmış işler), ama nereye gideceklerini belirlemedi ([observability §7](../standards/observability.md#7-uyarılar)).
- Hiçbir hizmet için ücret ödenmeyecektir. Demo sunucusunun kaynakları sınırlıdır ve işletim yükü düşük tutulmalıdır.
- Gizli bilgiler ortam değişkeniyle geçirilmez ([security §6](../standards/security.md#6-gizli-bilgiler)); OpenTelemetry'nin standart başlık değişkeni (`OTEL_EXPORTER_OTLP_HEADERS`) bu yüzden kullanılamaz.

## Karar

- **Hedef:** Grafana Cloud ücretsiz planı, AB bölgesi. Kredi kartı istemez; 14 gün saklama.
- **Toplayıcı:** Sunucuda Grafana Alloy (Apache 2.0). Uygulama iç ağdaki Alloy'a OTLP ile, kimlik bilgisi olmadan gönderir. Grafana Cloud belirteci yalnızca Alloy'dadır. Alloy ayrıca sunucu ölçümlerini, PostgreSQL istatistiklerini (izleme yetkili `festos_monitor` rolüyle) ve yedekleme betiklerinin sonuçlarını toplar.
- **Uyarılar:** Grafana'nın uyarı kuralları; e-postayla. Standarttaki uyarılara yedekleme, tatbikat, WAL arşivleme, bağlantı sınırı ve disk uyarıları eklenir. Kurallar dışa aktarılıp repoda saklanır.
- **Dışarıdan erişim kontrolü:** Grafana'nın erişim kontrolü (Synthetic Monitoring) genel adresi düzenli olarak dener.
- **Örnekleme:** Tüm izler alınmaya devam eder.
- **Saklama:** Demo'da 14 gün; standarttaki süreler (log 30, iz 7, ölçüm 90 gün) yayın hedefi olarak kalır.

## Sonuçlar

**Olumlu:**
- Kurulum ve bakım gerektiren bir telemetri sunucusu yoktur; uyarılar ve erişim kontrolü aynı yerdedir.
- Uygulamanın yapılandırmasında telemetri için gizli bilgi yoktur.
- Hedef, uygulama değişmeden yalnızca Alloy'un ayarıyla değiştirilebilir (başka bir sağlayıcı ya da kendi sunucumuz).
- Sunucu sorunları (disk, WAL arşivleme, bağlantılar) kullanıcıya hata olarak yansımadan fark edilir.

**Olumsuz / bedeli:**
- Telemetri üçüncü taraf bir hizmete gider. Loglarda yalnızca kimlikler bulunduğu için kişisel veri taşınmaz; gerçek kullanımda sağlayıcı ve bölge KVKK'ya göre yeniden değerlendirilir.
- Ücretsiz planın saklama süresi (14 gün), standarttaki log (30 gün) ve ölçüm (90 gün) sürelerinin altındadır.
- Ücretsiz planın koşulları değişebilir; bu durumda Alloy'un hedefi değiştirilir.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Aynı yığını sunucuda çalıştırmak (Grafana'nın tek konteynerlik paketi) | Paket yalnızca geliştirme ve demo için önerilir, kalıcılık garantisi yoktur; bileşenlerinin bir kısmı AGPL lisanslıdır; sunucunun belleğini ve bakım yükünü artırır. |
| Aspire panelinin bağımsız sürümü | Veriyi saklamaz; yeniden başlatınca her şey kaybolur; uyarı üretmez. |
| SigNoz, OpenObserve gibi kendi barındırılan araçlar | Tek sunuculu bir demo için ağır (ClickHouse gibi ek veritabanları) ya da AGPL lisanslı. |
| Uygulamanın doğrudan Grafana Cloud'a göndermesi | Belirteç uygulamanın gizli bilgisi olur; sunucu ve veritabanı ölçümleri için yine ayrı bir toplayıcı gerekir. |
