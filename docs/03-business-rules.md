# 03 — İş Kuralları Kataloğu

> **Durum:** v1.0 · **Son güncelleme:** 2026-09-24
> **Kararlar:** [Bölüm 7](#7-kararlar)

## 1. Bu belge ne işe yarar

Sistemin uyması gereken alan kurallarını tek yerde, numaralı olarak toplar. Bir kural kodda bir yerde uygulanır, testlerde numarasıyla doğrulanır, kullanıcıya gösterilen hata mesajında da numarasıyla anılır. Kullanıcı hikayelerindeki kabul kriterleri ile bu katalog çelişirse **katalog geçerlidir**; hikaye düzeltilir.

Bu sürüm S1 kurallarını içerir. Sonraki sürümlerin kuralları, o sürümün hikayeleriyle birlikte eklenir.

## 2. Hangi kurallar buraya girer

| Buraya girer | Buraya girmez |
|---|---|
| Birden fazla kaydı ya da modülü ilgilendiren kısıtlar (ör. bir birim aynı anda tek kasada olabilir) | Tek alan doğrulamaları: zorunlu alan, uzunluk, biçim, basit tekillik. Bunlar modül tasarımında ve API sözleşmesinde tanımlanır. |
| Durum geçişlerinin koşulları ve yan etkileri | Tam durum geçiş tabloları. Bunlar [04-state-machines.md](04-state-machines.md)'de. |
| Hesaplama formülleri (müsaitlik, net ihtiyaç) | Ekran düzeni, arayüz davranışları |
| Alan anlamı taşıyan yetki kuralları (ör. depo sorumlusu yalnızca kendi deposunda okutur) | Rol–yetki matrisinin tamamı |

## 3. Biçim ve kullanım

**Kimlik:** `BR-<MODÜL>-<NNN>`. Modül kodları kullanıcı hikayeleriyle aynıdır (`SYS`, `PTY`, `VEN`, `EVT`, `RDR`, `EQP`, `WHS`, `MRP`). Silinen kuralın numarası tekrar kullanılmaz.

**Kural türleri:**

| Tür | Anlamı |
|---|---|
| Kısıt | Her zaman doğru olması gereken koşul; ihlal eden işlem reddedilir. |
| Geçiş | Bir durum geçişinin koşulu. |
| Tetikleyici | Bir olayın otomatik yan etkisi. |
| Hesaplama | Bir değerin nasıl hesaplandığı. |
| Yetki | Alan anlamı olan erişim kuralı. |

**Kodda ve testlerde kullanımı:**

- Kural ihlali kodda kural numarasını taşıyan bir hata olarak fırlatılır. API bu numarayı hata yanıtında döner (ayrıntısı `standards/api.md`'de).
- Her kuralın en az bir otomatik testi olur ve test, kural numarasıyla etiketlenir. Böylece "BR-MRP-002'yi hangi testler doğruluyor?" sorusu tek aramayla cevaplanır.
- Bir kuralın metni değişirse [değişiklik kaydına](#8-değişiklik-kaydı) yazılır. Metni değişen kuralın testleri de aynı PR'da güncellenir.

**Zaman:** Tüm zamanlar UTC saklanır. Opsiyon günü ve opsiyon son tarihi Europe/Istanbul takvim gününe göre işler. Rezervasyon, transfer ve müsaitlik hesapları dakika hassasiyetinde zaman aralıklarıyla yapılır; böylece aynı gün dönüp yeniden çıkan ekipman doğru hesaplanır.

## 4. Parametreler

Kurallarda geçen sayısal değerler sabit kodlanmaz. P-06 ve P-07 sistem yöneticisi tarafından ayarlar ekranından değiştirilir (US-SYS-007); diğerleri S1'de yapılandırma dosyasından okunur.

| No | Parametre | Varsayılan | Kullanan kural |
|---|---|---|---|
| P-01 | Hesap kilidinden önceki hatalı giriş sayısı | 5 | BR-SYS-005 |
| P-02 | Hesap kilidi süresi | 15 dakika | BR-SYS-005 |
| P-03 | Hareketsizlik oturum süresi | 12 saat | BR-SYS-008 |
| P-04 | En kısa şifre uzunluğu | 10 karakter | BR-SYS-007 |
| P-05 | Opsiyon son tarihi uyarı eşiği | 3 gün | BR-EVT-008 |
| P-06 | Varsayılan hazırlık payı | 1 gün (24 saat) | BR-MRP-001 |
| P-07 | Varsayılan dönüş payı | 1 gün (24 saat) | BR-MRP-001 |
| P-08 | Anlık güncelleme gecikmesi üst sınırı | 2 saniye | BR-SYS-012 |
| P-09 | İhtiyaç hesabı süre hedefi (50 satır, 5.000 birim) | 2 saniye | BR-MRP-019 |
| P-10 | Kayıp raporu varsayılan dönemi | 90 gün | BR-EQP-010 |
| P-11 | Kayıp raporunda öne çıkan model sayısı | 10 | BR-EQP-010 |
| P-12 | Günlük çakışma kontrolü saati | 03:00 | BR-MRP-017 |

## 5. Kurallar

### 5.1 Çekirdek ve sistem (SYS)

#### BR-SYS-001 · Silme yerine pasifleştirme
Başka bir kaydın bağlı olduğu kayıt silinmez, pasifleştirilir. Pasif kayıt yeni işlemlerde seçilemez; bağlı olduğu mevcut kayıtlarda görünmeye devam eder.
*Tür:* Kısıt · *Hikayeler:* US-SYS-002, US-PTY-002, US-EQP-001, US-SYS-005

#### BR-SYS-002 · Yetki birleşimi ve sunucu kontrolü
Kullanıcının yetkileri, rollerinin yetkilerinin birleşimidir. Her yetki kontrolü sunucuda yapılır; arayüzde düğmelerin gizlenmesi yalnızca kolaylık içindir.
*Tür:* Yetki · *Hikayeler:* US-SYS-012, US-SYS-006

#### BR-SYS-003 · Depo kapsamı
Depo sorumlusu; çıkış, giriş, transfer çıkışı ve varışı, birim ekleme ve adetli stok düzeltmesi işlemlerini yalnızca bağlı olduğu depolarda yapar. Tüm depoların stoğunu görür.
*Tür:* Yetki · *Hikayeler:* US-EQP-003, US-EQP-004, US-WHS-002, US-WHS-003, US-WHS-004

#### BR-SYS-004 · Genel müdür salt okur
Genel müdür rolü hiçbir oluşturma, değiştirme, onaylama ya da silme yetkisi içermez. Kullanıcının başka bir rolü varsa o rolün yetkileri geçerlidir (BR-SYS-002).
*Tür:* Yetki · *Hikayeler:* US-SYS-006

#### BR-SYS-005 · Hesap kilidi
Aynı hesapla art arda P-01 hatalı giriş denemesinden sonra hesap P-02 süreyle kilitlenir. Hata mesajı e-postanın mı şifrenin mi yanlış olduğunu belirtmez.
*Tür:* Kısıt · *Hikayeler:* US-SYS-010

#### BR-SYS-006 · Geçici şifre
Sistem yöneticisinin ürettiği şifre yalnızca bir kez gösterilir. Geçici şifreyle giren kullanıcı yeni şifre belirlemeden hiçbir işlem yapamaz.
*Tür:* Kısıt · *Hikayeler:* US-SYS-001, US-SYS-002, US-SYS-010

#### BR-SYS-007 · Şifre politikası
Şifre en az P-04 karakterdir ve mevcut şifreyle aynı olamaz. Şifre değişince kullanıcının diğer oturumları sona erer.
*Tür:* Kısıt · *Hikayeler:* US-SYS-011

#### BR-SYS-008 · Oturum sonu
Oturum P-03 hareketsizlikten sonra sona erer. Pasifleştirilen kullanıcının tüm oturumları hemen sona erer.
*Tür:* Kısıt · *Hikayeler:* US-SYS-010, US-SYS-002

#### BR-SYS-009 · Sistem yöneticisi koruması
Sistem yöneticisi kendini pasifleştiremez. Son aktif sistem yöneticisi pasifleştirilemez ve sistem yöneticisi rolü ondan alınamaz.
*Tür:* Kısıt · *Hikayeler:* US-SYS-002

#### BR-SYS-010 · İşlem geçmişi
Her oluşturma, değişiklik ve durum geçişi; kullanıcı, zaman, kayıt ve değişen her alanın eski ve yeni değeriyle işlem geçmişine yazılır. İşlem geçmişi değiştirilemez ve silinemez. İşlem geçmişi ekranlarını genel müdür ve sistem yöneticisi görür.
*Tür:* Tetikleyici · *Hikayeler:* US-SYS-004

#### BR-SYS-011 · Eşzamanlı düzenleme
Kullanıcı bir kaydı açtıktan sonra kayıt başkası tarafından değiştirildiyse, kaydetme reddedilir ve kullanıcıya kaydın güncel hali gösterilir. Hiçbir değişiklik sessizce ezilmez.
*Tür:* Kısıt · *Hikayeler:* US-WHS-005, US-MRP-002

#### BR-SYS-012 · Anlık yayın
Stok, birim durumu, rezervasyon, transfer ve çakışma değişiklikleri, ilgili ekranı açık olan tüm kullanıcılara en geç P-08 içinde sayfa yenilemeden yansır. Bağlantı koptuğunda ekran uyarı gösterir. Bağlantı geri geldiğinde güncel veri yeniden yüklenir.
*Tür:* Tetikleyici · *Hikayeler:* US-WHS-005, US-WHS-001, US-EQP-008

#### BR-SYS-013 · Depo kısıtları
Sistemde her zaman en az bir aktif depo bulunur. Stoğu, açık transferi ya da açık rezervasyonu olan depo pasifleştirilemez.
*Tür:* Kısıt · *Hikayeler:* US-SYS-005

#### BR-SYS-014 · Depo sorumlusunun bağlı deposu
Depo sorumlusu rolündeki her kullanıcının en az bir aktif bağlı deposu vardır.
*Tür:* Kısıt · *Hikayeler:* US-SYS-001

### 5.2 Kişi ve firma (PTY)

#### BR-PTY-001 · Taraf rolleri
Her tarafın en az bir rolü vardır; bir taraf birden fazla role sahip olabilir. Taraf türü (kişi / firma) oluşturulduktan sonra değişmez.
*Tür:* Kısıt · *Hikayeler:* US-PTY-001

#### BR-PTY-002 · Birincil iletişim bilgisi
Bir tarafın her iletişim türünde (telefon, e-posta, adres) en fazla bir birincil kaydı olur. Bir türde tek kayıt varsa o kayıt birincildir.
*Tür:* Kısıt · *Hikayeler:* US-PTY-001

#### BR-PTY-003 · İletişim kişisi
İletişim kişisi yalnızca firma türündeki bir tarafa bağlanır ve kendisi kişi türündedir.
*Tür:* Kısıt · *Hikayeler:* US-PTY-001

#### BR-PTY-004 · Rol gerektiren seçimler
Bir tarafın bir alana seçilebilmesi için aktif olması ve ilgili role sahip olması gerekir: etkinlik müşterisi → Müşteri, dış kiralama tedarikçisi → Tedarikçi, prodüksiyonun sanatçısı → Sanatçı, sanatçının ajansı → Ajans, mekan işletmecisi → Mekan işletmecisi.
*Tür:* Kısıt · *Hikayeler:* US-EVT-001, US-MRP-005, US-ART-001, US-VEN-001

### 5.3 Mekan (VEN)

#### BR-VEN-001 · Hesaba giren mekan ekipmanı
Mekan ekipmanının yalnızca bir katalog modeline ya da kategorisine bağlı satırları ihtiyaç hesabında kullanılır. Serbest açıklamalı satırlar bilgi amaçlıdır.
*Tür:* Hesaplama · *Hikayeler:* US-VEN-002, US-MRP-001

### 5.4 Etkinlik ve opsiyon (EVT)

#### BR-EVT-001 · Opsiyon ekleme
Opsiyon, **Talep**, **Opsiyonda** veya **Müzakere** durumundaki etkinliklere eklenir. **Kendi etkinliği**, **Müzakere**'ye geçmeden önce en az bir opsiyon almış olmalıdır. **Teknik hizmet** etkinliğinde opsiyon isteğe bağlıdır: mekanı müşteri tuttuysa gerekmez, müşteri adına mekanı biz tutuyorsak opsiyon eklenir.
*Tür:* Kısıt · *Hikayeler:* US-EVT-002, US-EVT-004

#### BR-EVT-002 · Opsiyon günü ve sırası
Bir opsiyon tek bir mekan ve tek bir takvim günü içindir; çok günlü etkinlik her günü için ayrı opsiyon tutar. Aynı mekan ve gündeki aktif opsiyonlar (kendi ve dış) 1'den başlayarak boşluksuz sıralanır.
*Tür:* Kısıt · *Hikayeler:* US-EVT-002

#### BR-EVT-003 · Opsiyonu sıraya yerleştirme
Yeni opsiyon, mekanın bildirdiği N sırasına yerleştirilir:
- 1 ile N−1 arasındaki sıralardan kayıtlı olmayanlar için sahibi bilinmeyen dış opsiyonlar otomatik oluşturulur.
- N sırası doluysa, o sıradaki ve arkasındaki opsiyonlar birer geri kayar.
*Tür:* Tetikleyici · *Hikayeler:* US-EVT-002

#### BR-EVT-004 · Etkinlik başına tek opsiyon
Bir etkinliğin aynı mekan ve gün için en fazla bir aktif opsiyonu olur. Farklı etkinlikler aynı mekan ve gün için ayrı opsiyonlar tutabilir.
*Tür:* Kısıt · *Hikayeler:* US-EVT-002

#### BR-EVT-005 · Opsiyonun düşmesi ve yükselme
Bir opsiyon neden girilerek düşürülür. Düşen opsiyonun arkasındaki aktif opsiyonların sırası birer öne çıkar. Her sıra değişikliği işlem geçmişine eski ve yeni sırayla yazılır.
*Tür:* Tetikleyici · *Hikayeler:* US-EVT-003

#### BR-EVT-006 · Etkinliğin opsiyon sırası
Bir etkinliğin opsiyon sırası, opsiyon tuttuğu günlerdeki en büyük sıradır. Etkinlik, tüm günlerinde 1. sıradaysa "1. opsiyon" olarak işaretlenir.
*Tür:* Hesaplama · *Hikayeler:* US-EVT-003, US-EVT-007

#### BR-EVT-007 · Son opsiyonun düşmesi
**Opsiyonda** durumundaki etkinliğin son aktif opsiyonu düşerse etkinlik **Talep** durumuna döner.
*Tür:* Geçiş · *Hikayeler:* US-EVT-003

#### BR-EVT-008 · Opsiyon son tarihi
- Son tarihine P-05 veya daha az kalan aktif opsiyon **yaklaşıyor**, son tarihi geçmiş olan **süresi geçti** olarak işaretlenir. İki işaret listelerde ve etkinlik sayfasında farklı renklerle gösterilir.
- Etkinlik listesi, süresi geçmiş opsiyonu olan etkinliklere göre filtrelenebilir.
- Süresi geçen opsiyon otomatik düşmez ve sırasını korur. Mekan süreyi uzatırsa son tarih güncellenir; değişiklik işlem geçmişine yazılır.
- Süresi geçmiş opsiyonu olan etkinlik onaylanamaz (BR-EVT-009).
*Tür:* Tetikleyici · *Hikayeler:* US-EVT-002, US-EVT-007

#### BR-EVT-009 · Onay koşulu
**Müzakere** → **Onaylı** geçişi için:
- Etkinliğin opsiyonu varsa, opsiyon tuttuğu her günde 1. sırada olmalı ve hiçbir opsiyonunun süresi geçmemiş olmalıdır. Süresi geçmiş opsiyon için kullanıcı önce son tarihi günceller ya da opsiyonu düşürür.
- "Sözleşme imzalandı" elle onayı verilmelidir; onayı veren kullanıcı ve zaman kaydedilir.

Onaylı etkinliğin opsiyonları ancak etkinlik iptal edilirse düşer.
*Tür:* Geçiş · *Hikayeler:* US-EVT-004 · *Değişecek:* S3'te elle onay, imzalı sözleşme kontrolüyle değiştirilir.

#### BR-EVT-010 · Hazırlık koşulu
**Onaylı** → **Hazırlık** geçişi için mekan, kaynak depo ve bağlı bir rider versiyonu (teknik hizmette ihtiyaç listesi versiyonu) bulunmalıdır. Geçiş ihtiyaç hesabını çalıştırır (BR-MRP-003 ve devamı).
*Tür:* Geçiş · *Hikayeler:* US-EVT-005, US-MRP-001

#### BR-EVT-011 · Kurulum uyarısı
**Hazırlık** → **Kurulum** geçişinde çıkışı tamamlanmamış onaylı rezervasyon varsa uyarı gösterilir. Geçiş ancak kullanıcı onaylarsa yapılır.
*Tür:* Geçiş · *Hikayeler:* US-EVT-005

#### BR-EVT-012 · Kapanış koşulu
**Hesaplaşma** → **Kapandı** geçişi için:
- Etkinlik için **Etkinlikte** durumunda birim ya da girişi yapılmamış adet kalmamalıdır. **Kayıp** olarak işaretlenenler engel değildir.
- "Hesaplaşma onaylandı" elle onayı verilmelidir.

*Tür:* Geçiş · *Hikayeler:* US-EVT-005 · *Değişecek:* S4'te elle onay, hesaplaşma kontrolüyle değiştirilir.

#### BR-EVT-013 · Kapanmış etkinliğin değişmezliği
**Kapandı** durumundaki etkinlik ve ona bağlı opsiyon, rider bağlantısı, rezervasyon, çıkış ve giriş kayıtları değiştirilemez.
*Tür:* Kısıt · *Hikayeler:* US-EVT-005

#### BR-EVT-014 · İptal ve etkileri
Etkinlik **Kapandı** dışındaki her durumdan, neden girilerek iptal edilebilir. İptal edilen etkinlik yeniden açılamaz. İptal şu etkileri doğurur:
- Aktif opsiyonlar düşer (BR-EVT-005).
- **Önerildi** ve **Onaylandı** rezervasyonlar **Serbest bırakıldı** olur.
- Etkinlik için **Planlandı** durumundaki transferler iptal edilir; **Yolda** olanlar devam eder.
- **Taslak** dış kiralama siparişleri iptal edilir. **Sipariş verildi** durumundakiler için tedarikçiye elle haber verilmesi gerektiği uyarısı gösterilir.
- Çıkışı yapılmış ekipmanın giriş işlemi açık kalır ve etkinlik "dönmemiş ekipman" olarak işaretlenir.

*Tür:* Tetikleyici · *Hikayeler:* US-EVT-006

#### BR-EVT-015 · Durum geçmişi ve geçersiz geçişler
Her durum geçişi önceki durum, yeni durum, kullanıcı, zaman ve isteğe bağlı notla kaydedilir. [04-state-machines.md](04-state-machines.md)'de tanımlı olmayan bir geçiş sunucuda reddedilir.
*Tür:* Kısıt · *Hikayeler:* US-EVT-004, US-EVT-005

#### BR-EVT-016 · Türe göre zorunlu taraf
**Kendi etkinliği**nde prodüksiyon, **Teknik hizmet**te müşteri zorunludur. Etkinlik türü yalnızca **Talep** durumunda değiştirilebilir.
*Tür:* Kısıt · *Hikayeler:* US-EVT-001

#### BR-EVT-017 · Zaman değişikliği
Etkinliğin başlangıç ve bitiş zamanı **Kapandı** ve **İptal** dışındaki her durumda değiştirilebilir. Değişiklikte:
- Yeni tarih aralığının dışında kalan günlerin opsiyonları "tarih değişti" nedeniyle düşer.
- **Talep**, **Opsiyonda** ve **Müzakere** durumlarında yeni günler için opsiyonlar ayrıca eklenir (BR-EVT-003).
- **Onaylı** ve sonraki durumlarda değişiklik için "Mekan yeni tarihi onayladı" elle onayı zorunludur. Yeni günlerin opsiyonları 1. sıraya kaydedilir (BR-EVT-003).
- İhtiyaç hesabı "güncel değil" olur (BR-MRP-010) ve aşırı rezervasyon kontrolü çalışır (BR-MRP-017).

*Tür:* Tetikleyici · *Hikayeler:* US-EVT-007

### 5.5 Teknik rider (RDR)

#### BR-RDR-001 · Rider satırının hedefi
Her rider satırı tam olarak bir hedef içerir: bir model, bir kategori ya da (yalnızca ihtiyaç listesinde) bir kit. Adet en az 1 olan bir tam sayıdır.
*Tür:* Kısıt · *Hikayeler:* US-RDR-001, US-RDR-005

#### BR-RDR-002 · Muadiller
Muadil yalnızca **Esnek** ve hedefi model olan satıra girilir. Muadiller sıralıdır ve satırın istenen modelinden farklıdır. **Zorunlu** satırın ve kategori satırının muadili olmaz.
*Tür:* Kısıt · *Hikayeler:* US-RDR-001

#### BR-RDR-003 · Versiyonların değişmezliği
Rider'da yapılan her kayıt, 1'den başlayıp birer artan numarayla yeni bir versiyon oluşturur. Kaydedilmiş versiyon değiştirilemez.
*Tür:* Kısıt · *Hikayeler:* US-RDR-002

#### BR-RDR-004 · Etkinliğe versiyon bağlama
**Kendi etkinliği**ne yalnızca kendi prodüksiyonunun bir versiyonu ya da o etkinliğe özel bir versiyon bağlanır. Varsayılan, prodüksiyonun en güncel versiyonudur. Bağlı versiyon değişirse ihtiyaç hesabı "güncel değil" olur (BR-MRP-010).
*Tür:* Kısıt · *Hikayeler:* US-RDR-003

#### BR-RDR-005 · Yeni versiyon işareti
Prodüksiyonda yeni bir versiyon oluştuğunda, eski bir versiyona bağlı ve **Kapandı** ya da **İptal** olmayan etkinlikler "yeni rider versiyonu var" olarak işaretlenir. Bağlı versiyon otomatik değişmez.
*Tür:* Tetikleyici · *Hikayeler:* US-RDR-002

#### BR-RDR-006 · Etkinliğe özel versiyon
Etkinliğe özel versiyon yalnızca etkinliğe o an bağlı versiyondan türetilir ve türetildiği versiyonu saklar. Yalnızca o etkinlikte görünür; prodüksiyonun versiyon listesinde görünmez.
*Tür:* Kısıt · *Hikayeler:* US-RDR-004

#### BR-RDR-007 · Rider kaynağı ve etkinlik türü
**Kendi etkinliği**nin rider kaynağı prodüksiyondur. **Teknik hizmet** etkinliğinin rider kaynağı müşteridir; bu rider (ihtiyaç listesi) doğrudan etkinliğe bağlıdır.
*Tür:* Kısıt · *Hikayeler:* US-RDR-005

#### BR-RDR-008 · Rider'ı kim değiştirir
Rider'ı yalnızca teknik müdür oluşturur ve değiştirir.
*Tür:* Yetki · *Hikayeler:* US-RDR-001

### 5.6 Ekipman kataloğu ve stok (EQP)

#### BR-EQP-001 · Takip tipinin değişmezliği
Bir modelin birimi ya da adetli stok kaydı oluştuktan sonra takip tipi değiştirilemez.
*Tür:* Kısıt · *Hikayeler:* US-EQP-002

#### BR-EQP-002 · Kategori hiyerarşisi
Kategori hiyerarşisinde döngü olamaz. Bir kategoriyi hedefleyen her kural o kategorinin tüm alt kategorilerini de kapsar.
*Tür:* Kısıt · *Hikayeler:* US-EQP-001

#### BR-EQP-003 · Kit yapısı
Bir kit kendini doğrudan ya da dolaylı olarak içeremez. Kitin ağırlığı ve güç tüketimi içeriğinin toplamıdır; içerikte değeri girilmemiş model varsa toplam "eksik veri" olarak işaretlenir.
*Tür:* Kısıt, Hesaplama · *Hikayeler:* US-EQP-005

#### BR-EQP-004 · Etiket kodu
Her birime ve kasaya sistem tekil bir etiket kodu atar. Birim ve kasa kodları önekleriyle ayırt edilir. Bir etiket kodu hiçbir koşulda tekrar kullanılmaz. QR yalnızca etiket kodunu içerir.
*Tür:* Kısıt · *Hikayeler:* US-EQP-003, US-EQP-006, US-EQP-007, US-WHS-007

#### BR-EQP-005 · Adetli stok düzeltmesi
Adetli stok eksiye düşemez ve her düzeltmenin nedeni kaydedilir. Düzeltme sonrasında stok onaylı rezervasyonların altına düşüyorsa uyarı gösterilir ve aşırı rezervasyon kontrolü çalışır (BR-MRP-017).
*Tür:* Kısıt · *Hikayeler:* US-EQP-004

#### BR-EQP-006 · Kasa bütünlüğü
Bir birim aynı anda en fazla bir kasada olur. Bir kasa kendini doğrudan ya da dolaylı olarak içeremez. Kasa ile içeriği her zaman aynı konumdadır: kasa okutularak taşındığında içindeki her şey de taşınmış olur.
*Tür:* Kısıt · *Hikayeler:* US-EQP-006, US-WHS-002

#### BR-EQP-007 · Eksik kasa
Gerçek içeriği standart içeriğinden az olan kasa "eksik" olarak işaretlenir.
*Tür:* Hesaplama · *Hikayeler:* US-EQP-006

#### BR-EQP-008 · Elle birim durumu değişiklikleri
Birim durumu elle yalnızca şu yönlerde değiştirilir ve her değişiklikte neden zorunludur:

| Durumdan | Duruma |
|---|---|
| Bakımda | Depoda (onarıldı), Hurda |
| Depoda | Kayıp, Hurda |
| Kayıp | Depoda (bulundu) |
| Etkinlikte | Kayıp (yalnızca teknik müdür, bkz. BR-WHS-010) |

Diğer durum değişiklikleri yalnızca çıkış, giriş ve transfer işlemleriyle olur. Tam tablo [04-state-machines.md](04-state-machines.md)'de.
*Tür:* Geçiş · *Hikayeler:* US-WHS-006

#### BR-EQP-009 · Sahipliğe göre ayrım
Sahipliği **Dış kiralama** olan birim ve adetli stok; şirket stok toplamlarına, müsaitlik hesabına (BR-MRP-002) ve kayıp raporuna (BR-EQP-010) girmez.
*Tür:* Kısıt · *Hikayeler:* US-WHS-007, US-EQP-008

#### BR-EQP-010 · Kayıp oranı
Bir adetli model için kayıp oranı = seçilen dönemde sayım farkı olarak kaydedilen eksik adet / aynı dönemde çıkışı yapılan adet. Varsayılan dönem P-10'dur; en yüksek orana sahip P-11 model öne çıkarılır.
*Tür:* Hesaplama · *Hikayeler:* US-EQP-009

### 5.7 Depo işlemleri (WHS)

#### BR-WHS-001 · Toplama listesinin oluşumu
Toplama listesi, etkinliğin seçilen depodaki onaylı rezervasyonlarından oluşur. Bir kasanın standart içeriğinin tamamı, rezervasyonun henüz karşılanmamış kısmına sığıyorsa kasa tek satır olarak gösterilir. Kalan ihtiyaç model ve adet satırlarıyla gösterilir.
*Tür:* Hesaplama · *Hikayeler:* US-WHS-001

#### BR-WHS-002 · Okutmanın anında kaydedilmesi
Her okutma anında kaydedilir ve birimin durumunu hemen değiştirir. Çıkışı "tamamlamak" yalnızca işlemi kapatır ve eksik kalanları raporlar.
*Tür:* Kısıt · *Hikayeler:* US-WHS-002, US-WHS-005

#### BR-WHS-003 · Çıkış koşulları
Bir birimin çıkışı yapılabilmesi için:
- durumu **Depoda** olmalı,
- konumu kullanıcının bağlı olduğu depo olmalı,
- sahipliği **Şirket** olmalı ya da siparişi o etkinliğe bağlı bir dış kiralama olmalıdır.

Kasa okutulduğunda içindeki her birim bu koşulları sağlamalıdır. Sağlamayan birim varsa kasanın çıkışı engellenir ve sorunlu birimler listelenir.
*Tür:* Kısıt · *Hikayeler:* US-WHS-002, US-WHS-007

#### BR-WHS-004 · Rezervasyonla eşleşme
Okutulan birim ya da adet, etkinliğin o depodaki onaylı rezervasyonunda aynı modelin karşılanmamış kısmına yazılır. Rezervasyonda olmayan bir model okutulursa ya da rezerve adet aşılırsa uyarı gösterilir. Kullanıcı onaylarsa müsaitlik kontrol edilir (BR-MRP-002) ve fark, okutan kullanıcı adına onaylı rezervasyon olarak eklenir.
*Tür:* Kısıt, Tetikleyici · *Hikayeler:* US-WHS-002

#### BR-WHS-005 · Çıkışın geri alınması
Yanlış yapılan çıkış, etkinlik **Canlı** durumuna geçmeden geri alınabilir; birim yeniden **Depoda** olur. Geri alma işlem geçmişine yazılır.
*Tür:* Kısıt · *Hikayeler:* US-WHS-002

#### BR-WHS-006 · Eşzamanlı okutma
Aynı birim ya da kasa için aynı anda yapılan iki işlemden yalnızca ilk kaydedilen geçerli olur. İkinci işlem reddedilir ve ilk işlemi yapan kullanıcı ile zamanı gösterilir.
*Tür:* Kısıt · *Hikayeler:* US-WHS-005

#### BR-WHS-007 · Girişin yapıldığı depo
Giriş, okutan kullanıcının bağlı olduğu depoya yapılır; birden fazla bağlı deposu varsa kullanıcı seçer. Bu depo, etkinliğin kaynak deposundan farklı olabilir.
*Tür:* Kısıt · *Hikayeler:* US-WHS-003

#### BR-WHS-008 · Adetli girişte fark
Adetli bir kalem için beklenen giriş adedi = çıkışı yapılan adet − daha önce girişi yapılan adet.
- Girilen adet azsa fark, etkinlik ve depoyla birlikte sayım farkı olarak kaydedilir ve şirket stoğundan düşer.
- Girilen adet fazlaysa fazla kabul edilir, stoğa eklenir ve sayım farkı (fazla) olarak kaydedilir.

*Tür:* Hesaplama, Tetikleyici · *Hikayeler:* US-WHS-003, US-WHS-004

#### BR-WHS-009 · Hasar kaydı
Girişte hasarlı işaretlenen birim için açıklamalı hasar kaydı açılır ve birim **Bakımda** durumuna geçer. Hasarlı adetli kalemler **Bakımda** adedine eklenir.
*Tür:* Tetikleyici · *Hikayeler:* US-WHS-003

#### BR-WHS-010 · Dönmeyen birimin kayıp sayılması
**Etkinlikte** durumundaki birimi **Kayıp** olarak yalnızca teknik müdür işaretler ve bu yalnızca etkinlik **Söküm** ya da sonraki bir durumdayken yapılabilir.
*Tür:* Yetki · *Hikayeler:* US-WHS-003

#### BR-WHS-011 · Transferin işleyişi
- Transfer çıkışı yalnızca gönderen depoda, varışı yalnızca alan depoda okutulur.
- İlk çıkış okutmasıyla transfer **Yolda** olur.
- Tüm kalemlerin varışı okutulduğunda ya da alan depo eksiklerle tamamladığında transfer **Tamamlandı** olur. Eksikler sayım farkı olarak transferle birlikte kaydedilir (BR-WHS-008).
- Planlanan varış zamanı geçmiş ve tamamlanmamış transfer "gecikmiş" olarak işaretlenir.

*Tür:* Geçiş · *Hikayeler:* US-WHS-004

#### BR-WHS-012 · Elle transfer
Etkinliğe bağlı olmayan transfer oluşturulurken gönderen depodaki müsaitlik kontrol edilir. Planlanan çıkış zamanından itibaren bu adetler gönderen depoda ayrılmış sayılır.
*Tür:* Kısıt · *Hikayeler:* US-WHS-004

#### BR-WHS-013 · Dış kiralamanın QR ile takibi
- QR ile takip yalnızca teslim yeri depo olan sipariş satırları için seçilebilir.
- Bu satırlar için oluşturulan birim ve adetli stokların sahipliği **Dış kiralama** olur ve yalnızca siparişin bağlı olduğu etkinlikte kullanılabilir (BR-EQP-009).
- İadede birimler **Tedarikçiye iade edildi** durumuna geçer; eksikler sayım farkı olarak kaydedilir.
- Tüm satırlar iade edilince sipariş **İade edildi** olur.

*Tür:* Kısıt, Tetikleyici · *Hikayeler:* US-WHS-007, US-MRP-005

### 5.8 İhtiyaç hesabı ve rezervasyon (MRP)

#### BR-MRP-001 · Rezervasyon aralığı
Rezervasyon aralığı, etkinliğin başlangıç zamanından hazırlık payı kadar önce başlar ve bitiş zamanından dönüş payı kadar sonra biter. Paylar dakika hassasiyetinde bir süredir; arayüzde gün ve saat olarak girilir ve gösterilir (ör. "10 saat", "1 gün 6 saat"). Yeni etkinlikte paylar varsayılan değerlerle (P-06, P-07) dolar ve etkinlik bazında değiştirilebilir. Pay sıfır olabilir, eksi olamaz. Etkinliğin saati girilmemişse başlangıç 00:00, bitiş 23:59 kabul edilir.
*Tür:* Hesaplama · *Hikayeler:* US-MRP-001, US-MRP-008, US-SYS-007, US-EVT-001

#### BR-MRP-002 · Müsaitlik
Bir model için bir depoda belirli bir andaki müsait adet:

```
Müsait(an) = Havuz(an)
           − o anı kapsayan onaylı rezervasyonların adedi
           − planlanan çıkış zamanı o andan önce olup henüz yola çıkmamış giden transferlerin adedi
```

**Havuz(an)**, sahipliği **Şirket** olan ve şu kalemlerin toplamıdır:
- Konumu o depo olan ve durumu **Depoda** olan birimler ile o depodaki adetli stok.
- O depodan bir etkinliğe çıkmış, etkinliğin rezervasyon aralığı o an henüz bitmemiş birimler ve adetler. Bunların kaynak depoya döneceği varsayılır.
- O depoya gelen, gecikmemiş ve planlanan varış zamanı o andan önce olan transferlerdeki kalemler.

Bir zaman aralığındaki müsaitlik, aralıktaki tüm anların en düşük değeridir. Hesap her an için değil, değerin değişebildiği anlarda (rezervasyon ve transferlerin başlangıç ve bitişlerinde) yapılır. **Bakımda**, **Kayıp** ve **Hurda** birimler; gecikmiş transferler; rezervasyon aralığı bittiği halde dönmemiş birimler havuza girmez. **Önerildi** durumundaki rezervasyonlar müsaitliği düşürmez.
*Tür:* Hesaplama · *Hikayeler:* US-MRP-001, US-MRP-007, US-EQP-008

#### BR-MRP-003 · Net ihtiyaç
Her rider satırı için net ihtiyaç = en az 0 olmak üzere brüt ihtiyaç − mekan ekipmanından karşılanan adet. Mekan ekipmanı BR-MRP-004'teki eşleşme kurallarıyla, BR-MRP-005'teki satır sırasıyla dağıtılır.
*Tür:* Hesaplama · *Hikayeler:* US-MRP-001, US-VEN-002

#### BR-MRP-004 · Model eşleşmesi
- **Zorunlu** satır yalnızca istenen modelle karşılanır.
- **Esnek** satır önce istenen modelle, sonra muadillerle, girilme sırasıyla karşılanır.
- Kategori satırı, kategori ve alt kategorilerindeki herhangi bir modelle karşılanır. Modeller müsait adedi en yüksek olandan başlanarak kullanılır; eşitlikte model adı sırası geçerlidir.
- Kit satırı içeriğine açılır ve her içerik satırı **Zorunlu** satır gibi işlenir.

*Tür:* Hesaplama · *Hikayeler:* US-MRP-001, US-RDR-005

#### BR-MRP-005 · Karşılama sırası
Satırlar özelden genele işlenir: önce **Zorunlu** ve kit satırları, sonra **Esnek** satırlar, en son kategori satırları. Aynı grupta rider'daki sıra geçerlidir. Böylece genel bir satır, özel bir satırın ihtiyaç duyduğu modeli tüketmez.

Her satırın net ihtiyacı şu kaynak sırasıyla karşılanır:
1. Kaynak depo; BR-MRP-004'teki model tercih sırasıyla.
2. Diğer depolar; müsait adedi en yüksek depodan başlayarak, aynı model tercih sırasıyla (transfer önerisi, BR-MRP-006).
3. Dış kiralama önerisi (BR-MRP-008).

Muadil model kaynak depoda varsa, istenen modeli başka depodan transferle getirmek yerine muadil kullanılır. Muadil tanımı gereği kabul edilebilir bir karşılamadır ve transfer maliyeti yaratmaz.
*Tür:* Hesaplama · *Hikayeler:* US-MRP-001

#### BR-MRP-006 · Transfer önerisi
Diğer depolardan karşılanan adetler, o depodan kaynak depoya bir transfer önerisi olarak sunulur. Etkinliğin rezervasyonu kaynak depoda oluşur.
*Tür:* Hesaplama · *Hikayeler:* US-MRP-003

#### BR-MRP-007 · Transfer tarihleri
Transferin planlanan varış zamanı, etkinliğin rezervasyon aralığının başlangıcından sonra olamaz. Transferdeki adetler, planlanan çıkış zamanından itibaren gönderen depoda ayrılmış sayılır. Planlanan varış zamanından itibaren hedef depoda etkinliğe rezerve sayılır.
*Tür:* Kısıt · *Hikayeler:* US-MRP-003

#### BR-MRP-008 · Dış kiralama önerisi ve sayılması
Hiçbir depodan karşılanamayan net ihtiyaç için istenen modelle (kategori satırında kategoriyle) dış kiralama önerisi oluşur. **Sipariş verildi** ve **Teslim alındı** durumundaki sipariş satırları ihtiyaç hesabında karşılanmış sayılır; **Taslak** satırlar sayılmaz.
*Tür:* Hesaplama · *Hikayeler:* US-MRP-005

#### BR-MRP-009 · Yeniden hesaplama
Yeniden hesaplamada **Önerildi** rezervasyonlar silinip yeniden üretilir. **Onaylandı** rezervasyonlar korunur ve net ihtiyaçtan önce düşülür. Yeni net ihtiyacı aşan onaylı rezervasyonlar "fazla" olarak işaretlenir ama otomatik serbest bırakılmaz.
*Tür:* Hesaplama · *Hikayeler:* US-MRP-001

#### BR-MRP-010 · Güncel olmayan hesap
Şu değişikliklerden sonra etkinliğin ihtiyaç hesabı "güncel değil" olarak işaretlenir: bağlı rider versiyonu, mekan, mekan ekipmanı, etkinliğin başlangıç veya bitiş zamanı, hazırlık veya dönüş payı, kaynak depo. Onaylı rezervasyonları etkileyebileceği için sistem hesabı kendiliğinden yeniden çalıştırmaz.
*Tür:* Tetikleyici · *Hikayeler:* US-VEN-002, US-RDR-003, US-MRP-001, US-MRP-008

#### BR-MRP-011 · Hesabın kaydı
Her hesap çalıştığı zaman, çalıştıran kullanıcı, kullanılan rider versiyonu ve satır bazında sonuçlarıyla kaydedilir.
*Tür:* Tetikleyici · *Hikayeler:* US-MRP-001

#### BR-MRP-012 · Rezervasyon hangi durumlarda yapılır
Rezervasyon yalnızca **Onaylı**, **Hazırlık**, **Kurulum**, **Canlı** ve **Söküm** durumlarındaki etkinlikler için oluşturulur ve onaylanır. İhtiyaç hesabı **Hazırlık**'a geçişte otomatik çalışır; **Onaylı**, **Hazırlık** ve **Kurulum** durumlarında elle çalıştırılabilir. Böylece kıt ekipman etkinlik kesinleşir kesinleşmez ayrılabilir (bkz. R-03).
*Tür:* Kısıt · *Hikayeler:* US-MRP-001, US-MRP-002

#### BR-MRP-013 · Onayda yeniden doğrulama
Rezervasyon onaylanırken müsaitlik (BR-MRP-002) yeniden hesaplanır. Onaylanan adet müsaitliği aşıyorsa onay reddedilir ve güncel müsaitlik gösterilir.
*Tür:* Kısıt · *Hikayeler:* US-MRP-002

#### BR-MRP-014 · Rezervasyon seviyesi ve düzenleme
Rezervasyon model, depo ve adet seviyesinde yapılır. Seri no'lu modellerde birim, çıkışta okutulunca rezervasyona bağlanır. Onaydan önce model yalnızca satırın eşleşme kuralına (BR-MRP-004) uyan başka bir modelle değiştirilebilir.
*Tür:* Kısıt · *Hikayeler:* US-MRP-002

#### BR-MRP-015 · Serbest bırakma
Onaylı rezervasyon neden girilerek serbest bırakılır. **Serbest bırakıldı** son durumdur.
*Tür:* Geçiş · *Hikayeler:* US-MRP-002

#### BR-MRP-016 · Rakip talep çakışması
İhtiyaç hesabında bir satırın net ihtiyacı hiçbir depodan karşılanamıyorsa ve aynı model örtüşen zaman aralığında başka etkinliklere onaylı rezervasyonla ayrılmışsa, **Rakip talep** çakışması oluşur. Çakışma, ayrılmış stoğu tutan etkinlikleri listeler. Rakip rezervasyon yoksa eksik yalnızca dış kiralama önerisidir; çakışma sayılmaz.
*Tür:* Tetikleyici · *Hikayeler:* US-MRP-004

#### BR-MRP-017 · Aşırı rezervasyon çakışması
Bir model ve depo için herhangi bir anda Müsait(an) sıfırın altına düşerse **Aşırı rezervasyon** çakışması oluşur. Kontrol şu olaylardan sonra, ayrıca her gün P-12'de çalışır: birim durumu değişikliği, adetli stok düzeltmesi, etkinlik zamanı değişikliği, hazırlık veya dönüş payı değişikliği, transfer gecikmesi, rezervasyon aralığı bittiği halde dönmemiş ekipman.
*Tür:* Tetikleyici · *Hikayeler:* US-MRP-004, US-WHS-006, US-EQP-004, US-MRP-008

#### BR-MRP-018 · Çakışmanın kapanması
Nedeni ortadan kalkan çakışma otomatik kapanır. Çakışma, not girilerek elle "kabul edildi" olarak işaretlenebilir. Kabul edilen çakışmada eksik adet artarsa çakışma yeniden açılır.
*Tür:* Geçiş · *Hikayeler:* US-MRP-004

#### BR-MRP-019 · Hesap süresi
50 satırlık bir rider ve 5.000 birimlik stokla ihtiyaç hesabı P-09'dan kısa sürer.
*Tür:* Kısıt · *Hikayeler:* US-MRP-001

#### BR-MRP-020 · Karşılama raporunun dayanağı
Rider karşılama raporu, son ihtiyaç hesabından ve güncel onaylı rezervasyonlardan üretilir. Hesap "güncel değil" ise raporun başlığında bu belirtilir.
*Tür:* Hesaplama · *Hikayeler:* US-MRP-006

## 6. Sonraki sürümlerde değişecek kurallar

| Kural | S1'deki hali | Değişeceği sürüm | Yeni hali |
|---|---|---|---|
| BR-EVT-009 | "Sözleşme imzalandı" elle onayı | S3 | İmzalı sözleşme kaydı kontrolü |
| BR-EVT-012 | "Hesaplaşma onaylandı" elle onayı | S4 | Onaylı hesaplaşma kontrolü |
| BR-MRP-005 | Diğer depolar müsait adede göre sıralanır | S5 | Mesafe ve nakliye süresine göre sıralanır |
| BR-MRP-006 | Transfer her zaman kaynak depoya yapılır | S5 | Başka depodan doğrudan mekana sevkiyat mümkün olur |
| BR-MRP-017 | Yalnızca ekipman çakışmaları | S2 | Ekip, mekan ve araç çakışmaları eklenir |

## 7. Kararlar

| No | Soru | Karar | Gerekçe |
|---|---|---|---|
| R-01 | Hazırlık ve dönüş payı | Varsayılan 1'er gün. Paylar saat hassasiyetinde girilir (ör. "10 saat"). Varsayılanı sistem yöneticisi ayarlardan değiştirir (US-SYS-007); her etkinlikte ayrıca değiştirilebilir (US-MRP-008). Rezervasyon ve müsaitlik hesapları gün yerine dakika hassasiyetinde zaman aralıklarıyla yapılır (BR-MRP-001, BR-MRP-002). | Küçük bir iş saatler içinde hazırlanırken büyük bir prodüksiyon günler sürer. Saat hassasiyeti, sabah dönüp akşam yeniden çıkan ekipmanın da doğru planlanmasını sağlar. |
| R-02 | Son tarihi geçen opsiyon | Otomatik düşmez. "Yaklaşıyor" ve "süresi geçti" olarak farklı renklerle işaretlenir, listede filtrelenebilir. Süresi geçmiş opsiyonla etkinlik onaylanamaz; kullanıcı son tarihi günceller ya da opsiyonu düşürür (BR-EVT-008, BR-EVT-009). | Mekanlar süreyi sık uzatır, bu yüzden otomatik düşürmek yanlış olur. Ama süresi geçmiş bir opsiyonla onay vermek, mekanın tarihi başkasına vermiş olma riskini gözden kaçırır. |
| R-03 | Ekipman ne zaman ayrılabilir | Etkinlik **Onaylı** olur olmaz (BR-MRP-012). İhtiyaç hesabı **Onaylı** durumunda elle, **Hazırlık**'a geçişte otomatik çalışır. | Ekipmanı ilk onaylanan etkinlik ayırır. Aksi halde üç ay önce kesinleşen bir konser, sonradan onaylanan bir iş tarafından ekipmansız bırakılabilir. |
| R-04 | Teknik hizmette opsiyon | Eklenebilir, isteğe bağlıdır (BR-EVT-001). | Şirket müşteri adına da mekan tutabiliyor. |

## 8. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-09-24 | v0.1 | İlk taslak: S1 kuralları |
| 2026-09-24 | v1.0 | Açık sorular karara bağlandı: rezervasyon ve müsaitlik hesapları saat hassasiyetine geçti; süresi geçmiş opsiyon işaretleri ve onay engeli eklendi; teknik hizmette opsiyon isteğe bağlı oldu. |
