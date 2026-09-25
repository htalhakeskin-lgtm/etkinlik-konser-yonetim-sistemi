# ADR-0031: Yedekleme ve zamana göre geri dönüş

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [09 §8](../09-environments-and-deployment.md#8-veritabanı), [10 §4–5](../10-operations.md#4-yedekten-geri-dönüş), [standards/database.md §17](../standards/database.md#17-bağlantı-ve-işletim-ayarları)

## Bağlam

- Bir ERP'nin verisi şirketin kaydıdır: işlem geçmişi ve stok hareketleri silinemez ve değiştirilemez olarak tasarlandı ([ADR-0021](0021-database-roles-per-module.md)). Bu kayıtların disk arızasında ya da hatalı bir yayında kaybolmaması gerekir.
- Hatalı bir yayın ya da yanlış bir toplu işlem veriyi bozduğunda, bozulmadan hemen önceki ana dönebilmek gerekir. Günlük döküm (`pg_dump`) bir günlük veri kaybına izin verir ve belirli bir ana dönemez.
- Yedeğin çalıştığı ancak geri yüklenerek kanıtlanır.
- 2026'da PostgreSQL yedekleme araçlarından pgBackRest tek bakımcısının bırakmasıyla kısa bir süre sahipsiz kaldı, sonra bir sponsor grubuyla kurtarıldı ([kaynak](https://www.theregister.com/databases/2026/05/20/postgresql-backup-tool-gets-some-backup-of-its-own-after-sole-maintainer-sounds-alarm/5242822)). Barman GPL 3 lisanslıdır.

## Karar

- **Araç:** WAL-G (Apache 2.0).
- **Yöntem:** Sürekli WAL arşivleme ve gece temel yedek (haftada bir tam, arada artımlı), S3 uyumlu nesne deposuna, sunucudan çıkmadan önce libsodium ile şifreli.
- **Hedefler:** Veri kaybı en fazla 5 dakika (demo) ve 1 dakika (yayın); yeniden hizmete dönme 1 saat; saklama 7 gün (demo) ve 30 gün (yayın). Yayında yedekler sunucudan farklı bir sağlayıcıda ya da bölgede durur.
- **Geri dönüş noktaları:** Yayın betiği her yayından önce adlandırılmış bir geri dönüş noktası oluşturur.
- **Tatbikat:** Her hafta son yedek geçici bir konteynere otomatik geri yüklenir ve denetlenir. Temel yedeğin ve tatbikatın sonucu ölçüm olarak izlenir; gelmezse uyarı üretilir.
- **Sunucu dışı kopya:** Şifreleme anahtarı ve nesne deposu anahtarları bir parola yöneticisinde de saklanır.

## Sonuçlar

**Olumlu:**
- Veritabanı saklama süresi içindeki herhangi bir ana ya da bir yayından hemen önceki duruma döndürülebilir.
- Yedeğin açılabildiği her hafta kanıtlanır; bozuk ya da eksik yedek, ihtiyaç anında değil tatbikatta fark edilir.
- Yedekler nesne deposunda şifreli durur; depo erişimi sızsa bile veri okunamaz.

**Olumsuz / bedeli:**
- Şifreleme anahtarı kaybolursa yedekler açılamaz; anahtarın sunucu dışında saklanması zorunludur.
- Nesne deposuna erişim kesilirse WAL dosyaları diskte birikir; bunun için ayrıca bir uyarı gerekir.
- Demo'da yedekler sunucuyla aynı sağlayıcıda durur. Demo verisi repodan yeniden üretilebildiği için bu kabul edilir; yayında izin verilmez.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Gece `pg_dump` | Bir günlük veri kaybı; belirli bir ana dönülemez; büyük veritabanında yavaş geri yükleme. |
| pgBackRest | Özellikleri güçlü ve lisansı uygun (MIT); ama 2026'daki bakım krizi sürdürülebilirlik sorusu doğurdu. WAL-G daha basit ve aktif olarak geliştiriliyor. Gerekirse yöntem değişmeden geçilebilir. |
| Barman | GPL 3 lisanslı ve ayrı bir yedek sunucusu varsayar. |
| Sağlayıcının disk anlık görüntüleri | Sağlayıcıya bağımlı; belirli bir ana dönemez; veritabanı tutarlılığı ayrıca sağlanmalı. |
| Yönetilen PostgreSQL hizmeti | Ücretsiz planları küçük ve süreli; modül başına roller, yerleşik sıralama ve eklentiler her hizmette desteklenmiyor. |
