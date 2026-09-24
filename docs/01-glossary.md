# 01 — Terimler Sözlüğü

> **Durum:** v1.5 · **Son güncelleme:** 2026-09-24
> **Kararlar:** [Bölüm 5](#5-kararlar)

## 1. Bu belge ne işe yarar

Projedeki her kavramın tek bir anlamı, tek bir Türkçe adı ve tek bir kod adı olur. Arayüzde, belgelerde, kodda, veritabanında ve testlerde aynı kavram için aynı ad kullanılır. Böylece "rezervasyon" dendiğinde herkes aynı şeyi anlar ve kodda aynı şeyin üç farklı adı birikmez.

Sözlük tüm sistemi kapsar; her terimin hangi sürümde devreye girdiği [00-scope.md](00-scope.md)'deki sürüm planına göre belirtilir.

## 2. Kurallar

1. **Tek kavram, tek ad.** Tabloda "Terim" sütunundaki ad arayüzde ve belgelerde, "Kod adı" sütunundaki ad kodda kullanılır. Eş anlamlılar kullanılmaz; yasaklı olanlar [Bölüm 4](#4-kullanılmayan-terimler)'te listelenir.
2. **Kod adları İngilizce, PascalCase ve tekildir** (sınıf adı biçiminde). Tablo, kolon, endpoint ve dosya adlarına dönüşüm kuralları `standards/naming.md`'de tanımlanacak.
3. **Sektör terimleri korunur.** Sektörde Türkçe karşılığı yerleşmemiş terimler (rider, day sheet, call sheet, run of show, stage plot, input list) arayüzde de İngilizce kalır.
4. **Önce sözlük, sonra kod.** Yeni bir kavram koda girmeden önce bu sözlüğe eklenir. PR kontrol listesinde bu madde yer alacak.
5. **Durum ve varlık adları çakışmaz.** Bir durum adı, başka bir varlığın adıyla aynı olamaz. Örneğin "Etkinlik" bir varlık olduğu için etkinliğin durumlarından biri "Etkinlik" olamaz.
6. **"Event" kelimesi yalnızca etkinlik varlığı içindir.** Modüller arası haberleşmedeki domain event'ler kodda her zaman `DomainEvent` sonekiyle adlandırılır (ör. `EventConfirmedDomainEvent`).

## 3. Terimler

### 3.1 Genel ve sistem

| Terim | Kod adı | Tanım | Sürüm |
|---|---|---|---|
| Kullanıcı | `User` | Sisteme giriş yapan şirket çalışanı. Dış kişiler kullanıcı olamaz. | S1 |
| Rol | `Role` | Kullanıcının yetkilerini belirleyen sabit görev tanımı (ör. Depo sorumlusu). | S1 |
| Yetki | `Permission` | Bir rolün yapabildiği tek bir işlem (ör. rezervasyon onaylama). | S1 |
| İşlem geçmişi | `AuditLog` | Bir kayıtta kimin, neyi, ne zaman, hangi eski değerden hangi yeni değere değiştirdiğinin kaydı. | S1 |
| Pasif | `IsActive = false` | Silinmek yerine kullanımdan kaldırılmış kayıt. Geçmiş kayıtlarda görünür, yeni işlemlerde seçilemez. Başka kayıtların bağlı olduğu hiçbir kayıt silinmez. | S1 |
| Durum | `Status` | Bir varlığın yaşam döngüsündeki şu anki aşaması. Her varlığın kendi durum listesi vardır (ör. `EventStatus`). | S1 |
| Belge | `Document` | Sisteme yüklenen dosya (rider PDF'i, stage plot, sözleşme). | S2 |
| Bildirim | `Notification` | Kullanıcıya uygulama içinde gösterilen uyarı. | S2 |

### 3.2 Kişi ve firma

| Terim | Kod adı | Tanım | Sürüm |
|---|---|---|---|
| Taraf | `Party` | Şirketin ilişki kurduğu her kişi veya firma. Sanatçı, tedarikçi, müşteri, sponsor ve crew hepsi taraftır. | S1 |
| Kişi | `Person` | Gerçek kişi olan taraf. | S1 |
| Firma | `Organization` | Tüzel kişi olan taraf. | S1 |
| Taraf rolü | `PartyRole` | Bir tarafın şirketle ilişkisindeki rolü. Bir taraf birden çok role sahip olabilir. Değerler aşağıdaki satırlardadır. | S1 |
| Sanatçı | `PartyRole.Artist` | Sahneye çıkan kişi ya da grup. | S1 |
| Ajans | `PartyRole.Agency` | Sanatçıyı temsil eden ve booking görüşmelerini yürüten firma. | S1 |
| Mekan işletmecisi | `PartyRole.VenueOperator` | Bir veya daha fazla mekanı işleten taraf. | S1 |
| Tedarikçi | `PartyRole.Supplier` | Şirkete ekipman kiralayan ya da hizmet satan taraf. | S1 |
| Müşteri | `PartyRole.Customer` | Teknik hizmet kolunda şirketten hizmet satın alan taraf. Sanatçı müşteri değildir. | S1 |
| Crew | `PartyRole.Crew` | Şirket için sahada çalışan sabit veya freelance kişi. | S2 |
| Sponsor | `PartyRole.Sponsor` | Bir etkinliğe sponsorluk paketi alan taraf. | S3 |
| İletişim bilgisi | `ContactPoint` | Bir tarafın telefon, e-posta veya adres kaydı. | S1 |
| İletişim kişisi | `OrganizationContact` | Bir firma adına muhatap olunan kişi; kendisi de bir `Person` tarafıdır. | S1 |

### 3.3 Mekan

| Terim | Kod adı | Tanım | Sürüm |
|---|---|---|---|
| Mekan | `Venue` | Etkinliğin yapıldığı yer (salon, açık hava alanı, kulüp). | S1 |
| Kapasite | `Capacity` | Mekanın izin verilen en yüksek seyirci sayısı. | S1 |
| Yükleme kapısı | `LoadingDock` | Ekipmanın mekana indirildiği kapı ve rampa bilgisi. | S1 |
| Güç kapasitesi | `PowerCapacity` | Mekanın sağlayabildiği elektrik, amper cinsinden. | S1 |
| Sessizlik saati | `Curfew` | Mekanda sesin kesilmesi gereken en geç saat. | S1 |
| Mekan ekipmanı | `VenueEquipment` | Mekanın kendine ait olup etkinliğe verdiği ekipman (house equipment). İhtiyaç hesabında brüt ihtiyaçtan düşülür. | S1 |
| Opsiyon | `VenueHold` | Mekanın belirli bir tarih için bir etkinliğe verdiği ön rezervasyon (hold). Aynı tarihe birden çok opsiyon verilebilir. | S1 |
| Dış opsiyon | `VenueHold` (`EventId` boş) | Mekanın aynı tarih için başka bir firmaya verdiği opsiyon. Sahibi bilinmeyebilir. Kendi opsiyonumuzun sırasını doğru tutmak için kaydedilir. | S1 |
| Opsiyon son tarihi | `HoldExpiresAt` | Mekanın opsiyon için verdiği karar son tarihi. | S1 |
| Süresi geçmiş opsiyon | `VenueHold.IsExpired` | Son tarihi geçtiği halde düşürülmemiş opsiyon. Sırasını korur ama etkinlik bununla onaylanamaz. | S1 |
| Opsiyon sırası | `HoldRank` | Aynı mekan ve tarihteki opsiyonlar arasındaki öncelik (1. opsiyon, 2. opsiyon...). | S1 |
| Opsiyon yükselmesi | `HoldPromotion` | Öndeki opsiyon düştüğünde arkadakinin sırasının bir öne çıkması. | S1 |
| Opsiyonun düşmesi | `HoldRelease` | Bir opsiyonun iptal edilmesi ya da süresinin dolması. | S1 |
| Opsiyon durumu | `HoldStatus` | **Aktif** (`Active`), **Kesinleşti** (`Confirmed`, etkinlik onaylanınca; elle düşürülemez), **Düştü** (`Released`). | S1 |

### 3.4 Sanatçı, prodüksiyon ve turne

| Terim | Kod adı | Tanım | Sürüm |
|---|---|---|---|
| Prodüksiyon | `Production` | Bir sanatçının belirli bir şovu (ör. "Akustik set", "Full band stadyum şovu"). Rider prodüksiyona bağlıdır. | S1 |
| Turne | `Tour` | Aynı prodüksiyonun ardışık tarihlerde farklı şehirlerde yapılan etkinlikleri. | S5 |
| Turne tarihi | `TourDate` | Turnenin tek bir durağı; bir etkinliğe karşılık gelir. | S5 |

### 3.5 Etkinlik

| Terim | Kod adı | Tanım | Sürüm |
|---|---|---|---|
| Etkinlik | `Event` | Belirli tarih(ler)de belirli bir mekanda gerçekleşen, şirketin sorumluluk aldığı iş. Sistemin merkez varlığıdır. | S1 |
| Etkinlik zamanı | `StartsAt`, `EndsAt` | Mekanın etkinlik için bizde olduğu süre: kurulum başlangıcından söküm bitişine (tarih ve saat). Saat girilmezse başlangıç 00:00, bitiş 23:59 kabul edilir. Depodaki hazırlık ve yol hazırlık payına, dönüş yolu ve depodaki kontrol dönüş payına dahildir. | S1 |
| Etkinlik türü | `EventKind` | Etkinliğin hangi gelir koluna ait olduğu: **Kendi etkinliği** (`Promoted`) veya **Teknik hizmet** (`TechnicalService`). | S1 |
| Operasyon geçiş modu | `TransitionMode` | Etkinliğin Kurulum, Canlı, Söküm ve Hesaplaşma geçişlerinin **Elle** (`Manual`) mi, zamanı gelince **Otomatik** (`Automatic`) mi yapılacağı. Varsayılanı ayarlardan gelir, etkinlik bazında değiştirilir. | S1 |
| Kaynak depo | `SourceWarehouse` | Etkinliğin ekipmanını hazırlayıp gönderen depo. İhtiyaç önce buradan karşılanmaya çalışılır. | S1 |
| Seans | `Performance` | Bir etkinlik içindeki tek bir gösterim (ör. matine ve akşam seansı). | S2 |
| İptal nedeni | `CancellationReason` | Etkinliğin neden iptal edildiğinin kaydı. | S1 |
| Durum geçmişi | `EventStatusHistory` | Etkinliğin geçtiği her durumun, geçiş zamanı ve geçişi yapan kullanıcıyla kaydı. | S1 |

#### Etkinlik durumları (`EventStatus`)

Geçiş kuralları `04-state-machines.md`'de tanımlanacak.

| Terim | Kod adı | Anlamı |
|---|---|---|
| Talep | `Inquiry` | İlk görüşme; henüz mekan tarihi tutulmadı. |
| Opsiyonda | `HoldPlaced` | Mekandan tarih için opsiyon alındı. |
| Müzakere | `Negotiating` | Sanatçı veya müşteriyle şartlar görüşülüyor. |
| Onaylı | `Confirmed` | Etkinlik kesinleşti. |
| Hazırlık | `Advancing` | Mekan, sanatçı ve ekiple tüm teknik ve operasyonel detaylar netleştiriliyor (advance). Ekipman ihtiyacı bu aşamada hesaplanır. |
| Kurulum | `LoadIn` | Ekipman mekana geldi, sahne kuruluyor. |
| Canlı | `Live` | Etkinlik gerçekleşiyor. |
| Söküm | `LoadOut` | Ekipman sökülüyor ve depoya dönüyor. |
| Hesaplaşma | `Settling` | Gelir, gider ve sanatçı payı hesaplanıyor. |
| Kapandı | `Closed` | Tüm ekipman döndü, hesaplaşma onaylandı; kayıt artık değişmez. |
| İptal | `Cancelled` | Etkinlik herhangi bir aşamada iptal edildi. |

#### Etkinlik günü zaman noktaları

| Terim | Kod adı | Tanım | Sürüm |
|---|---|---|---|
| Kurulum başlangıcı | `StartsAt` | Etkinlik zamanının başlangıcı; ekibin mekana girip kuruluma başladığı an. | S1 |
| Ses provası | `SoundcheckAt` | Sanatçının ses provası saati. | S2 |
| Kapı açılışı | `DoorsAt` | Seyircinin içeri alınmaya başladığı saat. Otomatik modda Canlı geçişini tetikler. | S1 |
| Sahne saati | `SetTimeAt` | Sanatçının sahneye çıkış saati. | S2 |
| Söküm başlangıcı | `LoadOutAt` | Sökümün başladığı saat. Otomatik modda Söküm geçişini tetikler. | S1 |
| Söküm bitişi | `EndsAt` | Etkinlik zamanının sonu; sökümün bitip mekanın boşaltıldığı an. | S1 |

### 3.6 Teknik rider

| Terim | Kod adı | Tanım | Sürüm |
|---|---|---|---|
| Rider | `Rider` | Bir prodüksiyonun ya da teknik hizmet müşterisinin ekipman ihtiyaç listesi. Kaynak müşteri olduğunda arayüzde **İhtiyaç listesi** olarak gösterilir; yapısı ve kod adı aynıdır. | S1 |
| Rider kaynağı | `RiderSource` | Rider'ın kime ait olduğu: prodüksiyon (`Production`) veya müşteri (`Customer`). | S1 |
| Rider versiyonu | `RiderVersion` | Rider'ın belirli bir tarihteki değiştirilemez hali. Rider güncellenince yeni versiyon oluşur. | S1 |
| Etkinliğe özel versiyon | `RiderVersion` (`EventId` dolu) | Belirli bir etkinlik için, prodüksiyon versiyonundan türetilerek değiştirilmiş versiyon. | S1 |
| Rider satırı | `RiderLine` | Rider'da tek bir ihtiyaç: bir ekipman modeli ya da kategorisi ve adedi. | S1 |
| Satır esnekliği | `LineFlexibility` | Satırın **Zorunlu** (`Required`, yalnızca belirtilen model) mi, **Esnek** (`Flexible`, muadil kabul edilir) mi olduğu. | S1 |
| Muadil | `EquivalentModel` | Bir rider satırında istenen modelin yerine kabul edilen başka model. | S1 |
| Input list | `InputList` | Sahnedeki her ses kaynağının mikser kanalına eşleştiği liste. | S2 |
| Hospitality rider | `HospitalityRider` | Sanatçının kulis, yemek ve konaklama talepleri. | S2 |
| Stage plot | `StagePlot` | Sahnedeki ekipman ve müzisyen yerleşiminin çizimi; belge olarak tutulur. | S2 |

### 3.7 Ekipman ve depo

| Terim | Kod adı | Tanım | Sürüm |
|---|---|---|---|
| Ekipman modeli | `EquipmentModel` | Katalogdaki marka ve model (ör. "Shure SM58"). Ağırlık, güç tüketimi (watt) ve kasa hacmi burada tutulur. | S1 |
| Ekipman kategorisi | `EquipmentCategory` | Modellerin hiyerarşik grubu (ör. Ses › Mikrofon › Dinamik vokal). Rider satırı model yerine kategori isteyebilir. | S1 |
| Takip tipi | `TrackingType` | Bir modelin nasıl takip edildiği: **Seri no'lu** (`Serialized`, her birim ayrı) veya **Adetli** (`Bulk`, yalnızca miktar). | S1 |
| Birim | `EquipmentUnit` | Seri no'lu bir modelin tek bir fiziksel parçası. Seri numarası ve QR etiketi vardır. | S1 |
| Adetli stok | `BulkStock` | Adetli bir modelin bir konumdaki miktarı. | S1 |
| Kasa | `Case` | Ekipmanın içinde taşındığı fiziksel kutu (flight case, kablo kasası). QR etiketi vardır; içinde birimler, adetli kalemler veya başka kasalar olabilir. | S1 |
| Kasa standart içeriği | `CaseStandardContent` | Bir kasanın dolu olduğunda içermesi gereken kalemler (ör. "20 × XLR 10 m"). Tam kasa tek okutmayla işlem görür. | S1 |
| Kit | `Kit` | Birlikte planlanan modellerin adlandırılmış seti (ör. "Küçük sahne ışık paketi"). Kit mantıksal bir tanımdır, fiziksel değildir; başka kitleri içerebilir. | S1 |
| QR etiketi | `QrLabel` | Birime veya kasaya yapıştırılan, sistemdeki kaydı tanımlayan etiket. | S1 |
| Depo | `Warehouse` | Ekipmanın saklandığı fiziksel yer. Şirketin birden fazla deposu vardır. | S1 |
| Konum | `Location` | Bir birimin ya da adetli stoğun şu an fiziksel olarak nerede olduğu: bir depo, bir etkinlik, bir transfer ya da servis. | S1 |
| Stok | `Stock` | Bir modelin belirli bir konumdaki miktarı; seri no'lu birimlerin sayısı ya da adetli stok. | S1 |
| Sahiplik | `Ownership` | Ekipmanın kime ait olduğu: **Şirket** (`Owned`) veya **Dış kiralama** (`SubRented`, QR ile takibe alınmış dış kiralama ekipmanı). | S1 |
| Birim durumu | `UnitStatus` | Birimin fiziksel durumu. Değerler aşağıdaki tabloda. Rezervasyon bir durum değildir (bkz. 3.8 Müsaitlik). | S1 |
| Transfer | `WarehouseTransfer` | Ekipmanın bir depodan diğerine taşınması. Planlanan ve gerçekleşen varış zamanı vardır. | S1 |
| Gecikmiş transfer | `WarehouseTransfer.IsOverdue` | Planlanan varış zamanı geçtiği halde tamamlanmamış transfer. Kalemleri hedef depoda müsait sayılmaz. | S1 |
| Transfer durumu | `TransferStatus` | **Planlandı** (`Planned`), **Yolda** (`InTransit`), **Tamamlandı** (`Completed`), **İptal** (`Cancelled`). | S1 |
| Çıkış | `CheckOut` | Ekipmanın QR okutularak depodan etkinliğe ya da transfere çıkarılması. | S1 |
| Çıkışın geri alınması | `CheckOutReversal` | Yanlış yapılan çıkışın, etkinlik Canlı olmadan geri alınması. | S1 |
| Giriş | `CheckIn` | Ekipmanın QR okutularak depoya geri alınması. | S1 |
| Toplama listesi | `PickList` | Bir etkinlik için depodan çıkarılacak kasa, birim ve adetlerin QR kodlu listesi. | S1 |
| Sayım farkı | `CountDiscrepancy` | Adetli bir kalemde beklenen ile sayılan miktar arasındaki fark. Hangi etkinlik ya da transferde oluştuğuyla kaydedilir. | S1 |
| Hasar kaydı | `DamageReport` | Dönüşte hasarlı bulunan birim ya da adetli kalem için açılan kayıt. | S1 |
| Bakım kaydı | `MaintenanceLog` | Bir birime yapılan bakım ya da onarımın kaydı. | S2 |
| Depo sayımı | `StockCount` | Bir depodaki stoğun periyodik olarak sayılıp sistemle karşılaştırılması. | S2 |
| Sarf malzeme | `Consumable` | Kullanıldıkça tükenen ve geri dönmeyen malzeme (gaffer bant, pil). | S6 |

#### Birim durumları (`UnitStatus`)

| Terim | Kod adı | Anlamı |
|---|---|---|
| Depoda | `InWarehouse` | Birim bir depoda, kullanılabilir. |
| Yolda | `InTransit` | Birim depolar arası transferde. |
| Etkinlikte | `AtEvent` | Birim bir etkinlik için depodan çıkış yaptı. |
| Bakımda | `InMaintenance` | Birim hasarlı ya da bakımda; kullanılamaz. |
| Kayıp | `Lost` | Birim kayboldu. |
| Hurda | `Retired` | Birim kalıcı olarak kullanımdan çıkarıldı. |
| Tedarikçiye iade edildi | `ReturnedToSupplier` | Dış kiralama birimi tedarikçiye geri verildi. Yalnızca dış kiralama birimleri için geçerlidir. |

### 3.8 İhtiyaç hesabı ve rezervasyon (MRP)

| Terim | Kod adı | Tanım | Sürüm |
|---|---|---|---|
| İhtiyaç hesabı | `RequirementCalculation` | Bir etkinliğin rider'ından net ihtiyacı ve karşılama önerisini üreten hesap (MRP'deki netleştirme). | S1 |
| Brüt ihtiyaç | `GrossRequirement` | Rider satırlarında istenen toplam miktar. | S1 |
| Net ihtiyaç | `NetRequirement` | Brüt ihtiyaçtan mekan ekipmanı ve kabul edilen muadiller düşüldükten sonra kalan, şirketin karşılaması gereken miktar. | S1 |
| Rezervasyon | `EquipmentReservation` | Bir birimin ya da adetli miktarın belirli bir zaman aralığı için bir etkinliğe ayrılması. Fiziksel durumu değiştirmez. | S1 |
| Rezervasyon durumu | `ReservationStatus` | **Önerildi** (`Proposed`), **Onaylandı** (`Confirmed`), **Serbest bırakıldı** (`Released`), **Tamamlandı** (`Completed`, etkinlik kapanınca). | S1 |
| Hazırlık payı | `PrepBuffer` | Etkinlik öncesinde ekipmanın hazırlanması için rezervasyona eklenen süre. Saat hassasiyetindedir (ör. 10 saat); varsayılanı ayarlardan gelir, etkinlik bazında değiştirilir. | S1 |
| Dönüş payı | `ReturnBuffer` | Etkinlik sonrasında ekipmanın dönüp kontrol edilmesi için rezervasyona eklenen süre. Saat hassasiyetindedir; varsayılanı ayarlardan gelir, etkinlik bazında değiştirilir. | S1 |
| Rezervasyon aralığı | `ReservationWindow` | Etkinliğin başlangıç zamanından hazırlık payı kadar önce başlayıp bitiş zamanından dönüş payı kadar sonra biten zaman aralığı. Müsaitlik ve çakışma bu aralık üzerinden hesaplanır. | S1 |
| Stok havuzu | `StockPool` | Müsaitlik hesabında bir depoda belirli bir an için var sayılan şirket ekipmanı: depodakiler, o depodan etkinliğe çıkıp aralığı bitmemiş olanlar ve o depoya zamanında gelecek transferler. | S1 |
| Gecikmiş dönüş | `OverdueReturn` | Rezervasyon aralığı bittiği halde depoya dönmemiş ekipman. Stok havuzuna girmez. | S1 |
| Müsaitlik | `Availability` | Bir modelden belirli bir zaman aralığında, belirli bir depoda kaç adet kullanılabilir olduğu. Durumdan farklı olarak zamana bağlı ve hesaplanan bir değerdir. | S1 |
| Çakışma | `Conflict` | Aynı kaynağın örtüşen zaman aralıklarında birden fazla işe ayrılması. S1'de yalnızca ekipman için, S2'den itibaren ekip, mekan ve araç için de. | S1 |
| Çakışma türü | `ConflictType` | **Rakip talep** (`CompetingDemand`): net ihtiyaç, başka etkinliklerin onaylı rezervasyonları yüzünden karşılanamıyor. **Aşırı rezervasyon** (`Overbooking`): onaylı rezervasyonlar sonradan müsaitliği aşıyor. | S1 |
| Çakışma durumu | `ConflictStatus` | **Açık** (`Open`), **Kabul edildi** (`Acknowledged`), **Çözüldü** (`Resolved`). | S1 |
| Fazla rezervasyon | `EquipmentReservation.IsExcess` | Yeniden hesaplamadan sonra yeni net ihtiyacı aşan onaylı rezervasyon. Otomatik serbest bırakılmaz. | S1 |
| Güncel olmayan hesap | `RequirementCalculation.IsStale` | Hesaba giren bir veri (rider versiyonu, mekan, tarih, paylar, kaynak depo) değiştiği için yeniden çalıştırılması gereken ihtiyaç hesabı. | S1 |
| Karşılama | `Fulfillment` | Bir rider satırının nasıl karşılandığı. | S1 |
| Karşılama kaynağı | `FulfillmentSource` | **Depo** (`Warehouse`), **Mekan** (`Venue`), **Muadil** (`Equivalent`), **Dış kiralama** (`SubRental`). | S1 |
| Dış kiralama | `SubRental` | Şirketin hiçbir deposunda bulunmayan ekipmanın bir tedarikçiden kiralanması. | S1 |
| Dış kiralama siparişi | `SubRentalOrder` | Bir tedarikçiye verilen, model, adet ve tarih içeren kiralama siparişi. | S1 |
| Dış kiralama siparişi durumu | `SubRentalOrderStatus` | **Taslak** (`Draft`), **Sipariş verildi** (`Ordered`), **Teslim alındı** (`Received`), **İade edildi** (`Returned`), **İptal** (`Cancelled`). | S1 |
| Rider karşılama raporu | `RiderFulfillmentReport` | Her rider satırının hangi kaynaktan karşılandığını gösteren belge. | S1 |
| Güç hesabı | `PowerCalculation` | Rider'daki ekipmanın toplam güç tüketiminin mekanın güç kapasitesiyle karşılaştırılması. | S2 |

### 3.9 Ekip ve kaynak planlama (MRP II)

| Terim | Kod adı | Tanım | Sürüm |
|---|---|---|---|
| Crew üyesi | `CrewMember` | Crew rolündeki bir kişinin ücret, çalışma tipi ve yetkinlik bilgileri. | S2 |
| Çalışma tipi | `EmploymentType` | **Sabit** (`Staff`) veya **Freelance** (`Freelance`). | S2 |
| Yetkinlik | `Skill` | Bir crew üyesinin yapabildiği iş (ör. rigging, ışık operatörlüğü). | S2 |
| Sertifika | `Certification` | Geçerlilik tarihi olan resmi belge (ör. yüksekte çalışma, forklift). | S2 |
| Çağrı | `CrewCall` | Bir crew üyesinin belirli bir etkinliğe, göreve ve saate atanması. | S2 |
| Çağrı saati | `CallTime` | Crew üyesinin sahada olması gereken saat. | S2 |
| Dinlenme süresi | `RestPeriod` | Bir crew üyesinin iki çağrı arasında olması gereken en kısa süre. | S2 |
| Kaynak takvimi | `ResourceCalendar` | Ekipman, ekip ve araçların zaman içindeki atamalarını gösteren Gantt görünümü. | S2 |
| Day sheet | `DaySheet` | Bir etkinlik gününün mekan bilgisi, saatleri ve iletişim kişilerini içeren belge. | S2 |
| Run of show | `RunOfShow` | Etkinliğin dakika dakika akışı. | S2 |
| Call sheet | `CallSheet` | Kimin, nerede, kaçta, hangi görevde olacağını gösteren belge. | S2 |

### 3.10 Ticari (CRM)

| Terim | Kod adı | Tanım | Sürüm |
|---|---|---|---|
| Anlaşma | `Deal` | Satışa dönüşmesi beklenen ticari fırsat (sanatçı booking'i, teknik hizmet işi ya da sponsorluk). Etkinliğe bağlanabilir. | S3 |
| Anlaşma aşaması | `DealStage` | Anlaşmanın satış hunisindeki yeri. | S3 |
| Aktivite | `Activity` | Bir anlaşma ya da tarafla ilgili görüşme, arama, e-posta veya toplantı kaydı. | S3 |
| Sanatçı ücret modeli | `ArtistFeeModel` | **Garanti** (`Guarantee`), **Yüzde** (`Percentage`), **Garanti veya yüzde** (`VersusDeal`, hangisi yüksekse), **Bonus** (`Bonus`, eşik aşılınca ek ödeme). | S3 |
| Fiyat listesi | `PriceList` | Ekipman modellerinin kiralama fiyatlarının geçerlilik tarihli listesi. | S3 |
| Kiralama fiyatı | `RentalRate` | Bir modelin fiyat listesindeki birim fiyatı (günlük). | S3 |
| Teklif | `Quote` | Teknik hizmet müşterisine verilen, rider'dan hesaplanan fiyatlı öneri. | S3 |
| Sözleşme | `Contract` | Taraflar arasında imzalanan bağlayıcı belge ve imza durumu. | S3 |
| Sponsorluk paketi | `SponsorshipPackage` | Bir etkinlik için satılan sponsorluk hakları ve bedeli. | S3 |
| Sponsor teslimatı | `SponsorDeliverable` | Paket kapsamında sponsora verilmesi gereken tek bir hak (logo, stant, sosyal medya paylaşımı) ve teslim durumu. | S3 |

### 3.11 Finans

| Terim | Kod adı | Tanım | Sürüm |
|---|---|---|---|
| Bütçe | `Budget` | Bir etkinliğin planlanan gelir ve gideri. | S4 |
| Bütçe kalemi | `BudgetLine` | Bütçedeki tek bir planlanan gelir ya da gider. | S4 |
| Gider | `Expense` | Gerçekleşen bir harcama. | S4 |
| Gelir | `Revenue` | Gerçekleşen bir gelir. | S4 |
| Bütçe sapması | `BudgetVariance` | Bir bütçe kaleminde planlanan ile gerçekleşen arasındaki fark. | S4 |
| Bilet satış importu | `TicketSalesImport` | Dış bilet platformundan alınan satış verisinin sisteme yüklenmesi. | S4 |
| Brüt hasılat | `GrossBoxOffice` | Satılan biletlerin toplam tutarı. | S4 |
| Net hasılat | `NetBoxOffice` | Brüt hasılattan vergi ve bilet platformu kesintileri düşüldükten sonra kalan tutar. | S4 |
| Hesaplaşma | `Settlement` | Etkinlik sonrası sanatçı payının ücret modeline göre hesaplanması ve onaylanması. | S4 |
| Hesaplaşma föyü | `SettlementSheet` | Hesaplaşmanın hasılat, gider ve sanatçı payını gösteren belgesi. | S4 |
| Para birimi | `Currency` | ISO 4217 kodu (TRY, EUR, USD). | S4 |
| Kur | `ExchangeRate` | Belirli bir tarihte iki para birimi arasındaki oran. | S4 |
| Tutar | `Money` | Miktar ve para biriminden oluşan değer. Tutar hiçbir yerde para birimi olmadan tutulmaz. | S1 |

### 3.12 Turne ve lojistik (DRP)

| Terim | Kod adı | Tanım | Sürüm |
|---|---|---|---|
| Araç | `Vehicle` | Şirketin ya da kiralanan kamyon ve vanların hacim ve ağırlık kapasitesiyle kaydı. | S5 |
| Sevkiyat | `Shipment` | Bir aracın belirli bir yükle bir yerden diğerine yaptığı yolculuk. Bir transferi ya da bir etkinliğin yükünü taşıyabilir. | S5 |
| Yükleme listesi | `LoadList` | Bir sevkiyatta araca yüklenen kasaların listesi; toplam ağırlık ve hacimle. | S5 |
| Geçiş kontrolü | `TransitCheck` | Ardışık iki turne tarihi arasında ekipmanın yetişip yetişmediğinin kontrolü. | S5 |

## 4. Kullanılmayan terimler

Bu kelimeler arayüzde, belgelerde ve kodda kullanılmaz; yerine sağ sütundaki terim kullanılır.

| Kullanma | Yerine | Neden |
|---|---|---|
| Organizasyon (etkinlik anlamında) | Etkinlik | "Organizasyon" firma (`Organization`) ile karışır. |
| Proje, iş, gig | Etkinlik | Tek ad kuralı. |
| Envanter | Stok / Ekipman | Tek ad kuralı. |
| Ürün, malzeme | Ekipman modeli | Şirket ürün satmıyor; "malzeme" yalnızca sarf malzeme için kullanılır. |
| Rezerve (birim durumu olarak) | Rezervasyon | Rezervasyon zamana bağlıdır; birimin fiziksel durumu değildir. |
| Çıkış deposu | Kaynak depo | "Depo çıkışı" işlemiyle karışır. |
| Anlaşma (etkinlik durumu olarak) | Müzakere | "Anlaşma" CRM'deki `Deal` varlığıdır. |
| Kiralama (dış kiralama anlamında) | Dış kiralama | Şirketin müşteriye kiralaması ile tedarikçiden kiralaması karışır. |
| Satıcı | Tedarikçi | Tek ad kuralı. |
| Paket (kit anlamında) | Kit | "Paket" sponsorluk paketi ile karışır. |
| Case (Türkçe metinde) | Kasa | Arayüz Türkçe; sektörde iki kullanım da yaygın. |

### 4.1 Karıştırılmaması gereken çiftler

| Çift | Fark |
|---|---|
| Durum ↔ Müsaitlik | Durum birimin şu anki fiziksel hali, müsaitlik ise gelecekteki bir zaman aralığı için hesaplanan miktardır. Depoda duran bir birim, gelecek hafta için rezerve olduğu için o hafta müsait olmayabilir. |
| Kasa ↔ Kit | Kasa fiziksel bir kutudur ve QR etiketi vardır. Kit ise planlamada kullanılan mantıksal bir settir. |
| Opsiyon ↔ Rezervasyon | Opsiyon mekanın tarihini tutar. Rezervasyon ise ekipmanı ayırır. |
| Rider ↔ Rider versiyonu | Rider bir prodüksiyonun ihtiyaç listesinin kimliğidir. Versiyon ise o listenin belirli bir tarihteki değiştirilemez halidir. Etkinlik her zaman bir versiyona bağlanır. |
| Transfer ↔ Sevkiyat | Transfer stoğun hangi depodan hangi depoya geçtiğini (S1), sevkiyat ise hangi aracın ne taşıdığını (S5) kaydeder. S5'ten itibaren bir transfer bir sevkiyatla taşınır. |
| Brüt ihtiyaç ↔ Net ihtiyaç | Brüt ihtiyaç rider'ın istediği miktardır. Net ihtiyaç ise mekan ekipmanı ve muadiller düşüldükten sonra şirketin karşılaması gereken miktardır. |
| Anlaşma ↔ Sözleşme | Anlaşma satış fırsatıdır. Sözleşme ise o fırsatın imzalanmış belgesidir. |
| Hazırlık ↔ Hazırlık payı | Hazırlık etkinliğin bir durumudur (advance). Hazırlık payı ise rezervasyona eklenen süredir. |

## 5. Kararlar

| No | Soru | Karar |
|---|---|---|
| G-01 | Etkinlik durum adları | Müzakere, Canlı, Kurulum, Söküm adları kabul edildi. |
| G-02 | Depodan çıkarılacakların listesinin adı | Toplama listesi. |
| G-03 | Arayüzde İngilizce kalan sektör terimleri | Rider, day sheet, call sheet, run of show, stage plot, input list, hospitality rider İngilizce kalır. |

## 6. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-09-24 | v0.1 | İlk taslak |
| 2026-09-24 | v1.0 | Açık sorular varsayılanlarla karara bağlandı. Kullanıcı hikayeleri yazılırken çıkan terimler eklendi: pasif kayıt, dış opsiyon, opsiyon son tarihi, transfer durumu, dış kiralama siparişi durumu. |
| 2026-09-24 | v1.1 | Dış kiralamanın QR ile takibi için sahiplik terimi ve "Tedarikçiye iade edildi" birim durumu eklendi. |
| 2026-09-24 | v1.2 | İş kuralları yazılırken çıkan terimler eklendi: stok havuzu, gecikmiş dönüş, gecikmiş transfer, çakışma türleri, fazla rezervasyon, güncel olmayan hesap, çıkışın geri alınması. |
| 2026-09-24 | v1.3 | Etkinlik zamanı ve süresi geçmiş opsiyon eklendi; hazırlık payı, dönüş payı ve rezervasyon aralığı saat hassasiyetine göre yeniden tanımlandı. |
| 2026-09-25 | v1.4 | Durum makineleriyle uyum: opsiyon durumu, çakışma durumu, rezervasyonun Tamamlandı durumu eklendi; etkinlik zamanı, mekanın bizde olduğu süre olarak netleştirildi. |
| 2026-09-25 | v1.5 | Operasyon geçiş modu eklendi; kapı açılışı ve söküm başlangıcı S1'e alındı; kurulum başlangıcı ve söküm bitişi etkinlik zamanıyla eşlendi. |
