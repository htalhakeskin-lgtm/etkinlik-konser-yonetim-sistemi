# Modül tasarımı: Riders — Prodüksiyon ve rider

> **Durum:** v0.1 (taslak) · **Son güncelleme:** 2026-10-02
> **Adım:** Faz 1.3 (prodüksiyonlar ve rider versiyonları); etkinliğe bağlama, etkinliğe özel versiyon ve ihtiyaç listesi 1.4'te bu belgeye eklenir ([12 §3](../12-implementation-plan.md#3-adımlar)) · **Kararlar:** [Bölüm 9](#9-kararlar)

## 1. Bu belge ne işe yarar

Riders modülünün fiziksel tasarımıdır. Bu sürüm prodüksiyonları, prodüksiyon rider'ını, değişmez versiyonlarını ve versiyon karşılaştırmasını kapsar. Etkinlik kopyası, rider ataması (US-RDR-003), etkinliğe özel versiyon (US-RDR-004) ve teknik hizmet ihtiyaç listesi (US-RDR-005) Booking'le birlikte 1.4'te eklenir. Adımın ortak kararları [parties §2](parties.md#2-adımın-ortak-kararları)'dedir.

**Kaynaklar:** [05 §5.6](../05-module-map.md#56-riders--prodüksiyon-ve-rider), [06 §5.6](../06-erd-conceptual.md#56-riders--prodüksiyon-ve-rider), [06 E-01](../06-erd-conceptual.md#11-kararlar), [11 §3](../11-screens.md#3-s1-ekran-envanteri); hikayeler US-ART-001 (prodüksiyon), US-RDR-001, 002; kurallar BR-PTY-004, BR-RDR-001…003, BR-RDR-008, BR-RDR-009 (yeni).

## 2. Kapsam

| Var | Yok (1.4) |
|---|---|
| Prodüksiyonlar, sanatçıya bağlı | Etkinlik kopyası ve Booking olaylarının dinlenmesi |
| Prodüksiyon rider'ı, değişmez versiyonlar, satırlar ve muadiller | Rider ataması, `EventRiderAssigned` |
| Versiyon karşılaştırması | Etkinliğe özel versiyon (BR-RDR-006), ihtiyaç listesi ve kit satırı (BR-RDR-007) |
| `RiderVersionCreated` olayının yayınlanması | "Yeni rider versiyonu var" işareti (BR-RDR-005; dinleyicisi Booking) |

## 3. Projeler

`FestOS.Modules.Riders.{Domain, Application, Contracts, IntegrationEvents, Infrastructure}`; Application, Catalog ve Parties'in Contracts'ına başvurur. Şema `riders`, veritabanı rolü `festos_riders`.

## 4. Veritabanı (`riders` şeması)

| Tablo | Kolonlar (ortak kolonlar hariç) | Kısıtlar ve indeksler |
|---|---|---|
| `productions` | `id`, `artist_party_id`, `name varchar(200)`, `name_search`, `description varchar(2000) null`, `deactivated_at`, `deactivated_by`, `version` | `ux_productions_artist_name` (`artist_party_id, name_search`) → BR-RDR-009 |
| `riders` | `id`, `source` (`production`; `customer` 1.4'te), `production_id null`, `latest_version_number int`, `version` | FK `production_id` `RESTRICT`; `ux_riders_production_id` (prodüksiyon başına bir rider, 06 §5.6; rider'ı sistem açtığı için kullanıcı hatası olamaz, DT-04 istisnası); `ck_riders_source` → BR-RDR-007 |
| `rider_versions` | `id`, `rider_id`, `number int`, `note varchar(500) null`, `created_at`, `created_by`, `created_by_name varchar(200)` | FK `riders` `RESTRICT`; `ux_rider_versions_number` (`rider_id, number`) → BR-RDR-003 |
| `rider_lines` | `id`, `rider_version_id`, `line_key uuid`, `sort_order int`, `model_id null`, `category_id null`, `kit_id null`, `quantity int`, `flexibility null` (`required` / `flexible`), `note text null` | FK `CASCADE`; `ck_rider_lines_target` (`num_nonnulls(model_id, category_id, kit_id) = 1`), `ck_rider_lines_quantity` (`>= 1`), `ck_rider_lines_flexibility` (esneklik yalnızca model satırında, orada zorunlu) → BR-RDR-001; `ix_rider_lines_model_id`, `ix_rider_lines_category_id`, `ix_rider_lines_kit_id` |
| `rider_equivalent_models` | `id`, `rider_line_id`, `sort_order int`, `model_id` | FK `CASCADE`; `ux_rider_equivalent_models_model` (`rider_line_id, model_id`) → BR-RDR-002 |

- `Production` ve `Rider` toplu köktür; `RiderVersion` satırları ve muadilleriyle **değişmez** bir toplu köktür ([06 §7, §8](../06-erd-conceptual.md#8-değişmez-kayıtlar)). Modül rolünün `rider_versions`, `rider_lines` ve `rider_equivalent_models` tablolarında `UPDATE` ve `DELETE` yetkisi yoktur ([database §14.1](../standards/database.md#141-değişmez-kayıtlar)); versiyonun ortak kolonları değişmez kayıtlarınkidir.
- Rider, prodüksiyon oluşturulurken aynı işlemde boş olarak açılır (RD-01). İlk kayıt versiyon 1'i oluşturur (US-RDR-001).
- `created_by_name` versiyonu kaydedenin o anki adıdır; versiyon listesi Identity'ye sormadan "kim" sorusunu yanıtlar ([audit AU-01](audit.md#7-kararlar) ile aynı gerekçe).
- `line_key`, satırın versiyonlar boyunca kimliğidir: formda var olan satır anahtarını korur, yeni satır yeni anahtar alır. Karşılaştırma satırları bununla eşler (RD-03).
- `kit_id` kolonu ve kit kuralı 1.4'ün ihtiyaç listesi içindir; bu adımda prodüksiyon rider'ında kit satırı reddedilir (BR-RDR-001).

## 5. Kurallar

| Kural | Uygulama |
|---|---|
| BR-SYS-001 Silme yerine pasifleştirme | Prodüksiyon silinmez; pasifleştirilir ve yeniden etkinleştirilebilir. Pasif prodüksiyonun rider'ı görünür; 1.4'te yeni etkinliğe seçilemez. Yeni versiyonda pasif model, kategori ya da kit seçilemez; önceki versiyondan gelen böyle bir satır formda işaretlenir ve kayıttan önce değiştirilmelidir (RD-04). |
| BR-PTY-004 Rol gerektiren seçimler | Prodüksiyonun sanatçısı `IPartyDirectory` ile aktif ve **Sanatçı** rolünde olduğu doğrulanarak seçilir. Sanatçı değiştirilemez; yanlış sanatçıyla açılan prodüksiyon pasifleştirilir. |
| BR-RDR-001 Satırın hedefi | Tam olarak bir hedef: model ya da kategori (kit yalnızca ihtiyaç listesinde, 1.4); hedef `ICatalogDirectory` ile var ve aktif olarak doğrulanır. Adet 1 ve üstü tam sayıdır. Esneklik model satırında seçilir; kategori satırı zaten kategorinin her modeliyle karşılanır, esnekliği yoktur (RD-02). |
| BR-RDR-002 Muadiller | Yalnızca **Esnek** model satırına, sıralı olarak; satırın modelinden ve birbirinden farklı, aktif modeller. |
| BR-RDR-003 Versiyonların değişmezliği | Her kayıt `latest_version_number + 1` numaralı yeni versiyondur; rider'ın sürümü `If-Match` ile istenir, böylece aynı versiyondan başlayan iki teknik müdürün ikincisi `412` alır, birinin değişikliği sessizce yok olmaz. Versiyonu değiştiren ya da silen uç yoktur; veritabanı yetkisi de vermez. |
| BR-RDR-008 Rider'ı kim değiştirir | Versiyon kaydetme `Riders.Riders.Edit` ister ve bu yetki yalnızca teknik müdürdedir; booking müdürü rider'ı görür. |
| BR-RDR-009 Tekil prodüksiyon adı (yeni) | Prodüksiyon adı aynı sanatçı içinde, büyük-küçük harf ve Türkçe işaret farkı gözetmeden tekildir; pasifler dahil. US-ART-001'in kabul kriteriydi. |

## 6. Yetkiler ve uç noktalar

| Yetki | SY | BM | TM | DS | GM |
|---|---|---|---|---|---|
| `Riders.Productions.View` | | ✓ | ✓ | | ✓ |
| `Riders.Productions.Create / Edit / Deactivate` | | ✓ | | | |
| `Riders.Riders.View` | | ✓ | ✓ | | ✓ |
| `Riders.Riders.Edit` | | | ✓ | | |

| Yöntem ve adres | İşlem adı | Yetki | Not |
|---|---|---|---|
| `GET /api/v1/productions` | `ListProductions` | `Riders.Productions.View` | Sayfalı; `artistId`, `q`, `status`; sanatçı adı (MD-02), son versiyon numarası ve tarihi |
| `GET /api/v1/productions/{productionId}` | `GetProduction` | `Riders.Productions.View` | Sanatçı adı, rider kimliği, sürümü ve son versiyonu; `ETag` |
| `POST /api/v1/productions` | `CreateProduction` | `Riders.Productions.Create` | Sanatçı, ad, açıklama; boş rider'ı da açar; BR-PTY-004 |
| `PUT /api/v1/productions/{productionId}` | `EditProduction` | `Riders.Productions.Edit` | Ad, açıklama; `If-Match` |
| `POST …/{productionId}/deactivate`, `…/activate` | `DeactivateProduction`, `ActivateProduction` | `Riders.Productions.Deactivate` | `If-Match` |
| `GET /api/v1/riders/{riderId}/versions` | `ListRiderVersions` | `Riders.Riders.View` | Numara, not, kaydeden, zaman, satır sayısı; yeniden eskiye |
| `GET /api/v1/rider-versions/{versionId}` | `GetRiderVersion` | `Riders.Riders.View` | Satırlar: hedef adı, kategori yolu (gruplama için), pasiflik, adet, esneklik, muadiller, not. Değişmez olduğu için uzun süre önbelleğe alınabilir. |
| `POST /api/v1/riders/{riderId}/versions` | `CreateRiderVersion` | `Riders.Riders.Edit` | Satırların tamamı ve not; `If-Match` (rider'ın sürümü); `201` + yeni versiyon |

Doğrulama: prodüksiyon adı zorunlu (en çok 200), açıklama en çok 2000; versiyon notu en çok 500; en az bir satır, en çok 500; adet 1–9999; satır notu en çok 2000; satır başına en çok 10 muadil.

**Karşılaştırma** ön yüzde yapılır: iki versiyon `GetRiderVersion` ile okunur, satırlar `line_key` ile eşlenir; yalnızca birinde olan satır eklenmiş ya da çıkarılmış, ikisinde olup hedefi, adedi, esnekliği, muadilleri ya da notu farklı olan satır değişmiş sayılır (RD-03).

## 7. Sözleşme ve olaylar

- **Yayınladığı olay (IntegrationEvents):** `RiderVersionCreated { RiderId, RiderVersionId, Number, ProductionId }`, sıra anahtarı rider kimliği. Dinleyicisi Booking'dir (1.4, BR-RDR-005); bu adımda giden kutusuna yazılır ve `riders:{id}` grubuna `resourceChanged` olarak duyurulur, açık prodüksiyon sayfası yeni versiyonu gösterir (MD-06).
- **Senkron sözleşmeler:** Prodüksiyon doğrulama, etkinliğe atanmış versiyon ve satırları Booking ve Planning'in ihtiyacıdır; 1.4 ve 1.6'da `Contracts`'a eklenir.
- **Anlık bildirim grubu:** `riders:{id}`; rider'ı görme yetkisi olan her kullanıcı katılabilir.

## 8. Ekranlar, hikayeler ve PR planı

| Ekran | Adres | Not |
|---|---|---|
| Sanatçı detayı: Prodüksiyonlar | `/artists/{id}` | Sanatçının prodüksiyonları (ad, son versiyon, durum); "Prodüksiyon ekle" ve düzenleme diyaloğu (BM); satır prodüksiyon sayfasına gider ([parties.md](parties.md)) |
| Prodüksiyon ve rider | `/productions/{id}` | Başlık: prodüksiyon, sanatçı, durum. Rider: seçili versiyonun satırları üst kategoriye göre gruplu (06 E-01), muadiller sırasıyla, pasif hedefler rozetle; versiyon listesi (numara, not, kaydeden, zaman); `?version=` ile eski versiyon. İşlemler: "Rider'ı düzenle" (TM), "Karşılaştır". |
| Rider formu | `/productions/{id}/rider/edit` | Form sayfası (11 §2.4), son versiyondan başlar: satır tablosu (hedef seçimi model ya da kategori, adet, esneklik, muadiller, not; yukarı / aşağı taşıma, ekle, çıkar), versiyon notu. Kaydedilince yeni versiyon oluşur; kaydedilmemiş değişiklikle çıkışta uyarı. |
| Karşılaştırma | `/productions/{id}/rider/compare?from=&to=` | İki versiyon seçimi; eklenen, çıkarılan ve değişen satırlar renk ve simgeyle, değişen alanlar eski → yeni |

| Hikaye | Uç noktalar ve ekranlar | Kurallar |
|---|---|---|
| US-ART-001 Prodüksiyon | Prodüksiyon uçları; sanatçı detayı | BR-PTY-004, BR-RDR-009 |
| US-RDR-001 Rider girme | `CreateRiderVersion`, `GetRiderVersion`; rider formu, prodüksiyon sayfası | BR-RDR-001, 002, 008 |
| US-RDR-002 Güncelleme ve karşılaştırma | `ListRiderVersions`, `CreateRiderVersion`; karşılaştırma; `RiderVersionCreated` | BR-RDR-003 (BR-RDR-005 1.4'te) |

| # | PR | Kapsam |
|---|---|---|
| 1a | Riders iskeleti ve prodüksiyonlar (sunucu) | Projeler, şema, rol, yetkiler ve rol matrisi; prodüksiyon ve rider tabloları, uçlar, BR-PTY-004, BR-RDR-009 |
| 1b | Prodüksiyonlar ekranı | Sanatçı detayının prodüksiyon bölümü, prodüksiyon diyaloğu, prodüksiyon sayfasının başlığı |
| 2a | Rider versiyonları (sunucu) | Versiyon, satır ve muadil tabloları, uçlar, BR-RDR-001, 002, 003, 008, `RiderVersionCreated` |
| 2b | Rider görünümü ve formu | Prodüksiyon sayfasındaki rider ve versiyon listesi, rider formu |
| 2c | Karşılaştırma | Karşılaştırma ekranı ve eşleme fonksiyonu |

## 9. Kararlar

| No | Konu | Karar | Gerekçe |
|---|---|---|---|
| RD-01 | Rider'ın açılışı | Prodüksiyonla aynı işlemde, versiyonsuz | İlk versiyon da `If-Match` ile kaydedilir; "rider var mı" ayrımı uçlarda ve ekranda kalkar. |
| RD-02 | Kategori satırında esneklik | Yok; esneklik yalnızca model satırında | Kategori satırı tanımı gereği kategorinin her modeliyle karşılanır (BR-EQP-002, BR-MRP-004); "zorunlu kategori" bir anlam taşımaz. Hikayedeki "her satırda esneklik" model satırları içindir. |
| RD-03 | Karşılaştırmada satır eşleme | Versiyonlar boyunca korunan `line_key`; karşılaştırma ön yüzde | Aynı model rider'da iki satırda olabilir (vokal ve davul mikrofonları); hedefe göre eşleme değişeni silinmiş ve eklenmiş gösterirdi. Versiyonlar zaten tamamıyla okunur; 1.4'teki etkinliğe özel versiyon karşılaştırması aynı fonksiyonu kullanır. |
| RD-04 | Pasif hedefli satır | Yeni versiyona taşınamaz; formda işaretlenir | Pasif kayıt yeni işlemde seçilemez (BR-SYS-001); planlama pasif modeli istememeli. Eski versiyonlar olduğu gibi kalır. |
| RD-05 | Versiyon notu | İsteğe bağlı, en çok 500 karakter | Versiyon listesinde "neden yeni versiyon" sorusunu yanıtlar (ör. "2027 turne rider'ı"). |
| RD-06 | Aynı model iki satırda | İzinli | Rider'da aynı model farklı amaçlarla, farklı notlarla istenir. |

Bu belgede proje sahibine soru yoktur.

## 10. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-10-02 | v0.1 | İlk taslak (prodüksiyonlar ve rider versiyonları) |
