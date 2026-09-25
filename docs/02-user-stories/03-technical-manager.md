# Teknik müdür

[← Kullanıcı hikayeleri](README.md)

## Katalog ve mekan ekipmanı

### US-EQP-001 · Ekipman kategorileri
**Teknik müdür olarak** ekipmanı hiyerarşik kategorilere ayırmak **istiyorum**, **çünkü** rider'lar çoğu zaman belirli bir model değil, bir kategori ister.

Öncelik: Must · Demo adımı: —
Kurallar: BR-SYS-001, BR-EQP-002

**Kabul kriterleri**
1. Kategori bir üst kategoriye bağlanabilir (ör. Ses › Mikrofon › Dinamik vokal).
2. Kategori adı aynı üst kategori içinde tekildir.
3. Altında model veya kategori bulunan kategori silinemez, pasifleştirilir.

### US-EQP-002 · Ekipman modeli oluşturma
**Teknik müdür olarak** ekipman kataloğunu gerçek teknik değerleriyle tutmak **istiyorum**, **çünkü** ihtiyaç hesabı, ileride güç ve yükleme hesapları bu değerlere dayanır.

Öncelik: Must · Demo adımı: —
Kurallar: BR-EQP-001

**Kabul kriterleri**
1. Marka, model adı, kategori ve takip tipi (**Seri no'lu** / **Adetli**) zorunludur.
2. Marka ve model adı birlikte tekildir.
3. İsteğe bağlı değerler: ağırlık (kg), güç tüketimi (W), taşıma hacmi (m³).
4. Modelin stoğu oluştuktan sonra takip tipi değiştirilemez.

### US-EQP-005 · Kit tanımlama
**Teknik müdür olarak** sık birlikte kullanılan ekipmanı kit olarak tanımlamak **istiyorum**, **çünkü** "küçük sahne ışık paketi" gibi setleri her seferinde satır satır girmek istemiyorum.

Öncelik: Should · Demo adımı: —
Kurallar: BR-EQP-003

**Kabul kriterleri**
1. Kit bir ad ve satırlardan oluşur; her satır bir model ya da başka bir kit ve adettir.
2. Bir kit doğrudan ya da dolaylı olarak kendini içeremez.
3. Kitin toplam ağırlığı ve güç tüketimi içindeki modellerden hesaplanıp gösterilir.

### US-VEN-002 · Mekan ekipmanını girme
**Teknik müdür olarak** mekanın kendi ekipmanını kaydetmek **istiyorum**, **çünkü** mekanın verdiği ekipmanı depodan götürmemeliyiz.

Öncelik: Must · Demo adımı: 5
Kurallar: BR-VEN-001, BR-VEN-002, BR-MRP-003, BR-MRP-010

**Kabul kriterleri**
1. Her satır bir model ya da kategori ve adettir.
2. Katalogda olmayan ekipman serbest açıklamayla girilebilir; bu satırlar ihtiyaç hesabında kullanılmaz ve ekranda ayrıca belirtilir.
3. Her satıra isteğe bağlı geçerlilik başlangıcı ve bitişi girilir (ör. mekan yeni bir konsol aldı ya da eski birini elden çıkardı). Elden çıkan ekipman silinmez, geçerlilik bitişi girilir.
4. Bir satır için geçici kullanılamama dönemi girilebilir: tarih aralığı, kullanılamayan adet ve neden (ör. mekan o tarihlerde ekipmanı başka etkinliğe verdi).
5. Mekan sayfasında seçilen bir tarih aralığında kullanılabilir mekan ekipmanı görüntülenebilir.
6. İhtiyaç hesabı, etkinlik zamanının tamamında geçerli ve kullanılabilir olan adedi kullanır.
7. Mekan ekipmanındaki değişiklik, zamanı bu değişiklikle örtüşen **Onaylı**, **Hazırlık** ve **Kurulum** durumundaki etkinliklerde "ihtiyaç hesabı güncel değil" uyarısı oluşturur.

## Rider

### US-RDR-001 · Prodüksiyon rider'ı girme
**Teknik müdür olarak** sanatçıdan gelen rider'ı prodüksiyona girmek **istiyorum**, **çünkü** ekipman ihtiyacı rider'dan hesaplanır.

Öncelik: Must · Demo adımı: 4
Kurallar: BR-RDR-001, BR-RDR-002, BR-RDR-008

**Kabul kriterleri**
1. Her rider satırında bir model ya da kategori (biri zorunlu), adet (en az 1), satır esnekliği ve isteğe bağlı not bulunur.
2. **Zorunlu** satıra muadil girilemez. **Esnek** satıra bir veya daha fazla muadil model sırasıyla girilebilir.
3. İlk kayıtta versiyon 1 oluşur.
4. Booking müdürü rider'ı görür ama değiştiremez.

### US-RDR-002 · Rider'ı güncelleme ve versiyonları karşılaştırma
**Teknik müdür olarak** sanatçı rider'ını güncellediğinde yeni versiyon oluşturmak ve eskisiyle karşılaştırmak **istiyorum**, **çünkü** sanatçılar rider'ı sık değiştirir ve neyin değiştiğini görmeliyim.

Öncelik: Must · Demo adımı: 4
Kurallar: BR-RDR-003, BR-RDR-005

**Kabul kriterleri**
1. Rider'da yapılan her kayıt yeni bir versiyon oluşturur; eski versiyonlar değiştirilemez.
2. İki versiyon seçilince eklenen, silinen ve değişen satırlar gösterilir. (Should)
3. Yeni versiyon oluşunca eski versiyona bağlı ve **Kapandı** olmayan etkinliklerde "yeni rider versiyonu var" işareti belirir.

### US-RDR-003 · Etkinliğe rider versiyonu bağlama
**Teknik müdür olarak** etkinliğe prodüksiyonun belirli bir rider versiyonunu bağlamak **istiyorum**, **çünkü** rider sonradan değişse de etkinliğin hangi rider'la planlandığı sabit kalmalı.

Öncelik: Must · Demo adımı: 4
Kurallar: BR-RDR-004, BR-MRP-010

**Kabul kriterleri**
1. Kendi etkinliğine, prodüksiyonunun versiyonlarından biri bağlanır; varsayılan en güncel versiyondur.
2. Bağlı versiyon değiştirilebilir. Etkinlik **Hazırlık** veya sonraki bir durumdaysa ihtiyaç hesabının yeniden çalıştırılması gerektiği uyarısı gösterilir.

### US-RDR-004 · Etkinliğe özel rider versiyonu
**Teknik müdür olarak** rider'ı yalnızca bir etkinlik için değiştirmek **istiyorum**, **çünkü** mekan koşulları yüzünden sanatçıyla o etkinliğe özel değişiklik üzerinde anlaşabiliyoruz.

Öncelik: Should · Demo adımı: —
Kurallar: BR-RDR-006

**Kabul kriterleri**
1. Etkinliğe bağlı versiyondan etkinliğe özel yeni bir versiyon türetilir ve etkinliğe bağlanır.
2. Etkinliğe özel versiyon yalnızca o etkinlikte görünür, prodüksiyonun versiyon listesinde görünmez.
3. Hangi versiyondan türetildiği gösterilir ve iki versiyon karşılaştırılabilir.

### US-RDR-005 · Teknik hizmet ihtiyaç listesi
**Teknik müdür olarak** teknik hizmet müşterisinin ihtiyaçlarını rider ile aynı yapıda girmek **istiyorum**, **çünkü** ihtiyaç hesabı ve rezervasyon iki gelir kolunda da aynı şekilde çalışmalı.

Öncelik: Must · Demo adımı: —
Kurallar: BR-RDR-001, BR-RDR-007, BR-MRP-004

**Kabul kriterleri**
1. Teknik hizmet etkinliğinde rider, arayüzde "İhtiyaç listesi" adıyla etkinliğin kendisine bağlı oluşturulur.
2. Satır yapısı, versiyonlama ve ihtiyaç hesabı prodüksiyon rider'ıyla aynıdır.
3. İhtiyaç listesinde bir satır kit olabilir; ihtiyaç hesabında kit, içindeki modellere açılır.

## İhtiyaç hesabı ve rezervasyon

### US-MRP-001 · İhtiyaç hesabı
**Teknik müdür olarak** rider'dan net ekipman ihtiyacını ve bunun nereden karşılanacağını otomatik görmek **istiyorum**, **çünkü** bunu elle hesaplamak hem uzun sürüyor hem de hataya açık.

Öncelik: Must · Demo adımı: 5, 6, 7, 9
Kurallar: BR-VEN-001, BR-EVT-010, BR-MRP-001, BR-MRP-002, BR-MRP-003, BR-MRP-004, BR-MRP-005, BR-MRP-009, BR-MRP-010, BR-MRP-011, BR-MRP-012, BR-MRP-019

**Kabul kriterleri**
1. Hesap, etkinlik **Hazırlık** durumuna geçerken otomatik çalışır. **Onaylı**, **Hazırlık** ve **Kurulum** durumlarında elle çalıştırılabilir.
2. Her rider satırı için brüt ihtiyaç, mekan ekipmanından karşılanan adet, net ihtiyaç ve net ihtiyacın kaynaklara dağılımı gösterilir.
3. **Zorunlu** satır yalnızca istenen modelle karşılanır. **Esnek** satır önce istenen modelle, sonra muadillerle sırasıyla karşılanır. Kategori satırı o kategori ve alt kategorilerindeki herhangi bir modelle karşılanır.
4. Mekan ekipmanı, 3. maddedeki eşleşme kurallarıyla brüt ihtiyaçtan önce düşülür.
5. Net ihtiyaç sırasıyla kaynak depodan, sonra diğer depolardan (transfer önerisi), en son dış kiralama önerisiyle karşılanır.
6. Müsaitlik, rezervasyon aralığı (etkinliğin başlangıç ve bitiş zamanı, hazırlık payı ve dönüş payı) üzerinden, saat hassasiyetinde hesaplanır. Başka etkinliklerin onaylı rezervasyonları düşülür. **Bakımda**, **Kayıp** ve **Hurda** birimler sayılmaz. Yoldaki transferler planlanan varış tarihinden itibaren hedef depoda sayılır.
7. Yeniden hesaplamada **Önerildi** rezervasyonlar silinip yeniden üretilir, **Onaylandı** rezervasyonlar korunur. Yeni net ihtiyacı aşan onaylı rezervasyonlar "fazla" olarak işaretlenir.
8. Her hesabın zamanı, çalıştıran kullanıcı ve kullanılan rider versiyonu kaydedilir.
9. 50 satırlık bir rider ve 5.000 birimlik stokla hesap 1 saniyeden kısa sürer.

### US-MRP-002 · Rezervasyon önerisini onaylama
**Teknik müdür olarak** ihtiyaç hesabının önerdiği rezervasyonları kontrol edip onaylamak **istiyorum**, **çünkü** ekipman ancak onayımla ayrılmış sayılmalı.

Öncelik: Must · Demo adımı: 6
Kurallar: BR-SYS-011, BR-MRP-012, BR-MRP-013, BR-MRP-014, BR-MRP-015

**Kabul kriterleri**
1. Öneriler satır satır ya da toplu onaylanır.
2. Onaydan önce model, adet ve depo değiştirilebilir; değişiklik müsaitliği aşıyorsa kabul edilmez.
3. Rezervasyon model, depo ve adet seviyesinde yapılır. Seri no'lu modellerde hangi birimin gideceği çıkışta belli olur.
4. Onay anında müsaitlik yeniden kontrol edilir. Bu arada başka bir kullanıcı aynı stoğu ayırdıysa onay reddedilir ve güncel müsaitlik gösterilir.
5. Onaylı rezervasyon, neden girilerek serbest bırakılabilir.

### US-MRP-003 · Depolar arası transfer önerisini onaylama
**Teknik müdür olarak** kaynak depoda yetmeyen ekipmanın başka depodan getirilmesini planlamak **istiyorum**, **çünkü** aynı ekipman başka şehirdeki depomuzda boş dururken dışarıdan kiralamak para kaybı.

Öncelik: Must · Demo adımı: 7
Kurallar: BR-MRP-006, BR-MRP-007

**Kabul kriterleri**
1. Öneri hangi depodan hangi modelden kaç adet getirileceğini gösterir.
2. Onaylarken planlanan çıkış ve varış zamanı girilir. Varış, rezervasyon aralığının başlangıcından sonraysa onay engellenir.
3. Onaylanan öneri **Planlandı** durumunda bir transfer oluşturur. Ekipman, çıkış tarihinden itibaren gönderen depoda ayrılmış, varış tarihinden itibaren hedef depoda etkinliğe rezerve sayılır.
4. Transferin çıkış ve girişini depo sorumluları yapar (US-WHS-004).

### US-MRP-004 · Çakışma paneli
**Teknik müdür olarak** karşılanamayan ya da fazla ayrılmış ekipmanı tek listede görmek **istiyorum**, **çünkü** çakışmaları etkinlik gününden önce çözmeliyim.

Öncelik: Must · Demo adımı: 8
Kurallar: BR-MRP-016, BR-MRP-017, BR-MRP-018

**Kabul kriterleri**
1. Her çakışma için model, depo, zaman aralığı, istenen ve müsait adet ile etkilenen etkinlikler listelenir.
2. İki tür çakışma gösterilir:
   - **Rakip talep:** Bir etkinliğin net ihtiyacı, başka etkinliklerin onaylı rezervasyonları yüzünden karşılanamıyor.
   - **Aşırı rezervasyon:** Onaylı rezervasyonlar sonradan müsaitliği aşıyor (etkinlik tarihi değişti, birim bakıma girdi ya da kayboldu, transfer gecikti).
3. Liste depo, tarih aralığı ve etkinliğe göre filtrelenir.
4. Her çakışmadan ilgili etkinliğin ihtiyaç ekranına gidilir.
5. Nedeni ortadan kalkan çakışma otomatik kapanır. Çakışma not girilerek elle "kabul edildi" olarak da işaretlenebilir.

### US-MRP-005 · Dış kiralama
**Teknik müdür olarak** hiçbir depoda bulunmayan ekipman için tedarikçiye sipariş açmak **istiyorum**, **çünkü** eksik ekipman etkinlikten önce kesin olarak karşılanmalı.

Öncelik: Must · Demo adımı: 9
Kurallar: BR-PTY-004, BR-WHS-013, BR-MRP-008

**Kabul kriterleri**
1. İhtiyaç hesabının dış kiralama önerisinden sipariş oluşturulur.
2. Siparişte tedarikçi (tedarikçi rolündeki taraf), kiralama başlangıç ve bitiş tarihi, teslim yeri (kaynak depo ya da mekan) ve satırlar bulunur.
3. Satır bir katalog modeli ya da serbest açıklama ve kategori olabilir, çünkü tedarikçinin modeli katalogda olmayabilir.
4. Sipariş durumları: **Taslak** → **Sipariş verildi** → **Teslim alındı** → **İade edildi**; ayrıca **İptal**.
5. **Sipariş verildi** ve **Teslim alındı** durumundaki satırlar, ihtiyaç hesabında dış kiralamayla karşılanmış sayılır.
6. Dış kiralanan ekipman varsayılan olarak yalnızca sipariş kaydıyla izlenir. Depoya teslim alınan kalemler istenirse QR ile takibe alınabilir (US-WHS-007). Doğrudan mekana teslim edilenler okutulmaz.

### US-MRP-006 · Rider karşılama raporu
**Teknik müdür olarak** rider'ın her satırının nereden karşılandığını gösteren bir belge almak **istiyorum**, **çünkü** sanatçının teknik ekibine ve mekana bunu göndermem gerekiyor.

Öncelik: Must · Demo adımı: 10
Kurallar: BR-MRP-020

**Kabul kriterleri**
1. PDF olarak üretilir; başlıkta etkinlik, mekan, tarih, rider versiyonu ve belgenin üretilme zamanı bulunur.
2. Her satır için istenen ekipman ve adet ile karşılama dağılımı gösterilir: hangi depo, mekan, muadil (model adıyla) ya da dış kiralama (tedarikçiyle).
3. Karşılanamayan kısım belirgin şekilde işaretlenir.

### US-MRP-007 · Müsaitlik sorgulama
**Teknik müdür olarak** bir ekipmanın belirli tarihlerde hangi depoda kaç adet boş olduğunu sorgulamak **istiyorum**, **çünkü** müşteriye teklif vermeden önce yapabilir miyiz görmeliyim.

Öncelik: Must · Demo adımı: —
Kurallar: BR-MRP-002

**Kabul kriterleri**
1. Model ya da kategori, zaman aralığı (tarih ve isteğe bağlı saat) ve isteğe bağlı depo seçilerek sorgulanır.
2. Sonuç her depo için toplam, rezerve ve müsait adeti tablo halinde gösterir.
3. Rezerve adetlerin hangi etkinliklere ait olduğu görülebilir.

### US-MRP-008 · Etkinlik bazında hazırlık ve dönüş payı
**Teknik müdür olarak** her etkinlik için ekipmanın ne kadar önce hazırlanacağını ve döndükten sonra ne kadar kontrol edileceğini saat hassasiyetinde belirlemek **istiyorum**, **çünkü** küçük bir kurumsal iş 10 saatte hazırlanırken bir stadyum konseri günler sürer.

Öncelik: Must · Demo adımı: 5
Kurallar: BR-MRP-001, BR-MRP-010, BR-MRP-017

**Kabul kriterleri**
1. Etkinlik oluşturulduğunda hazırlık ve dönüş payı varsayılan değerlerle (US-SYS-007) dolar.
2. Paylar etkinlik sayfasında gün ve saat olarak girilir (ör. "10 saat", "1 gün 6 saat"); sıfır olabilir, eksi olamaz.
3. Etkinlik sayfasında rezervasyon aralığının başlangıç ve bitiş zamanı açıkça gösterilir.
4. Payları booking müdürü ve teknik müdür değiştirebilir.
5. Değişiklik ihtiyaç hesabını "güncel değil" yapar ve aşırı rezervasyon kontrolünü çalıştırır.
