# Tüm kullanıcılar

[← Kullanıcı hikayeleri](README.md)

### US-SYS-010 · Giriş ve çıkış
**Kullanıcı olarak** e-posta ve şifremle sisteme girmek **istiyorum**, **çünkü** yalnızca yetkili çalışanlar şirket verisine erişebilmeli.

Öncelik: Must · Demo adımı: —

**Kabul kriterleri**
1. Kullanıcı e-posta ve şifreyle giriş yapar; hatalı bilgide hangi alanın yanlış olduğu söylenmez.
2. Aynı hesapla art arda 5 hatalı denemeden sonra hesap 15 dakika kilitlenir.
3. Pasif kullanıcı giriş yapamaz.
4. Sistem yöneticisinin verdiği geçici şifreyle ilk girişte kullanıcı yeni şifre belirlemeden devam edemez.
5. Oturum 12 saat hareketsizlikten sonra sona erer. Depo ekranları bir vardiya boyunca yeniden giriş istemez.
6. Kullanıcı her ekrandan çıkış yapabilir; çıkıştan sonra geri tuşuyla korumalı sayfalar açılmaz.

### US-SYS-011 · Şifre değiştirme
**Kullanıcı olarak** şifremi değiştirmek **istiyorum**, **çünkü** şifremin güvenliğinden ben sorumluyum.

Öncelik: Should · Demo adımı: —

**Kabul kriterleri**
1. Değişiklik için mevcut şifre istenir.
2. Yeni şifre en az 10 karakterdir ve mevcut şifreyle aynı olamaz.
3. Şifre değişince kullanıcının diğer cihazlardaki oturumları sonlanır.

### US-SYS-012 · Role göre arayüz
**Kullanıcı olarak** yalnızca yetkim olan ekranları ve işlemleri görmek **istiyorum**, **çünkü** yapamayacağım işlemlerin düğmeleri kafa karıştırır.

Öncelik: Must · Demo adımı: —

**Kabul kriterleri**
1. Menüde yalnızca kullanıcının rollerinden en az birinin erişebildiği ekranlar görünür.
2. Yetkisi olmayan işlemlerin düğmeleri gösterilmez.
3. Yetkisiz bir işlem arayüz dışından (doğrudan API çağrısıyla) denenirse reddedilir.
4. Birden fazla rolü olan kullanıcı, rollerinin tüm yetkilerinin birleşimine sahiptir.
