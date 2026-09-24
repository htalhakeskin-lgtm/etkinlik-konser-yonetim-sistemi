# Depo sorumlusu

[← Kullanıcı hikayeleri](README.md)

Depo sorumlusu okutma işlemlerini (çıkış, giriş, transfer) yalnızca bağlı olduğu depoda yapar. Mobil ekranlar [kapsam belgesindeki adetli kalem ilkelerine](../00-scope.md#771-adetli-kalem-yönetimi-ilkeleri) göre tasarlanır: büyük düğmeler, tek elle kullanım, hazır gelen adetler.

## Stok ve etiketleme

### US-EQP-003 · Seri no'lu birim ekleme
**Depo sorumlusu olarak** yeni gelen ekipmanı seri numarasıyla depoma kaydetmek **istiyorum**, **çünkü** her birimin nerede olduğu izlenebilmeli.

Öncelik: Must · Demo adımı: —

**Kabul kriterleri**
1. Seri no'lu bir model, seri numarası ve depo zorunludur; edinme tarihi isteğe bağlıdır.
2. Seri numarası aynı model içinde tekildir.
3. Birden fazla seri numarası liste olarak yapıştırılıp tek seferde eklenebilir; hatalı ya da mükerrer satırlar kaydedilmeden gösterilir.
4. Her birime otomatik bir etiket kodu atanır ve birim **Depoda** durumunda başlar.

### US-EQP-004 · Adetli stok girişi ve düzeltme
**Depo sorumlusu olarak** kablo, klemp gibi adetli kalemlerin miktarını artırıp azaltmak **istiyorum**, **çünkü** yeni alımlar ve bulunan ya da hurdaya ayrılan parçalar stoğa yansımalı.

Öncelik: Must · Demo adımı: —

**Kabul kriterleri**
1. Adetli bir model, depo ve adet değişimi (artış ya da azalış) girilir.
2. Her düzeltmede neden zorunludur (satın alma, bulundu, hurdaya ayrıldı, sayım düzeltmesi).
3. Stok eksiye düşemez; rezerve adetin altına düşürülürse uyarı verilir ve çakışma oluşur.

### US-EQP-006 · Kasa tanımlama ve doldurma
**Depo sorumlusu olarak** kasaları standart içerikleriyle tanımlamak **istiyorum**, **çünkü** çıkış ve girişte kasadaki her parçayı tek tek okutmak istemiyorum.

Öncelik: Must · Demo adımı: 11

**Kabul kriterleri**
1. Kasanın etiket kodu, bulunduğu depo ve standart içeriği (model ve adet satırları) tanımlanır.
2. Seri no'lu birimler kasaya atanır; bir birim aynı anda yalnızca bir kasada olabilir.
3. Adetli kalemler kasaya adet olarak konur.
4. Bir kasa başka bir kasanın içinde olabilir; bir kasa kendini doğrudan ya da dolaylı olarak içeremez.
5. Gerçek içeriği standart içerikten eksik olan kasa "eksik" olarak işaretlenir.

### US-EQP-007 · QR etiketi yazdırma
**Depo sorumlusu olarak** birim ve kasalar için QR etiketi yazdırmak **istiyorum**, **çünkü** okutmanın ilk şartı her şeyin etiketli olması.

Öncelik: Must · Demo adımı: 11

**Kabul kriterleri**
1. Seçilen birim ve kasalar için A4 etiket sayfası PDF olarak üretilir. Her etikette QR, okunabilir etiket kodu ve model adı bulunur.
2. QR yalnızca etiket kodunu içerir. Etiket sistem dışında okutulduğunda şirket bilgisi göstermez.

### US-EQP-008 · Depo stok görünümü
**Depo sorumlusu olarak** her modelin hangi depoda ne durumda olduğunu görmek **istiyorum**, **çünkü** hazırlık ve transfer kararları bu bilgiye dayanıyor.

Öncelik: Must · Demo adımı: 13

**Kabul kriterleri**
1. Model bazında her depo için **Depoda**, **Yolda**, **Etkinlikte** ve **Bakımda** adetleri gösterilir.
2. Bir tarih aralığı seçilirse o aralık için rezerve ve müsait adet de gösterilir.
3. Liste kategori ve depoya göre filtrelenir.
4. Seri no'lu modellerde birim listesine inilip her birimin durumu, konumu ve kasası görülür.

## Depo işlemleri

### US-WHS-001 · Toplama listesi
**Depo sorumlusu olarak** bir etkinlik için depomdan neyin çıkacağını kasa kasa görmek **istiyorum**, **çünkü** hazırlığı listeden takip ediyoruz.

Öncelik: Must · Demo adımı: 11

**Kabul kriterleri**
1. Liste, etkinliğin seçilen depodaki onaylı rezervasyonlarından oluşur.
2. Standart içeriği rezervasyonu karşılayan kasalar kasa satırı olarak, kalanlar model ve adet satırı olarak gösterilir.
3. Seri no'lu modellerde hangi birimin gideceği listede yazmaz; çıkışta belli olur.
4. QR ile takibe alınmamış dış kiralama ekipmanı ayrı bölümde, tedarikçi ve teslim yeriyle gösterilir. QR ile takip edilenler şirket ekipmanıyla aynı satırlarda, "dış kiralama" işaretiyle yer alır.
5. PDF çıktısında her satırın QR kodu bulunur.
6. Ekrandaki liste, çıkışı yapılan ve kalan adetleri anlık gösterir.

### US-WHS-002 · Etkinlik için çıkış
**Depo sorumlusu olarak** telefonumla okutarak ekipmanın etkinliğe çıkışını yapmak **istiyorum**, **çünkü** neyin depodan çıktığı kâğıt yerine sistemde kayıtlı olmalı.

Öncelik: Must · Demo adımı: 11

**Kabul kriterleri**
1. Etkinlik seçilir; bugün ve yarın çıkışı olan etkinlikler listenin başında gelir.
2. Seri no'lu birim okutulunca rezervasyondaki modelle eşleşirse listeye eklenir.
3. Kasa okutulunca kasanın içeriği toplu olarak eklenir.
4. Adetli kalemlerde beklenen adet hazır gelir; kullanıcı yalnızca fark varsa büyük +/− düğmeleriyle düzeltir.
5. Listede olmayan bir model okutulursa uyarı verilir. Kullanıcı onaylarsa müsaitlik kontrol edilir ve kalem etkinliğin rezervasyonuna eklenir.
6. **Depoda** durumunda olmayan ya da başka bir depoda kayıtlı birimin çıkışı engellenir ve nedeni gösterilir.
7. Çıkış kısmi kaydedilebilir; eksik kalanlar listede görünür.
8. Çıkış tamamlanınca birimler **Etkinlikte** durumuna geçer ve konumları etkinlik olur.
9. Kamera kullanılamıyorsa etiket kodu elle girilebilir.

### US-WHS-003 · Etkinlikten giriş
**Depo sorumlusu olarak** dönen ekipmanı okutarak depoya almak ve eksik ya da hasarlı olanı kaydetmek **istiyorum**, **çünkü** kayıp ve hasarın hangi etkinlikte oluştuğu bilinmeli.

Öncelik: Must · Demo adımı: 12

**Kabul kriterleri**
1. Okutulan birimler **Depoda** durumuna geçer; konumları okutan depo sorumlusunun deposu olur. Bu depo, etkinliğin kaynak deposundan farklı olabilir.
2. Adetli kalemlerde çıkan adet hazır gelir; eksik adet girilirse etkinlik ve depoyla birlikte sayım farkı kaydedilir.
3. Hasarlı işaretlenen birim için açıklamalı hasar kaydı açılır ve birim **Bakımda** durumuna geçer.
4. Etkinlikten çıkan her şey döndüğünde giriş tamamlanmış olur.
5. Dönmeyen birimler teknik müdür tarafından **Kayıp** olarak işaretlenebilir.

### US-WHS-004 · Transfer çıkışı ve girişi
**Depo sorumlusu olarak** depolar arası transferin çıkışını ve varışını okutarak yapmak **istiyorum**, **çünkü** yoldaki ekipman her an izlenebilmeli.

Öncelik: Must · Demo adımı: 7

**Kabul kriterleri**
1. Gönderen depo sorumlusu **Planlandı** durumundaki transferleri görür ve okutarak çıkış yapar; transfer ve birimler **Yolda** durumuna geçer.
2. Alan depo sorumlusu gelen kalemleri okutur; transfer **Tamamlandı** olur, birimler hedef depoda **Depoda** durumuna geçer ve gerçekleşen varış zamanı kaydedilir.
3. Gelen adet eksikse sayım farkı transferle birlikte kaydedilir.
4. Planlanan varış zamanı geçmiş ama tamamlanmamış transferler vurgulanır ve çakışma kontrolüne dahil edilir.
5. Etkinliğe bağlı olmayan, stok dengelemek için elle transfer de oluşturulabilir.

### US-WHS-005 · Anlık güncelleme ve eşzamanlı işlem
**Depo sorumlusu olarak** başka depolardaki ve cihazlardaki işlemleri sayfayı yenilemeden görmek **istiyorum**, **çünkü** aynı anda birden fazla kişi aynı stok üzerinde çalışıyor.

Öncelik: Must · Demo adımı: 13

**Kabul kriterleri**
1. Stok görünümü, toplama listesi ve çakışma paneli, başka bir kullanıcının yaptığı değişikliği en geç 2 saniye içinde sayfa yenilemeden gösterir.
2. Aynı birim iki cihazda aynı anda okutulursa yalnızca biri başarılı olur; diğerine birimin kim tarafından, ne zaman çıkış yapıldığı gösterilir.
3. Bağlantı koparsa ekranda uyarı görünür. Bağlantı geri gelince güncel veri yeniden yüklenir.

### US-WHS-006 · Birim durumunu değiştirme
**Depo sorumlusu olarak** onarımdan dönen, kaybolan ya da kullanımdan kalkan birimin durumunu güncellemek **istiyorum**, **çünkü** müsaitlik hesabı birimin gerçek durumuna dayanır.

Öncelik: Must · Demo adımı: 12

**Kabul kriterleri**
1. İzin verilen değişiklikler: **Bakımda** → **Depoda** (onarıldı), **Bakımda** → **Hurda**, **Depoda** → **Kayıp** / **Hurda**, **Kayıp** → **Depoda** (bulundu).
2. Her değişiklikte neden zorunludur ve değişiklik işlem geçmişine yazılır.
3. Onaylı rezervasyonu etkileyen değişiklik çakışma kontrolünü tetikler.

### US-WHS-007 · Dış kiralanan ekipmanı QR ile takip etme
**Depo sorumlusu olarak** depoma teslim edilen dış kiralama ekipmanını istersem kendi ekipmanım gibi okutarak takip etmek **istiyorum**, **çünkü** kiralık ekipmanın tedarikçiye eksiksiz iade edilmesinden şirket sorumlu.

Öncelik: Should · Demo adımı: —

**Kabul kriterleri**
1. Teslim yeri depo olan siparişi teslim alırken her sipariş satırı için "QR ile takip et" seçilebilir. Seçilmeyen satırlar yalnızca sipariş kaydıyla izlenir.
2. QR ile takip edilen seri no'lu kalemler için birimler, adetli kalemler için adetli stok oluşturulur ve sahiplikleri **Dış kiralama** olarak işaretlenir. Tedarikçinin seri numarası isteğe bağlı girilir. Etiketler US-EQP-007 ile yazdırılır.
3. Bu ekipman yalnızca siparişin bağlı olduğu etkinlik için kullanılabilir. Şirket stoğuna, müsaitlik hesabına ve kayıp raporlarına şirket ekipmanı olarak girmez.
4. Çıkış ve giriş, şirket ekipmanıyla aynı ekranlardan yapılır.
5. Tedarikçiye iadede kalemler okutulur. Birimler **Tedarikçiye iade edildi** durumuna geçer, eksik adetler sayım farkı olarak kaydedilir. Tüm satırlar iade edilince sipariş **İade edildi** olur.
6. İade edilen ekipmanın etiket kodları tekrar kullanılmaz.
