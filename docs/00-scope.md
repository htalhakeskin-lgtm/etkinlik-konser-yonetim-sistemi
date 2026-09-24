# 00 — Kapsam ve MVP Sınırları

> **Durum:** v1.4 · **Son güncelleme:** 2026-09-24
> **Kararlar:** [Bölüm 10](#10-kararlar)

## 1. Bu belge ne işe yarar

Bu belge ürünün neyi yapacağını, neyi yapmayacağını ve her özelliğin hangi sürümde geleceğini belirler. Faz 0'daki diğer belgeler (sözlük, kullanıcı hikayeleri, iş kuralları, ERD) yalnızca burada kapsama alınmış konuları ele alır. Bir özellik burada yoksa tasarlanmaz ve kodlanmaz.

Kapsam değişirse bu belge güncellenir ve [Değişiklik kaydı](#11-değişiklik-kaydı)'na bir satır eklenir. Sürüm sırasını ya da kalıcı kapsam dışı listesini etkileyen değişiklikler için ayrıca bir ADR yazılır.

## 2. Ürün tanımı

Konser ve etkinlik sektöründe hem kendi etkinliklerini düzenleyen (promoter) hem de başkalarının etkinliklerine teknik hizmet veren bir prodüksiyon şirketi için ERP. Sistem, bir etkinliğin talepten hesaplaşmaya kadar tüm yaşam döngüsünü yönetir: sanatçının teknik rider'ından ekipman ihtiyacını hesaplar, birden fazla depodaki ekipmanı, ekibi ve araçları çakışmasız planlar, ticari anlaşmaları ve sponsorlukları izler, etkinlik sonunda bütçeyi ve sanatçı hesaplaşmasını çıkarır.

Proje aynı zamanda ERP'nin tarihsel katmanlarını tek bir üründe uygulamayı hedefler:

| Katman | Bu üründeki karşılığı |
|---|---|
| MRP | Rider'dan net ekipman ihtiyacı, çoklu depo müsaitliği, dış kiralama önerisi |
| MRP II | Ekip ve kapasite planlama, çakışma motoru, kaynak takvimi |
| CRM | Anlaşmalar, teklifler, sözleşmeler, sponsorluk |
| ERP (finans) | Bütçe, gelir/gider, çoklu para birimi, hesaplaşma |
| DRP | Depolar arası transfer, turne lojistiği, araç ve sevkiyat planlama |

## 3. Modellenen şirket

Orta ölçekli, tek şirketli bir prodüksiyon ve organizasyon firması. İki gelir kolu aynı ekipman parkını ve aynı ekibi paylaşır:

- **Promoter kolu:** Sanatçıyı getirir, mekanı kiralar, bilet satar, kâr veya zararı kendisi üstlenir.
- **Teknik hizmet kolu:** Kurumsal lansman, festival sahnesi gibi başkasının etkinliğine ses, ışık, sahne ve ekip kiralar.

Ekipman parkı farklı şehirlerdeki birden fazla depoya dağılmıştır. Tüm depolar tek sistemden, aynı anda ve anlık olarak yönetilir.

Tasarım bu ölçeğe göre yapılır (performans ve arayüz kararları için referans):

| Ölçü | Değer |
|---|---|
| Depo | 3 (ör. İstanbul, Ankara, İzmir) |
| Ekipman birimi | ~5.000 (seri no'lu ve adetli kalemler dahil) |
| Ekip | ~25 sabit, ~75 freelance |
| Etkinlik | Yılda ~300 (iki kol toplamı) |
| Eşzamanlı kullanıcı | ~20 |

## 4. Kullanıcı rolleri

| Rol | Ana işi | Ekranları geldiği sürüm |
|---|---|---|
| Genel müdür | Onaylar, genel durum, kârlılık | S1 (salt okuma), S6 (dashboard) |
| Booking / prodüksiyon müdürü | Etkinlik, opsiyon, sanatçı, rider | S1 |
| Teknik müdür | İhtiyaç hesabı, rezervasyon, çakışma çözümü | S1 |
| Depo sorumlusu (her depo için) | Çıkış/giriş, transfer, birim durumu, bakım | S1 |
| Ekip üyesi (crew) | Kendi çağrıları, day sheet | S2 (PDF), S6 (mobil görünüm) |
| Sponsorluk / satış | Anlaşma, teklif, sponsor teslimatları | S3 |
| Muhasebe | Bütçe, gider, hesaplaşma, dışa aktarım | S4 |
| Sistem yöneticisi | Kullanıcı ve rol yönetimi | S1 |

## 5. Sürüm planı

Her sürüm tek başına çalışan ve demo edilebilen bir bütündür.

| Sürüm | Adı | Hedef | ERP katmanı |
|---|---|---|---|
| **S1 (MVP)** | Rider'dan depoya | Etkinlik yaşam döngüsü, çoklu depo ve transfer, rider'dan net ihtiyaç, rezervasyon, ekipman çakışması, QR ile çıkış/giriş | Çekirdek + MRP |
| S2 | Kaynak planlama | Ekip, yetkinlik ve çağrılar; tüm kaynaklarda çakışma; kaynak takvimi; day sheet ve call sheet | MRP II |
| S3 | Ticari | Anlaşma hunisi, fiyat listesi, teklif, sözleşme, sponsorluk | CRM |
| S4 | Finans | Bütçe, gelir/gider, bilet satışı importu, çoklu para birimi, hesaplaşma | ERP finans |
| S5 | Turne ve lojistik | Araç, sevkiyat, yükleme hesabı, şehirler arası geçiş kontrolü, turne haritası | DRP |
| S6 | Analiz ve yayın | Dashboard'lar, kullanım oranı, satın al/kirala analizi, bildirimler, crew mobil görünümü, demo senaryoları | Raporlama |

### Sıralamanın gerekçesi

Sıra veri bağımlılığına göre kurulmuştur. Her sürüm bir öncekinin ürettiği veriyi kullanır:

- **S2, S1'den sonra gelir:** Ekip ve takvim planlaması, S1'in etkinlik tarihlerine ve çakışma motoruna dayanır.
- **S3, S2'den sonra gelir:** Teknik hizmet teklifi rider'dan (S1) ve ekip maliyetinden (S2) fiyat üretir.
- **S4, S3'ten sonra gelir:** Hesaplaşma formülü, S3'te tanımlanan anlaşma tipine bağlıdır. Bütçe ise dış kiralama (S1), ekip (S2) ve sözleşme (S3) maliyetlerini toplar.
- **S5, S4'ten sonra gelir:** Turne lojistiği başka bir modülü engellemez. Depolar arası transfer zaten S1'de olduğu için şirket turne modülü olmadan da çalışabilir. Bu yüzden para akışını tamamlayan S3 ve S4 önce gelir.
- **S6 en sondadır:** Analiz, önceki sürümlerin biriktirdiği veriye ihtiyaç duyar.

## 6. MVP (S1) tanımı

### 6.1 Bitti kriteri: MVP demo senaryosu

Aşağıdaki akış seed verili bir ortamda baştan sona çalıştığında S1 tamamlanmış sayılır:

1. Booking müdürü bir konser talebi oluşturur ve mekan için 2. opsiyonu alır.
2. 1. opsiyon düşürülür; sistem etkinliğin opsiyonunu otomatik olarak 1. sıraya yükseltir.
3. Etkinlik "Müzakere" ve "Onaylı" durumlarına geçer; her geçiş durum geçmişine yazılır.
4. Sanatçının prodüksiyonuna ait rider'ın güncel versiyonu etkinliğe bağlanır.
5. "Hazırlık" durumuna geçişte sistem net ihtiyacı hesaplar: rider brüt ihtiyacı − mekanın kendi ekipmanı − muadil eşleşmeler.
6. Net ihtiyaç, etkinliğin kaynak deposundaki müsaitlikle (hazırlık ve dönüş payı dahil) karşılaştırılır; karşılanabilen kalemler için rezervasyon önerisi çıkar, teknik müdür onaylar.
7. Kaynak depoda yetmeyen kalemlerin bir kısmı başka bir depoda bulunur. Sistem depolar arası transfer önerir. Transfer onaylanınca birimler "Yolda" görünür ve planlanan varış tarihinden itibaren hedef depoda müsait sayılır.
8. Aynı tarihlerde ikinci bir etkinlik aynı ekipmanı isteyince çakışma panelinde görünür.
9. Hiçbir depoda bulunmayan kalemler için dış kiralama önerisi çıkar ve bir tedarikçiye dış kiralama siparişi açılır.
10. Rider karşılama raporu (PDF) her rider satırının nereden karşılandığını gösterir: hangi depo, mekan, muadil veya dış kiralama.
11. Depo sorumlusu QR kodlu toplama listesiyle telefonundan çıkış yapar. Seri no'lu birimler tek tek okutulur; adetli kalemlerde kasa okutulur ve hazır gelen adet onaylanır. Birimlerin durumu "Etkinlikte" olur.
12. Dönüşte giriş yapılır. Eksik ve hasarlı birimler kaydedilir; hasarlı birim "Bakımda" durumuna geçer, adetli kalemlerdeki sayım farkı etkinliğe bağlı kayıp olarak işlenir.
13. İki farklı depodaki sorumlular aynı anda çalışırken stok değişiklikleri birbirlerinin ekranına sayfa yenilemeden yansır. Aynı birim iki kez çıkış yapılamaz.
14. Tüm değişiklikler audit log'da görünür.

### 6.2 MVP'ye bilinçli olarak alınmayanlar

- **Ekip planlama:** MRP II'nin konusu, S2'de. S1'de çakışma motoru yalnızca ekipman için çalışır, ama ileride dört kaynak türüne genişleyecek şekilde tasarlanır.
- **Sözleşme ve hesaplaşma:** Etkinlik durum makinesi S1'de eksiksiz kurulur. Henüz yazılmamış modüllere bağlı geçiş kuralları ("Onaylı için imzalı sözleşme", "Kapandı için hesaplaşma onayı") S1'de elle onay adımıyla karşılanır. İlgili modül geldiğinde gerçek kontrolle değiştirilir (bkz. K-07).
- **Mesafeye göre depo seçimi:** S1'de etkinliğin kaynak deposunu kullanıcı seçer; transferlerin varış tarihi elle girilir. Mesafe ve nakliye süresine göre otomatik seçim S5'te gelir.
- **Gantt görünümlü kaynak takvimi:** S1'de müsaitlik liste ve tablo ile gösterilir; Gantt S2'de gelir.
- **Güç hesabı:** Veri S1'de toplanır (ekipman watt değeri, mekan amper kapasitesi), hesap ve uyarı S2'de gelir.

## 7. Modül bazında kapsam matrisi

"Sürüm" sütunu özelliğin ilk geldiği sürümdür. **Dışarıda** = kalıcı kapsam dışı (bkz. Bölüm 8).

### 7.1 Çekirdek ve sistem

| Özellik | Sürüm |
|---|---|
| E-posta ve şifre ile giriş | S1 |
| Sabit roller ve rol bazlı yetkiler | S1 |
| Kullanıcı ve rol yönetimi ekranı | S1 |
| Ayarlar ekranı: varsayılan hazırlık ve dönüş payı, varsayılan operasyon geçiş modu | S1 |
| Kullanıcının bağlı olduğu depo (depo sorumlusu kendi deposunda işlem yapar) | S1 |
| Audit log (kim, neyi, ne zaman, eski ve yeni değer) | S1 |
| Anlık güncellenen ekranlar (stok ve durum değişiklikleri sayfa yenilemeden yansır) | S1 |
| Belge yükleme (rider PDF, stage plot, sözleşme dosyası) | S2 |
| Uygulama içi bildirimler | S2 |
| Listelerin Excel/CSV dışa aktarımı | S2 |
| E-posta bildirimleri | S6 |
| Kayıt bazlı ince yetki (ör. yalnızca atandığı etkinlikler) | S6 |
| İki adımlı doğrulama (2FA) | S6 |
| SSO / kurumsal kimlik sağlayıcı | Dışarıda |

### 7.2 Kişi ve firma (Party)

| Özellik | Sürüm |
|---|---|
| Tek party yapısı: kişi veya firma, birden çok rol | S1 |
| İletişim bilgileri ve firmaya bağlı iletişim kişileri | S1 |
| Roller: sanatçı, ajans, mekan sahibi, tedarikçi, müşteri | S1 |
| Roller: crew | S2 |
| Roller: sponsor | S3 |
| Mükerrer kayıt tespiti ve birleştirme | S6 |

### 7.3 Mekan

| Özellik | Sürüm |
|---|---|
| Mekan kaydı: kapasite, sahne ölçüleri, yükleme kapısı ve rampa, amper kapasitesi, sessizlik saati | S1 |
| Mekanın kendi ekipmanı (house equipment) | S1 |
| Tarih bazlı müsaitlik ve opsiyon sırası | S1 |
| Mekan dosyaları (teknik çizim, fotoğraf) | S2 |

### 7.4 Sanatçı ve prodüksiyon

| Özellik | Sürüm |
|---|---|
| Sanatçı kaydı (party rolü olarak) | S1 |
| Prodüksiyon (sanatçının belirli bir şovu; rider buna bağlanır, bkz. K-05) | S1 |
| Turne ve turne tarihleri | S5 |

### 7.5 Etkinlik

| Özellik | Sürüm |
|---|---|
| Etkinlik kaydı ve tipi (kendi etkinliği / teknik hizmet) | S1 |
| Etkinliğin başlangıç ve bitiş zamanı (tarih ve saat) | S1 |
| Etkinlik bazında, saat hassasiyetinde hazırlık ve dönüş payı | S1 |
| Etkinliğin kaynak deposu | S1 |
| Tam durum makinesi, geçiş kuralları, durum geçmişi | S1 |
| Opsiyon (hold) sırası ve otomatik yükselme; her iki etkinlik türünde | S1 |
| Opsiyon son tarihi, "yaklaşıyor" ve "süresi geçti" işaretleri | S1 |
| İptal ve iptal nedeni | S1 |
| Seanslar (matine, akşam gibi birden fazla performans) | S2 |
| Operasyon geçiş modu: elle / otomatik (kurulum, canlı, söküm, hesaplaşma) | S1 |
| Kapı açılışı ve söküm başlangıcı zamanları | S1 |
| Onaylı etkinliği müzakereye geri alma | S1 |
| Diğer zaman noktaları (ses provası, sahne saati) | S2 |

### 7.6 Teknik rider (BOM)

| Özellik | Sürüm |
|---|---|
| Rider satırları: kategori veya model, adet, zorunlu / esnek | S1 |
| Prodüksiyon altında rider versiyonlama | S1 |
| Etkinliğe özel rider versiyonu (prodüksiyon versiyonundan türetilir) | S1 |
| Satır bazında kabul edilen muadil modeller | S1 |
| Teknik hizmet müşterisinin ihtiyaç listesi (rider ile aynı yapı, bkz. K-06) | S1 |
| Input list | S2 |
| Hospitality rider (kontrol listesi olarak) | S2 |
| Rider PDF'inden otomatik satır okuma | Dışarıda |

### 7.7 Ekipman ve depo

| Özellik | Sürüm |
|---|---|
| Model kataloğu: kategori, marka/model, ağırlık, watt, case hacmi | S1 |
| Modelin takip tipi: seri no'lu veya adetli | S1 |
| Seri numaralı birimler ve QR etiketi | S1 |
| Adetli kalemler: kablo, klemp, adaptör (bkz. 7.7.1) | S1 |
| Kit ve case (iç içe yapı) | S1 |
| Birden fazla depo; her birimin ve adetli stoğun bulunduğu depo | S1 |
| Depo bazında stok görünümü: toplam, rezerve, etkinlikte, yolda, bakımda | S1 |
| Depolar arası transfer ("Yolda" durumu, planlanan ve gerçekleşen varış) | S1 |
| Birim durumu: depoda, yolda, etkinlikte, bakımda, kayıp, hurda (rezervasyon bir durum değildir, bkz. sözlük) | S1 |
| QR ile depo çıkış ve giriş (telefon tarayıcısından) | S1 |
| Dönüşte eksik, hasarlı ve sayım farkı kaydı | S1 |
| Bakım kayıtları ve periyodik bakım hatırlatması | S2 |
| Periyodik depo sayımı ve sayım farkı düzeltmesi | S2 |
| Sarf malzeme (gaffer bant, pil) stok takibi | S6 |
| Amortisman ve sabit kıymet takibi | Dışarıda |

#### 7.7.1 Adetli kalem yönetimi ilkeleri

Adetli kalemler binlerce parçadan oluşur. Takip, depo çalışanına ek yük getirmeyecek ve yöneticiye güvenilir sayı verecek şekilde tasarlanır:

- **QR etiketi kasaya yapıştırılır, parçaya değil.** Kablolar ve klempler kasalarda durur (ör. "XLR 10 m kasası"). Çalışan tek tek parça okutmaz.
- **Kasanın standart içeriği tanımlıdır** (ör. "20 × XLR 10 m"). Tam kasa gidip tam dönüyorsa tek okutma yeterlidir.
- **Beklenen adet ekrana hazır gelir.** Çıkışta toplama listesindeki adet, dönüşte çıkan adet önceden doldurulur. Çalışan yalnızca fark varsa düzeltir.
- **Mobil ekran depo koşullarına göre tasarlanır.** Büyük +/− butonları, tek elle kullanım, eldivenle basılabilecek boyutlar.
- **Kayıp etkinliğe bağlanır.** Sayım farkı, hangi etkinlikte ve hangi depoda oluştuğuyla birlikte kaydedilir.
- **Yönetici tek ekranda görür:** Model bazında her depodaki toplam, rezerve, etkinlikte ve yolda olan adet ile kayıp oranı en yüksek kalemler.

### 7.8 İhtiyaç hesabı ve rezervasyon (MRP)

| Özellik | Sürüm |
|---|---|
| Net ihtiyaç hesabı: rider − mekan ekipmanı − muadil | S1 |
| Tüm depolarda müsaitlik kontrolü (hazırlık ve dönüş payı, yoldaki transferler dahil) | S1 |
| Rezervasyon önerisi (önce kaynak depo, sonra diğer depolar) ve elle onay | S1 |
| Depolar arası transfer önerisi | S1 |
| Ekipman çakışma tespiti ve çakışma paneli | S1 |
| Dış kiralama önerisi ve dış kiralama siparişi | S1 |
| Depoya teslim alınan dış kiralama ekipmanının isteğe bağlı QR ile takibi | S1 |
| Rider karşılama raporu (PDF) | S1 |
| QR kodlu toplama listesi (PDF) | S1 |
| Güç hesabı ve jeneratör uyarısı | S2 |
| Dış kiralama sipariş formu (PDF) | S3 |
| Mesafe ve nakliye süresine göre otomatik depo seçimi | S5 |

### 7.9 Ekip ve kaynak planlama (MRP II)

| Özellik | Sürüm |
|---|---|
| Crew kaydı: sabit / freelance, günlük ücret | S2 |
| Yetkinlik ve sertifika (geçerlilik tarihiyle) | S2 |
| Crew müsaitliği | S2 |
| Çağrı (crew call) ve call time | S2 |
| Ekip çakışması: yetkinlik uyumu ve dinlenme süresi | S2 |
| Mekan çakışması (opsiyon önceliğiyle) | S2 |
| Kaynak takvimi (Gantt, çakışmalar kırmızı) | S2 |
| Day sheet, run of show, crew call sheet (PDF) | S2 |
| Crew mobil görünümü | S6 |
| Bordro, SGK | Dışarıda |

### 7.10 Ticari (CRM)

| Özellik | Sürüm |
|---|---|
| Anlaşma (deal) hunisi ve aktiviteler | S3 |
| Sanatçı anlaşma tipleri: garanti, yüzde, hangisi yüksekse, bonus eşikleri | S3 |
| Ekipman kiralama fiyat listesi (model bazında) | S3 |
| Teknik hizmet teklifi (rider'dan otomatik maliyet ve fiyat) | S3 |
| Sözleşme kaydı ve imza durumu (dosya olarak) | S3 |
| Sponsorluk paketleri ve teslimat takibi | S3 |
| E-imza entegrasyonu | Dışarıda |

### 7.11 Finans

| Özellik | Sürüm |
|---|---|
| Etkinlik bütçesi ve bütçe kalemleri | S4 |
| Gider ve gelir kaydı | S4 |
| Bilet satışı importu (CSV / Excel) | S4 |
| Çoklu para birimi ve kur kaydı | S4 |
| Hesaplaşma hesabı ve hesaplaşma föyü (PDF) | S4 |
| Bütçe sapma uyarısı | S4 |
| Muhasebe yazılımı için dışa aktarım | S4 |
| Resmi fatura, e-fatura, e-arşiv, vergi beyanı | Dışarıda |
| Tam cari hesap ve tahsilat takibi | Dışarıda |

### 7.12 Turne ve lojistik (DRP)

| Özellik | Sürüm |
|---|---|
| Araç kaydı: hacim ve ağırlık kapasitesi | S5 |
| Sevkiyat ve yükleme listesi | S5 |
| Yükleme hesabı (kaç araç gerekir) | S5 |
| Şehirler arası geçiş kontrolü ve öneriler (ikinci set / yerel kiralama) | S5 |
| Turne haritası | S5 |
| Rota optimizasyonu | Dışarıda |

### 7.13 Raporlama ve analiz

| Özellik | Sürüm |
|---|---|
| Etkinlik bazında kârlılık | S6 |
| Ekipman kullanım oranı (depo bazında) | S6 |
| Ekip iş yükü dağılımı | S6 |
| Sponsor teslimat durumu raporu | S6 |
| Dış kiralama harcamaları | S6 |
| Satın al / kirala analizi | S6 |
| Yönetim dashboard'u | S6 |

## 8. Kalıcı kapsam dışı

Bunlar hiçbir sürümde yapılmayacak. Gerekçe: ya başka bir yazılımın işi ya da projeyi bitmez hale getirip ERP becerisini göstermeye katkı sağlamıyor.

- **Bilet satış platformu:** Bilet verisi dışarıdan import edilir.
- **Resmi muhasebe:** Fatura, e-fatura, e-arşiv, vergi beyanı, amortisman. Muhasebe yazılımına dışa aktarım yapılır.
- **Bordro ve SGK işlemleri.**
- **Native mobil uygulama:** Mobil ihtiyaçlar responsive web ile karşılanır.
- **Çok kiracılı (multi-tenant) SaaS:** Sistem tek bir şirket için kurulur.
- **Çevrimdışı çalışma.**
- **Sensör ve IoT entegrasyonu:** Bakım kayıtları elle girilir.
- **Depo içi raf ve bölge adreslemesi:** Konum depo seviyesinde tutulur.
- **Dış kullanıcı portalları:** Sanatçı, müşteri ya da tedarikçi sisteme giriş yapmaz.
- **E-imza, SSO, rider PDF'inden otomatik okuma, rota optimizasyonu.**

## 9. Fonksiyonel olmayan varsayımlar

| Konu | Karar |
|---|---|
| Kurulum | Tek şirket, tek kurulum; tüm depolar aynı sistemi ve aynı veritabanını kullanır |
| Eşzamanlılık | Aynı kayıt üzerindeki çakışan değişiklikler sessizce ezilmez (iyimser kilitleme). Aynı birim iki kez rezerve edilemez ya da çıkış yapılamaz. Stok ve durum değişiklikleri açık ekranlara anında yansır |
| Platform | Web. Ofis ekranları masaüstü öncelikli; depo ve crew ekranları mobil öncelikli |
| Tarayıcı | Chrome, Edge, Firefox, Safari'nin güncel sürümleri |
| Zaman | Veritabanında UTC, arayüzde Europe/Istanbul |
| Para birimi | S4'e kadar yalnızca TRY; çoklu para birimi S4'te |
| Dil | Arayüz Türkçe; metinler kaynak dosyalarında (bkz. K-02) |
| Erişilebilirlik | Klavye ile kullanım ve yeterli kontrast; tam WCAG denetimi hedeflenmiyor |
| Kişisel veri (KVKK) | Rol bazlı erişim ve audit log. Demo verisi tamamen kurgusaldır; aydınlatma metni, silme talebi gibi KVKK süreçleri kapsam dışı |

## 10. Kararlar

| No | Soru | Karar | Gerekçe |
|---|---|---|---|
| K-01 | Belgelerin dili | Belgeler Türkçe; README'de İngilizce özet. Kod, commit ve veritabanı adları İngilizce. | Önerilen varsayılan uygulandı, itiraz gelirse değişir. |
| K-02 | Arayüz dili | Türkçe; metinler baştan kaynak dosyalarında tutulur. | Önerilen varsayılan uygulandı. İkinci dil sonradan kod değişikliği olmadan eklenebilir. |
| K-03 | Kablo, klemp gibi kalemlerin takibi | Adetli takip; kullanım ilkeleri 7.7.1'de. | Seri no'yla parça takibi sahada uygulanamaz. Kasa bazlı okutma hem hızlı hem güvenilir. |
| K-04 | Depo sayısı | S1'den itibaren birden fazla depo, depolar arası transfer ve anlık senkron ekranlar. | Şirketin gerçek yapısı çok depolu. Sonradan eklemek stok modelinin tamamını değiştirirdi. |
| K-05 | Rider nereye bağlı | Prodüksiyona bağlı ve prodüksiyon altında versiyonlanır. Etkinlik belirli bir versiyonu sabitler. Etkinliğe özel değişiklik gerekirse o versiyondan türetilmiş, etkinliğe özel bir versiyon oluşur. | Aynı sanatçının akustik ve full band şovları farklı rider ister. Sabitleme sayesinde rider sonradan güncellense de geçmiş etkinliklerin ihtiyaç hesabı değişmez. |
| K-06 | Teknik hizmet ihtiyaç listesi | Rider ile aynı yapı; kaynağı sanatçı prodüksiyonu veya müşteri olur. | İhtiyaç hesabı, rezervasyon ve karşılama raporu iki kolda tek kod yoluyla çalışır. |
| K-07 | Henüz yazılmamış modüllere bağlı geçiş kuralları | S1'de elle onay adımı; iş kuralı kataloğunda hangi sürümde gerçek kontrole döneceği not edilir. | Durum makinesi baştan eksiksiz kurulur, sonradan yeniden tasarlanmaz. |
| K-08 | Sürüm sırası | Mevcut sıra onaylandı. S3'e ekipman kiralama fiyat listesi eklendi. | Gerekçe Bölüm 5'te. Teklifin rider'dan fiyat üretebilmesi için model bazlı fiyat listesi eksikti. |

## 11. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-09-24 | v0.1 | İlk taslak |
| 2026-09-24 | v1.0 | Açık sorular karara bağlandı. Çoklu depo ve depolar arası transfer S1'e alındı; adetli kalem ilkeleri eklendi; etkinliğe özel rider versiyonu, kiralama fiyat listesi ve sıralama gerekçesi eklendi. |
| 2026-09-24 | v1.1 | Terimler sözlüğüyle uyum: "çıkış deposu" → "kaynak depo", "pick list" → "toplama listesi", etkinlik durumu "Anlaşma" → "Müzakere"; "rezerve" birim durumlarından çıkarıldı. |
| 2026-09-24 | v1.2 | Dış kiralama ekipmanının isteğe bağlı QR ile takibi S1'e eklendi. |
| 2026-09-24 | v1.3 | İş kuralı kararları: saat hassasiyetinde etkinlik zamanı ve hazırlık/dönüş payı, ayarlar ekranı, opsiyon süresi işaretleri, teknik hizmette opsiyon. |
| 2026-09-25 | v1.4 | Durum makinesi kararları: otomatik operasyon geçişleri, kapı açılışı ve söküm başlangıcı S1'e alındı; onaydan geri alma eklendi. |
