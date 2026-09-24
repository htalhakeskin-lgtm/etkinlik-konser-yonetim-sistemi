# Booking / prodüksiyon müdürü

[← Kullanıcı hikayeleri](README.md)

## Ana veriler

### US-PTY-001 · Taraf oluşturma
**Booking müdürü olarak** sanatçı, ajans, mekan işletmecisi, tedarikçi ve müşterileri tek yerde kaydetmek **istiyorum**, **çünkü** aynı firma farklı işlerde farklı rollerle karşıma çıkıyor.

Öncelik: Must · Demo adımı: —

**Kabul kriterleri**
1. Taraf türü (kişi / firma), ad ve en az bir taraf rolü zorunludur.
2. Bir tarafın birden fazla rolü olabilir (ör. hem tedarikçi hem müşteri).
3. Birden fazla iletişim bilgisi (telefon, e-posta, adres) eklenebilir; her türde biri birincil olarak işaretlenir.
4. Bir firmaya iletişim kişisi bağlanabilir; iletişim kişisi kendisi de bir kişi tarafıdır.

### US-PTY-002 · Taraf arama, düzenleme ve pasifleştirme
**Booking müdürü olarak** kayıtlı tarafları bulup güncellemek **istiyorum**, **çünkü** iletişim bilgileri sık değişir.

Öncelik: Must · Demo adımı: —

**Kabul kriterleri**
1. Taraflar ad, rol ve iletişim bilgisine göre aranır ve role göre filtrelenir.
2. Başka kayıtların bağlı olduğu taraf silinmez, pasifleştirilir; pasif taraf yeni kayıtlarda seçilemez.
3. Taraftaki her değişiklik işlem geçmişine yazılır.

### US-VEN-001 · Mekan oluşturma
**Booking müdürü olarak** mekanları teknik bilgileriyle kaydetmek **istiyorum**, **çünkü** opsiyon, ihtiyaç hesabı ve ileride güç hesabı bu bilgilere dayanır.

Öncelik: Must · Demo adımı: —

**Kabul kriterleri**
1. Ad, şehir, adres ve kapasite zorunludur.
2. İsteğe bağlı teknik bilgiler: mekan işletmecisi (taraf), sahne genişliği / derinliği / yüksekliği (m), yükleme kapısı açıklaması, güç kapasitesi (A), sessizlik saati.
3. Güç kapasitesi S1'de yalnızca kaydedilir; güç hesabı S2'de kullanır.
4. Mekan ekipmanı teknik müdür tarafından girilir (US-VEN-002).

### US-ART-001 · Sanatçı ve prodüksiyon oluşturma
**Booking müdürü olarak** sanatçıları ve şovlarını kaydetmek **istiyorum**, **çünkü** rider ve etkinlikler prodüksiyona bağlanır.

Öncelik: Must · Demo adımı: —

**Kabul kriterleri**
1. Sanatçı, sanatçı rolündeki bir taraftır; varsa ajansı bağlanır.
2. Bir sanatçının birden fazla prodüksiyonu olabilir (ör. "Akustik set", "Full band").
3. Prodüksiyon adı aynı sanatçı içinde tekildir.

## Etkinlik

### US-EVT-001 · Etkinlik talebi oluşturma
**Booking müdürü olarak** yeni bir etkinlik talebini kaydetmek **istiyorum**, **çünkü** görüşme başladığı andan itibaren etkinliğin tek bir kaydı olmalı.

Öncelik: Must · Demo adımı: 1

**Kabul kriterleri**
1. Ad, etkinlik türü, başlangıç ve bitiş tarihi zorunludur; bitiş başlangıçtan önce olamaz.
2. Etkinlik türü **Kendi etkinliği** ise prodüksiyon, **Teknik hizmet** ise müşteri zorunludur.
3. Mekan ve kaynak depo talep aşamasında boş bırakılabilir.
4. Yeni etkinlik **Talep** durumunda başlar ve durum geçmişine ilk kayıt yazılır.

### US-EVT-002 · Mekandan opsiyon alma
**Booking müdürü olarak** mekanın bize verdiği opsiyonu sırasıyla kaydetmek **istiyorum**, **çünkü** hangi tarihlerde kaçıncı sırada olduğumuzu takip etmeliyim.

Öncelik: Must · Demo adımı: 1

**Kabul kriterleri**
1. Etkinlik **Talep** veya **Opsiyonda** durumundayken bir mekan ve tarih için opsiyon eklenir.
2. Opsiyon sırası, mekanın bildirdiği sıra olarak girilir.
3. Aynı mekan ve tarihteki tüm opsiyonlar (kendi ve dış) sırasıyla gösterilir.
4. Girilen sıradan önceki sıralar sistemde kayıtlı değilse, eksik sıralar için sahibi bilinmeyen dış opsiyonlar otomatik oluşturulur. Örneğin "2. opsiyon" girildiğinde 1. sıraya bir dış opsiyon eklenir.
5. Dış opsiyon elle de eklenebilir; sahibinin adı isteğe bağlı serbest metindir.
6. Opsiyon son tarihi isteğe bağlıdır; son tarihine 3 gün veya daha az kalan opsiyonlar listelerde vurgulanır.
7. Bir etkinliğin aynı mekan ve tarih için tek aktif opsiyonu olabilir. Farklı etkinliklerimiz aynı tarih için ayrı opsiyonlar tutabilir.
8. İlk opsiyon eklendiğinde etkinlik **Talep** durumundan **Opsiyonda** durumuna geçer.

### US-EVT-003 · Opsiyonun düşmesi ve yükselme
**Booking müdürü olarak** mekan bir opsiyonu düşürdüğünde sıramızın otomatik güncellenmesini **istiyorum**, **çünkü** sırayı elle takip etmek hataya açık.

Öncelik: Must · Demo adımı: 2

**Kabul kriterleri**
1. Kendi opsiyonumuz ya da bir dış opsiyon, neden seçilerek düşürülür (iptal, süre doldu, mekan başkasına verdi).
2. Düşen opsiyonun arkasındaki tüm opsiyonların sırası birer öne çıkar.
3. Kendi opsiyonumuz 1. sıraya çıktığında etkinlik sayfasında ve etkinlik listesinde görünür bir işaret belirir.
4. Her sıra değişikliği işlem geçmişine eski ve yeni sırayla yazılır.
5. **Opsiyonda** durumundaki bir etkinliğin son aktif opsiyonu düşerse etkinlik **Talep** durumuna döner.

### US-EVT-004 · Müzakere ve onay
**Booking müdürü olarak** etkinliği müzakereye ve ardından onaya taşımak **istiyorum**, **çünkü** hangi etkinliklerin kesinleştiği herkes için net olmalı.

Öncelik: Must · Demo adımı: 3

**Kabul kriterleri**
1. **Kendi etkinliği** **Opsiyonda** durumundan, **Teknik hizmet** **Talep** durumundan **Müzakere**'ye geçer. Teknik hizmette mekan müşterinin olduğu için opsiyon gerekmez.
2. **Müzakere**'den **Onaylı**'ya geçişte kendi etkinliğinin mekan opsiyonu 1. sırada olmalıdır.
3. **Onaylı**'ya geçişte "Sözleşme imzalandı" elle onayı zorunludur; onayı veren kullanıcı kaydedilir. S3'te bu onay sözleşme kontrolüyle değiştirilecek.
4. Her geçiş durum geçmişine önceki durum, yeni durum, kullanıcı, zaman ve isteğe bağlı notla yazılır.
5. O an geçerli olmayan geçişler arayüzde sunulmaz. Tam geçiş tablosu `04-state-machines.md`'de tanımlanacak.

### US-EVT-005 · Hazırlıktan kapanışa ilerletme
**Booking müdürü olarak** onaylı etkinliği operasyon aşamalarından geçirip kapatmak **istiyorum**, **çünkü** etkinliğin hangi aşamada olduğu depo ve teknik ekip için yol gösterici.

Öncelik: Must · Demo adımı: 5, 11, 12

**Kabul kriterleri**
1. **Onaylı** → **Hazırlık** geçişi için mekan, kaynak depo ve bir rider versiyonu tanımlı olmalıdır. Geçişte ihtiyaç hesabı otomatik çalışır (US-MRP-001).
2. **Hazırlık** → **Kurulum** geçişinde çıkışı tamamlanmamış onaylı rezervasyon varsa uyarı gösterilir; kullanıcı onaylarsa geçiş yapılır.
3. **Kurulum** → **Canlı** → **Söküm** → **Hesaplaşma** geçişleri elle yapılır.
4. **Hesaplaşma** → **Kapandı** geçişi için etkinlikten çıkışı yapılıp girişi yapılmamış birim ya da adet kalmamalıdır (kayıp olarak işaretlenenler hariç). Ayrıca "Hesaplaşma onaylandı" elle onayı zorunludur. S4'te bu onay hesaplaşma kontrolüyle değiştirilecek.
5. **Kapandı** durumundaki etkinlik ve bağlı kayıtları değiştirilemez.

### US-EVT-006 · Etkinliği iptal etme
**Booking müdürü olarak** bir etkinliği herhangi bir aşamada iptal etmek **istiyorum**, **çünkü** iptal edilen etkinlik ayırdığı mekan ve ekipmanı bırakmalı.

Öncelik: Must · Demo adımı: —

**Kabul kriterleri**
1. **Kapandı** dışındaki her durumdan iptal edilebilir; iptal nedeni zorunludur.
2. Etkinliğin aktif opsiyonları düşer ve arkadaki opsiyonlar yükselir (US-EVT-003).
3. Etkinliğin önerilen ve onaylı rezervasyonları serbest bırakılır.
4. Etkinlik için **Planlandı** durumundaki transferler iptal edilir; **Yolda** olanlar tamamlanmaya devam eder.
5. **Taslak** dış kiralama siparişleri iptal edilir. **Sipariş verildi** durumundakiler için tedarikçiye elle haber verilmesi gerektiği uyarısı gösterilir.
6. Çıkışı yapılmış ekipman varsa iptal yine yapılır. Giriş işlemi açık kalır ve etkinlik listesinde "dönmemiş ekipman" işaretiyle görünür.

### US-EVT-007 · Etkinlik listesi ve detay sayfası
**Booking müdürü olarak** tüm etkinlikleri filtreleyip bir etkinliğin her bilgisine tek sayfadan ulaşmak **istiyorum**, **çünkü** aynı anda onlarca etkinliği takip ediyorum.

Öncelik: Must · Demo adımı: —

**Kabul kriterleri**
1. Liste durum, tarih aralığı, etkinlik türü, mekan ve kaynak depoya göre filtrelenir; varsayılan sıralama başlangıç tarihidir.
2. Listede opsiyon sırası, açık çakışma sayısı ve "dönmemiş ekipman" işaretleri görünür.
3. Detay sayfası sekmelerden oluşur: Genel, Opsiyonlar, Rider, İhtiyaç ve rezervasyon, Depo işlemleri, Durum geçmişi.
4. Detay sayfasında geçerli durum geçişleri düğme olarak sunulur.
