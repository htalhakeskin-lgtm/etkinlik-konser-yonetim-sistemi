# Gözlemlenebilirlik Standardı

> **Durum:** v1.1 · **Son güncelleme:** 2026-09-25
> **Kararlar:** [Bölüm 9](#9-kararlar)

## 1. Bu belge ne işe yarar

Uygulamanın çalışırken ne yaptığının nasıl görüleceğini tanımlar: loglar, izler (trace), ölçümler (metric), sağlık kontrolleri ve uyarılar. Araç seçimi [ADR-0016](../adr/0016-observability-and-local-dev.md)'dadır: .NET'in yerleşik loglaması ve OpenTelemetry; yerel geliştirmede Aspire paneli. Yayın ortamında telemetri, sunucudaki Grafana Alloy toplayıcısı üzerinden Grafana Cloud'a gider ([ADR-0032](../adr/0032-production-telemetry-and-alerts.md), [09 §11](../09-environments-and-deployment.md#11-telemetri-ve-uyarılar)).

Log şablonlarının yazımı [code-style §4.7](code-style.md#47-loglama)'de, telemetri adları [naming §9](naming.md#9-telemetri-ve-log-adları)'dadır.

## 2. Hangi bilgi nerede

| Kayıt | Amaç | Kim okur | Nerede | Ne kadar süre |
|---|---|---|---|---|
| **İşlem geçmişi** | İş kaydı: kim, neyi, ne zaman, hangi değerden hangi değere değiştirdi (BR-SYS-010) | Genel müdür, sistem yöneticisi | Veritabanı (`audit`) | Silinmez |
| **Log** | Teknik olay: uygulama ne yaptı, ne ters gitti | Geliştirici | Telemetri sistemi | 30 gün |
| **İz** | Bir isteğin ve doğurduğu olayların modüller boyunca yolculuğu ve süresi | Geliştirici | Telemetri sistemi | 7 gün |
| **Ölçüm** | Sayılar ve süreler: istek süresi, olay gecikmesi, hata sayısı | Geliştirici, uyarılar | Telemetri sistemi | 90 gün |

İşlem geçmişi bir log değildir: iş verisidir, işlemle aynı anda yazılır ve hiç kaybolmaz. Loglar ise kaybolabilir ve iş kuralı hiçbir zaman loga dayanmaz.

## 3. Loglar

### 3.1 Seviyeler

| Seviye | Ne zaman | Örnek |
|---|---|---|
| `Trace`, `Debug` | Yalnızca geliştirmede; ayrıntılı akış | Hesaplamanın ara değerleri |
| `Information` | Normal ama anlamlı olaylar | Uygulama başladı, zamanlanmış iş bitti (süresiyle), migration uygulandı |
| `Warning` | Beklenmeyen ama karşılanan durum | Geçici hata sonrası yeniden deneme, olay gecikmesi P-08'i aştı, işlenemeyen bir olay yeniden denenecek |
| `Error` | İlgilenilmesi gereken hata | Beklenmeyen istisna (`500`), olay tüm denemelerden sonra hatalı olaylar listesine düştü |
| `Critical` | Uygulama çalışamıyor | Açılışta veritabanına bağlanılamıyor, ayar doğrulaması başarısız |

**Beklenen iş sonuçları hata değildir.** Doğrulama hatası (`400`), iş kuralı ihlali (`422`), sürüm çakışması (`412`) ve yetkisiz istek (`403`) sistemin doğru çalıştığını gösterir. Bunlar en fazla `Information` ya da `Debug` seviyesinde loglanır. Aksi halde gerçek hatalar gürültü içinde kaybolur ve uyarılar anlamını yitirir.

### 3.2 Kategori seviyeleri (yayın)

| Kategori | Seviye |
|---|---|
| `FestOS.*` | `Information` |
| `Microsoft.Hosting.Lifetime` | `Information` |
| `Microsoft.AspNetCore.*`, `Microsoft.EntityFrameworkCore.*`, `Npgsql` | `Warning` |
| `Microsoft.EntityFrameworkCore.Database.Command` | `Warning` (çalışan SQL loglanmaz) |

Framework loglarını `Warning`'e çekip uygulama loglarını `Information`'da tutmak, gürültüyü kesip sinyali korumanın önerilen yoludur ([kaynak](https://www.devleader.ca/2026/07/03/logging-in-net-the-complete-developers-guide)).

### 3.3 Ne loglanır

- **Her istek loglanmaz.** İstek bilgisi (adres, süre, durum kodu) OpenTelemetry'nin ASP.NET Core izlerinde zaten vardır. Ayrı bir istek logu aynı bilgiyi ikinci kez yazar; ASP.NET'in HTTP loglama ara katmanı kullanılmaz.
- Her log, iz kimliğini (`TraceId`, `SpanId`) otomatik taşır; böylece bir istekle ilgili tüm loglar tek aramayla bulunur.
- Log kapsamına (scope) modül adı ve işlemi yapan kullanıcının **kimliği** eklenir. Ad ya da e-posta eklenmez.
- Zamanlanmış işin her çalışması başlangıç ve bitişte, süresiyle loglanır.
- Olay işleme: yeniden deneme `Warning`, hatalı olaylar listesine düşme `Error`.

### 3.4 Kişisel veri ve gizli bilgi

**Kural: yalnızca kimlikler ve teknik değerler loglanır.** Nesnenin kendisi, kullanıcı girdisi, istek ya da yanıt gövdesi loglanmaz.

| Asla loglanmaz | Yerine |
|---|---|
| Ad, e-posta, telefon, adres | Kullanıcı ya da taraf kimliği |
| Şifre, geçici şifre, oturum anahtarı, antiforgery belirteci | Hiçbir şey |
| İstek ve yanıt gövdeleri, komut nesneleri | İlgili kimlikler (`{EventId}`, `{WarehouseId}`) |
| SQL parametre değerleri | — |

- EF Core'un `EnableSensitiveDataLogging` ve `EnableDetailedErrors` ayarları yalnızca geliştirme ortamında açıktır. Aksi halde SQL parametreleri, yani kayıtların içeriği loglara düşer. Bir yapılandırma testi bunu denetler.
- OpenTelemetry'nin HTTP izlerinde adresin sorgu kısmındaki değerleri maskeleme davranışı açık kalır. Örneğin arama metni (`q=…`) izlere düz yazılmaz.
- .NET'in veri maskeleme kütüphanesi (Microsoft.Extensions.Compliance.Redaction) nesne loglarken sınıflandırılmış alanları otomatik gizleyebilir ([kaynak](https://learn.microsoft.com/en-us/dotnet/core/extensions/data-redaction)). S1'de "nesne loglanmaz" kuralı yeterli olduğu için eklenmez. İhtiyaç doğarsa bu belge güncellenerek eklenir.

## 4. Dağıtık izleme

| Konu | Kural |
|---|---|
| Otomatik izler | ASP.NET Core istekleri, dışa giden HTTP çağrıları, PostgreSQL komutları (Npgsql), SignalR bağlantıları |
| Komut ve sorgu izi | İşlem birimi dekoratörü her komut için bir iz aralığı (span) açar; adı komutun adıdır (`ConfirmEvent`) |
| Olayların izi | Outbox kaydı, olayı doğuran isteğin iz bağlamını (`traceparent`) saklar. Dinleyicinin iz aralığı bu bağlamla ilişkilendirilir. Böylece bir onayın Planning ve Inventory'deki etkileri aynı iz zincirinde görünür ([ADR-0016](../adr/0016-observability-and-local-dev.md)). |
| Özel öznitelikler | `festos.` önekli, yalnızca kimlik ve teknik değer ([naming §9](naming.md#9-telemetri-ve-log-adları)) |
| Örnekleme (sampling) | Tüm izler alınır (üst izin kararına uyan, her zaman açık örnekleme). Bu hacimde örnekleme gerekmez; demo ortamında da tüm izler alınır. Gerçek kullanım hacminde yeniden değerlendirilir. |
| Hata yanıtı | Problem Details'teki `traceId`, kullanıcının gördüğü hatayı ize bağlar ([api §8](api.md#8-hata-yanıtları)) |

## 5. Ölçümler

**Hazır ölçümler:** ASP.NET Core (istek süresi, etkin istekler), .NET çalışma ortamı (bellek, çöp toplama, iş parçacıkları), Kestrel bağlantıları, Npgsql bağlantı havuzu.

**Uygulama ölçümleri** (`FestOS.{Modül}` ölçüm kaynakları; [ADR-0016](../adr/0016-observability-and-local-dev.md)):

| Ölçüm | Tip | Birim | Neyi izler |
|---|---|---|---|
| `festos.messaging.event.latency` | Histogram | s | Olayın oluşmasından işlenmesine geçen süre; P-08 ve P-15 hedefleri buradan izlenir (BR-SYS-012) |
| `festos.messaging.outbox.pending` | Gösterge | {olay} | Gönderilmeyi bekleyen olay sayısı |
| `festos.messaging.dead_letters` | Sayaç | {olay} | Hatalı olaylar listesine düşen olaylar |
| `festos.planning.requirement_calculation.duration` | Histogram | s | İhtiyaç hesabı süresi; P-09 hedefi |
| `festos.jobs.run.duration` | Histogram | s | Zamanlanmış işlerin süresi (iş adıyla) |
| `festos.realtime.connections` | Gösterge | {bağlantı} | Açık SignalR bağlantıları |

- Birimler OpenTelemetry kuralına uyar: süreler saniye (`s`), sayımlar `{ad}` biçiminde.
- Ölçüm etiketlerinde (tag) kayıt kimliği gibi sınırsız değerler kullanılmaz. Her farklı değer ayrı bir zaman serisi açar ve telemetri sistemini şişirir. Etiketler sınırlı kümelerdir: modül, olay tipi, iş adı.
- Olay gecikmesi P-08'i aşan her olay ayrıca `Warning` olarak loglanır ([05 §9.2](../05-module-map.md#92-olayların-teslimi)).

## 6. Sağlık kontrolleri

| Uç nokta | Anlamı | Ne kontrol eder |
|---|---|---|
| `/alive` | Canlılık: uygulama çalışıyor mu, yeniden başlatılmalı mı | Hiçbir bağımlılık; yalnızca uygulamanın yanıt vermesi |
| `/health` | Hazırlık: istek almaya hazır mı | PostgreSQL bağlantısı, açılış işlemlerinin (ayar doğrulama) bitmesi |

- Yanıtlar ayrıntı içermez; yalnızca durum döner (`Healthy` / `Unhealthy`).
- Aspire şablonu bu uçları güvenlik nedeniyle yalnızca geliştirmede açar ([kaynak](https://aspire.dev/get-started/csharp-service-defaults/)). Demo ve yayında uçlar **ayrı bir iç porta** bağlanır. Bu port ters proxy üzerinden dışarıya açılmaz; yalnızca konteyner sağlık kontrolü ve izleme erişir.
- Sağlık kontrolü veritabanına her çağrıda sorgu atar. Bu yüzden sonuç birkaç saniye önbellekte tutulur.

## 7. Uyarılar

S1'de izlenecek durumlar aşağıdadır. Uyarılar Grafana Cloud'da tanımlanır ve e-postayla gelir; yedekleme ve sunucu uyarılarıyla birlikte tam liste [09 §11](../09-environments-and-deployment.md#11-telemetri-ve-uyarılar)'dedir.

| Durum | Neden önemli |
|---|---|
| Hatalı olaylar listesinde olay var | Bir modülün verisi diğerinden geri kalmıştır |
| Olay gecikmesi P-15'i sürekli aşıyor | Anlık güncelleme hedefi tutmuyor (BR-SYS-012) |
| `500` oranı artıyor | Beklenmeyen hata |
| `/health` başarısız | Uygulama istek alamıyor |
| Zamanlanmış iş beklenen zamanda çalışmadı | Otomatik geçişler ve çakışma kontrolü durmuş olabilir |

## 8. Yerel geliştirme

Aspire paneli logları, izleri ve ölçümleri aynı ekranda gösterir. Geliştirmede `FestOS.*` kategorisi `Debug` seviyesindedir ve EF Core'un SQL logları açıktır.

## 9. Kararlar

| No | Konu | Karar | Gerekçe |
|---|---|---|---|
| O-01 | Beklenen iş sonuçlarının log seviyesi | `Information` / `Debug`; hata değil | §3.1 |
| O-02 | İstek logu | Yok; istek bilgisi izlerde | §3.3 |
| O-03 | Kişisel veri | Yalnızca kimlik loglanır; maskeleme kütüphanesi S1'de yok | §3.4 |
| O-04 | Olayların izi | Outbox iz bağlamını saklar; dinleyici aynı zincire bağlanır | §4 |
| O-05 | Örnekleme | Tüm izler, demo ortamında da; gerçek kullanım hacminde yeniden değerlendirilir | §4 |
| O-06 | Sağlık uçlarının erişimi | Demo ve yayında iç port | §6 |
| O-07 | Saklama süreleri | Log 30, iz 7, ölçüm 90 gün; işlem geçmişi silinmez | §2 |

## 10. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-09-25 | v0.1 | İlk taslak |
| 2026-09-25 | v1.0 | Kesinleşti. |
| 2026-09-25 | v1.1 | Yayın ortamındaki telemetri hedefi ve uyarıların yeri bağlandı; örnekleme kararı kesinleşti (D.3). |
