# Genel müdür

[← Kullanıcı hikayeleri](README.md)

### US-SYS-006 · Salt okunur tam erişim
**Genel müdür olarak** S1'deki tüm ekranları görmek ama hiçbir şeyi yanlışlıkla değiştirmemek **istiyorum**, **çünkü** işleri takip ediyorum ama operasyonu ekip yürütüyor.

Öncelik: Must · Demo adımı: —
Kurallar: BR-SYS-002, BR-SYS-004

**Kabul kriterleri**
1. Genel müdür S1'in tüm liste, detay ve rapor ekranlarını görür.
2. Hiçbir ekranda değiştirme, onaylama ya da silme düğmesi görmez; bu işlemler API tarafında da reddedilir.
3. Genel müdüre ayrıca başka bir rol verilirse o rolün yetkileri de geçerli olur.

### US-SYS-004 · İşlem geçmişi
**Genel müdür olarak** hangi kaydı kimin, ne zaman, nasıl değiştirdiğini görmek **istiyorum**, **çünkü** bir sorun çıktığında ne olduğunu kanıtıyla bilmeliyim.

Öncelik: Must · Demo adımı: 14
Kurallar: BR-SYS-010

**Kabul kriterleri**
1. Her kaydın detay sayfasında o kaydın geçmişi görünür.
2. Genel işlem geçmişi ekranı kullanıcı, tarih aralığı ve kayıt türüne göre filtrelenir.
3. Her satırda kullanıcı, zaman, işlem ve değişen alanların eski ve yeni değerleri bulunur.
4. İşlem geçmişi kimse tarafından değiştirilemez ya da silinemez.
5. İşlem geçmişini genel müdür ve sistem yöneticisi görür.

### US-EQP-009 · Adetli kalem kayıp görünümü
**Genel müdür olarak** en çok kaybolan adetli kalemleri görmek **istiyorum**, **çünkü** kablo ve klemp kayıpları fark edilmeden ciddi maliyete dönüşüyor.

Öncelik: Should · Demo adımı: —
Kurallar: BR-EQP-010

**Kabul kriterleri**
1. Seçilen dönemde (varsayılan son 90 gün) adetli modeller için kaybolan adet ve kayıp oranı (kaybolan / çıkış yapılan) gösterilir.
2. Kayıp oranı en yüksek 10 model öne çıkarılır.
3. Sonuç depo ve etkinlik bazında kırılabilir.
4. Bu ekranı teknik müdür de görür.
