# Yapılandırma ve Altyapı Hizmetleri Standardı

> **Durum:** v1.2 · **Son güncelleme:** 2026-09-25
> **Kararlar:** [Bölüm 10](#10-kararlar)

## 1. Bu belge ne işe yarar

Uygulamanın nasıl yapılandırıldığını ve ortak altyapı hizmetlerinin kurallarını tanımlar:
- ortamlar,
- ayar kaynakları ve öncelikleri,
- tip güvenli ayarlar ve açılışta doğrulama,
- iş kuralı parametrelerinin (P-01…P-16) ayar anahtarlarıyla eşlemesi,
- dosya saklama,
- dış servis çağrıları.

Gizli bilgilerin kaynağı [security §6](security.md#6-gizli-bilgiler)'dadır. Ayar anahtarlarının yazımı [naming §8.2](naming.md#82-ayar-anahtarları)'dedir.

## 2. Ortamlar

| Ortam (`ASPNETCORE_ENVIRONMENT`) | Nerede | Özellikleri |
|---|---|---|
| `Development` | Geliştiricinin bilgisayarı, Aspire ile | Migration'lar açılışta uygulanır; Scalar arayüzü, ayrıntılı hata sayfaları ve EF'in SQL logları açık; sağlık uçları açık |
| `Testing` | Otomatik testler ve sürekli entegrasyon | Testcontainers veritabanı, sahte saat; dış servis yok |
| `Demo` | Portfolyo için yayındaki demo | Kurgusal veri; yayın ayarlarıyla aynı güvenlik; migration'lar ayrı adımda |
| `Production` | İleride gerçek kullanım | Demo ile aynı kurallar |

Ortamların nerede ve nasıl çalıştığı [09 §2](../09-environments-and-deployment.md#2-ortamlar)'dedir.

**Kural:** İş mantığı ortama göre dallanmaz. Ortam kontrolü (`IsDevelopment()`) yalnızca altyapıda kullanılır: migration'ın açılışta uygulanması, API belgesi arayüzü, ayrıntılı hata sayfaları.

## 3. Ayar kaynakları ve öncelik

Sonra gelen kaynak öncekini ezer:

| Sıra | Kaynak | İçerik |
|---|---|---|
| 1 | `appsettings.json` | Tüm varsayılanlar; gizli bilgi yok |
| 2 | `appsettings.{Ortam}.json` | Ortama özgü farklar |
| 3 | Gizli bilgiler | Geliştirmede kullanıcı gizlileri; demo ve yayında dosya başına anahtar sağlayıcısı (`/run/secrets`) ([security §6](security.md#6-gizli-bilgiler)) |
| 4 | Ortam değişkenleri | Gizli olmayan ortam farkları (ör. port, telemetri adresi) |

- Her ayarın varsayılanı `appsettings.json`'da görünür. Böylece hangi ayarların var olduğu tek dosyadan okunur.
- Ayarlar çalışırken değiştirilmez (canlı yeniden yükleme yok); değişiklik uygulamanın yeniden başlatılmasıyla geçerli olur. Çalışırken değişmesi gereken parametreler veritabanındadır (§6).

## 4. Tip güvenli ayarlar ve doğrulama

- Her modülün ayarları kendi sınıfındadır (`BookingOptions`) ve `Modules:{Modül}` bölümünden okunur. Ortak altyapı ayarları (`Messaging`, `Hosting`) kendi bölümlerindedir.
- Her ayar sınıfı doğrulama kuralları taşır (zorunlu, aralık). Doğrulama kodu derleme zamanında üretilir (`[OptionsValidator]`); çalışırken yansıma kullanılmaz ([kaynak](https://learn.microsoft.com/en-us/dotnet/core/extensions/options-validation-generator)).
- **Açılışta doğrulama:** Tüm ayar sınıfları `ValidateOnStart()` ile kaydedilir. Eksik ya da hatalı bir ayar, uygulamanın ilk isteği almadan, açılışta hata vermesine yol açar ([kaynak](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-10.0)). Hatalı yapılandırmayla yayına çıkan uygulama, ilk kullanıcı o özelliğe ulaşana kadar sessiz kalmaz.
- Ayar sınıfları kodda `IOptions<T>` ile alınır. Ayar değişikliği yeniden başlatma gerektirdiği için `IOptionsMonitor` kullanılmaz.

## 5. İş kuralı parametreleri

[03-business-rules.md §4](../03-business-rules.md#4-parametreler)'teki parametrelerin karşılığı:

| No | Parametre | Nerede | Anahtar | Varsayılan |
|---|---|---|---|---|
| P-01 | Hesap kilidinden önceki hatalı giriş sayısı | Yapılandırma | `Modules:Identity:LockoutMaxFailedAttempts` | `5` |
| P-02 | Hesap kilidi süresi | Yapılandırma | `Modules:Identity:LockoutDuration` | `00:15:00` |
| P-03 | Hareketsizlik oturum süresi | Yapılandırma | `Modules:Identity:SessionIdleTimeout` | `12:00:00` |
| P-04 | En kısa şifre uzunluğu | Yapılandırma | `Modules:Identity:PasswordMinLength` | `15` |
| P-05 | Opsiyon son tarihi uyarı eşiği | Yapılandırma | `Modules:Booking:HoldExpiryWarningThreshold` | `3.00:00:00` |
| P-06 | Varsayılan hazırlık payı | **Veritabanı** (`EventDefaults`) | — | 24 saat |
| P-07 | Varsayılan dönüş payı | **Veritabanı** (`EventDefaults`) | — | 24 saat |
| P-08 | Anlık güncelleme gecikmesi hedefi | Yapılandırma | `Messaging:EventLatencyTarget` | `00:00:00.300` |
| P-09 | İhtiyaç hesabı süre hedefi | Performans testi hedefi; çalışma zamanı ayarı değil | — | 1 saniye |
| P-10 | Kayıp raporu varsayılan dönemi | Yapılandırma | `Modules:Inventory:LossReportDefaultPeriod` | `90.00:00:00` |
| P-11 | Kayıp raporunda öne çıkan model sayısı | Yapılandırma | `Modules:Inventory:LossReportTopModelCount` | `10` |
| P-12 | Günlük çakışma kontrolü saati | Yapılandırma | `Modules:Planning:DailyConflictCheckSchedule` | `0 3 * * *` (cron, Europe/Istanbul) |
| P-13 | Otomatik geçişlerin kontrol aralığı | Yapılandırma | `Modules:Booking:AutomaticTransitionsInterval` | `00:05:00` |
| P-14 | Varsayılan operasyon geçiş modu | **Veritabanı** (`EventDefaults`) | — | Otomatik |
| P-15 | Anlık güncelleme gecikmesi üst sınırı | Yapılandırma | `Messaging:EventLatencyLimit` | `00:00:01` |
| P-16 | Mutlak oturum süresi | Yapılandırma | `Modules:Identity:SessionAbsoluteLifetime` | `1.00:00:00` |

Süreler `TimeSpan` metin biçimindedir (`gün.saat:dakika:saniye`); ayar dosyası bu biçimi doğrudan okur.

## 6. Arayüzden değişen parametreler

P-06, P-07 ve P-14 sistem yöneticisi tarafından ayarlar ekranından değiştirilir (US-SYS-007). Bunlar yapılandırmada değil, veritabanında tutulur:
- Booking modülündeki `EventDefaults` toplu kökündedir. Tek kayıttır, sürüm numarası taşır ve her değişikliği işlem geçmişine yazılır.
- İlk değerler migration ile eklenir ([database §16.4](database.md#164-başlangıç-verisi)).
- Değişiklik yalnızca bundan sonra oluşturulan etkinlikleri etkiler; etkinlik oluşturulurken değerler etkinliğe kopyalanır (US-SYS-007).

## 7. Özellik bayrakları

S1'de özellik bayrağı (feature flag) kullanılmaz. Bitmemiş bir özelliği gizlemek için bayrak yerine dikey dilimler ve dal düzeni kullanılır ([git §3.3](git.md#33-bitmemiş-işler)). İhtiyaç doğarsa .NET'in kendi özellik yönetimi kütüphanesi bu belge güncellenerek eklenir.

## 8. Dosya saklama

**S1:** Yüklenen dosya yoktur. PDF'ler (toplama listesi, QR etiketleri) istek anında üretilir, doğrudan gönderilir ve saklanmaz ([api §7](api.md#7-belgeler-ve-dosya-çıktıları)).

**S2 ve sonrası (belge: `Document`) için ilkeler:**

| Konu | Kural |
|---|---|
| Nerede | Dosya bilgisi (ad, tip, boyut, özet, kim yükledi) veritabanında; dosyanın içeriği nesne deposunda. İkisi BuildingBlocks'taki `IFileStorage` arayüzüyle ayrılır. |
| Depo | Geliştirme ve demoda yerel disk (kalıcı birim); yayında S3 uyumlu bir depo (bulut ya da SeaweedFS, Apache 2.0). Kesin seçim S2'de yapılır. |
| Neden veritabanında değil | Dosyaları veritabanında saklamak basittir ve yedeklere girer. Ama veritabanı ve yedekler dosyalarla şişer, geri yükleme yavaşlar ([kaynak](https://wiki.postgresql.org/wiki/BinaryFilesInDB)). |
| MinIO | Kullanılmaz: 2025'te topluluk sürümünden özellikler çıkarıldı ve hazır imaj dağıtımı durdu. Proje 2026 başında arşivlendi ([kaynak](https://bizety.com/2025/12/06/minio-in-maintenance-mode-open-source-alternatives/)). |
| Dosya adı | Depodaki anahtar kullanıcının dosya adı değildir: `{modül}/{yıl}/{ay}/{uuid}`. Özgün ad yalnızca veritabanında tutulur. Böylece yol aşımı (`../`) saldırısı ve Türkçe karakter sorunları olmaz. |
| Erişim | Dosyalar herkese açık bir adresle sunulmaz. İndirme her zaman API üzerinden, kaydın yetkisi kontrol edilerek yapılır. |
| Sınırlar | Boyut üst sınırı (öneri 20 MB) ve izin verilen tipler. Tip, dosya uzantısına değil dosyanın ilk baytlarına bakılarak doğrulanır. |
| Virüs taraması | S2'de değerlendirilir. |

## 9. Dış servis çağrıları

S1'de dış servis yoktur. Sonraki sürümlerde (e-posta S6, güzergah hesabı S5, bilet verisi aktarımı) şu kurallar geçerlidir:

| Kural | Açıklama |
|---|---|
| İstemci | Tüm HTTP çağrıları `IHttpClientFactory` üzerinden, modülün altyapı katmanındaki tipli istemcilerle yapılır. |
| Dayanıklılık | .NET'in standart dayanıklılık katmanı (Microsoft.Extensions.Http.Resilience, MIT): zaman aşımı, rastgele bekleme süreli yeniden deneme ve devre kesici. Aspire'ın ortak ayarları bu katmanı tüm HTTP istemcilerine varsayılan olarak ekler ([kaynak](https://aspire.dev/get-started/csharp-service-defaults/)). |
| Yeniden deneme | Yalnızca tekrarlanması güvenli isteklerde; ya da dış servis tekrar güvenliği anahtarı destekliyorsa. |
| İşlem birimi | **Dış çağrı veritabanı işlemi açıkken yapılmaz.** Yavaş bir dış servis, işlemi ve tuttuğu kilitleri uzatır. Dış çağrı ya işlemden önce yapılır ya da bir olayla işlemden sonraya bırakılır. |
| Güvenilirlik | "Mutlaka yapılmalı" türündeki dış işler (ör. e-posta gönderimi) outbox'tan çıkan bir olayla tetiklenir. Böylece uygulama çökse bile iş kaybolmaz ([ADR-0010](../adr/0010-messaging-infrastructure.md)). |
| Hata | Geçici hatalar `Warning`, denemeler tükenince `Error` loglanır ([observability §3.1](observability.md#31-seviyeler)). |

## 10. Kararlar

| No | Konu | Karar | Gerekçe |
|---|---|---|---|
| Y-01 | Ortamlar | `Development`, `Testing`, `Demo`, `Production`; iş mantığı ortama göre dallanmaz | §2 |
| Y-02 | Canlı ayar değişikliği | Yok; yeniden başlatma. Çalışırken değişenler veritabanında. | §3 |
| Y-03 | Ayar doğrulama | Derleme zamanında üretilen doğrulayıcı + açılışta doğrulama | §4 |
| Y-04 | Arayüzden değişen parametreler | `EventDefaults` toplu kökünde, işlem geçmişiyle | §6 |
| Y-05 | Özellik bayrakları | S1'de yok | §7 |
| Y-06 | Dosya saklama (S2) | Bilgi veritabanında, içerik nesne deposunda; MinIO yok; kesin depo S2'de | §8 |
| Y-07 | Dış çağrılar | Standart dayanıklılık katmanı; işlem açıkken çağrı yok | §9 |

## 11. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-09-25 | v0.1 | İlk taslak |
| 2026-09-25 | v1.0 | P-04 = 15, P-16 = 24 saat. |
| 2026-09-25 | v1.1 | Özellik bayrağı yerine bitmemiş işlerin nasıl ele alınacağı bağlandı (D.1). |
| 2026-09-25 | v1.2 | Ortamlar belgesine bağlantı (D.3). |
