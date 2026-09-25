# Test Stratejisi

> **Durum:** v1.1 · **Son güncelleme:** 2026-09-25
> **Kararlar:** [Bölüm 14](#14-kararlar)

## 1. Bu belge ne işe yarar

Neyin, hangi katmanda ve ne kadar test edileceğini tanımlar. Test araçları [ADR-0015](../adr/0015-testing-tools.md)'te seçildi; test projelerinin yeri ve mimari testler [08 §12](../08-architecture.md#12-testlerin-yeri-ve-mimari-testler)'de, test adları [naming §10](naming.md#10-test-adları)'dadır. Bu belge bunların üzerine kurulur:
- test katmanları ve her katmanın sorumluluğu,
- hangi değişikliğin hangi testi gerektirdiği,
- iş kurallarının ve durum geçişlerinin testlere bağlanması,
- test verisi, zaman ve kültür,
- kararsız testler ve kod kapsamı.

Bir işin ne zaman bitmiş sayılacağı [definition-of-done.md](definition-of-done.md)'dedir. Testlerin CI'da nasıl çalıştığı [ci.md](ci.md)'dedir.

## 2. İlkeler

1. **Önce iş kuralları.** Test önceliği, yanlış çalıştığında şirkete para ya da güven kaybettirecek davranışlardadır: ihtiyaç hesabı, müsaitlik, çakışma motoru, durum geçişleri, aynı birimin iki kez çıkışının engellenmesi.
2. **Davranış test edilir, iç yapı değil.** Test, kodun dışarıdan görülen sonucunu sınar. Kod yeniden düzenlendiğinde (refactor) davranış değişmediyse testler değişmez.
3. **Gerçek bağımlılıklar tercih edilir.** Veritabanı sahte değildir; sahte nesne yalnızca modülün kendi sınırının dışındaki şeyler için kullanılır: başka modüllerin sözleşmeleri (birim testlerinde), zaman, dış servisler.
4. **Her kural ve geçiş numarasıyla izlenir.** Kodda uygulanan her iş kuralının ve durum geçişinin, numarasıyla etiketlenmiş en az bir testi vardır; bu bir araçla denetlenir (§10).
5. **Testler belirlenimcidir (deterministic).** Aynı kod her çalıştırmada aynı sonucu verir: zaman sabittir, testler birbirinin verisine dayanmaz, sıra önemli değildir.
6. **Hata önce testle yakalanır.** Her hata düzeltmesi, hatayı yeniden üreten ve düzeltmeden önce başarısız olan bir testle gelir.
7. **Testi önce yazmak önerilir.** İş kuralları ve hesaplamalar için kural metni ve örnekler belgede hazırdır; testi koddan önce yazmak kuralın doğru anlaşıldığını erken gösterir. Zorunlu değildir.

## 3. Test katmanları

| Katman | Ne sınanır | Araç | Veritabanı | Ne zaman çalışır |
|---|---|---|---|---|
| Statik denetim | Tipler, analizörler, lint, biçim, yasak API'ler | Derleyici, .NET analizörleri, ESLint, TypeScript | — | Editörde, commit öncesi, CI |
| Birim | Alan kuralları, durum makineleri, hesaplama motorları, değer nesneleri | xUnit v3, Shouldly, NSubstitute, `FakeTimeProvider` | Yok | Her derlemede, saniyeler içinde |
| Özellik tabanlı | Hesaplama motorlarının değişmezleri (§5.2) | CsCheck | Yok | Birim testleriyle birlikte |
| Modül entegrasyonu | Uç noktalar, yetki, doğrulama, kural hataları, eşzamanlılık, tekrar güvenliği, kalıcılık, olay dinleyicileri, anlık bildirimler | `WebApplicationFactory` + Testcontainers (PostgreSQL 18) + Respawn | Gerçek | CI'da her PR'da; yerelde Docker ile |
| Veritabanı | DT-01…DT-05 ([database §17.1](database.md#171-veritabanı-testleri)) | xUnit v3 + Testcontainers | Gerçek | CI'da her PR'da |
| Mimari | AT-01…AT-15 ([08 §12.2](../08-architecture.md#122-mimari-testler)) | ArchUnitNET | — | Her derlemede |
| API sözleşmesi | Üretilen API istemcisinin güncelliği; OpenAPI 3.1 çıktısının istemci üreteciyle uyumu ([api §14](api.md#14-openapi-istemci-üretimi-ve-uç-nokta-yazımı)) | Orval, TypeScript | — | CI'da her PR'da |
| Ön yüz bileşen | Ekranların kullanıcı gözünden davranışı | Vitest, Testing Library, MSW | — (sahte API) | Commit sonrası yerelde, CI'da her PR'da |
| Uçtan uca | MVP demo senaryosu ve kritik akışlar, gerçek tarayıcıda | Playwright | Gerçek, demo verisiyle | CI'da her PR'da (Chromium); gece tüm tarayıcılarda |
| Erişilebilirlik | Ana ekranlarda ciddi ve kritik erişilebilirlik ihlalleri | `@axe-core/playwright` | Gerçek | Uçtan uca testlerle birlikte |
| Performans | BR-MRP-019 (P-09) ve BR-SYS-012 (P-08, P-15) | xUnit v3, Playwright | Gerçek | Gece |
| Mutasyon | Hesaplama motorlarının testlerinin gücü | Stryker.NET | Yok | Sürüm öncesi, elle |

### 3.1 Dağılım

Testlerin çoğu **entegrasyon** katmanındadır; birim testleri hesaplama ve durum kurallarında yoğunlaşır; uçtan uca testler az ve kritiktir.

Bu, klasik test piramidinden (çok birim testi, az entegrasyon) bilinçli bir sapmadır:
- Piramit, gerçek veritabanıyla test yapmanın yavaş ve zor olduğu döneme göre çizildi. Testcontainers ile gerçek PostgreSQL saniyeler içinde açılıyor ve bir modülün entegrasyon testleri birkaç dakikada bitiyor.
- Bu tür uygulamalarda hataların çoğu parçaların birleştiği yerlerde çıkar: eksik `SaveChangesAsync`, yanlış işlem sınırı, eksik yetki, eşlenmemiş kolon, yanlış durum kodu. Sahte nesnelerle yazılmış birim testleri bunları göremez.
- Ön yüz için de aynı yaklaşım önerilir: "Test yaz. Çok değil. Çoğunlukla entegrasyon." ([kaynak](https://kentcdodds.com/blog/write-tests)).

Modüler monolitler için önerilen kaba oran birim %15–25, entegrasyon %60–70, uçtan uca %10'un altıdır ([kaynak](https://milanjovanovic.tech/blog/the-test-pyramid-is-a-lie-and-what-i-do-instead)). Bu oran hedef değil yön göstergesidir. Planning modülü gibi hesaplama ağırlıklı modüllerde birim testlerinin payı doğal olarak daha yüksektir.

## 4. Hangi değişiklik hangi testi ister

| Değişiklik | En az bu testler |
|---|---|
| **Kısıt** türünde kural ([03 §3](../03-business-rules.md#3-biçim-ve-kullanım)) | Kuralın ihlal edildiği ve edilmediği durumlar. Mantık alan katmanındaysa birim testi; her durumda uç noktanın `422` ve kural koduyla yanıt verdiğini gösteren bir entegrasyon testi. Kural veritabanı kısıtıyla da korunuyorsa, kısıtın kural koduna eşlendiği DT-04 ile denetlenir. |
| **Hesaplama** türünde kural | Belgedeki örneklerle tablo biçimli birim testleri (`[Theory]`); sınır değerleri (sıfır, eşitlik, aralık sınırı); değişmezler için özellik tabanlı testler (§5.2); sonucun kalıcı hale geldiği bir entegrasyon testi. |
| **Geçiş** (T-…) | Durum makinesinin birim testi: geçişin izin verildiği durum, koşul sağlanmadığında reddedilmesi, izin verilmeyen başlangıç durumları. Elle geçişte uç noktanın entegrasyon testi; otomatik geçişte tetikleyen olayın ya da zamanlanmış işin entegrasyon testi. Durum geçmişine yazıldığı doğrulanır (BR-EVT-015). |
| **Tetikleyici** türünde kural | Olay yayımlanır, dinleyici çalışır, etkisi veritabanında görülür (entegrasyon). Aynı olayın ikinci kez işlenmesinin etkisiz olduğu (inbox) doğrulanır. |
| **Yetki** türünde kural | İzinli rol başarılı olur; diğer roller `403` alır. Kayda bağlı kurallarda (depo kapsamı, BR-SYS-003) başka deponun kaydıyla `403` ve kural kodu. |
| Yeni uç nokta | Başarılı yol; `400` doğrulama; `403` yetkisiz rol; `404`; değiştiren uçlarda `If-Match` eksik (`428`) ve eski sürüm (`412`); aynı `Idempotency-Key` ile tekrar. Adlandırma, yetki ve başlık kuralları mimari testlerle zaten denetlenir (AT-09, AT-14, AT-15). |
| Anlık güncellenen kaynak | Değişiklik kaydedildikten sonra ilgili gruba `resourceChanged` mesajının gittiği bir entegrasyon testi ([api §13](api.md#13-anlık-bildirimler)). |
| Migration | DT-01 otomatik çalışır. Mevcut veriyi dönüştüren migration için, eski biçimde veri eklenip migration sonrası sonucu doğrulayan ayrı bir test. |
| Zamanlanmış iş | Sahte saat ilerletilir, iş doğrudan çağrılır, etkisi doğrulanır. Zamanlayıcının kendisi beklenmez. |
| Ekran | Kullanıcının göreceği davranış: form doğrulaması, sunucu hatasının ilgili alanda gösterilmesi, `412` sonrası çakışma uyarısı, yetkiye göre gizlenen düğmeler, boş ve yükleniyor durumları. |
| Depo okutma ekranı | Bileşen testi (elle kod girişi) ve uçtan uca test (sahte kamerayla QR okutma, §8). |
| Hata düzeltmesi | Önce hatayı yeniden üreten test (§2, ilke 6). |
| Performansa duyarlı kod | Performans testi (§9.1). |

## 5. Birim testleri

### 5.1 Kurallar

- Alan katmanı (Domain) ve uygulama katmanının saf mantığı birim testiyle sınanır. Veritabanı, HTTP ve dosya sistemi yoktur.
- Sahte nesne (NSubstitute) yalnızca modül dışındaki sözleşmeler (başka modüllerin `Contracts` arayüzleri) ve dış servisler için kullanılır. Modülün kendi sınıfları sahtelenmez; sahtelemek gerekiyorsa o davranış entegrasyon testine aittir.
- Zaman her zaman `FakeTimeProvider`'dan gelir (`DateTime.UtcNow` zaten yasaktır).
- Belgedeki örnekler (ör. net ihtiyaç hesabının adımları) tablo biçimli testlere aynen dökülür; belge ile test aynı örneği paylaşır.

### 5.2 Özellik tabanlı testler

Hesaplama motorlarında örnek tabanlı testlere ek olarak **özellik tabanlı testler** yazılır: araç rastgele girdiler üretir ve her girdide doğru olması gereken bir özelliği (değişmez) dener. Başarısız olursa en küçük karşı örneği gösterir. Araç [CsCheck](https://github.com/AnthonyLloyd/CsCheck)'tir (Apache 2.0); C# için yazılmıştır ve herhangi bir test çatısıyla çalışır ([kaynak](https://bartwullems.blogspot.com/2024/02/property-based-testing-in-ccscheck.html)).

S1'de aday değişmezler:

| Motor | Değişmez |
|---|---|
| Net ihtiyaç | Net ihtiyaç hiçbir zaman negatif değildir ve brüt ihtiyacı aşmaz. |
| Net ihtiyaç | Mekan ekipmanı ya da muadil eşleşme eklemek net ihtiyacı artırmaz. |
| Müsaitlik | Müsait adet hiçbir zaman negatif değildir ve depodaki toplam adedi aşmaz. |
| Müsaitlik | Birbirine yalnızca uç uca değen yarı açık aralıklar (`[başlangıç, bitiş)`) çakışmaz. |
| Rezervasyon önerisi | Önerilen rezervasyonların toplamı net ihtiyacı ve müsait adedi aşmaz. |
| Çakışma motoru | Çakışma simetriktir: A, B ile çakışıyorsa B de A ile çakışır. |
| Para | Belge toplamı, satırların yuvarlanmış tutarlarının toplamıdır; toplam ayrıca yuvarlanmadığı için kuruş farkı oluşmaz ([database §8](database.md#8-para-oran-ve-ölçüler)). |

## 6. Entegrasyon testleri

| Konu | Kural |
|---|---|
| Uygulama | Tüm uygulama `WebApplicationFactory<Program>` ile bellek içinde açılır; gerçek servis kayıtları ve gerçek ara katmanlar kullanılır ([kaynak](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0)). Testler uç noktalara HTTP ile gider. |
| Veritabanı | Test projesi başına bir PostgreSQL 18 konteyneri (xUnit v3 derleme düzeyi paylaşımı, `AssemblyFixture`; [kaynak](https://dotnet.testcontainers.org/test_frameworks/xunit_net/)). Migration'lar bir kez uygulanır. Testler arasında Respawn verileri temizler. |
| Veritabanı rolleri | Uygulama testte de modülün kendi veritabanı rolüyle bağlanır ([database §4](database.md#4-roller-ve-yetkiler)). Eksik bir yetki testte ortaya çıkar, yayında değil. |
| Kimlik doğrulama | Test yardımcısı, istenen rollerde bir kullanıcı için gerçek oturum kaydı ve çerez üretir. Yetki kontrolü atlanmaz; giriş uç noktasının kendisi ayrıca test edilir. |
| İstek başlıkları | Test HTTP istemcisi, ön yüzdeki istek sarmalayıcısı gibi CSRF başlığını ve `Idempotency-Key`'i otomatik ekler. Bu başlıkların eksik olduğu durumlar ayrı testlerle sınanır. |
| Olaylar | Outbox dağıtıcısı test uygulamasında da çalışır. Test yardımcısı, bekleyen tüm olaylar işlenene kadar bekler (üst süre sınırıyla); sabit süreli bekleme (`Task.Delay`) kullanılmaz. |
| Zaman | Uygulamaya `FakeTimeProvider` verilir; son tarihler ve otomatik geçişler saat ilerletilerek sınanır. |
| Anlık bildirimler | Test, SignalR istemcisiyle test sunucusuna bağlanır ve mesajın geldiğini doğrular. |
| Paralellik | Her test projesinin kendi konteyneri olduğu için modüllerin testleri birbirine paralel çalışır. Bir projenin içindeki testler aynı veritabanını paylaştığı için sırayla çalışır. Süre sorun olursa, test sınıfı başına şablon veritabanından (`CREATE DATABASE … TEMPLATE`) kopya açmak sonraki adımdır. |

Yerelde konteynerin her test çalıştırmasında yeniden açılmaması için Testcontainers'ın yeniden kullanım seçeneği açılabilir; CI'da her çalıştırma temiz konteynerle başlar.

## 7. Ön yüz testleri

- **Araçlar:** Vitest, Testing Library ve `user-event`; sunucu yanıtları MSW ile sahtelenir. MSW işleyicilerinin iskeleti Orval'ın sahte veri üretimiyle (`mock`) çıkarılabilir ([kaynak](https://orval.dev/docs/guides/msw/)); her test, sınadığı durum için yanıtı kendisi belirler.
- **Yaklaşım:** Bileşen kullanıcının gördüğü gibi sınanır. Öğeler rol ve erişilebilir adla bulunur (`getByRole('button', { name: … })`). CSS sınıfı ya da iç durum sorgulanmaz.
- **Öncelikli davranışlar:** Problem Details hatalarının ilgili alana yerleşmesi; kural kodlarının `errors:` çevirisiyle gösterilmesi; `412` sonrası çakışma uyarısı; "Yeni sürüm hazır" şeridi; yetkiye göre gizlenen işlemler; okutma ekranında elle kod girişi ve hata geri bildirimi.
- **Anlık görüntü (snapshot) testi kullanılmaz.** Her görsel değişiklikte kırılır ve neyin doğru olduğunu söylemez.
- **Ortam:** jsdom. Kamera ve gerçek tarayıcı gerektiren davranışlar uçtan uca testlerdedir.

## 8. Uçtan uca testler

| Konu | Kural |
|---|---|
| Kapsam | MVP demo senaryosu ([00 §6.1](../00-scope.md#61-bitti-kriteri-mvp-demo-senaryosu)) ve kritik akışlar: giriş ve oturum, depo çıkış ve dönüşü, eşzamanlı iki depo sorumlusu. Kural ayrıntıları burada değil, entegrasyon testlerinde sınanır. |
| Hedef | Yayındaki yığının kopyası: aynı uygulama imajı ve Compose tanımı, `Demo` yapılandırması, Caddy ile HTTPS, temiz PostgreSQL, demo verisi ([ci §4](ci.md#4-pr-hattı)). |
| Veri | Her test, kendi etkinliğini ve rezervasyonunu önce API üzerinden hızlıca oluşturur; yalnızca sınanan adım arayüzden yapılır. Testler birbirinin verisine dayanmaz. |
| Eşzamanlılık | Demo senaryosunun 13. adımı iki ayrı tarayıcı bağlamıyla (iki kullanıcı) sınanır: birinde yapılan çıkış, diğerinde sayfa yenilenmeden görünür; aynı birimin ikinci çıkışı reddedilir. |
| Mobil | Depo ekranları telefon görünümünde (dokunmatik, dar ekran) çalıştırılır. |
| QR okutma | Chromium'a sahte kamera olarak QR kodlu bir video verilir (`--use-fake-device-for-media-stream`, `--use-file-for-fake-video-capture`); okutma gerçek `BarcodeDetector` akışından geçer ([kaynak](https://daviddalbusco.com/blog/fake-video-capture-with-playwright/)). Bu yalnızca Chromium'da mümkündür; diğer tarayıcılarda okutma ekranı elle kod girişiyle sınanır. |
| Tarayıcılar | Her PR'da Chromium. Gece: Chromium, Firefox ve WebKit (Safari motoru); desteklenen tarayıcılar [00 §9](../00-scope.md#9-fonksiyonel-olmayan-varsayımlar)'dadır. |
| Saat dilimi ve dil | Tarayıcı dili `tr-TR`, tarayıcı saat dilimi bilerek **UTC**. Arayüz zamanları tarayıcının saat diliminden bağımsız olarak Europe/Istanbul göstermelidir; farklı saat dilimi bu hatayı yakalar. |
| Etiketler | Her test, kapsadığı hikayelerle etiketlenir: `test('…', { tag: ['@US-WHS-003'] }, …)` ([kaynak](https://playwright.dev/docs/test-annotations)). Hikaye numaraları §10'daki araçla denetlenir. |
| Bekleme | Playwright'ın kendiliğinden bekleyen doğrulamaları kullanılır; sabit süreli bekleme yoktur. |

## 9. Performans ve erişilebilirlik testleri

### 9.1 Performans

| Hedef | Test | Eşik |
|---|---|---|
| İhtiyaç hesabı (BR-MRP-019) | 50 satırlık rider ve 5.000 birimlik stokla hazırlanmış veride hesap süresi (entegrasyon testi, `[Trait("Category", "Performance")]`) | P-09 (1 saniye) |
| Anlık güncelleme (BR-SYS-012) | İki istemci: biri art arda stok değişikliği yapar, diğeri mesajın geliş süresini ölçer | İşlemlerin %95'i P-08 (300 ms), hiçbiri P-15'ten (1 saniye) uzun değil |

- Performans testleri her PR'da değil **gece** çalışır. Paylaşımlı CI makinelerinde süre dalgalanır; her PR'da çalıştırmak kararsız sonuç üretir.
- Başarısız bir gece çalıştırması `type:bug` issue'su açılmasını gerektirir.
- Yayında aynı hedefler `festos.messaging.event.latency` ve `festos.planning.requirement_calculation.duration` ölçümleriyle izlenir ([observability §5](observability.md#5-ölçümler)).

### 9.2 Erişilebilirlik

- Kapsam [00 §9](../00-scope.md#9-fonksiyonel-olmayan-varsayımlar)'daki gibidir: klavyeyle kullanım ve yeterli kontrast; tam WCAG denetimi hedeflenmez.
- Uçtan uca testlerde her ana ekran açıldığında [axe-core](https://playwright.dev/docs/accessibility-testing) taraması yapılır. **Ciddi (serious) ve kritik (critical)** ihlaller testi düşürür; daha düşük seviyeler raporlanır.
- Yazarken `eslint-plugin-jsx-a11y` kuralları çalışır ([code-style §5.3](code-style.md#53-lint-eslint)).
- Araç MPL 2.0 lisanslıdır ve değiştirilmeden kullanılır; [ADR-0005](../adr/0005-dependency-license-policy.md)'e uygundur.

## 10. Kural ve geçiş izlenebilirliği

**Etiketler:**
- .NET testleri: `[Trait("Rule", "BR-MRP-002")]`, `[Trait("Transition", "T-EVT-05")]` ([naming §10](naming.md#10-test-adları)). Numara metin olarak yazılır, sabit kullanılmaz; böylece tek bir metin aramasıyla bulunur.
- Uçtan uca testler: hikaye numarasıyla etiket (`@US-WHS-003`).

**Denetim aracı** (`tools/docs/check_traceability.py`, kod yazılmaya başlanınca eklenir). CI'da her PR'da çalışır ve şunları denetler:

| No | Denetim | Sonuç |
|---|---|---|
| TR-01 | Testlerdeki ve kaynak koddaki her kural (`BR-…`), geçiş (`T-…`) ve hikaye (`US-…`) numarası belgelerde tanımlıdır. | Hata |
| TR-02 | Kaynak kodda (`src/`) geçen her kural numarasının, o numarayla etiketlenmiş en az bir testi vardır. Kaynak kodda geçmek, kuralın uygulandığı anlamına gelir: kural kodu sabitleri (`…RuleCodes`, [naming §4.2](naming.md#42-mimari-yapı-taşlarının-adları)) ya da hesaplama kodundaki belge yorumu. | Hata |
| TR-03 | Kaynak kodda geçen her geçiş numarasının en az bir testi vardır. | Hata |
| TR-04 | API'nin döndürebildiği her kural kodunun (Kısıt, Geçiş ve Yetki türleri) ön yüzün `errors` çeviri dosyasında Türkçe karşılığı vardır ([naming §7.1](naming.md#71-çeviri-anahtarları)). | Hata |
| TR-05 | Hikaye başına kural durumu: hikayenin "Kurallar" satırındaki hangi kuralların uygulandığı ve test edildiği. | Rapor |
| TR-06 | S1 kurallarından ve geçişlerinden henüz kaynak kodda olmayanların listesi. | Rapor; S1 sürümünde boş olmalıdır |

Bu denetimle "BR-MRP-002'yi hangi testler doğruluyor?" sorusunun cevabı her zaman vardır ve bir kural testsiz uygulanamaz.

## 11. Test verisi, zaman ve kültür

| Konu | Kural |
|---|---|
| Veri kurucular | Her varlığın geçerli varsayılanlarla dolu bir kurucusu vardır (`EventBuilder`). Test yalnızca sınadığı alanı belirtir; okuyan, testin neye baktığını hemen görür. |
| Veri paylaşımı | Testler ortak, değiştirilebilir bir başlangıç verisine dayanmaz; her test kendi verisini oluşturur. Sabit başvuru verisi (ör. roller) migration'la gelir. |
| Demo verisi | Uçtan uca testler ve demo ortamı aynı kurgusal veri setini kullanır. Veri tamamen kurgusaldır; gerçek kişi ya da şirket bilgisi içermez ([00 §9](../00-scope.md#9-fonksiyonel-olmayan-varsayımlar)). Veri setinin yapısı ve yükleme yöntemi [09 §9](../09-environments-and-deployment.md#9-demo-verisi-ve-sıfırlama)'dadır. |
| Zaman | Sahte saatin varsayılan başlangıcı sabit bir andır (Europe/Istanbul'da bir iş günü sabahı). Gün ve ay sonu, yıl dönümü ve aralık sınırları ayrıca sınanır. |
| Kültür | Tüm .NET testleri **`tr-TR`** kültüründe çalışır (xUnit v3 `culture` ayarı; [kaynak](https://xunit.net/docs/config-xunit-runner-json)). Sunucu değişmez kültürle (invariant) çalışır ve analizörler kültürsüz metin işlemlerini yasaklar ([code-style §4.6](code-style.md#46-kültür-ve-metin)). Testlerin Türkçe kültürde çalışması, bu korumadan kaçan bir `ToUpper()` ya da sayı biçimlendirme hatasını ("I" / "ı" sorunu, ondalık virgül) yakalayan son katmandır. |
| Kimlikler | Test sonuçları kimlik değerine ya da kimlik sırasına bağlı olmaz (UUIDv7 zaman sırası için kullanılmaz; [ADR-0020](../adr/0020-entity-identifiers.md)). |

## 12. Kararsız testler ve hız

**Kararsız (flaky) test:** Kod değişmeden bazen geçen, bazen düşen test. Google'ın verilerine göre testlerin önemli bir kısmında görülür ve geçme / düşme değişimlerinin büyük çoğunluğu gerçek hatadan değil kararsızlıktan gelir ([kaynak](https://visdom-maturity-matrix.virtuslab.com/guides/development/flaky-tests-16-of-dev-time-google-data)). Kararsız testler CI sonucuna güveni yok eder.

| Kural | Açıklama |
|---|---|
| Otomatik tekrar yok (.NET) | .NET testleri CI'da başarısız olunca tekrar çalıştırılmaz. Tekrar, gerçek hataları gizler ve sonucu güvenilmez yapar ([kaynak](https://contextqa.com/blog/flaky-tests-in-ci-cd-without-retries/)). |
| Uçtan uca testlerde bir tekrar | Tarayıcı testleri doğası gereği daha değişkendir. CI'da en fazla bir tekrar yapılır; tekrarla geçen test raporda "kararsız" olarak görünür ve issue açılır. |
| Karantina | Kararsız test ya hemen düzeltilir ya da `[Trait("Quarantine", "#123")]` ile işaretlenip PR'ı engellemeyen ayrı bir çalıştırmaya alınır. Karantina en fazla **2 hafta** sürer; sonunda test düzeltilir ya da silinir. |
| Yaygın nedenler | Sabit süreli bekleme, gerçek saat, testler arası paylaşılan veri, sıraya bağımlılık, zamanında temizlenmeyen kaynaklar. Bu belgedeki kurallar (sahte saat, test başına veri, bekleme yardımcıları) bunları baştan önler. |

**Hız hedefleri** (CI makinesinde):

| Katman | Hedef |
|---|---|
| Birim testleri (tümü) | 1 dakikanın altı |
| Bir modülün entegrasyon testleri | 3 dakikanın altı |
| PR hattının tamamı | 10 dakikanın altı (paralel işlerle; [ci §4](ci.md#4-pr-hattı)) |

Hedef aşılırsa önce yavaş testler incelenir; test silmek son çaredir.

## 13. Kod kapsamı

- Kapsam [coverlet](https://github.com/coverlet-coverage/coverlet) (MIT, Microsoft Testing Platform eklentisi) ile ölçülür ve ReportGenerator (Apache 2.0) ile CI özetine yazılır.
- **Kapsam oranı birleştirmeyi engelleyen bir eşik değildir.** Oran hedef yapıldığında, bir şeyi doğrulamayan ama satırları çalıştıran testler yazılmaya başlanır. Google'ın rehberi de genel oranların yukarıdan dayatılmasını önermiyor; asıl değerli olan, hangi satırların neden kapsanmadığına bakmaktır ([kaynak](https://testing.googleblog.com/2020/08/code-coverage-best-practices.html)).
- Kapsamın yerine iki kapı vardır: kural ve geçiş izlenebilirliği (§10) ve değişiklik türüne göre test matrisi (§4).
- PR'da, değişen kodda kapsanmayan satırlar kendi kendine incelemede kontrol edilir.
- Yön gösterici olarak Google'ın genel ölçeği: %60 kabul edilebilir, %75 iyi, %90 örnek düzeyde. Hesaplama motorlarının alan projelerinde %90'ın üzeri beklenir.
- Microsoft'un kendi kapsam aracı ücretsizdir ama kapalı kaynak lisanslıdır; [ADR-0005](../adr/0005-dependency-license-policy.md)'teki listede olmadığı için seçilmedi.

**Mutasyon testi:** Kapsam, bir satırın çalıştığını söyler; testin o satırdaki hatayı yakalayıp yakalamadığını söylemez. Mutasyon testi koda bilerek küçük hatalar ekler (ör. `>` yerine `>=`) ve testlerin bunları yakalayıp yakalamadığına bakar. [Stryker.NET](https://stryker-mutator.io/blog/stryker-net-mtp-runner/) (Apache 2.0) her sürümden önce hesaplama motorlarının alan projelerinde elle çalıştırılır; yakalanmayan mutasyonlar incelenir. Birleştirmeyi engelleyen bir kapı değildir.

## 14. Kararlar

| No | Konu | Karar | Gerekçe |
|---|---|---|---|
| TS-01 | Testlerin dağılımı | Entegrasyon ağırlıklı; hesaplama ve durum kuralları birim testlerinde yoğun; uçtan uca az ve kritik | Gerçek PostgreSQL ile test hızlı; hatalar çoğunlukla birleşme noktalarında (§3.1). |
| TS-02 | Entegrasyon testlerinin yolu | HTTP üzerinden, gerçek veritabanı, gerçek oturum ve modülün kendi veritabanı rolüyle | Yetki, CSRF, işlem sınırı ve veritabanı izinleri gibi katmanlar testte atlanmaz. |
| TS-03 | Kural izlenebilirliği | Kaynak kodda geçen her kural ve geçiş numarasının testi olması CI'da zorunlu (TR-01…TR-04) | "Her kuralın en az bir testi olur" ilkesi ([03 §3](../03-business-rules.md#3-biçim-ve-kullanım)) elle takip edilmez, araçla korunur. |
| TS-04 | Testlerin kültürü | `tr-TR` | Türkçe "I" sorunu ve ondalık virgül hatalarına karşı son katman. |
| TS-05 | Kod kapsamı | Ölçülür ve raporlanır; eşik değildir; coverlet (MIT) | Oran hedefi kalitesiz testleri teşvik eder; izlenebilirlik ve test matrisi daha anlamlı kapılar. |
| TS-06 | Kararsız testler | .NET'te otomatik tekrar yok; uçtan uca testlerde bir tekrar ve raporlama; karantina en fazla 2 hafta | CI sonucuna güven korunur. |
| TS-07 | Özellik tabanlı test | CsCheck (Apache 2.0), yalnızca hesaplama motorlarında | Örnek tabanlı testlerin göremeyeceği sınır durumlarını bulur; ihtiyaç hesabı ve müsaitlik bunun en değerli olduğu yer. |
| TS-08 | Mutasyon testi | Stryker.NET (Apache 2.0), sürüm öncesi, hesaplama motorlarında, kapı değil | Kritik hesaplamaların testlerinin gerçekten hata yakaladığını gösterir. |
| TS-09 | Erişilebilirlik testi | Uçtan uca testlerde axe-core; ciddi ve kritik ihlaller testi düşürür | Kapsam hedefiyle (klavye, kontrast) uyumlu ve otomatik. |
| TS-10 | Performans testleri | Ayrı kategori, gece çalışır | Paylaşımlı CI makinesinde süre ölçümü her PR'da kararsız olur. |
| TS-11 | Uçtan uca tarayıcılar | Her PR'da Chromium; gece Firefox ve WebKit de | Geri bildirim hızı ile tarayıcı kapsamı arasında denge; CI dakikası tasarrufu. |
| TS-12 | Tarayıcı saat dilimi | Uçtan uca testlerde UTC | Arayüzün her zaman Europe/Istanbul göstermesi gerektiğini kanıtlar. |

## 15. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-09-25 | v0.1 | İlk taslak |
| 2026-09-25 | v1.0 | Kesinleşti; ADR-0029 kabul edildi. |
| 2026-09-25 | v1.1 | CI hattı, uçtan uca test yığını ve demo verisi bağlandı (D.3). |
