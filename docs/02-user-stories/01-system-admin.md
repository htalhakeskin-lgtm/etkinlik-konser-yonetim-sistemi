# Sistem yöneticisi

[← Kullanıcı hikayeleri](README.md)

### US-SYS-001 · Kullanıcı oluşturma
**Sistem yöneticisi olarak** yeni bir çalışan için kullanıcı açıp rol atamak **istiyorum**, **çünkü** her çalışan yalnızca işinin gerektirdiği yetkilerle çalışmalı.

Öncelik: Must · Demo adımı: —
Kurallar: BR-SYS-006, BR-SYS-014

**Kabul kriterleri**
1. Ad soyad, e-posta ve en az bir rol zorunludur.
2. E-posta sistemde tekildir; pasif kullanıcıların e-postası da tekrar kullanılamaz.
3. Depo sorumlusu rolü seçilirse en az bir bağlı depo seçmek zorunludur.
4. Kayıt sonunda geçici şifre üretilir ve yalnızca bir kez ekranda gösterilir.
5. Kullanıcının rolleri ve bağlı depoları sonradan değiştirilebilir; değişiklik işlem geçmişine yazılır.

### US-SYS-002 · Kullanıcıyı pasifleştirme ve şifre sıfırlama
**Sistem yöneticisi olarak** ayrılan çalışanın erişimini kapatmak ve şifresini unutanın şifresini sıfırlamak **istiyorum**, **çünkü** erişimi kontrol etmek benim sorumluluğum.

Öncelik: Must · Demo adımı: —
Kurallar: BR-SYS-001, BR-SYS-006, BR-SYS-008, BR-SYS-009

**Kabul kriterleri**
1. Kullanıcılar silinmez, pasifleştirilir. Pasif kullanıcının adı geçmiş kayıtlarda görünmeye devam eder.
2. Pasifleştirilen kullanıcının açık oturumları hemen sona erer.
3. Sistem yöneticisi kendini pasifleştiremez; son aktif sistem yöneticisi pasifleştirilemez.
4. Şifre sıfırlamada yeni geçici şifre üretilir ve bir kez gösterilir; kullanıcı ilk girişte değiştirmek zorundadır.

### US-SYS-003 · Rol ve yetki matrisini görme
**Sistem yöneticisi olarak** her rolün hangi işlemleri yapabildiğini tek tabloda görmek **istiyorum**, **çünkü** bir çalışana hangi rolü vereceğime karar vermeliyim.

Öncelik: Could · Demo adımı: —
Kurallar: —

**Kabul kriterleri**
1. Roller sütunlarda, yetkiler satırlarda, modül bazında gruplanmış gösterilir.
2. Roller S1'de sabittir; tablo salt okunurdur.

### US-SYS-005 · Depo tanımlama
**Sistem yöneticisi olarak** şirketin depolarını tanımlamak **istiyorum**, **çünkü** tüm stok ve depo işlemleri bir depoya bağlıdır.

Öncelik: Must · Demo adımı: —
Kurallar: BR-SYS-001, BR-SYS-013

**Kabul kriterleri**
1. Depo adı, şehir ve adres zorunludur; depo adı tekildir.
2. İçinde stok, açık transfer ya da açık rezervasyon bulunan depo pasifleştirilemez.
3. Sistemde her zaman en az bir aktif depo bulunur.

### US-SYS-007 · Varsayılan hazırlık ve dönüş payı
**Sistem yöneticisi olarak** yeni etkinliklere gelecek varsayılan hazırlık ve dönüş payını ayarlardan değiştirmek **istiyorum**, **çünkü** şirketin çalışma temposu zamanla değişir.

Öncelik: Should · Demo adımı: —
Kurallar: BR-MRP-001

**Kabul kriterleri**
1. Varsayılan paylar ayarlar ekranında gün ve saat olarak girilir; başlangıç değerleri 1'er gündür.
2. Değişiklik yalnızca bundan sonra oluşturulan etkinlikleri etkiler; mevcut etkinliklerin payları değişmez.
3. Değişiklik işlem geçmişine yazılır.
