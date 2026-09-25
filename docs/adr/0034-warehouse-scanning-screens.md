# ADR-0034: Depo okutma ekranları

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [standards/ui.md §14](../standards/ui.md#14-depo-ekranları), [11 §2.7](../11-screens.md#27-depo-okutma-ekranı-telefon), [ADR-0014](0014-documents-and-qr.md), [ADR-0025](0025-idempotency-keys.md), [00 §7.7.1](../00-scope.md#771-adetli-kalem-yönetimi-ilkeleri), US-WHS-002, US-WHS-003, US-WHS-004, US-WHS-005

## Bağlam

- Depo çıkış, giriş ve transfer işlemleri telefondan, tek elle, çoğu zaman eldivenle, gürültülü ve aydınlığı değişken bir ortamda yapılır.
- Profesyonel depolarda telefon kamerasının yanı sıra tümleşik okuyuculu dayanıklı cihazlar ve Bluetooth el okuyucuları yaygındır.
- Safari, tarayıcının yerleşik barkod okuyucusunu (`BarcodeDetector`) Haziran 2026 itibarıyla desteklemiyor; iPhone'larda WebAssembly tabanlı bir yedek gerekir ([kaynak](https://caniuse.com/mdn-api_barcodedetector)). Titreşim de iOS Safari'de desteklenmiyor. Ekranın kararmasını engelleme (Screen Wake Lock) tüm tarayıcılarda var.
- Arka planda eşitleme (Background Sync) Safari'de yok; iPhone'da uygulama kapalıyken bekleyen bir işlem gönderilemez ([kaynak](https://www.magicbell.com/blog/pwa-ios-limitations-safari-support-complete-guide)).
- Aynı birim iki kez çıkış yapılamaz (BR-WHS-006); bu anlık kontrol sunucuya erişim gerektirir.

## Karar

- **Tek uygulama, ayrı ekranlar:** Depo ekranları aynı web uygulamasının telefon için tasarlanmış ekranlarıdır (`/warehouse/…`). Uygulama ana ekrana eklenebilir (web uygulaması bildirimi).
- **Okutma kaynakları:** Kamera (yerleşik okuyucu; yoksa yalnızca gerektiğinde yüklenen yedek kütüphane), klavye gibi davranan donanım okuyucular ve elle giriş. Okutma alanı her zaman odaktadır; donanım okuyucu kipinde ekran klavyesi açılmaz.
- **Geri bildirim:** 200 ms içinde; üç düzey (başarılı, hafif uyarı, engelleyen hata) ve onay kartı; görsel ve ses asıl, titreşim ek. Sesler tarayıcıda üretilir.
- **Boyutlar:** Her dokunma hedefi en az 56 px; ana işlemler ve +/− düğmeleri 72 px.
- **Ekran açık kalır:** Okutma süresince Screen Wake Lock.
- **Tekrar güvenliği:** Her okutma bir tekrar güvenliği anahtarıyla gönderilir; yanıtı gelmeyen okutma aynı anahtarla yeniden denenir ve sunucu iki kez işlemez.
- **Bağlantı kesildiğinde:** Okutma çevrimiçidir. Yanıtı gelmemiş okutma bağlantı gelince aynı anahtarla kendiliğinden yeniden gönderilir. Bağlantı yokken yeni okutma alınmaz; donanım okuyucudan gelen her okutma "okutma alınmadı" uyarısıyla karşılanır. Çevrimdışı kuyruk S1'de yoktur.

## Sonuçlar

**Olumlu:**
- Telefon, dayanıklı cihaz ve el okuyucusu aynı ekranla çalışır; şirket donanım seçerken uygulamaya bağlı kalmaz.
- Depo çalışanı ekrana bakmadan sesle ve renkle sonucu anlar; hata durumunda iş durur ve gözden kaçmaz.
- Ayrı bir mobil uygulama, mağaza dağıtımı ya da ikinci kod tabanı yoktur.

**Olumsuz / bedeli:**
- iPhone'larda kamera okuması yedek kütüphaneyle, Android'deki yerleşik okuyucudan daha yavaş olabilir; ilk kullanımda kütüphanenin indirilmesi gerekir.
- iOS'ta titreşim yoktur; sesler kullanıcı ekrana ilk dokunduktan sonra çalabilir.
- Web uygulaması, cihazın donanım okuyucusunun gelişmiş ayarlarına (ör. okuma modları) erişemez; okuyucunun `Enter` ekleyecek şekilde ayarlanması gerekir.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Ayrı yerel mobil uygulama (React Native, .NET MAUI) | Kamera ve donanım okuyucuya tam erişim sağlar; ama ikinci bir kod tabanı, mağaza dağıtımı ve güncelleme süreci getirir. Web uygulaması S1'in ihtiyaçlarını karşılıyor. |
| Ticari okutma kütüphaneleri (Scandit, Dynamsoft) | Hızlı ve güvenilir; ama ücretli. |
| html5-qrcode | Bakımı zayıfladı ([ADR-0014](0014-documents-and-qr.md)). |
| Yalnızca kamera | Profesyonel depolarda yaygın olan donanım okuyucular dışarıda kalır; oysa desteklemenin ek maliyeti yoktur. |
| Kısa süreli çevrimdışı okutma kuyruğu | Bağlantı yokken çalışmaya devam edilir; ama çift çıkış engeli çevrimdışıyken çalışmaz, okutmalar sonradan reddedilebilir ve iPhone'da arka planda gönderim yoktur. Tekrar güvenliği anahtarları sayesinde ileride sunucu değişmeden eklenebilir. |
| Tam çevrimdışı mod (liste ve stoğun telefona indirilmesi) | En dayanıklısı; ama yerel veri, eşitleme ve çakışma çözümü S1 kapsamını büyütür. |
| Standart mobil boyutlar (44–48 px) | Eldivenle kullanımda yanlış dokunma artar; araştırmalar eldiven için en az 11,4 mm öneriyor ([kaynak](https://www.w3.org/WAI/GL/mobile-a11y-tf/wiki/Summary_of_Research_on_Touch/Pointer_Target_Size)). |
