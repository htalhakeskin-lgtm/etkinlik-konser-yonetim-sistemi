# 10 — İşletim El Kitabı

> **Durum:** v1.0 · **Son güncelleme:** 2026-09-25

## 1. Bu belge ne işe yarar

Demo ortamının (ve ileride yayın ortamının) işletilmesi için adım adım yordamlar. Ortamın yapısı ve kararların gerekçeleri [09-environments-and-deployment.md](09-environments-and-deployment.md)'dedir; bu belge "ne yapılır" sorusunu cevaplar.

- Yordamlarda adı geçen betikler (`deploy/` klasörü: Compose tanımı, Caddy ayarı, kurulum, yayın, yedekleme ve tatbikat betikleri) Faz 1'de yazılır. Betik değiştiğinde ilgili yordam aynı PR'da güncellenir.
- Komutlar sunucuda `/opt/festos/compose` klasöründe çalıştırılır.
- Her yordamın sonunda bir **doğrulama** adımı vardır; doğrulanmayan işlem bitmiş sayılmaz.

## 2. Sunucunun ilk kurulumu

1. **Sunucu:** Ubuntu Server 24.04 LTS ile bir sanal sunucu açılır ([09 §4.3](09-environments-and-deployment.md#43-sunucu)). Sağlayıcının ağ kurallarında gelen 22, 80, 443 (TCP) ve 443 (UDP) açılır. Oracle'da:
   - ana bölge 09 §4.3'teki tercih sırasıyla seçilir (sonradan değiştirilemez),
   - şekil `VM.Standard.A1.Flex`, 2 OCPU ve 12 GB bellek; Ubuntu 24.04 ARM (aarch64) imajı,
   - port kuralları sanal ağın güvenlik listesinde tanımlanır,
   - yedekler için bir nesne deposu kovası (bucket) açılır.
2. **Alan adı:** deSEC'te `dedyn.io` alt alan adı alınır ve A kaydı sunucunun genel IP adresine yönlendirilir.
3. **Temel kurulum:** Yönetici kullanıcısıyla bağlanılır ve repodaki `deploy/bootstrap.sh` çalıştırılır. Betik:
   - Docker Engine ve Compose eklentisini kurar,
   - SSH'ı sıkılaştırır (yalnızca anahtar, root girişi kapalı),
   - sunucu güvenlik duvarını kurar,
   - otomatik güvenlik güncellemelerini açar,
   - zorunlu komutlu yayın kullanıcısını (`festos-deploy`) oluşturur,
   - `/opt/festos/{compose,secrets,bin}` klasörlerini ve systemd zamanlayıcılarını ([09 §4.2](09-environments-and-deployment.md#42-bileşenler)) kurar.
4. **Gizli bilgiler:** [09 §10](09-environments-and-deployment.md#10-gizli-bilgiler)'daki her gizli bilgi üretilir ve `/opt/festos/secrets/` altına yazılır:
   - veritabanı parolaları: `openssl rand -base64 32`,
   - veri koruma sertifikası: 2 yıl geçerli, kendinden imzalı X.509 (§6.2),
   - WAL-G şifreleme anahtarı: `openssl rand -hex 32`,
   - nesne deposu anahtarları: sağlayıcının konsolundan (Oracle'da S3 uyumluluk API'si için "Customer Secret Key"),
   - demo hesaplarının parolaları: rol başına bir tane, şifre kurallarına uygun (en az 15 karakter, boşluksuz), ör. `openssl rand -base64 24`,
   - Grafana Cloud belirteci: Grafana Cloud'dan, yalnızca veri yazma yetkisiyle.
   Veri koruma sertifikası, WAL-G anahtarı ve nesne deposu anahtarları parola yöneticisine de kaydedilir.
5. **Yapılandırma:** `deploy/` içeriği `/opt/festos/compose`'a kopyalanır; `.env` dosyasına alan adı ve imaj sürümü yazılır.
6. **Altyapı konteynerleri:** `docker compose up -d postgres alloy caddy`. PostgreSQL ilk açılışta veritabanını yerleşik `C.UTF-8` sıralamasıyla oluşturur, rolleri ve eklentileri kurar.
7. **Yayın anahtarı:** Yeni bir SSH anahtar çifti üretilir. Açık anahtar `festos-deploy` kullanıcısına zorunlu komutla eklenir; özel anahtar ve sunucunun parmak izi GitHub'daki `demo` ortamına gizli bilgi olarak girilir.
8. **İlk yayın:** GitHub'da `deploy.yml` güncel sürümle elle çalıştırılır. Ardından sunucuda `docker compose run --rm app reset-demo` ile demo verisi yüklenir.
9. **İlk yedek:** Temel yedek ve geri yükleme tatbikatı elle bir kez çalıştırılır (§5).
10. **Grafana:** Uyarı kuralları `deploy/grafana/`'dan içe aktarılır; e-posta iletişim noktası ve genel adres için dışarıdan erişim kontrolü tanımlanır.

**Doğrulama:**
- Genel adres HTTPS ile açılıyor; HTTP, HTTPS'e yönleniyor.
- Güvenlik başlıkları [security §8](standards/security.md#8-tarayıcı-güvenlik-başlıkları)'deki gibi (tarayıcının geliştirici araçlarında kontrol edilir).
- Demo hesaplarıyla giriş çalışıyor; iki depo sorumlusu hesabıyla iki tarayıcıda anlık güncelleme görünüyor.
- Grafana'da uygulama logları, izler, sunucu ölçümleri ve yedek ölçümleri görünüyor.
- Sunucu dışından 5432, 8080 ve 8081 portlarına erişilemiyor.

## 3. Yayın ve geri alma

**Normal yayın:** Sürüm PR'ı birleştirilir; gerisi otomatiktir ([09 §7](09-environments-and-deployment.md#7-yayın-akışı)). Yayın işinin sonucu GitHub Actions'ta görünür; başarısızlıkta e-posta gelir.

**Belirli bir sürümü yayınlama ya da geri alma:**
1. GitHub'da `deploy.yml` açılır, istenen sürüm numarası girilir ve çalıştırılır.
2. Betik aynı adımları izler: imajı çeker, `migrate` çalıştırır (eski sürümde bekleyen migration yoktur), uygulamayı değiştirir, sağlık kontrolünü bekler.

**Migration başarısız olursa:**
- Kilit zaman aşımı (`lock_timeout`, 5 saniye): yük azaldığında yayın yeniden çalıştırılır.
- Migration hatası: yeni sürüm hiç başlamamıştır, eski sürüm çalışır. Hata yeni bir migration ile düzeltilir ve yeni bir yama sürümü çıkarılır. Uygulanmış bir migration değiştirilmez ([database §16.1](standards/database.md#161-üretme-ve-inceleme)).

**Doğrulama:** Genel adreste sayfa açılıyor; API yanıtındaki `X-App-Version` beklenen sürüm; Grafana'da `service.version` yeni sürüm.

## 4. Yedekten geri dönüş

**Ne zaman:** Veri bozulduğunda (hatalı bir yayın, yanlış toplu işlem) ya da veritabanı diski kaybolduğunda.

**Hedef seçimi:**
- Belirli bir yayından hemen önceki an: yayın betiğinin oluşturduğu geri dönüş noktası (`deploy-1.4.0`).
- Belirli bir an: UTC zaman (ör. hatalı işlemin işlem geçmişindeki zamanından hemen öncesi).
- Veri diski kaybolduysa: arşivdeki en son an.

**Adımlar:**
1. Uygulama durdurulur (`docker compose stop app`); veri değişmeye devam etmez.
2. Mevcut veri birimi silinmez, adı değiştirilerek kenara alınır. Geri dönüş istenmeyen bir sonuç verirse bu birime dönülebilir.
3. PostgreSQL boş bir birimle geri yükleme kipinde açılır:
   - `wal-g backup-fetch` ile hedefin öncesindeki son temel yedek indirilir;
   - `restore_command = 'wal-g wal-fetch %f %p'`, hedef (`recovery_target_name` ya da `recovery_target_time`) ve `recovery_target_action = 'promote'` ayarlanır; `recovery.signal` dosyası oluşturulur ([kaynak](https://www.postgresql.org/docs/current/continuous-archiving.html)).
4. PostgreSQL açılır; WAL dosyalarını hedefe kadar uygular ve yazılabilir hale gelir.
5. Denetimler (§5'teki tatbikat denetimleri) ve iş açısından kontrol: hatalı işlemin etkisi yok, ondan önceki veriler yerinde.
6. Uygulama başlatılır ve duman testi yapılır.
7. Hemen yeni bir temel yedek alınır; geri dönüşten sonra veritabanı yeni bir zaman çizgisindedir.
8. Kenara alınan birim, sonuç kesinleştikten 7 gün sonra silinir.

**Yalnızca bazı kayıtlar geri getirilecekse:** Geri dönüş canlı veritabanına değil, ayrı bir geçici konteynere yapılır; gereken kayıtlar oradan incelenip elle ya da bir komutla aktarılır. Canlı verideki sonraki değişiklikler kaybolmaz.

**Hedef süre:** 1 saat ([09 §8.2](09-environments-and-deployment.md#82-yedekleme-ve-zamana-göre-geri-dönüş)).

## 5. Geri yükleme tatbikatı

Otomatik tatbikat her pazar çalışır ([09 §8.3](09-environments-and-deployment.md#83-geri-yükleme-tatbikatı)). Elle çalıştırmak için: `/opt/festos/bin/restore-drill`.

Denetimler:
- geri yükleme hatasız tamamlandı,
- her modül şemasında migration geçmişi var,
- işlem geçmişindeki en son kayıt son 24 saat içinde,
- geri yükleme süresi kaydedildi (RTO hedefiyle karşılaştırmak için).

Yayın ortamında ayrıca üç ayda bir, §4'teki zamana göre geri dönüş yordamı ayrı bir sunucuda baştan sona elle uygulanır ve süresi kaydedilir.

## 6. Gizli bilgilerin yenilenmesi

**Ne zaman:** Aşağıdaki takvimde; ya da sızma şüphesinde **hemen** ([git §10](standards/git.md#10-gizli-bilgi-taraması)).

| Gizli bilgi | Sıklık |
|---|---|
| Veritabanı rol parolaları | Yılda bir |
| Veri koruma sertifikası | 2 yılda bir, süresi dolmadan en az 60 gün önce |
| WAL-G şifreleme anahtarı | Yılda bir |
| Nesne deposu anahtarları | Yılda bir |
| Grafana Cloud belirteci | Yılda bir (belirteç 1 yıl geçerli üretilir) |
| Yayın SSH anahtarı | Yılda bir |
| Demo hesaplarının parolaları | Bir kişinin erişimi geri alınacağında (§8); yoksa yılda bir |
| TLS sertifikası | Caddy otomatik yeniler; işlem gerekmez |

Her yenileme için tarihli bir issue açılır; bir sonraki yenileme tarihi issue'ya yazılır.

### 6.1 Veritabanı rol parolası

1. Yeni parola üretilir ve ilgili gizli bilgi dosyasına yazılır.
2. `ALTER ROLE festos_booking PASSWORD '…'` (yönetici bağlantısıyla).
3. Uygulama hemen yeniden başlatılır (`docker compose up -d --force-recreate app`). Açık bağlantılar eski parolayla çalışmaya devam ettiği için kesinti yalnızca yeniden başlatma kadardır.

**Doğrulama:** Uygulama sağlıklı; ilgili modülün ekranı açılıyor.

### 6.2 Veri koruma sertifikası

Veri koruma anahtarları sertifikayla şifrelenir ([security §7](standards/security.md#7-veri-koruma-anahtarları)). Eski anahtarları açabilmek için eski sertifika bir süre daha gerekir.

1. Yeni sertifika üretilir: `openssl req -x509 -newkey rsa:3072 -sha256 -days 730 -nodes -subj "/CN=FestOS Data Protection" …`, ardından parolalı `.pfx` dosyasına dönüştürülür.
2. Yeni sertifika şifreleme için, eski ve yeni sertifika birlikte şifre çözme için ayarlanır (ASP.NET: yeni sertifikayla koruma, her iki sertifikayla çözme).
3. Uygulama yeniden başlatılır. Yeni anahtarlar yeni sertifikayla şifrelenir; eski anahtarlar eski sertifikayla açılmaya devam eder, kullanıcılar oturumdan düşmez.
4. 90 gün sonra (anahtar ömrü) eski sertifika ayardan ve gizli bilgilerden kaldırılır.
5. Yeni sertifika parola yöneticisine kaydedilir.

### 6.3 WAL-G şifreleme anahtarı

1. Yeni anahtar üretilir, gizli bilgi dosyasına yazılır, PostgreSQL konteyneri yeniden başlatılır.
2. Hemen yeni bir tam temel yedek alınır.
3. Eski anahtar, onunla şifrelenmiş son yedeğin saklama süresi dolana kadar (demo'da 7, yayında 30 gün) parola yöneticisinde tutulur; eski yedekleri açmak için gerekir.

### 6.4 Nesne deposu anahtarları, Grafana belirteci, yayın SSH anahtarı

Ortak yol: yenisi üretilir → gizli bilgi güncellenir → ilgili bileşen yeniden başlatılır (PostgreSQL, Alloy) ya da GitHub'daki gizli bilgi değiştirilir → yenisinin çalıştığı doğrulanır (bir WAL dosyasının gönderildiği, Grafana'ya verinin geldiği, elle bir yayının başarılı olduğu) → **ardından** eskisi iptal edilir.

## 7. Saat dilimi verisi güncellemesi

- Saat dilimi verisi ve ICU temel imajla gelir ([09 §5](09-environments-and-deployment.md#5-konteyner-imajı)). Her yayından sonra `recalculate-local-times` otomatik çalışır ([ADR-0022](adr/0022-future-wall-clock-times.md)).
- Bir ülke saat kuralını değiştirdiğinde (ör. yaz saatine dönüş) ve bu, gelecekteki etkinlikleri etkileyecekse: .NET'in güncel yamasıyla bir yama sürümü çıkarılır (§9, aylık yama sürümü).
- Elle çalıştırmak için: `docker compose run --rm app recalculate-local-times`.

**Doğrulama:** Komutun çıktısındaki "değişen kayıt sayısı"; etkilenen bir etkinliğin saatinin arayüzde doğru görünmesi.

## 8. Demo verisini sıfırlama ve erişim

**Sıfırlama:**
- Her gece 04:00'te otomatik ([09 §9](09-environments-and-deployment.md#9-demo-verisi-ve-sıfırlama)).
- Elle: `docker compose run --rm app reset-demo`. Çalışırken demo kullanıcılarının oturumu kapanır.

**Doğrulama:** Demo senaryosunun başlangıç durumu görünüyor; yaklaşan etkinliklerin tarihleri bugünden sonra.

**Erişim vermek:** İsteyen kişiye ilgili rolün hesap adı ve parolası güvenli bir kanalla gönderilir (parola yöneticisinin paylaşım özelliği ya da bir kez açılabilen bağlantı; düz e-posta ya da mesaj değil). Kime, hangi rolün, ne zaman verildiği kısaca not edilir.

**Erişimi geri almak:** O rolün parolası gizli bilgi dosyasında değiştirilir ve `reset-demo` elle çalıştırılır. Aynı rolü kullanan diğer kişilere yeni parola gönderilir. Kişi başına ayrı erişim gerekirse aynı rol için ikinci bir demo hesabı eklenir.

## 9. Düzenli bakım

| Sıklık | İş |
|---|---|
| Haftalık | Dependabot PR'larını incelemek ve birleştirmek ([git §9](standards/git.md#9-bağımlılık-güncellemeleri)); haftalık tarama issue'larına bakmak ([ci §5](standards/ci.md#5-gece-ve-haftalık-işler)) |
| Aylık | .NET'in aylık güvenlik yamasından (her ayın ikinci salısı) sonra, o ay sürüm çıkmadıysa bir **yama sürümü**: Dependabot'un SDK güncellemesi birleştirilir, ardından `Release-As: <sürüm>` alt bilgili boş bir commit'le release-please'e sürüm PR'ı açtırılır. Böylece imaj, temel imajın güvenlik yamalarını alır. |
| Aylık | Grafana'da disk, bellek ve olay gecikmesi eğilimlerine bakmak; sağlayıcı hesabına giriş yapmak |
| Üç ayda bir | Yayın ortamında elle zamana göre geri dönüş tatbikatı (§5) |
| Yılda bir | Gizli bilgilerin yenilenmesi (§6) |

**Oracle'ın boştaki sunucuyu geri alması:** Oracle, 7 gün boyunca işlemci, ağ ve bellek kullanımı %20'nin altında kalan ücretsiz sunucuları geri alabilir ([kaynak](https://docs.oracle.com/en-us/iaas/Content/FreeTier/freetier_topic-Always_Free_Resources.htm)). PostgreSQL'in `shared_buffers` ayarı belleğin yaklaşık %25'i olduğu için bellek kullanımı eşiğin üzerindedir. Sunucu yine de geri alınırsa §11'deki yordamla yeniden kurulur; yedekler nesne deposunda olduğu için etkilenmez.

## 10. Sorun giderme ve olay müdahalesi

**Genel akış:**
1. Uyarı e-postasından hangi kuralın tetiklendiği okunur ([09 §11](09-environments-and-deployment.md#11-telemetri-ve-uyarılar)).
2. Grafana'da ilgili zaman aralığındaki hatalar ve izler incelenir. Kullanıcının gördüğü hatadaki `traceId` doğrudan izi bulur ([api §8](standards/api.md#8-hata-yanıtları)).
3. Neden bulunur, düzeltilir; kalıcı bir düzeltme gerekiyorsa `type:bug` issue'su açılır ve düzeltme hata düzeltmesi bitti tanımına göre yapılır ([definition-of-done §5](standards/definition-of-done.md#5-hata-düzeltmesi-için)).

| Belirti | İlk bakılacak yer | Yapılacak |
|---|---|---|
| Site açılmıyor | `docker compose ps`, `docker compose logs --tail 200 app caddy` | Konteyner durmuşsa nedeni loglardan bulunur, `docker compose up -d`. Sertifika sorunu varsa Caddy logları. |
| `500` oranı arttı | Grafana'da hata logları ve izler | Son yayınla başladıysa önceki sürüme dönülür (§3), sonra incelenir. |
| Hatalı olaylar listesinde olay var | Grafana'da olay tipi ve hata; sistem yöneticisi ekranındaki hatalı olaylar listesi | Neden giderilir, olay yeniden işlemeye alınır. Olay kaybolmaz ([05 §9.2](05-module-map.md#92-olayların-teslimi)). |
| Olay gecikmesi yüksek | Olay gecikmesi ölçümü, veritabanı bağlantıları, işlemci | Uzun süren bir dinleyici ya da kilit beklemesi aranır. |
| Zamanlanmış iş çalışmadı | İş logları, uygulamanın yeniden başlama zamanı | Uygulama sağlıklıysa iş elle tetiklenir; iş kilidi takıldıysa loglardan bulunur. |
| WAL arşivleme başarısız | PostgreSQL logları, nesne deposu erişimi ve anahtarları | Arşivleme başarısız oldukça WAL dosyaları diskte birikir ve disk dolar. Erişim hemen düzeltilir; biriken dosyalar arşivlenince PostgreSQL onları kendisi temizler. |
| Disk doluyor | `df -h`, `docker system df`, PostgreSQL'in `pg_wal` klasörü | Önce WAL arşivlemenin çalıştığı doğrulanır; kullanılmayan imajlar silinir (`docker image prune`). |
| Yedek ya da tatbikat başarısız | Betiğin logu (systemd günlüğü) | Neden düzeltilir, iş elle çalıştırılır (§5). |
| Gizli bilgi sızdı | — | §6'daki yordamla **hemen** yenilenir. Sunucuya yetkisiz erişim şüphesi varsa tüm gizli bilgiler yenilenir, SSH ve işlem geçmişi incelenir, sunucu §11'le sıfırdan kurulur. |

Her olaydan sonra ilgili issue'ya kısa bir not yazılır: ne oldu, neden oldu, ne yapıldı, tekrarlanmaması için ne değişti.

## 11. Sunucuyu yeniden kurma ya da taşıma

Sunucu kaybolduğunda, geri alındığında ya da başka bir sağlayıcıya geçilirken:

1. §2'deki 1–7. adımlar yeni sunucuda uygulanır. Gizli bilgiler **yeniden üretilmez**, parola yöneticisinden geri yüklenir; veri koruma sertifikası ve WAL-G anahtarı eskisiyle aynı olmalıdır.
2. Veritabanı, nesne deposundaki en son yedekten geri yüklenir (§4, hedef: en son an).
3. En son sürüm `deploy.yml` ile yayınlanır.
4. Alan adının A kaydı yeni IP adresine çevrilir.
5. Eski sunucu hâlâ erişilebilirse kapatılır; yayın SSH anahtarı yenilenir (§6.4).

**Doğrulama:** §2'nin doğrulama listesi; ayrıca kullanıcılar oturumdan düşmeden devam ediyor (veri koruma anahtarları korunduysa).

**Hedef süre:** 1 saat.

## 12. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-09-25 | v0.1 | İlk taslak |
