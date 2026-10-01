# Modül tasarımı: Identity — Kimlik ve erişim

> **Durum:** v0.1 (taslak, proje sahibinin onayını bekliyor) · **Son güncelleme:** 2026-10-01
> **Adım:** Faz 1.2 ([12 §3](../12-implementation-plan.md#3-adımlar)) · **Kararlar:** [Bölüm 12](#12-kararlar)

## 1. Bu belge ne işe yarar

Identity modülünün fiziksel tasarımıdır: tablolar ve kısıtlar, yetki kataloğu ve rol matrisi, giriş ve oturum akışı, uç noktalar, olaylar, ekranlar, hikaye ve kural eşlemesi, PR planı. Kavramsal kararlar Faz 0'dadır ve burada tekrarlanmaz:

**Kaynaklar:** [05 §5.1](../05-module-map.md#51-identity--kimlik-ve-erişim), [06 §5.1](../06-erd-conceptual.md#51-identity--kimlik-ve-erişim), [security §3–§5](../standards/security.md#3-yetki-modeli), [api §11](../standards/api.md#11-güvenlik-kuralları), [11 §2.1, §2.8](../11-screens.md#21-uygulama-kabuğu); ADR [0011](../adr/0011-authentication.md), [0026](../adr/0026-authorization-model.md), [0027](../adr/0027-session-and-password-security.md); hikayeler US-SYS-001, 002, 003, 006, 010, 011, 012; kurallar BR-SYS-002…009, BR-SYS-014, BR-SYS-015 (yeni).

Aynı adımın diğer belgeleri: [audit.md](audit.md) (işlem geçmişinin okunması), [inventory.md](inventory.md) (depolar).

## 2. Kapsam

| Var | Yok (sonraki sürüm) |
|---|---|
| Giriş, çıkış, oturum, hesap kilidi, geçici şifre, şifre değiştirme | 2FA, kayıt bazlı ince yetki (S6) |
| Kullanıcı oluşturma, düzenleme, pasifleştirme, yeniden etkinleştirme, şifre sıfırlama | Kullanıcının kendi kendine kaydı, e-postayla şifre sıfırlama |
| Sabit roller, yetki kataloğu, salt okunur rol matrisi | Arayüzden rol tanımlama |
| Depo atamaları ve `WarehouseDeactivated` dinleyicisi | — |
| İlk sistem yöneticisini oluşturan komut | — |
| Uygulama kabuğu, giriş ve yeni şifre ekranları, yetkiye göre menü, yeniden giriş diyaloğu | — |

## 3. Projeler

[08 §3](../08-architecture.md#3-bir-modülün-yapısı)'teki modül yapısıdır: `FestOS.Modules.Identity.{Domain, Application, Contracts, Infrastructure}`. Identity olay yayınlamadığı için `IntegrationEvents` projesi yoktur.

**Veri erişim kalıbı (ID-01):** Identity ilk iş modülüdür ve kalıbı belirler.
- **Komutlar** Application'dadır. Toplu kökü, Application'daki depo (repository) arayüzüyle yükler (`IUserRepository.GetAsync`, `Add`); arayüzün EF uygulaması Infrastructure'dadır. Kaydetmeyi işlem birimi dekoratörü yapar. Böylece iş akışı veritabanı olmadan birim testiyle sınanır.
- **Sorgular** Infrastructure'dadır. Sorgu ve sonuç tipi Application'da (ya da Contracts'ta), işleyicisi Infrastructure'da EF izdüşümüyle (`Select`) yazılır; okuma için toplu kök yüklenmez.

## 4. Veritabanı (`identity` şeması)

| Tablo | Kolonlar (ortak kolonlar hariç) | Kısıtlar ve indeksler |
|---|---|---|
| `users` | `id`, `full_name varchar(200)`, `email varchar(320)` (küçük harf, NFC), `password_hash text`, `must_change_password bool`, `is_active bool`, `failed_login_count int`, `locked_until timestamptz null`, `version` | `ux_users_email` → BR-SYS-015; e-posta pasif kullanıcılarda da tekildir |
| `user_roles` | `user_id`, `role varchar(64)` | PK `(user_id, role)`; `ck_user_roles_role_enum`; `users` silinmez, FK basamaklıdır (alt varlık) |
| `user_warehouses` | `user_id`, `warehouse_id` | PK `(user_id, warehouse_id)`; `ix_user_warehouses_warehouse_id` (depo pasifleşince atamalar bununla bulunur, DT-03); depoya FK yoktur (modüller arası, [database §10](../standards/database.md)) |
| `sessions` | `id`, `user_id`, `key_hash char(64)`, `created_at`, `last_seen_at`, `expires_at`, `permissions text[]`, `warehouse_ids uuid[]`, `user_agent varchar(400)` | `ux_sessions_key_hash` (teknik, DT-04 istisnası); FK `users` basamaklı |
| `login_attempts` | `id`, `email varchar(320)`, `user_id null`, `occurred_at`, `succeeded bool`, `ip_address inet null` | `ix_login_attempts_occurred_at`; 90 gün sonra silinir (kişisel veri, [security §10](../standards/security.md)) |
| `data_protection_keys` | ASP.NET veri koruma anahtarları (EF sağlayıcısı) | [security §7](../standards/security.md#7-veri-koruma-anahtarları) |

- `User` tek toplu köktür; roller ve depo atamaları alt varlıklarıdır, sürüm kökte artar.
- `sessions` ve `login_attempts` teknik tablolardır: işlem geçmişine yazılmaz (`[NotAudited]`), sürüm taşımaz. `users` tablosunda `password_hash`, `failed_login_count` ve `locked_until` işlem geçmişine yazılmaz.
- Oturum satırı kullanıcının yetkilerini ve depo atamalarını taşır (ADR-0011). Rol ya da depo ataması değişince açık oturumlar **aynı işlemde** güncellenir ([security §3.5](../standards/security.md#35-yetki-değişikliklerinin-etkisi)).

## 5. Yetkiler ve roller

### 5.1 Yetki kataloğu

Her modül yetkilerini kendi `…Permissions` sınıfında tanımlar ve `IModuleDefinition.Permissions` ile bildirir; Host bunları açılışta katalogda toplar. Bu adımın yetkileri:

| Modül | Yetkiler |
|---|---|
| Identity | `Identity.Users.View`, `Identity.Users.Create`, `Identity.Users.Edit`, `Identity.Users.Deactivate`, `Identity.Users.ResetPassword`, `Identity.Roles.View` |
| Audit | `Audit.Entries.View` |
| Inventory | `Inventory.Warehouses.View`, `Inventory.Warehouses.Create`, `Inventory.Warehouses.Edit`, `Inventory.Warehouses.Deactivate` |

Giriş, çıkış, `/me` ve kendi şifresini değiştirme her oturum açmış kullanıcıya açıktır; yetki istemez.

### 5.2 Rol matrisi

Matris, Identity'nin kodunda tek tanımdır (`RoleCatalog`); arayüzdeki matris ekranı ve testler bu tanımı okur ([security §3.2](../standards/security.md#32-s1-rolleri)). Her modül adımı kendi yetkilerini matrise ekler.

| Yetki | SY | BM | TM | DS | GM |
|---|---|---|---|---|---|
| `Identity.Users.*`, `Identity.Roles.View` | ✓ | | | | yalnızca `View` |
| `Audit.Entries.View` | ✓ | | | | ✓ |
| `Inventory.Warehouses.View` | ✓ | ✓ | ✓ | ✓ | ✓ |
| `Inventory.Warehouses.Create / Edit / Deactivate` | ✓ | | | | |

Testler: rollerdeki her kod katalogda vardır; katalogdaki her yetki en az bir roldedir; genel müdürde `View` dışında eylem yoktur (BR-SYS-004); `Audit.Entries.View` yalnızca SY ve GM'dedir (BR-SYS-010).

### 5.3 Uç noktada denetim

- `RequirePermission(kod)` uç nokta metadatasıdır; kod aynı adlı bir yetki politikasına dönüşür ve politikalar katalogdan dinamik üretilir ([security §3.3](../standards/security.md#33-uç-nokta-düzeyinde-kontrol)). Yetki yoksa `403 forbidden`.
- Bu altyapı Identity'ye değil BuildingBlocks'a aittir (BB-01): `RequirePermission`, politika sağlayıcısı ve AT-09 testi (her uç nokta yetki ister ya da açıkça anonimdir) bu adımın ilk PR'larındadır.
- **Geçici şifre kapısı (BR-SYS-006):** `must_change_password` olan oturum yalnızca `/me`, şifre belirleme ve çıkış uçlarını çağırabilir; diğer her uç `403` ve `code: BR-SYS-006` döner.

## 6. Giriş ve oturum

| Konu | Uygulama |
|---|---|
| Kimlik doğrulama | ASP.NET Core çerez kimlik doğrulaması, `ITicketStore` ile sunucu tarafı oturum: çerezde yalnızca rastgele 32 baytlık anahtar bulunur, veritabanında SHA-256 özeti (ADR-0027). |
| Çerez | `__Host-festos_session`; `HttpOnly`, `Secure`, `SameSite=Strict`, `Path=/`. Geliştirmede önek ve `Secure` yoktur (BB-11). |
| Süreler | Hareketsizlik P-03 (12 saat), mutlak P-16 (24 saat); hangisi önce dolarsa (BR-SYS-008). `last_seen_at` her istekte değil, son yazmadan 1 dakika geçtiyse güncellenir (ID-04). |
| Önbellek | Doğrulanmış oturum 30 saniye bellekte tutulur. Oturumu silen ya da güncelleyen işlem (çıkış, pasifleştirme, rol değişikliği, şifre değişikliği) önbellek kaydını da siler; tek uygulama örneği olduğu için yeterlidir. |
| Hesap kilidi | Hatalı şifre `failed_login_count`'u artırır; P-01'e (5) ulaşınca `locked_until = şimdi + P-02` (15 dakika) olur ve sayaç sıfırlanır. Başarılı giriş sayacı sıfırlar. Kilitliyken doğru şifre de reddedilir (BR-SYS-005). |
| Zamanlama eşitliği | E-posta kayıtlı değilse de sahte bir özet üzerinde doğrulama yapılır ([security §4.1](../standards/security.md#41-giriş)). |
| İstek sınırı | Giriş ucunda ASP.NET Core istek sınırlayıcı: IP başına dakikada 10, e-posta başına dakikada 5 deneme; aşılınca `429` ve `Retry-After` ([api §11](../standards/api.md#11-güvenlik-kuralları)). |
| Şifre | ASP.NET Core Identity `PasswordHasher` (PBKDF2-HMAC-SHA512, 210.000 tur). Politika (BR-SYS-007): en az P-04 (15), en çok 128 karakter, boşluk yok, NFC, yaygın şifre listesinde değil, e-posta ve ürün adı değil, mevcut şifreyle aynı değil. Yaygın şifre listesi SecLists'in ilk 100.000 şifresidir (MIT), sunucuda gömülü kaynak olarak durur. |
| Geçici şifre | 16 karakter, karıştırılmayan harf ve rakamlardan (`0/O`, `1/l/I` yok), 4'lü gruplar halinde; kriptografik rastgele. Yalnızca oluşturan yanıtta bir kez döner (BR-SYS-006). |
| Şifre değişince | Kullanıcının diğer tüm oturumları silinir; bu oturum kalır (BR-SYS-007). Geçici şifreden yeni şifreye geçişte mevcut şifre istenmez; oturum onu zaten kanıtlamıştır. |
| `ICurrentUser` | Identity'nin Infrastructure'ı uygular: oturumdan kullanıcı kimliği, adı, yetkileri ve depo atamaları. İsteğin dışındaki işler `SystemUser`'dır. Bu, 1.1'den kalan `ICurrentUser` kaydı sorusunu kapatır. |

## 7. Uç noktalar

| Yöntem ve adres | İşlem adı | Yetki | Not |
|---|---|---|---|
| `POST /api/v1/auth/login` | `Login` | anonim | `200` + kullanıcı bilgisi + oturum çerezi + yeni antiforgery belirteci. Hatalı bilgi `401 invalidCredentials`; kilitli hesap `401`, `code: BR-SYS-005`, `params.lockedUntil`. |
| `POST /api/v1/auth/logout` | `Logout` | oturum | `204`; oturum satırı silinir |
| `GET /api/v1/me` | `GetMe` | oturum | Ad, roller, yetkiler, depo atamaları, `mustChangePassword`; yeni antiforgery belirteci |
| `POST /api/v1/me/password` | `ChangeMyPassword` | oturum | Normalde mevcut şifre zorunlu; geçici şifre kapısındayken istenmez |
| `GET /api/v1/users` | `ListUsers` | `Identity.Users.View` | Sayfalı; arama (ad, e-posta), rol ve aktiflik süzgeci (varsayılan: aktif) |
| `GET /api/v1/users/{userId}` | `GetUser` | `Identity.Users.View` | `ETag` |
| `POST /api/v1/users` | `CreateUser` | `Identity.Users.Create` | Yanıtta geçici şifre bir kez döner |
| `PUT /api/v1/users/{userId}` | `EditUser` | `Identity.Users.Edit` | Ad, e-posta (S2'ye bağlı), roller, depolar; `If-Match` |
| `POST /api/v1/users/{userId}/deactivate` | `DeactivateUser` | `Identity.Users.Deactivate` | Oturumları siler; BR-SYS-009 |
| `POST /api/v1/users/{userId}/activate` | `ActivateUser` | `Identity.Users.Deactivate` | Pasif kullanıcıyı geri açar (ID-05) |
| `POST /api/v1/users/{userId}/reset-password` | `ResetUserPassword` | `Identity.Users.ResetPassword` | Yeni geçici şifre bir kez döner; kilidi de kaldırır; kullanıcının oturumlarını siler |
| `GET /api/v1/roles` | `ListRoles` | `Identity.Roles.View` | Katalog (modüle göre gruplu) ve rol matrisi |

Doğrulama: ad soyad zorunlu, en çok 200; e-posta geçerli biçim, en çok 320; en az bir rol; depo sorumlusu rolünde en az bir depo (BR-SYS-014). Atanan depoların var ve aktif olduğu Inventory'nin senkron sözleşmesiyle doğrulanır ([05 §3](../05-module-map.md), `IWarehouseDirectory`).

## 8. Olaylar

- **Yayınladığı:** yok.
- **Dinlediği:** `WarehouseDeactivated` (Inventory). Depoya atanmış kullanıcıların atamaları kaldırılır. Bu, bir depo sorumlusunun son deposunu kaldırıyorsa kullanıcı aktif kalır; sistem yöneticisine kullanıcının yeni depo beklediği kullanıcı listesinde bir uyarıyla gösterilir (BR-SYS-014 yalnızca kayıtta uygulanır; olay sonrasında uyarı yeterlidir).
- **Zamanlanmış iş:** `login-attempts-cleanup` (gecelik, 90 gün) ve `sessions-cleanup` (saatlik, süresi dolan oturumlar).

## 9. Ekranlar

| Ekran | Adres | Not |
|---|---|---|
| Giriş | `/login` | [11 §2.8](../11-screens.md#28-giriş); kilitli hesapta kalan süre yazılır |
| Yeni şifre belirleme | `/set-password` | Geçici şifreyle girişte zorunlu ara ekran; kural metni alanın altında |
| Şifre değiştirme | Kullanıcı menüsünden diyalog | Mevcut şifre, yeni şifre |
| Kullanıcılar | `/admin/users` | Liste + oluşturma / düzenleme diyaloğu; geçici şifre diyaloğu (bir kez, kopyalanabilir) |
| Roller ve yetkiler | `/admin/roles` | Salt okunur matris |
| Uygulama kabuğu | — | Menü, üst çubuk, kullanıcı menüsü, sürüm ve bağlantı şeritleri, yeniden giriş diyaloğu ([ui §7.7](../standards/ui.md)) |

Kabuk oturum açınca `RealtimeClient`'ı başlatır ve `ConnectionIndicator`'ı yerleştirir (building-blocks §12). Menü `/me`'deki yetkilerden üretilir; boş kalan grup gizlenir. `/` rolüne göre yönlendirir ([11 §4](../11-screens.md#4-başlangıç-ekranı)).

## 10. Hikaye ve kural eşlemesi

| Hikaye | Uç noktalar ve ekranlar | Kurallar |
|---|---|---|
| US-SYS-010 Giriş ve çıkış | `Login`, `Logout`, `GetMe`; giriş, yeni şifre | BR-SYS-005, 006, 008 |
| US-SYS-011 Şifre değiştirme | `ChangeMyPassword`; diyalog | BR-SYS-007 |
| US-SYS-012 Role göre arayüz | `GetMe`; kabuk ve menü; `RequirePermission` | BR-SYS-002 |
| US-SYS-001 Kullanıcı oluşturma | `CreateUser`, `EditUser`; kullanıcılar | BR-SYS-006, 014, 015 |
| US-SYS-002 Pasifleştirme ve şifre sıfırlama | `DeactivateUser`, `ActivateUser`, `ResetUserPassword` | BR-SYS-001, 006, 008, 009 |
| US-SYS-003 Rol matrisi | `ListRoles`; roller ve yetkiler | — |
| US-SYS-006 Salt okunur erişim | Rol matrisi; menü ve düğmeler | BR-SYS-002, 004 |

**Yeni kural (BR-SYS-015 · Tekil e-posta):** Kullanıcı e-postası, büyük-küçük harf ve Unicode biçimi farkı gözetmeden sistemde tekildir; pasif kullanıcıların e-postası da yeniden kullanılamaz. US-SYS-001'in kabul kriteriydi; veritabanı kısıtına bağlanabilmesi (DT-04) ve testle izlenebilmesi için numaralı kural olur.

## 11. PR planı

| # | PR | Kapsam |
|---|---|---|
| 1 | Yetki altyapısı | BuildingBlocks: `RequirePermission`, dinamik politika sağlayıcısı, yetki kataloğu, AT-09 |
| 2 | Identity iskeleti | Projeler, şema, `users` / `user_roles` / `user_warehouses`, rol matrisi ve testleri, AppHost rolü |
| 3 | `create-admin` komutu | İlk sistem yöneticisi (S1'e bağlı) |
| 4 | Giriş ve oturum | `ITicketStore`, oturum tablosu ve önbelleği, kilit, istek sınırı, `ICurrentUser`, geçici şifre kapısı, `Login` / `Logout` / `GetMe` |
| 5 | Şifre değiştirme | Politika, yaygın şifre listesi, `ChangeMyPassword` |
| 6 | Kabuk ve giriş ekranları | Giriş, yeni şifre, kabuk, menü, kullanıcı menüsü, yeniden giriş diyaloğu, kabukta anlık bildirim |
| 7 | Kullanıcı yönetimi (sunucu) | Oluşturma, düzenleme, pasifleştirme, etkinleştirme, sıfırlama |
| 8 | Kullanıcı yönetimi (ekran) | `/admin/users` |
| 9 | Rol matrisi ekranı | `ListRoles`, `/admin/roles` |
| 10 | Depo atamaları | `IWarehouseDirectory` ile doğrulama, `WarehouseDeactivated` dinleyicisi, temizlik işleri |

Depo ve işlem geçmişi PR'ları kendi belgelerindedir; sıra Inventory'nin depo PR'larıyla iç içe geçer (depo ataması depolardan sonra gelir).

## 12. Kararlar

| No | Konu | Karar | Gerekçe |
|---|---|---|---|
| ID-01 | Veri erişim kalıbı | Komutlar Application'da, toplu kök depo arayüzüyle; sorgular Infrastructure'da EF izdüşümüyle | Application EF'e referans veremez ([08 §3.2](../08-architecture.md#32-proje-bağımlılıkları)); iş akışı birim testiyle sınanır, okumada gereksiz toplu kök yüklenmez |
| ID-02 | Oturumda yetkiler | Oturum satırında `permissions` ve `warehouse_ids` dizileri | Her istekte rol tablosuna gitmez; değişiklik aynı işlemde oturuma yazılır |
| ID-03 | Oturum önbelleği | 30 saniyelik bellek önbelleği, değiştiren işlemde silinir | Her istekte veritabanı sorgusunu önler; tek örnekte tutarlıdır |
| ID-04 | `last_seen_at` güncellemesi | En çok dakikada bir | Her istekte yazma yükü olmaz; 12 saatlik sürede bir dakikalık sapma önemsizdir |
| ID-05 | Yeniden etkinleştirme | Pasif kullanıcı geri açılabilir; yetki pasifleştirmeyle aynıdır | Hatalı pasifleştirme silme olmadığı için düzeltilebilir olmalı; hikayeler yasaklamıyor |
| ID-06 | Yaygın şifre listesi | SecLists ilk 100.000, gömülü kaynak | Dış servis yok (ADR-0027); MIT lisanslı; 100.000 kayıt bellekte küçük |

## 13. Proje sahibine sorular

| Soru | Seçenekler | Öneri |
|---|---|---|
| S1 — Yeni kurulan sistemde ilk sistem yöneticisi nasıl oluşsun? | (a) Host'ta `create-admin --email … --name …` komutu; geçici şifreyi bir kez ekrana yazar (yayın betiğindeki `migrate` gibi). (b) İlk açılışta ayarlardaki e-postayla kendiliğinden. (c) Yalnızca demo verisiyle. | **(a)**: açık, tekrarlanabilir, gizli bilgi ayarlarda durmaz; demo verisi de aynı yolu kullanır |
| S2 — Sistem yöneticisi bir kullanıcının e-postasını sonradan değiştirebilsin mi? | (a) Evet; değişince kullanıcının oturumları sonlanır. (b) Hayır; e-posta değişmez, gerekirse yeni kullanıcı açılır. | **(a)**: evlilik, alan adı değişikliği gibi gerçek durumlar var; kimlik `id`'dir, e-posta değil |

## 14. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-10-01 | v0.1 | İlk taslak |
