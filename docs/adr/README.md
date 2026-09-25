# Mimari Karar Kayıtları (ADR)

[← Teknoloji yığını](../07-tech-stack.md)

## Bu klasör ne işe yarar

Mimari Karar Kaydı (Architecture Decision Record, ADR), önemli bir teknik kararı; bağlamı, seçilen yol, bedeli ve değerlendirilen alternatiflerle birlikte kaydeden kısa bir belgedir. Amaç, "bunu neden böyle yaptık?" sorusunun cevabının kodla birlikte versiyonlanması ve aylar sonra da okunabilmesidir.

## Ne zaman ADR yazılır

- Geri dönmesi pahalı olan her teknik karar: platform, veritabanı, mimari tarz, modül sınırları, güvenlik modeli.
- Yığına yeni bir çatı ya da önemli bir kütüphane eklenmesi.
- Faz 0 standartlarından birinin değiştirilmesi.
- Kapsam belgesinde sürüm sırasının ya da kalıcı kapsam dışı listesinin değişmesi.

Küçük kütüphane seçimleri ADR gerektirmez; [07-tech-stack.md](../07-tech-stack.md) tablosuna eklenir.

## Durumlar

| Durum | Anlamı |
|---|---|
| **Önerildi** | Yazıldı, onay bekliyor |
| **Kabul edildi** | Geçerli karar |
| **Yerine geçildi (ADR-XXXX)** | Daha yeni bir ADR bu kararın yerini aldı |
| **Reddedildi** | Değerlendirildi ama uygulanmadı; neden reddedildiği kayıt altında kalsın diye silinmez |

## Kurallar

1. ADR'ler sırayla numaralanır. Numara tekrar kullanılmaz.
2. Kabul edilmiş bir ADR'nin içeriği değiştirilmez. Yalnızca durumu ve bağlantıları güncellenebilir. Karar değişirse yeni bir ADR yazılır ve eskisi "Yerine geçildi" olarak işaretlenir.
3. Dosya adı `NNNN-kisa-baslik.md` biçimindedir, başlık İngilizce kebab-case yazılır.

## Şablon

```markdown
# ADR-NNNN: Başlık

- **Durum:** Önerildi
- **Tarih:** YYYY-AA-GG
- **İlgili:** ilgili belgeler ve ADR'ler

## Bağlam
Kararı gerektiren durum, kısıtlar ve ihtiyaçlar.

## Karar
Seçilen yol, açık ve uygulanabilir biçimde.

## Sonuçlar
**Olumlu:** kararın getirdikleri.
**Olumsuz / bedeli:** kararın götürdükleri ve nasıl yönetileceği.

## Değerlendirilen alternatifler
Her alternatif ve neden seçilmediği.
```

## Dizin

| No | Başlık | Durum |
|---|---|---|
| [0001](0001-modular-monolith.md) | Modüler monolit | Kabul edildi |
| [0002](0002-module-boundaries-and-layers.md) | Modül sınırları ve katman kuralı | Kabul edildi |
| [0003](0003-integration-events-and-outbox.md) | Modüller arası iletişim: entegrasyon olayları, outbox ve inbox | Kabul edildi |
| [0004](0004-commitments-vs-facts.md) | Taahhüt ile olgunun ayrılması | Kabul edildi |
| [0005](0005-dependency-license-policy.md) | Bağımlılık lisans politikası | Kabul edildi |
| [0006](0006-backend-platform.md) | Sunucu platformu: .NET 10 LTS ve ASP.NET Core Minimal API | Kabul edildi |
| [0007](0007-data-access.md) | Veri erişimi: PostgreSQL 18 ve EF Core 10 | Kabul edildi |
| [0008](0008-frontend-architecture.md) | Ön yüz: React + Vite tek sayfalı uygulama | Kabul edildi |
| [0009](0009-ui-components.md) | Arayüz bileşenleri: shadcn/ui ve Tailwind CSS | Kabul edildi |
| [0010](0010-messaging-infrastructure.md) | Olay altyapısının kendimiz tarafından yazılması | Kabul edildi |
| [0011](0011-authentication.md) | Kimlik doğrulama: sunucu tarafı oturumlu çerez | Kabul edildi |
| [0012](0012-realtime-signalr.md) | Anlık güncelleme: SignalR | Kabul edildi |
| [0013](0013-scheduled-jobs.md) | Zamanlanmış işler | Kabul edildi |
| [0014](0014-documents-and-qr.md) | PDF üretimi ve QR | Kabul edildi |
| [0015](0015-testing-tools.md) | Test araçları | Kabul edildi |
| [0016](0016-observability-and-local-dev.md) | Gözlemlenebilirlik ve yerel geliştirme ortamı | Kabul edildi |
| [0017](0017-time-and-money-types.md) | Zaman ve para tipleri | Kabul edildi |
| [0018](0018-integration-events-project.md) | Entegrasyon olaylarının ayrı projede tutulması | Kabul edildi |
| [0019](0019-code-style-and-static-analysis-tools.md) | Kod biçimi ve statik analiz araçları | Kabul edildi |
