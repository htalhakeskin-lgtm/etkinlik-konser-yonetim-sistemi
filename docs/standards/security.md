# Güvenlik Standardı

> **Durum:** v1.2 · **Son güncelleme:** 2026-09-25
> **Kararlar:** [Bölüm 13](#13-kararlar)

## 1. Bu belge ne işe yarar

Uygulamanın güvenlik kurallarını tek yerde toplar:
- yetki modeli, roller ve yetki kataloğu,
- kimlik doğrulama, oturum ve şifre,
- gizli bilgiler ve veri koruma anahtarları,
- tarayıcı güvenlik başlıkları,
- girdi doğrulama katmanları,
- kişisel veri ve bağımlılık güvenliği.

API'ye özgü kurallar (CSRF, CORS, istek sınırı, API yanıt başlıkları) [api.md §11](api.md#11-güvenlik-kuralları)'dedir. Veritabanı rolleri ve yetkileri [database.md §4](database.md#4-roller-ve-yetkiler)'tedir. Oturum ve kimlik doğrulamanın temel kararı [ADR-0011](../adr/0011-authentication.md)'dedir; bu belge onun ayrıntılarını verir.

## 2. Temel ilkeler

1. **Her kontrol sunucudadır.** Arayüzde düğmeyi gizlemek yalnızca kolaylıktır (BR-SYS-002).
2. **En az yetki.** Kullanıcı, veritabanı rolü ve bağlantı yalnızca işinin gerektirdiği kadar yetkiyle çalışır.
3. **Katmanlı savunma.** Tek bir önlem yeterli sayılmaz. Örnek: CSRF'e karşı üç katman, değişmez kayıtlara karşı kod ve veritabanı yetkisi.
4. **Güvenli varsayılan.** Yeni bir uç nokta yetki istemeden yayına çıkamaz (AT-09). Yeni bir tablo yetkisi verilmeden erişilemez (DT-02).
5. **Gizli bilgi depoda olmaz.** Parola, bağlantı dizesi ve sertifika hiçbir zaman kaynak kodunda ya da yapılandırma dosyasında bulunmaz.

## 3. Yetki modeli

### 3.1 Kavramlar

| Kavram | Nerede tanımlanır | Açıklama |
|---|---|---|
| Yetki (`Permission`) | Sahibi modülün kodunda, `…Permissions` sabitleri | Tek bir işlem: `Booking.Events.Confirm` ([naming §8.1](naming.md#81-yetki-kodları)). Modül, yetkilerini modül tanımıyla Host'a bildirir ([08 §5](../08-architecture.md#5-ana-uygulama-host-ve-modüllerin-kaydı)). |
| Yetki kataloğu | Host, açılışta modüllerden toplar | Tüm yetkilerin listesi. Identity modülü diğer modüllere referans vermeden kataloğu buradan alır. |
| Rol (`Role`) | Identity modülünün kodunda | Adlandırılmış bir yetki kümesi. S1'de roller sabittir (US-SYS-003); tanımları sürüm kontrolündedir ve testlerle doğrulanır ([ADR-0026](../adr/0026-authorization-model.md)). |
| Rol ataması | Veritabanı (`UserRole`) | Kullanıcıya hangi rollerin verildiği; sistem yöneticisi değiştirir. |
| Depo ataması | Veritabanı (`UserWarehouseAssignment`) | Depo sorumlusunun işlem yapabildiği depolar (BR-SYS-003). |

Kullanıcının yetkileri, rollerinin yetkilerinin birleşimidir (BR-SYS-002).

### 3.2 S1 rolleri

| Rol | Kod adı | Özeti |
|---|---|---|
| Sistem yöneticisi | `SystemAdministrator` | Kullanıcı, depo ve etkinlik varsayılanları yönetimi; işlem geçmişini görür |
| Booking müdürü | `BookingManager` | Taraf, mekan, prodüksiyon, rider, etkinlik ve opsiyon işlemleri |
| Teknik müdür | `TechnicalManager` | Katalog, ihtiyaç hesabı, rezervasyon, transfer planı, çakışma, dış kiralama |
| Depo sorumlusu | `WarehouseManager` | Okutma işlemleri (bağlı depolarda), birim durumu, stok görünümü |
| Genel müdür | `GeneralManager` | Tüm ekranları görür, hiçbir şeyi değiştiremez (BR-SYS-004); işlem geçmişini görür |

Rol–yetki matrisinin tamamı kodda tek bir tanımdır. Arayüzdeki salt okunur matris ekranı (US-SYS-003) da bu tanımdan üretilir. S2 ve sonraki roller (crew, satış, muhasebe) kendi sürümlerinde eklenir.

**Matrisin doğruluğu testlerle korunur:**
- Rollerde geçen her yetki kodu katalogda vardır; katalogdaki her yetki en az bir rolde bulunur (sahipsiz yetki olmaz).
- Genel müdür rolünde `View` dışında hiçbir eylem yoktur (BR-SYS-004).
- İşlem geçmişi görüntüleme yetkisi yalnızca genel müdür ve sistem yöneticisindedir (BR-SYS-010).

### 3.3 Uç nokta düzeyinde kontrol

- Her uç nokta tek bir yetki ister: `.RequirePermission(BookingPermissions.Events.Confirm)`. Yetki kodu, ASP.NET Core'da aynı adlı bir yetki politikasına dönüşür. Politikalar, yetki kataloğundan dinamik olarak üretilir; tek tek elle kaydedilmez ([kaynak](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/iauthorizationpolicyprovider?view=aspnetcore-10.0)).
- Yetki yoksa `403`, `code: forbidden`.
- Anonim uç noktalar yalnızca giriş, antiforgery belirteci ve sağlık kontrolleridir (AT-09).

### 3.4 Kayıt düzeyinde kontrol (kapsam)

Bazı yetkiler kayda göre değişir. S1'de tek örnek depo kapsamıdır: depo sorumlusu okutma işlemlerini yalnızca bağlı olduğu depolarda yapar, ama tüm depoların stoğunu görür (BR-SYS-003).
- Bu kontrol uç noktada değil, **uygulama katmanındaki işleyicide** yapılır, çünkü hangi depo olduğu ancak istek okunup kayıt bulunduktan sonra bellidir.
- İşleyici, oturumdaki kullanıcının depo atamalarını `ICurrentUser` üzerinden okur.
- İhlal `403` döner ve kural numarasını taşır: `code: BR-SYS-003` ([api §4.2](api.md#42-durum-kodları)).

### 3.5 Yetki değişikliklerinin etkisi

- Kullanıcının yetkileri ve depo atamaları girişte oturuma yüklenir ([ADR-0011](../adr/0011-authentication.md)).
- Sistem yöneticisi bir kullanıcının rolünü ya da depo atamasını değiştirdiğinde, o kullanıcının açık oturumlarındaki yetki bilgisi **aynı işlemde** güncellenir. Yeniden giriş gerekmez; değişiklik bir sonraki istekte geçerlidir.
- Pasifleştirilen kullanıcının tüm oturumları hemen silinir (BR-SYS-008).

### 3.6 Arayüz

`GET /api/v1/me`, kullanıcının yetkilerini ve depo atamalarını döndürür. Ön yüz menüyü ve düğmeleri buna göre gösterir (US-SYS-012). Gizlenen bir işlem doğrudan API'den denenirse yine reddedilir.

## 4. Kimlik doğrulama ve oturum

### 4.1 Giriş

- Giriş e-posta ve şifreyle yapılır. E-posta normalleştirilmiş haliyle aranır ([database §13](database.md#13-metin-sıralama-ve-arama)).
- Hata mesajı hangi alanın yanlış olduğunu söylemez (US-SYS-010).
- **Zamanlama eşitliği:** E-posta sistemde yoksa da şifre özetleme işlemi sahte bir özet üzerinde yapılır. Aksi halde yanıt süresinin kısalmasından hangi e-postaların kayıtlı olduğu anlaşılabilir.
- Hesap kilidi (BR-SYS-005: P-01 deneme, P-02 süre) alan kuralıdır; IP ve hesap başına istek sınırı ayrıca uygulanır ([api §11](api.md#11-güvenlik-kuralları)).
- Her deneme `LoginAttempt` olarak kaydedilir.

### 4.2 Oturum

| Kural | Karar |
|---|---|
| Saklama | Sunucu tarafında, `identity.sessions` tablosunda. Çerezde yalnızca rastgele bir oturum anahtarı bulunur (ASP.NET Core çerez kimlik doğrulaması + sunucu tarafı oturum deposu; [kaynak](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.cookies.cookieauthenticationoptions.sessionstore?view=aspnetcore-10.0)). |
| Anahtarın saklanması | Veritabanında anahtarın kendisi değil, SHA-256 özeti tutulur. Veritabanı sızsa bile geçerli bir çerez üretilemez. |
| Çerez | Adı `__Host-festos_session`; `HttpOnly`, `Secure`, `SameSite=Strict`, `Path=/`. `__Host-` öneki, tarayıcının çerezi yalnızca HTTPS'te ve alan adı belirtilmeden kabul etmesini sağlar; alt alan adlarından çerez enjeksiyonunu engeller. |
| Hareketsizlik süresi | P-03 (12 saat). Süre her istekte uzar; veritabanı yükü için son etkinlik zamanı en fazla 5 dakikada bir yazılır. |
| Mutlak süre | P-16 (24 saat, [G-02](#13-kararlar)). Oturum, etkinlikten bağımsız olarak açıldıktan bu kadar süre sonra sona erer. OWASP, her oturum için mutlak bir süre önerir; çalınmış bir çerez sürekli kullanılarak sonsuza kadar açık tutulamaz ([kaynak](https://cheatsheetseries.owasp.org/cheatsheets/Session_Management_Cheat_Sheet.html)). |
| Girişte yenileme | Her başarılı girişte yeni bir oturum oluşturulur; eski oturum anahtarı kullanılmaz. |
| Çıkış | Sunucudaki oturum kaydı silinir; yalnızca çerezin silinmesi yeterli sayılmaz. |
| Şifre değişikliği | Kullanıcının diğer tüm oturumları sona erer (BR-SYS-007). |
| Geçici şifre | Geçici şifreyle açılan oturum "şifre değiştirilmeli" olarak işaretlenir. Şifre değiştirme, çıkış ve `/me` dışındaki tüm uç noktalar `403`, `code: BR-SYS-006` döner. |
| Süresi dolan oturumlar | Zamanlanmış bir iş, süresi dolan oturum kayıtlarını siler. |

### 4.3 İki adımlı doğrulama

S1'de yoktur ([G-03](#13-kararlar)). Oturum ve kullanıcı modeli, ileride uygulama tabanlı tek kullanımlık kodun (TOTP) eklenebileceği biçimde kurulur. Eklendiğinde en kısa şifre uzunluğu yeniden değerlendirilebilir; NIST, iki adımlı doğrulamanın parçası olan şifre için 8 karakteri yeterli görür.

## 5. Şifreler

### 5.1 Şifre politikası

NIST'in 2025'te yayımlanan kimlik doğrulama rehberi (SP 800-63B-4) esas alınır ([kaynak](https://csrc.nist.gov/pubs/sp/800/63/b/4/final)):

| Kural | Karar |
|---|---|
| En kısa uzunluk | P-04 = **15** karakter ([G-01](#13-kararlar)). NIST, şifrenin tek doğrulama yöntemi olduğu durumda en az 15, iki adımlı doğrulamanın bir parçası olduğu durumda en az 8 karakter ister ([kaynak](https://www.enzoic.com/blog/nist-sp-800-63b-rev4/)). Uzunluk, Unicode karakter (kod noktası) sayısıyla ölçülür. |
| En uzun | 128 karakter (NIST en az 64'e izin verilmesini ister) |
| Karakterler | Türkçe karakterler ve semboller dahil tüm yazdırılabilir karakterler serbesttir. **Boşluk kullanılamaz**: boşluk, sekme, bölünemez boşluk gibi hiçbir boşluk karakteri şifrede bulunamaz ([G-01](#13-kararlar)). Şifre özetlenmeden önce Unicode NFC biçimine getirilir; böylece aynı görünen iki şifre aynı özeti verir. |
| Karışım kuralı ("en az bir büyük harf, bir rakam") | **Yoktur.** NIST bu kuralları öngörülebilir şifrelere yol açtığı için yasaklar. Uzunluk karmaşıklıktan önemlidir. |
| Düzenli değiştirme zorunluluğu | **Yoktur.** Şifre yalnızca ele geçirildiğine dair bir belirti varsa değiştirilir. |
| Yaygın ve sızmış şifre kontrolü | Yeni şifre, uygulamayla birlikte gelen yaygın şifreler listesine karşı kontrol edilir; listedeki şifre reddedilir. Kullanıcının e-postasını ya da "festos" kelimesini içeren şifre de reddedilir. Kontrol dış bir servise istek atmadan yapılır. |
| Yapıştırma | Serbesttir. Şifre yöneticilerinin kullanılabilmesi için şifre alanlarında yapıştırma engellenmez. |
| Geçmiş şifre | Yeni şifre mevcut şifreyle aynı olamaz (BR-SYS-007). |

### 5.2 Şifre özetleme

- ASP.NET Core Identity'nin şifre özetleyicisi kullanılır: PBKDF2-HMAC-SHA512 ([ADR-0011](../adr/0011-authentication.md)).
- **Tur sayısı 210.000'e çıkarılır.** Kütüphanenin varsayılanı 100.000'dir; OWASP bu algoritma için en az 210.000 tur önerir ([kaynak](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)).
- Tur sayısı ileride yükseltilirse, kullanıcının şifresi bir sonraki başarılı girişinde yeni ayarla otomatik olarak yeniden özetlenir. Kütüphane bunu "yeniden özetleme gerekli" sonucuyla bildirir.

### 5.3 Geçici şifre

Sistem yöneticisinin ürettiği geçici şifre (BR-SYS-006) kriptografik rastgele sayı üreteciyle oluşturulur. 16 karakterdir ve karışabilecek karakterler (`0/O`, `1/l/I`) kullanılmaz. Yalnızca bir kez gösterilir ve hiçbir yerde düz metin olarak saklanmaz.

## 6. Gizli bilgiler

Gizli bilgiler: 10 modül rolünün ve migration rolünün veritabanı parolaları ([ADR-0021](../adr/0021-database-roles-per-module.md)), veri koruma anahtarlarını şifreleyen sertifika (§7), yedeklerin şifreleme ve depo anahtarları, telemetri belirteci, demo ortamında demo hesaplarının parolaları ve sonraki sürümlerde e-posta gibi dış servislerin anahtarları. Demo ortamındaki tam liste [09 §10](../09-environments-and-deployment.md#10-gizli-bilgiler)'dadır.

| Ortam | Kaynak |
|---|---|
| Geliştirme | Aspire AppHost parametreleri (`secret: true`); değerler geliştiricinin bilgisayarında .NET kullanıcı gizlileri (user secrets) deposunda tutulur |
| Test (sürekli entegrasyon) | Testcontainers her çalıştırmada rastgele parola üretir; kalıcı gizli bilgi yoktur |
| Demo ve yayın | Sunucunun gizli bilgi deposundan **dosya olarak** bağlanır (ör. Docker secrets, `/run/secrets/…`). .NET'in dosya başına anahtar (key-per-file) yapılandırma sağlayıcısıyla okunur. |

**Kurallar:**
- Gizli bilgiler ortam değişkeniyle geçirilmez. Ortam değişkenleri konteyner inceleme çıktılarında ve süreç listelerinde görünür ([kaynak](https://snapdeploy.dev/blog/environment-variables-security-best-practices)).
- `appsettings*.json` dosyalarında gizli bilgi bulunmaz. Bağlantı dizelerinin parolasız kısmı yapılandırmada, parola gizli bilgi kaynağından gelir.
- Depoya yanlışlıkla gizli bilgi eklenmesi, commit öncesinde ve sürekli entegrasyonda gitleaks taramasıyla engellenir ([git §10](git.md#10-gizli-bilgi-taraması)).
- Parolalar kod değişikliği olmadan döndürülebilir (rotation); yordam [10 §6](../10-operations.md#6-gizli-bilgilerin-yenilenmesi)'dadır. Demo ortamındaki gizli bilgilerin listesi [09 §10](../09-environments-and-deployment.md#10-gizli-bilgiler)'dadır.

## 7. Veri koruma anahtarları

ASP.NET Core, oturum çerezini ve antiforgery belirtecini "veri koruma" (Data Protection) anahtarlarıyla şifreler. Varsayılan olarak bu anahtarlar konteynerin dosya sisteminde durur ve **her yeniden başlatmada kaybolur**. Bu olunca:
- tüm kullanıcılar oturumdan düşer,
- açık sekmelerdeki antiforgery belirteçleri "çözülemedi" hatası verir ([kaynak](https://startdebugging.net/2026/06/fix-the-antiforgery-token-could-not-be-decrypted-in-aspnetcore/)).

| Kural | Karar |
|---|---|
| Saklama | Veritabanında, `identity.data_protection_keys` tablosunda (EF Core sağlayıcısı, MIT). Yeniden başlatmalarda ve birden fazla uygulama örneğinde aynı anahtarlar kullanılır. |
| Uygulama adı | Sabit: `FestOS`. Belirtilmezse ASP.NET anahtarları uygulamanın dosya yoluna göre ayırır; farklı yolda çalışan yeni bir yayın eski anahtarları göremez. |
| Anahtarların şifrelenmesi | Demo ve yayında anahtarlar bir X.509 sertifikasıyla şifrelenmiş saklanır; sertifika gizli bilgidir (§6). Özel bir depo seçildiğinde ASP.NET anahtarları kendiliğinden şifrelemeyi bırakır; bu ayar bunu geri getirir ([kaynak](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-10.0)). |
| Anahtar ömrü | Varsayılan (90 gün, otomatik yenileme) |

## 8. Tarayıcı güvenlik başlıkları

Ön yüz sayfası (HTML ve statik dosyalar) şu başlıklarla sunulur. API yanıtlarının başlıkları [api §11](api.md#11-güvenlik-kuralları)'dedir.

| Başlık | Değer | Neden |
|---|---|---|
| `Content-Security-Policy` | `default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob:; font-src 'self'; connect-src 'self'; object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'` | Betik yalnızca kendi sunucumuzdan yüklenir; satır içi betik ve `eval` çalışmaz. XSS'e karşı en güçlü tarayıcı önlemidir. |
| `Permissions-Policy` | `camera=(self), microphone=(), geolocation=(), payment=(), usb=()` | QR okutma kamerayı kullanır; `camera=()` yazılırsa tarayıcı kamerayı açmaz ([kaynak](https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/Permissions-Policy/camera)). Kullanılmayan özellikler kapatılır. |
| `Referrer-Policy` | `no-referrer` | Adresler (kayıt kimlikleri) başka sitelere taşınmaz. |
| `X-Content-Type-Options` | `nosniff` | — |
| `Cross-Origin-Opener-Policy` | `same-origin` | Başka sitenin açtığı pencere uygulamaya erişemez. |
| `Strict-Transport-Security` | Caddy'de: `max-age=31536000` ([09 §6](../09-environments-and-deployment.md#6-ters-proxy-ve-https)) | Tarayıcı siteye bir daha HTTP ile bağlanmaz. |
| `Cache-Control` | `index.html`: `no-cache`; adında özet (hash) olan statik dosyalar: bir yıl, `immutable` | Yeni sürüm hemen yüklenir; değişmeyen dosyalar yeniden indirilmez ([api §12](api.md#12-sürümleme-ve-uyumluluk)). |

**Bilinen ödün — `style-src 'unsafe-inline'`:** Radix tabanlı bazı bileşenler (ör. diyalog açıkken sayfa kaydırmasını kilitleyen yardımcı) çalışma anında `<style>` etiketi ekler ([kaynak](https://gist.github.com/rbonestell/4fcba81d05413f4e27bab8ebf787624c)). Satır içi stil, satır içi betiğe göre çok daha düşük risklidir. S1'de bu izin olmadan çalışıp çalışmadığı denenir; çalışırsa kaldırılır. Yazı tipleri de kendi sunucumuzdan sunulur; Google Fonts gibi dış kaynaklar kullanılmaz.

## 9. Girdi doğrulama katmanları

Her kural tek bir katmanda yazılır; aynı kural iki yerde tekrarlanmaz:

| Katman | Ne denetler | Hata |
|---|---|---|
| JSON okuma | Şema: tip, bilinmeyen alan, zorunlu alan, boş olabilirlik ([api §5.1](api.md#51-serileştirici-ayarları)) | `400` |
| Doğrulayıcı (FluentValidation, Application) | Alanın biçimi: uzunluk, aralık, e-posta biçimi, liste içinde tekrar | `400`, `validation` |
| Alan katmanı (Domain) | İş kuralları (BR-…): duruma, başka kayıtlara ve zamana bağlı kurallar | `422` |
| Veritabanı | Son savunma: kısıtlar ([database §12.1](database.md#121-kısıtlar)) | `422` (kurala eşlenmiş) |

**Çıktı tarafında:** React, metni HTML olarak değil düz metin olarak yazar. HTML'i doğrudan basan `dangerouslySetInnerHTML` yasaktır (lint kuralı, [code-style §5.3](code-style.md#53-lint-eslint)). Kullanıcı girdisi hiçbir zaman SQL'e metin olarak eklenmez ([database §17](database.md#17-bağlantı-ve-işletim-ayarları)).

## 10. Kişisel veri

Kapsam 00-scope'taki karara göre sınırlıdır: rol bazlı erişim ve işlem geçmişi uygulanır; aydınlatma metni ve silme talebi gibi KVKK süreçleri kapsam dışıdır. Demo verisi tamamen kurgusaldır ([00 §9](../00-scope.md#9-fonksiyonel-olmayan-varsayımlar)).

**Kişisel veri:** kullanıcıların adı, e-postası ve telefonu; taraflardaki kişilerin iletişim bilgileri; S2'de crew bilgileri.

| Kural | Açıklama |
|---|---|
| Loglarda yok | Loglara ve izlere yalnızca kimlikler yazılır; ad, e-posta, telefon yazılmaz ([observability §3.4](observability.md#34-kişisel-veri-ve-gizli-bilgi)) |
| Adreslerde yok | Kişisel veri adres yoluna konmaz; arama metni (`q`) gibi değerler izlerde maskelenir |
| Hata mesajlarında sınırlı | `detail` alanında kişisel veri bulunmaz. `params` yalnızca kullanıcının zaten görmeye yetkili olduğu bilgiyi taşır (ör. BR-WHS-006'da okutan kişinin adı). |
| Saklama süresi | Giriş denemeleri 90 gün, süresi dolmuş oturum kayıtları 30 gün sonra silinir. İş kayıtları ve işlem geçmişi silinmez. |
| Erişim | Yalnızca rol yetkileriyle; tüm değişiklikler işlem geçmişindedir |

## 11. Bağımlılık güvenliği

- Paket sürümleri kilit dosyalarıyla sabittir ([07 §7](../07-tech-stack.md#7-sürüm-politikası)).
- .NET derlemesi bilinen güvenlik açığı olan paketleri raporlar (NuGet denetimi). Yüksek ve kritik açıklar sürekli entegrasyonda derlemeyi durdurur; ön yüzde `pnpm audit` aynı işi yapar.
- Güncellemeler Dependabot ile otomatik PR olarak gelir; yeni yayımlanan sürümler birkaç gün bekletilir ([git §9](git.md#9-bağımlılık-güncellemeleri)).

## 12. Denetim

| Kural | Denetleyen |
|---|---|
| Her uç nokta yetki ister | AT-09 |
| Rol–yetki matrisinin tutarlılığı, genel müdürün salt okunurluğu | Birim testleri (§3.2) |
| Depo kapsamı (BR-SYS-003) | Kural testleri |
| Oturum süreleri, çıkışta sunucu kaydının silinmesi, şifre değişince diğer oturumların bitmesi | Entegrasyon testleri (sahte saatle) |
| CSRF katmanları, güvenlik başlıkları | Entegrasyon testleri |
| Veritabanı rol yetkileri | DT-02 |
| Gizli bilgi taraması, bağımlılık açıkları | gitleaks, NuGet denetimi, `pnpm audit`, Dependabot uyarıları ([git §9–10](git.md#9-bağımlılık-güncellemeleri), [ci §9–10](ci.md#9-lisans-denetimi)) |

## 13. Kararlar

| No | Konu | Karar | Gerekçe |
|---|---|---|---|
| G-01 | Şifre uzunluğu ve boşluk | En az 15 karakter (P-04); boşluk karakteri kullanılamaz | 15 karakter NIST SP 800-63B-4'ün tek doğrulama yöntemi için istediği değerdir. Boşluk yasağı kullanıcının kararıdır. NIST boşluğa izin verilmesini önerir, çünkü cümle biçimli şifreleri kolaylaştırır. Yasakla birlikte kelimeler `-` ya da `.` ile ayrılabilir (ör. `depo-sabah-kahve-7`). Yasak ayrıca kopyala-yapıştırda şifrenin başına ya da sonuna fark edilmeden eklenen boşluklardan doğan giriş sorunlarını da önler. |
| G-02 | Mutlak oturum süresi (P-16) | 24 saat | Çalınmış bir çerez sınırsız kullanılamaz; bir vardiya boyunca yeniden giriş istenmez. |
| G-03 | İki adımlı doğrulama | S1'de yok; model ileride eklenebilecek biçimde | S1 kapsamını büyütmemek için; §4.3 |
| G-04 | Rollerin yeri | Kodda sabit tanım; atamalar veritabanında ([ADR-0026](../adr/0026-authorization-model.md)) | §3.1 |
| G-05 | Kapsam kontrolü | Uygulama katmanında, `403` + kural numarası | §3.4 |
| G-06 | Yetki değişikliğinin etkisi | Açık oturumlarda aynı işlemde güncellenir | §3.5 |
| G-07 | Oturum anahtarı | Veritabanında yalnızca özeti; çerez `__Host-` önekli | §4.2 |
| G-08 | Şifre özetleme | PBKDF2-HMAC-SHA512, 210.000 tur, otomatik yeniden özetleme | §5.2 |
| G-09 | Şifre kuralları | Karışım kuralı ve süre zorunluluğu yok; yaygın şifre listesi kontrolü | §5.1 |
| G-10 | Gizli bilgilerin kaynağı | Geliştirmede kullanıcı gizlileri, yayında dosya olarak; ortam değişkeni değil | §6 |
| G-11 | Veri koruma anahtarları | Veritabanında, sabit uygulama adıyla, yayında sertifikayla şifreli | §7 |
| G-12 | CSP | Betikte katı; stilde `unsafe-inline` bilinen ödün | §8 |

## 14. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-09-25 | v0.1 | İlk taslak |
| 2026-09-25 | v1.0 | G-01 (en az 15 karakter, boşluk yok), G-02 (P-16 = 24 saat), G-03 (iki adımlı doğrulama S1'de yok) kararlaştırıldı; ADR-0026 ve ADR-0027 kabul edildi. |
| 2026-09-25 | v1.1 | Gizli bilgi taraması (gitleaks) ve bağımlılık güncelleme aracı (Dependabot) bağlandı (D.1). |
| 2026-09-25 | v1.2 | HSTS, gizli bilgi yenileme yordamı ve CI denetimleri bağlandı (D.3). |
