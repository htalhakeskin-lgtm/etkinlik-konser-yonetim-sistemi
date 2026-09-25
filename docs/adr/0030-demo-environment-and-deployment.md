# ADR-0030: Demo ortamı ve yayın modeli

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [09-environments-and-deployment.md](../09-environments-and-deployment.md), [standards/ci.md](../standards/ci.md), [10-operations.md](../10-operations.md), [ADR-0028](0028-development-workflow.md), [standards/database.md §16](../standards/database.md#16-migrationlar)

## Bağlam

- Portfolyo için uygulamanın yayında çalışan bir demosu gerekir. Hiçbir bağımlılık ve hizmet için ücret ödenmeyecektir.
- Uygulamanın her zaman açık bir sunucuya ihtiyacı vardır: zamanlanmış işler (5 dakikalık otomatik geçişler, gece çakışma kontrolü), SignalR bağlantıları ve PostgreSQL 18'e özgü özellikler (yerleşik `C.UTF-8` sıralaması, modül başına roller, ICU). Boştayken uyuyan ücretsiz platformlarda (Render, Koyeb) bunlar çalışmaz. Fly.io'nun ücretsiz kotası 2024'te kalktı.
- Ücretsiz sağlayıcıların koşulları habersiz değişebiliyor: Oracle, Haziran 2026'da ücretsiz ARM sunucu sınırını duyuru yapmadan yarıya indirdi ([kaynak](https://www.infoq.com/news/2026/07/oracle-cloud-free-tier-limits/)).
- Veritabanı standardı yayında migration'ların uygulama açılışında değil ayrı bir adımda, ayrı rolle çalışmasını istiyor ([database §16.2](../standards/database.md#162-uygulama)).

## Karar

- **Yapı:** Tek bir Linux sunucuda Docker Compose: Caddy (HTTPS, HSTS), FestOS Host, WAL-G eklenmiş PostgreSQL 18 ve Grafana Alloy. Yalnızca Caddy dışarıya port açar. Kurulum repodaki betiklerle yapılır; yapı sağlayıcıdan bağımsızdır.
- **İmaj:** .NET SDK'nın yerleşik konteyner üretimiyle, Dockerfile olmadan; `aspnet:10.0-noble-chiseled-extra` temel imajı (kabuk yok, root olmayan kullanıcı, ICU ve saat dilimi verisi dahil); tek adla x64 ve ARM64; `ghcr.io`'da; her sürüm için derleme kaynağı kanıtı.
- **Host komutları:** Aynı imaj `migrate`, `seed-demo`, `reset-demo`, `recalculate-local-times` ve `probe-health` komutlarını da çalıştırır.
- **Migration:** Yayında aynı imajdaki `migrate` komutuyla, uygulama başlamadan önce ayrı bir adımda, `festos_migrator` rolüyle. EF migration paketi (bundle) kullanılmaz.
- **Yayın:** Sürüm PR'ı birleştirilince otomatik. GitHub Actions sunucuya zorunlu komutlu bir kullanıcıyla SSH ile bağlanır; betik geri dönüş noktası oluşturur, migration'ı uygular, yeni sürümü başlatır, sağlık kontrolü geçmezse önceki sürüme döner. Ardından genel adresten duman testi yapılır.
- **Demo verisi:** Her modül kendi demo verisini kendi komutlarıyla yükler; tarihler yükleme gününe göredir; demo her gece sıfırlanır.
- **Sağlayıcı:** Oracle Cloud "Always Free" ARM sunucusu (2 çekirdek, 12 GB RAM); yalnızca ücretsiz kaynaklar; hesap S1 sonunda açılır.
- **Demo erişimi:** Kapalı. Demo verisi rol başına bir hesap içerir; hesaplar istek üzerine verilir, parolalar gizli bilgidir ve sıfırlamada korunur. Demoya özel bir giriş yolu yoktur.
- **Alan adı:** deSEC'in ücretsiz `dedyn.io` alt alan adı.

## Sonuçlar

**Olumlu:**
- Yayında çalışan imaj, migration komutu ve demo verisi her PR'daki uçtan uca testlerde aynen denenir.
- Çalınan bir yayın anahtarı yalnızca belirli bir sürümü yayınlamak için kullanılabilir; sunucuda kabuk açamaz.
- Migration'lar uygulamanın o sürümüyle birebir aynıdır; ayrı paket dosyası üretilmez ve saklanmaz.
- Sağlayıcı koşullarını değiştirirse ortam, betiklerle ve yedeklerden bir saat içinde başka bir sunucuya taşınır.
- Tüm bileşenler ücretsiz ve serbest lisanslıdır (Caddy, WAL-G, Alloy: Apache 2.0).

**Olumsuz / bedeli:**
- Tek sunucu ve tek uygulama örneği vardır; yayın sırasında birkaç saniyelik kesinti olur. Demo için kabul edilir.
- Sunucunun işletim sistemi güncellemeleri ve güvenliği bizim sorumluluğumuzdadır (otomatik güvenlik güncellemeleriyle).
- Chiseled imajda kabuk olmadığı için sağlık kontrolü uygulamanın kendi komutuyla yapılır.
- Demo kapalı olduğu için inceleyen kişinin önce hesap istemesi gerekir. Buna karşılık demoya özel, yanlışlıkla yayın ortamında açılabilecek bir giriş yolu kodda bulunmaz.
- Ücretsiz sağlayıcının koşulları habersiz değişebilir; ortamın taşınabilir olması bu yüzden bir gerekliliktir.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Azure for Students | Kart istemez, ama yıllık 100 $ kredi ve öğrencilik süresiyle sınırlı; bu krediyle alınabilecek sunucu (1–2 GB RAM) uygulama ve veritabanı için sınırda. |
| Kendi bilgisayarında çalıştırıp tünelle yayınlamak | Demo yalnızca bilgisayar açıkken erişilebilir; kalıcı adres için kendi alan adı gerekir. |
| Herkese açık demo, rol seçerek şifresiz giriş | Portfolyoyu inceleyenler için en kolayı; ama yalnızca demoya özel bir giriş yolu gerektirir ve demo verisi herkesçe değiştirilebilir. Kullanıcı kapalı demoyu seçti. |
| Herkese açık demo, ortak şifreyle | Hesap kilidi kuralı (BR-SYS-005) nedeniyle biri bilerek yanlış şifre girip demo hesaplarını kilitleyebilir. |
| Render, Koyeb gibi ücretsiz platformlar | Boştayken uyurlar; zamanlanmış işler ve SignalR çalışmaz, ilk istek bir dakika bekler. Render'ın ücretsiz veritabanı 30 günde silinir; Koyeb 2026'da yeni ücretsiz kaydı kapattı. |
| Kubernetes (k3s dahil) | Tek örnekli bir demo için işletim yükü karşılıksız kalır. |
| Dockerfile ile imaj | SDK'nın yerleşik üretimi aynı sonucu daha az dosyayla verir ve çok mimarili imajı Docker'a gerek kalmadan üretir. |
| Tam `aspnet` (Ubuntu) temel imajı | Kabuk ve paket yöneticisi saldırı yüzeyini büyütür; chiseled-extra gereken ICU ve saat dilimi verisini zaten içerir. |
| EF migration paketi (bundle) | Veritabanı bağlamı başına ayrı dosya: 10 modül × 2 mimari = 20 dosya. Aynı imajdaki komut daha basit ve sürüm uyumsuzluğu riski yok. |
| Uygulama açılışında migration | Uygulama rolüne şema değiştirme yetkisi verilmesi ve birden fazla örneğin aynı anda migration uygulaması riski. |
| Sunucunun yeni sürümü kendisinin çekmesi (pull) | GitHub'da gizli bilgi gerekmez, ama yayın geçmişi ve sonucu GitHub'da görünmez; duman testi ve hata bildirimi ayrıca kurulmalıdır. |
| Sunucuda GitHub'ın kendi çalıştırıcısı (self-hosted runner) | Açık repolarda fork PR'larının sunucuda kod çalıştırma riski nedeniyle GitHub tarafından önerilmiyor. |
