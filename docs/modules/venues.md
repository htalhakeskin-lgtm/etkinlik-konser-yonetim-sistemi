# Modül tasarımı: Venues — Mekanlar

> **Durum:** v1.0 (onaylandı) · **Son güncelleme:** 2026-10-02
> **Adım:** Faz 1.3 ([12 §3](../12-implementation-plan.md#3-adımlar)) · **Kararlar:** [Bölüm 9](#9-kararlar)

## 1. Bu belge ne işe yarar

Venues modülünün fiziksel tasarımıdır: mekanlar, mekan ekipmanı ve kullanılamama dönemleri. Adımın ortak kararları [parties §2](parties.md#2-adımın-ortak-kararları)'dedir.

**Kaynaklar:** [05 §5.5](../05-module-map.md#55-venues--mekanlar), [06 §5.5](../06-erd-conceptual.md#55-venues--mekanlar), [06 karar 16](../06-erd-conceptual.md#4-önemli-modelleme-kararları), [database §7](../standards/database.md#7-zaman), [11 §3](../11-screens.md#3-s1-ekran-envanteri); hikayeler US-VEN-001, 002; kurallar BR-SYS-001, BR-PTY-004, BR-VEN-001, 002, BR-VEN-003 (yeni).

## 2. Kapsam

| Var | Yok (sonraki adım ya da sürüm) |
|---|---|
| Mekanlar ve teknik bilgileri, pasifleştirme | Opsiyonlar (Booking, 1.4) |
| Mekan ekipmanı, geçerlilik aralığı, kullanılamama dönemleri | "İhtiyaç hesabı güncel değil" işareti (US-VEN-002 kriter 7; olayın dinleyicisi Planning, 1.6) |
| Seçilen aralıkta kullanılabilir mekan ekipmanı (BR-VEN-001) | Kullanılabilir ekipman sözleşmesi (Planning, 1.6) ve mekan doğrulama sözleşmesi (Booking, 1.4) |
| `VenueEquipmentChanged` olayının yayınlanması | Güç hesabı (S2), mekan dosyaları (S2) |

## 3. Projeler

`FestOS.Modules.Venues.{Domain, Application, Contracts, IntegrationEvents, Infrastructure}`; Application, Catalog ve Parties'in Contracts'ına başvurur. Şema `venues`, veritabanı rolü `festos_venues`.

## 4. Veritabanı (`venues` şeması)

| Tablo | Kolonlar (ortak kolonlar hariç) | Kısıtlar ve indeksler |
|---|---|---|
| `venues` | `id`, `name varchar(200)`, `name_search`, `city varchar(100)`, `city_search`, `address varchar(500)`, `operator_party_id uuid null`, `capacity int`, `stage_width_meters numeric(6,2) null`, `stage_depth_meters numeric(6,2) null`, `stage_height_meters numeric(6,2) null`, `loading_dock varchar(2000) null`, `power_capacity_amperes numeric(7,2) null`, `curfew time null`, `time_zone varchar(64)`, `deactivated_at`, `deactivated_by`, `version` | `ux_venues_name_city` (`city_search, name_search`) → BR-VEN-003; `ix_venues_operator_party_id` (modüller arası referans, DT-03); ölçülerde ve kapasitede `CHECK (> 0)` (MD-05) |
| `venue_equipment` | `id`, `venue_id`, `model_id uuid null`, `category_id uuid null`, `description varchar(200) null`, `quantity int`, `validity_start date null`, `validity_end date null` | FK `venue_id` `CASCADE`; `ck_venue_equipment_target` (`num_nonnulls(model_id, category_id, description) = 1`) → BR-VEN-001; `ck_venue_equipment_validity` (`validity_start < validity_end`) → BR-VEN-002; `quantity >= 1` (MD-05); `ix_venue_equipment_model_id`, `ix_venue_equipment_category_id` |
| `venue_equipment_unavailabilities` | `id`, `venue_equipment_id`, `period_start date`, `period_end date`, `quantity int`, `reason varchar(500)` | FK `CASCADE`; `ck_…_period` (`period_start < period_end`) → BR-VEN-002; `ex_venue_equipment_unavailabilities_overlap` (`EXCLUDE USING gist (venue_equipment_id WITH =, daterange(period_start, period_end, '[)') WITH &&)`) → BR-VEN-002; `quantity >= 1` (MD-05) |

- `Venue` toplu köktür; ekipman satırları ve dönemleri ona aittir ([06 §7](../06-erd-conceptual.md#7-toplu-kökler-ve-eşzamanlılık)). Dışlama kısıtı migration'da SQL ile yazılır ([database §12.1](../standards/database.md#121-kısıtlar)); `btree_gist` eklentisi hazırlık adımında zaten kuruludur.
- **Günler ve aralıklar (VN-01):** Geçerlilik ve kullanılamama dönemleri takvim günüdür ve yarı açıktır: `…_end` hariçtir ([database §7.3](../standards/database.md#73-zaman-aralıkları)). Arayüz son günü dahil gösterir ve gönderir; dönüşüm ön yüzdedir (işlem geçmişi ekranındaki tarih süzgeci gibi). Hesapta gün, mekanın saat diliminde `[başlangıç günü 00:00, bitiş günü 00:00)` anlarına çevrilir.
- **Saat dilimi:** `time_zone` IANA kimliğidir, varsayılanı `Europe/Istanbul`'dur ([database §7.2](../standards/database.md#72-gelecekteki-duvar-saati-zamanları)). Etkinlik zamanları 1.4'te bu dilimden hesaplanır. Sessizlik saati mekanın yerel saatidir (`time`).
- Güç kapasitesi S1'de yalnızca kaydedilir (US-VEN-001).

## 5. Kurallar

| Kural | Uygulama |
|---|---|
| BR-SYS-001 Silme yerine pasifleştirme | Mekan silinmez; pasifleştirilir ve yeniden etkinleştirilebilir. Ekipman satırı ve dönem modül içi kayıttır: yanlış girilen satır silinebilir, elden çıkan ekipmanda geçerlilik bitişi girilir (VN-03). |
| BR-PTY-004 Rol gerektiren seçimler | İşletmeci, `IPartyDirectory` ile aktif ve **Mekan işletmecisi** rolünde olduğu doğrulanarak seçilir. Kayıttaki işletmeci sonradan pasifleşse ya da rolünü kaybetse de değişmez, "Pasif" rozetiyle görünür. |
| BR-VEN-001 Hesaba giren mekan ekipmanı | Satırın tam olarak bir hedefi vardır: aktif bir model, aktif bir kategori (`ICatalogDirectory` ile doğrulanır) ya da serbest açıklama. Kullanılabilir adet alan katmanında saf bir hesaptır (`VenueEquipment.UsableQuantity(aralık)`): satır aralığın tamamında geçerli değilse 0, geçerliyse adetten, aralıkla örtüşen dönemlerin en yüksek kullanılamayan adedi düşülür. Bu adımda mekan sayfasındaki sorgu, 1.6'da Planning'in sözleşmesi aynı hesabı kullanır. Serbest açıklamalı satır hesapta yer almaz, ekranda "hesaba girmez" olarak belirtilir. |
| BR-VEN-002 Tarihe göre değişme | Geçerlilik uçları isteğe bağlıdır, boş uç sınırsızdır; bitiş başlangıçtan sonradır. Dönemin kullanılamayan adedi satırın adedini aşmaz ve dönem satırın geçerlilik aralığının içindedir; satırın adedi ya da geçerliliği, var olan bir dönemi bu koşulun dışına itecek şekilde değiştirilemez. Bir satırın dönemleri örtüşmez (dışlama kısıtı); BR-VEN-001'deki "en yüksek" hesabı böylece doğru kalır. |
| BR-VEN-003 Tekil mekan adı (yeni) | Mekan adı aynı şehirde, büyük-küçük harf ve Türkçe işaret farkı gözetmeden tekildir; pasif mekanlar da dahildir. Farklı şehirlerde aynı adlı mekan olabilir (ör. bir kulüp zincirinin şubeleri). |

## 6. Yetkiler ve uç noktalar

| Yetki | SY | BM | TM | DS | GM |
|---|---|---|---|---|---|
| `Venues.Venues.View` | | ✓ | ✓ | | ✓ |
| `Venues.Venues.Create / Edit / Deactivate` | | ✓ | | | |
| `Venues.Equipment.Edit` | | | ✓ | | |

Mekan ekipmanını teknik müdür girer (US-VEN-001 kriter 4); booking müdürü görür.

| Yöntem ve adres | İşlem adı | Yetki | Not |
|---|---|---|---|
| `GET /api/v1/venues` | `ListVenues` | `Venues.Venues.View` | Sayfalı; `q` (ad, şehir), `city`, `status`; sıralama `name`, `city`, `capacity`; işletmeci adı (MD-02) |
| `GET /api/v1/venues/{venueId}` | `GetVenue` | `Venues.Venues.View` | Genel bilgiler, işletmecinin adı ve durumu; `ETag` |
| `POST /api/v1/venues` | `CreateVenue` | `Venues.Venues.Create` | BR-PTY-004 |
| `PUT /api/v1/venues/{venueId}` | `EditVenue` | `Venues.Venues.Edit` | `If-Match` |
| `POST …/{venueId}/deactivate`, `…/activate` | `DeactivateVenue`, `ActivateVenue` | `Venues.Venues.Deactivate` | `If-Match` |
| `GET /api/v1/venues/{venueId}/equipment` | `ListVenueEquipment` | `Venues.Venues.View` | Satırlar (hedef adı ve pasifliği, geçerlilik, dönemler); `?status=current` (bugün geçerli, varsayılan), `ended`, `all` |
| `GET /api/v1/venues/{venueId}/equipment/usable?from=&to=` | `ListUsableVenueEquipment` | `Venues.Venues.View` | Seçilen günlerde kullanılabilir adetler (BR-VEN-001), serbest satırlar ayrı |
| `POST /api/v1/venues/{venueId}/equipment` | `AddVenueEquipment` | `Venues.Equipment.Edit` | `If-Match` (mekanın sürümü) |
| `PUT /api/v1/venues/{venueId}/equipment/{lineId}` | `EditVenueEquipment` | `Venues.Equipment.Edit` | Hedef, adet, geçerlilik; `If-Match` |
| `DELETE /api/v1/venues/{venueId}/equipment/{lineId}` | `RemoveVenueEquipment` | `Venues.Equipment.Edit` | Yanlış giriş için; `If-Match` |
| `POST …/equipment/{lineId}/unavailabilities` | `AddVenueEquipmentUnavailability` | `Venues.Equipment.Edit` | Günler, adet, neden; `If-Match` |
| `PUT …/unavailabilities/{periodId}`, `DELETE …` | `EditVenueEquipmentUnavailability`, `RemoveVenueEquipmentUnavailability` | `Venues.Equipment.Edit` | `If-Match` |

Doğrulama: ad, şehir, adres zorunlu (200 / 100 / 500); kapasite 1 ve üstü tam sayı; sahne ölçüleri 0'dan büyük, en çok 999,99 m; güç kapasitesi 0'dan büyük; saat dilimi tanınan bir IANA kimliği; satırda adet 1–9999; neden zorunlu.

## 7. Sözleşme ve olaylar

- **Yayınladığı olay (IntegrationEvents):** `VenueEquipmentChanged { VenueId, AffectedStart?, AffectedEnd? }`, sıra anahtarı mekan kimliği. Satır eklenince, değişince, silinince ve dönem değişince yayınlanır; etkilenen aralık eski ve yeni geçerlilik ya da dönem aralıklarının birleşimidir, boş uç sınırsızdır. Dinleyicisi Planning'dir (1.6); bu adımda giden kutusuna yazılır ve `venues:{id}` grubuna `resourceChanged` olarak duyurulur (MD-06).
- **Senkron sözleşmeler:** Mekan doğrulaması (Booking, 1.4) ve zaman aralığında kullanılabilir ekipman (Planning, 1.6) o adımlarda `IVenueDirectory`'ye eklenir; hesap bu adımda alan katmanında yazılıp testlenir.
- **Anlık bildirim grubu:** `venues:{id}`; mekanı görme yetkisi olan her kullanıcı katılabilir.

## 8. Ekranlar, hikayeler ve PR planı

| Ekran | Adres | Not |
|---|---|---|
| Mekanlar | `/venues` | Liste: ad, şehir, kapasite, işletmeci, durum; arama, şehir ve durum süzgeci adreste. "Mekan ekle" diyaloğu (BM). |
| Mekan: Genel | `/venues/{id}` | Bilgiler; Düzenle (diyalog), Pasifleştir / Etkinleştir; işletmeci seçimi yazdıkça arayan kutuyla (MD-03) |
| Mekan: Ekipman | `/venues/{id}/equipment` | Satır tablosu: hedef (model, kategori ya da serbest açıklama, "hesaba girmez" etiketi), adet, geçerlilik, dönem sayısı; "Bugün geçerli / Sona erenler / Tümü" süzgeci; satır ve dönem diyalogları (TM). Üstte "Kullanılabilir ekipman" aracı: gün aralığı seçilir, her satırın kullanılabilir adedi gösterilir. Başka kullanıcının değişikliği sayfa yenilenmeden gelir. |
| Mekan: Geçmiş | `/venues/{id}/history` | Mekan, satırlar ve dönemler tek akışta (MD-01) |

| Hikaye | Uç noktalar ve ekranlar | Kurallar |
|---|---|---|
| US-VEN-001 Mekan oluşturma | Mekan uçları; mekanlar, Genel | BR-PTY-004, BR-VEN-003 |
| US-VEN-002 Mekan ekipmanı | Ekipman uçları; Ekipman sekmesi; `VenueEquipmentChanged` | BR-VEN-001, BR-VEN-002 (BR-MRP-003 ve BR-MRP-010 1.6'da) |

| # | PR | Kapsam |
|---|---|---|
| 1a | Venues iskeleti ve mekanlar (sunucu) | Projeler, şema, rol, yetkiler ve rol matrisi; mekan tablosu ve uçları, BR-PTY-004, BR-VEN-003 |
| 1b | Mekanlar ekranı | Liste, Genel sekmesi, diyaloglar, menüde "Ana veriler › Mekanlar" |
| 2a | Mekan ekipmanı (sunucu) | Satır ve dönem tabloları ve uçları, BR-VEN-001 hesabı, BR-VEN-002, `VenueEquipmentChanged` |
| 2b | Mekan ekipmanı ekranı | Ekipman sekmesi, kullanılabilir ekipman aracı, Geçmiş sekmesi |

## 9. Kararlar

| No | Konu | Karar | Gerekçe |
|---|---|---|---|
| VN-01 | Gün hassasiyeti | Geçerlilik ve dönemler `date`, yarı açık; arayüzde son gün dahil | S3; mekanlar ekipman değişikliğini ve ödünç vermeyi gün olarak bildirir, form ve hesap sade kalır. |
| VN-02 | Mekan adının tekilliği | Şehir içinde tekil | Aynı adlı şubeler farklı şehirlerdedir; aynı şehirde iki aynı ad, seçim kutusunda karışırdı. |
| VN-03 | Ekipman satırının silinmesi | Silinebilir; asıl yol geçerlilik bitişidir ve formda önerilir | Satır modül içi kayıttır (BR-SYS-001); yanlış girişi düzeltmenin yolu olmalı. Geçmiş hesaplar Planning'de kendi sonuçlarıyla saklanır (BR-MRP-011), silme onları değiştirmez; silme işlem geçmişinde kalır. |
| VN-04 | Satırların toplu kökü | Ekipman değişiklikleri mekanın sürümüyle (`If-Match`) korunur | 06 §7'deki toplu kök sınırı; eşzamanlı iki düzenleme birbirini sessizce ezmez. Booking müdürünün genel bilgi düzenlemesiyle çakışırsa ikincisi `412` alır; bu kabul edildi. |
| VN-05 | Kullanılabilir adet hesabının yeri | Alan katmanında saf fonksiyon | BR-VEN-001 bu adımda ekranda, 1.6'da ihtiyaç hesabında kullanılır; aynı hesap birim testleriyle bir kez sınanır. |

## 10. Proje sahibine sorulanlar

Soru 2026-10-02'de yanıtlandı; önerilen seçenek seçildi.

| Soru | Seçenekler | Yanıt |
|---|---|---|
| S3 — Mekan ekipmanının geçerlilik aralığı ve kullanılamama dönemi gün olarak mı, saatle mi girilsin? | Gün / tarih ve saat | **Gün** (VN-01) |

## 11. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-10-02 | v0.1 | İlk taslak |
| 2026-10-02 | v1.0 | S3 yanıtlandı (VN-01, gün); onaylandı. |
