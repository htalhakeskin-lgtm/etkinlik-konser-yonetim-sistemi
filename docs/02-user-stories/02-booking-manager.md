# Booking / prodüksiyon müdürü

[← Kullanıcı hikayeleri](README.md)

## Ana veriler

### US-PTY-001 · Taraf oluşturma
**Booking müdürü olarak** sanatçı, ajans, mekan işletmecisi, tedarikçi ve müşterileri tek yerde kaydetmek **istiyorum**, **çünkü** aynı firma farklı işlerde farklı rollerle karşıma çıkıyor.

Öncelik: Must · Demo adımı: —
Kurallar: BR-PTY-001, BR-PTY-002, BR-PTY-003

**Kabul kriterleri**
1. Taraf türü (kişi / firma), ad ve en az bir taraf rolü zorunludur.
2. Bir tarafın birden fazla rolü olabilir (ör. hem tedarikçi hem müşteri).
3. Birden fazla iletişim bilgisi (telefon, e-posta, adres) eklenebilir; her türde biri birincil olarak işaretlenir.
4. Bir firmaya iletişim kişisi bağlanabilir; iletişim kişisi kendisi de bir kişi tarafıdır.

### US-PTY-002 · Taraf arama, düzenleme ve pasifleştirme
**Booking müdürü olarak** kayıtlı tarafları bulup güncellemek **istiyorum**, **çünkü** iletişim bilgileri sık değişir.

Öncelik: Must · Demo adımı: —
Kurallar: BR-SYS-001

**Kabul kriterleri**
1. Taraflar ad, rol ve iletişim bilgisine göre aranır ve role göre filtrelenir.
2. Başka kayıtların bağlı olduğu taraf silinmez, pasifleştirilir; pasif taraf yeni kayıtlarda seçilemez.
3. Taraftaki her değişiklik işlem geçmişine yazılır.

### US-VEN-001 · Mekan oluşturma
**Booking müdürü olarak** mekanları teknik bilgileriyle kaydetmek **istiyorum**, **çünkü** opsiyon, ihtiyaç hesabı ve ileride güç hesabı bu bilgilere dayanır.

Öncelik: Must · Demo adımı: —
Kurallar: BR-PTY-004

**Kabul kriterleri**
1. Ad, şehir, adres ve kapasite zorunludur.
2. İsteğe bağlı teknik bilgiler: mekan işletmecisi (taraf), sahne genişliği / derinliği / yüksekliği (m), yükleme kapısı açıklaması, güç kapasitesi (A), sessizlik saati.
3. Güç kapasitesi S1'de yalnızca kaydedilir; güç hesabı S2'de kullanır.
4. Mekan ekipmanı teknik müdür tarafından girilir (US-VEN-002).

### US-ART-001 · Sanatçı ve prodüksiyon oluşturma
**Booking müdürü olarak** sanatçıları ve şovlarını kaydetmek **istiyorum**, **çünkü** rider ve etkinlikler prodüksiyona bağlanır.

Öncelik: Must · Demo adımı: —
Kurallar: BR-PTY-004

**Kabul kriterleri**
1. Sanatçı, sanatçı rolündeki bir taraftır; varsa ajansı bağlanır.
2. Bir sanatçının birden fazla prodüksiyonu olabilir (ör. "Akustik set", "Full band").
3. Prodüksiyon adı aynı sanatçı içinde tekildir.

## Etkinlik

### US-EVT-001 · Etkinlik talebi oluşturma
**Booking müdürü olarak** yeni bir etkinlik talebini kaydetmek **istiyorum**, **çünkü** görüşme başladığı andan itibaren etkinliğin tek bir kaydı olmalı.

Öncelik: Must · Demo adımı: 1
Kurallar: BR-PTY-004, BR-EVT-016, BR-MRP-001

**Kabul kriterleri**
1. Ad, etkinlik türü, başlangıç ve bitiş zamanı zorunludur; bitiş başlangıçtan önce olamaz. Etkinlik zamanı, mekanın bizde olduğu süredir (kurulum başlangıcından söküm bitişine). Talep aşamasında yalnızca tarih girilebilir; saat girilmezse başlangıç 00:00, bitiş 23:59 kabul edilir.
2. Etkinlik türü **Kendi etkinliği** ise prodüksiyon, **Teknik hizmet** ise müşteri zorunludur.
3. Mekan ve kaynak depo talep aşamasında boş bırakılabilir.
4. Yeni etkinlik **Talep** durumunda başlar ve durum geçmişine ilk kayıt yazılır.

### US-EVT-002 · Mekandan opsiyon alma
**Booking müdürü olarak** mekanın bize verdiği opsiyonu sırasıyla kaydetmek **istiyorum**, **çünkü** hangi tarihlerde kaçıncı sırada olduğumuzu takip etmeliyim.

Öncelik: Must · Demo adımı: 1
Kurallar: BR-EVT-001, BR-EVT-002, BR-EVT-003, BR-EVT-004, BR-EVT-008

**Kabul kriterleri**
1. Etkinlik **Talep** veya **Opsiyonda** durumundayken bir mekan ve tarih için opsiyon eklenir.
2. Opsiyon sırası, mekanın bildirdiği sıra olarak girilir.
3. Aynı mekan ve tarihteki tüm opsiyonlar (kendi ve dış) sırasıyla gösterilir.
4. Girilen sıradan önceki sıralar sistemde kayıtlı değilse, eksik sıralar için sahibi bilinmeyen dış opsiyonlar otomatik oluşturulur. Örneğin "2. opsiyon" girildiğinde 1. sıraya bir dış opsiyon eklenir.
5. Dış opsiyon elle de eklenebilir; sahibinin adı isteğe bağlı serbest metindir.
6. Opsiyon son tarihi isteğe bağlıdır. Son tarihi yaklaşan (3 gün veya daha az kalan) ve süresi geçmiş opsiyonlar listelerde ve etkinlik sayfasında farklı renklerle işaretlenir. Süresi geçen opsiyon düşmez; mekan süreyi uzatırsa son tarih güncellenir.
7. Bir etkinliğin aynı mekan ve tarih için tek aktif opsiyonu olabilir. Farklı etkinliklerimiz aynı tarih için ayrı opsiyonlar tutabilir.
8. İlk opsiyon eklendiğinde etkinlik **Talep** durumundan **Opsiyonda** durumuna geçer.

### US-EVT-003 · Opsiyonun düşmesi ve yükselme
**Booking müdürü olarak** mekan bir opsiyonu düşürdüğünde sıramızın otomatik güncellenmesini **istiyorum**, **çünkü** sırayı elle takip etmek hataya açık.

Öncelik: Must · Demo adımı: 2
Kurallar: BR-EVT-005, BR-EVT-006, BR-EVT-007

**Kabul kriterleri**
1. Kendi opsiyonumuz ya da bir dış opsiyon, neden seçilerek düşürülür (iptal, süre doldu, mekan başkasına verdi).
2. Düşen opsiyonun arkasındaki tüm opsiyonların sırası birer öne çıkar.
3. Kendi opsiyonumuz 1. sıraya çıktığında etkinlik sayfasında ve etkinlik listesinde görünür bir işaret belirir.
4. Her sıra değişikliği işlem geçmişine eski ve yeni sırayla yazılır.
5. **Opsiyonda** durumundaki bir etkinliğin son aktif opsiyonu düşerse etkinlik **Talep** durumuna döner.

### US-EVT-004 · Müzakere ve onay
**Booking müdürü olarak** etkinliği müzakereye ve ardından onaya taşımak **istiyorum**, **çünkü** hangi etkinliklerin kesinleştiği herkes için net olmalı.

Öncelik: Must · Demo adımı: 3
Kurallar: BR-EVT-001, BR-EVT-009, BR-EVT-015, BR-EVT-019

**Kabul kriterleri**
1. **Kendi etkinliği** **Opsiyonda** durumundan **Müzakere**'ye geçer. **Teknik hizmet** **Talep** ya da **Opsiyonda** durumundan geçebilir: mekanı müşteri tuttuysa opsiyon gerekmez, müşteri adına mekanı biz tutuyorsak opsiyon alınır.
2. **Müzakere**'den **Onaylı**'ya geçişte etkinliğin opsiyonları varsa hepsi 1. sırada olmalı ve hiçbirinin süresi geçmemiş olmalıdır.
3. **Onaylı**'ya geçişte "Sözleşme imzalandı" elle onayı zorunludur; onayı veren kullanıcı kaydedilir. S3'te bu onay sözleşme kontrolüyle değiştirilecek.
4. Kendi etkinliğinin, etkinlik zamanının kapsadığı her gün için opsiyonu olmalıdır. Onaylanınca opsiyonlar kesinleşir ve artık elle düşürülemez.
5. Her geçiş durum geçmişine önceki durum, yeni durum, kullanıcı, zaman ve isteğe bağlı notla yazılır.
6. O an geçerli olmayan geçişler arayüzde sunulmaz. Tam geçiş tablosu [04-state-machines.md](../04-state-machines.md)'de.
7. **Onaylı** ya da **Hazırlık** durumundaki etkinlik, çıkışı yapılmış ekipman yoksa, neden girilerek **Müzakere**'ye geri alınabilir. Kesinleşmiş opsiyonlar yeniden aktif olur, sözleşme onayı geçersiz olur, onaylı rezervasyonlar korunur.

### US-EVT-005 · Hazırlıktan kapanışa ilerletme
**Booking müdürü olarak** onaylı etkinliği operasyon aşamalarından geçirip kapatmak **istiyorum**, **çünkü** etkinliğin hangi aşamada olduğu depo ve teknik ekip için yol gösterici.

Öncelik: Must · Demo adımı: 5, 11, 12
Kurallar: BR-EVT-010, BR-EVT-011, BR-EVT-012, BR-EVT-013, BR-EVT-015

**Kabul kriterleri**
1. **Onaylı** → **Hazırlık** geçişi için mekan, kaynak depo ve bir rider versiyonu tanımlı olmalıdır. Geçişte ihtiyaç hesabı otomatik çalışır (US-MRP-001).
2. **Hazırlık** → **Kurulum** geçişinde çıkışı tamamlanmamış onaylı rezervasyon varsa uyarı gösterilir; kullanıcı onaylarsa geçiş yapılır.
3. **Kurulum** → **Canlı** → **Söküm** → **Hesaplaşma** geçişleri elle yapılır. **Onaylı**'dan **Hesaplaşma**'ya kadarki geçişleri teknik müdür de yapabilir.
4. **Hesaplaşma** → **Kapandı** geçişi için etkinlikten çıkışı yapılıp girişi yapılmamış birim ya da adet kalmamalıdır (kayıp olarak işaretlenenler hariç). Ayrıca "Hesaplaşma onaylandı" elle onayı zorunludur. S4'te bu onay hesaplaşma kontrolüyle değiştirilecek.
5. Kapanışta onaylı rezervasyonlar **Tamamlandı** olur. **Kapandı** durumundaki etkinlik ve bağlı kayıtları değiştirilemez.

### US-EVT-008 · Operasyon geçişlerinin otomatik yapılması
**Booking müdürü olarak** etkinliğin kurulum, canlı, söküm ve hesaplaşma aşamalarına zamanı geldiğinde kendiliğinden geçmesini **istiyorum**, **çünkü** sahadaki ekip yoğunken durumu güncellemeyi unutabiliyor.

Öncelik: Should · Demo adımı: —
Kurallar: BR-EVT-018

**Kabul kriterleri**
1. Her etkinliğin operasyon geçiş modu **Elle** ya da **Otomatik**tir. Yeni etkinlik modu ayarlardaki varsayılandan alır; mod etkinlik sayfasında değiştirilebilir.
2. Kapı açılışı ve söküm başlangıcı zamanları etkinlikte isteğe bağlı olarak girilir.
3. Otomatik modda etkinlik başlangıç zamanında **Kurulum**'a, kapı açılışında **Canlı**'ya, söküm başlangıcında **Söküm**'e, bitiş zamanında **Hesaplaşma**'ya geçer. Girilmemiş zaman noktası atlanır; geçiş bir sonraki zaman noktasında yapılır.
4. Otomatik geçiş yalnızca **Hazırlık** ve sonraki durumlardan ileri doğru yapılır. Başlangıç zamanı geldiği halde **Hazırlık**'a geçmemiş etkinlik listede uyarıyla işaretlenir.
5. Otomatik modda da geçişler elle, zamanından önce yapılabilir.
6. Otomatik geçişler durum geçmişine "sistem" tarafından yapılmış olarak yazılır.

### US-EVT-006 · Etkinliği iptal etme
**Booking müdürü olarak** bir etkinliği herhangi bir aşamada iptal etmek **istiyorum**, **çünkü** iptal edilen etkinlik ayırdığı mekan ve ekipmanı bırakmalı.

Öncelik: Must · Demo adımı: —
Kurallar: BR-EVT-014

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
Kurallar: BR-EVT-006, BR-EVT-008, BR-EVT-017

**Kabul kriterleri**
1. Liste durum, tarih aralığı, etkinlik türü, mekan ve kaynak depoya göre filtrelenir; varsayılan sıralama başlangıç tarihidir.
2. Listede opsiyon sırası, opsiyon süresi işaretleri, açık çakışma sayısı ve "dönmemiş ekipman" işaretleri görünür. Liste, süresi geçmiş opsiyonu olan etkinliklere göre filtrelenebilir.
3. Detay sayfası sekmelerden oluşur: Genel, Opsiyonlar, Rider, İhtiyaç ve rezervasyon, Depo işlemleri, Durum geçmişi.
4. Detay sayfasında geçerli durum geçişleri düğme olarak sunulur.
