# Modül tasarımı: Catalog — Ekipman kataloğu

> **Durum:** v1.6 (onaylandı) · **Son güncelleme:** 2026-10-02
> **Adım:** Faz 1.3 ([12 §3](../12-implementation-plan.md#3-adımlar)) · **Kararlar:** [Bölüm 9](#9-kararlar)

## 1. Bu belge ne işe yarar

Catalog modülünün fiziksel tasarımıdır: kategoriler, modeller, kitler. Adımın ortak kararları [parties §2](parties.md#2-adımın-ortak-kararları)'dedir.

**Kaynaklar:** [05 §5.4](../05-module-map.md#54-catalog--ekipman-kataloğu), [06 §5.4](../06-erd-conceptual.md#54-catalog--ekipman-kataloğu), [11 §3](../11-screens.md#3-s1-ekran-envanteri); hikayeler US-EQP-001, 002, 005; kurallar BR-SYS-001, BR-EQP-001…003, BR-EQP-011…013 (yeni).

## 2. Kapsam

| Var | Yok (sonraki adım ya da sürüm) |
|---|---|
| Hiyerarşik kategoriler, modeller, kitler; pasifleştirme ve yeniden etkinleştirme | `StockCreatedForModel` dinleyicisi (1.5; kolon ve alan kuralı bu adımda) |
| Kitin ağırlık ve güç toplamı | Kitin modellere açılmış içeriğinin ve kategori alt ağacının sözleşmesi (Planning, 1.6) |
| `ICatalogDirectory` sözleşmesi (Venues ve Riders için doğrulama ve adlar) | Sarf malzeme tanımı (S6) |

## 3. Projeler

`FestOS.Modules.Catalog.{Domain, Application, Contracts, Infrastructure}`. S1'de olay yayınlamadığı için `IntegrationEvents` projesi yoktur; 1.5'te Inventory'nin olayını dinlemek için yalnızca Inventory'nin IntegrationEvents projesine başvurur. Şema `catalog`, veritabanı rolü `festos_catalog`.

## 4. Veritabanı (`catalog` şeması)

| Tablo | Kolonlar (ortak kolonlar hariç) | Kısıtlar ve indeksler |
|---|---|---|
| `equipment_categories` | `id`, `name varchar(200)`, `name_search`, `parent_id null`, `deactivated_at`, `deactivated_by`, `version` | FK `parent_id` → kendisi `RESTRICT`; `ux_equipment_categories_name` (`parent_id, name_search`, `NULLS NOT DISTINCT`) → BR-EQP-011 |
| `equipment_models` | `id`, `brand varchar(100)`, `name varchar(200)`, `brand_name_search`, `category_id`, `tracking_type` (`serialized` / `bulk`), `weight_kilograms numeric(10,3) null`, `power_watts integer null`, `transport_volume_cubic_meters numeric(8,3) null`, `has_stock bool`, `deactivated_at`, `deactivated_by`, `version` | FK `category_id` `RESTRICT`; `ux_equipment_models_brand_name` → BR-EQP-012; ölçülerde `CHECK (> 0)` (MD-05); `ix_equipment_models_search` (`pg_trgm`) |
| `kits` | `id`, `name varchar(200)`, `name_search`, `deactivated_at`, `deactivated_by`, `version` | `ux_kits_name` → BR-EQP-013 |
| `kit_lines` | `id`, `kit_id`, `sort_order int`, `model_id null`, `sub_kit_id null`, `quantity int` | FK `kit_id` `CASCADE`; FK `model_id`, `sub_kit_id` `RESTRICT`; `ck_kit_lines_target` (`num_nonnulls(model_id, sub_kit_id) = 1`) → BR-EQP-003; `quantity >= 1` (MD-05) |

- `EquipmentCategory`, `EquipmentModel` ve `Kit` ayrı toplu köklerdir; kit satırı kite aittir ([06 §7](../06-erd-conceptual.md#7-toplu-kökler-ve-eşzamanlılık)).
- Benzersizlikler pasif kayıtları da kapsar: pasif bir modelin adıyla yeni model açılamaz, gerekirse eskisi etkinleştirilir (depolardaki BR-SYS-016 ile aynı).
- Modelin görünen adı "marka + model adı"dır (ör. "Shure SM58"). Arama anahtarı ikisinden birlikte üretilir.
- Kategori yolu ("Ses › Mikrofon › Dinamik vokal") saklanmaz, okurken üretilir; ağaç küçüktür (CT-02).

**Uygulama (PR 1a):** Modül projeleri, `catalog` şeması ve `festos_catalog` rolü açıldı; yetkiler rol matrisine yazıyla eklendi (MD-04): kategorileri booking müdürü, teknik müdür, depo sorumlusu ve genel müdür görür, yalnızca teknik müdür yönetir. Ad tekilliği `(parent_id, name_search)` üzerinde `NULLS NOT DISTINCT` benzersiz indekstir; üst düzeydeki kategoriler de tek bir üst altında sayılır (BR-EQP-011). Kategori ağacının kuralları komutlardadır (BR-EQP-002): oluşturma, taşıma, pasifleştirme ve etkinleştirme `catalog:categories:tree` kilidini alıp ağacı okur; üst kategori var ve aktif olmalı, kategori kendisinin ya da altındakinin altına taşınamaz, aktif alt kategorisi olan kategori pasifleşmez (ret `params.activeCategories` ve `params.activeModels` taşır; modeller 2a'da sayılır), pasif üst kategorinin altındaki kategori etkinleşmez. Ret nedeni `params.reason`'dadır (`inactiveParent`, `cycle`, `activeChildren`); var olmayan üst kategori `400` ve `parentId` alanında `invalidValue` döner. Liste sayfasızdır, Türkçe ada göre sıralıdır ve her kategorinin yolunu verir; değiştiren uçlar kategorinin kaydedilmiş halini yeni `ETag` ile döner.

**Uygulama (PR 2a):** `EquipmentModel` toplu köktür; görünen adı "marka model"dir. Marka ve ad birlikte arama anahtarına çevrilir; bu kolonda iki indeks vardır: tekillik için b-tree (`ux_equipment_models_brand_name`, BR-EQP-012) ve içinde arama için `pg_trgm` GIN (`ix_equipment_models_search`). Aynı kolondaki iki indeks EF'te ayrı adla tanımlanır, yoksa EF onları tek indekste birleştirir. Ölçüler `numeric(10,3)` ve `numeric(8,3)`, güç `integer`'dır; API'de ondalıklar metindir (api A-11). Ölçülerin `> 0` kısıtları alan doğrulamasını tekrarlar ve DT-04 istisna listesindedir (MD-05). Model ancak var ve aktif bir kategoriye konur, pasif kategorideki model etkinleşmez (BR-EQP-002, `reason: inactiveParent`); bu denetimler de ağaç kilidini alır, böylece kategori pasifleştirmesi (artık aktif model sayısını da `params.activeModels` ile söyler) aradaki model eklemesini kaçırmaz. Takip tipi değişikliği `has_stock` doluysa `422 BR-EQP-001`'dir; kolonu 1.5'te Inventory'nin olayı dolduracak. Liste `q`, `categoryId` (alt ağaç dahil), `trackingType`, `status` ile süzer, `name` ve `brand`'e göre Türkçe sıralar, satırda kategori yolunu verir. Kategori listesindeki aktif model sayısı artık gerçektir. `ICatalogDirectory` modelleri (görünen ad, kategori ve yolu, takip tipi, aktiflik) ve kategorileri (ad, yol, aktiflik) verir; kitler 3a'da eklenir. Model yetkileri kategorilerinkiyle aynı rollere verildi.

**Uygulama (PR 3a):** `Kit` toplu köktür, satırları (`kit_lines`) ona aittir; satırın hedefi model ya da alt kittir (`ck_kit_lines_target` → BR-EQP-003), alt kite ve modele yabancı anahtar `RESTRICT`'tir. Kit her kayıtta satırların tamamıyla gelir (CT-04); aynı modelin ya da alt kitin satırı kimliğini korur, böylece geçmişte değişmiş görünür. Aynı hedef ikinci kez `400 duplicateLine`, var olmayan hedef ya da yeni eklenen pasif hedef `400 invalidValue` (`lines[i]`) döner; kitte zaten olan bir hedef sonradan pasifleşse de satır kalır (BR-SYS-001). Kitin kendini listelemesi alan kuralıyla, başka kit üzerinden kendine dönmesi `catalog:kits:structure` kilidi altında okunan kit yapısıyla reddedilir (`422 BR-EQP-003`). Toplamlar, kit modellere açılarak (alt kitin adedi çarpan olur) bellekte hesaplanır: ağırlık metin ondalık, güç tam sayı; değeri olmayan model toplamı "eksik" işaretler, bilinen değerler yine toplanır. Liste sayfasında her kitin satır sayısı ve toplamları, detayda satırlar (ad, pasiflik), açılmış içerik ve toplamlar vardır. Model detayı modeli doğrudan içeren kitleri gösterir; `ICatalogDirectory.FindKitsAsync` kitlerin adını ve aktifliğini verir.

## 5. Kurallar

| Kural | Uygulama |
|---|---|
| BR-SYS-001 Silme yerine pasifleştirme | Kategori, model ve kit silinmez; pasifleştirilir ve yeniden etkinleştirilebilir. Pasif kategoriye model ya da alt kategori, kite pasif model ya da kit eklenemez; var olan bağlar görünmeye devam eder ve "Pasif" rozeti taşır. Kit satırı modül içi kayıttır, kaldırılabilir. |
| BR-EQP-001 Takip tipinin değişmezliği | `has_stock` doluysa takip tipi değişikliği reddedilir (alan kuralı). Kolonu 1.5'te `StockCreatedForModel` dinleyicisi doldurur; bu adımda alan kuralı birim testiyle sınanır. |
| BR-EQP-002 Kategori hiyerarşisi | Kategori kendisinin ya da alt kategorisinin altına taşınamaz. Taşıma ve oluşturma `catalog:categories:tree` danışma kilidi altında yapılır, iki eşzamanlı taşıma döngü kuramaz (CT-03). Aktif alt kategorisi ya da aktif modeli olan kategori pasifleştirilemez; ret `params` ile alttaki aktif kategori ve model sayısını söyler (CT-06). Pasif kategorinin altında kategori ya da model oluşturulamaz, etkinleştirilemez ve oraya taşınamaz. "Alt kategorileri de kapsar" kısmı kategoriyi hedefleyen hesaplarındır (Planning, 1.6). |
| BR-EQP-003 Kit yapısı | Kayıtta alt kitlerin kapanışı hesaplanır, kit kendini içeriyorsa reddedilir; kit satırı kaydı `catalog:kits:structure` danışma kilidi altındadır (iki kit aynı anda birbirini eklerse ikincisi döngüyü görür). Aynı model ya da alt kit bir kitte bir kez geçer (doğrulama). Toplam ağırlık ve güç, içerik modellere açılarak hesaplanır; değeri boş bir model varsa toplam "eksik veri" işaretlidir. |
| BR-EQP-011 Tekil kategori adı (yeni) | Kategori adı aynı üst kategori içinde, büyük-küçük harf ve Türkçe işaret farkı gözetmeden tekildir. US-EQP-001'in kabul kriteriydi. |
| BR-EQP-012 Tekil model (yeni) | Marka ve model adı birlikte, aynı biçimde tekildir. US-EQP-002'nin kabul kriteriydi. |
| BR-EQP-013 Tekil kit adı (yeni) | Kit adı tekildir; seçim kutularında iki aynı adlı kit karışırdı. |

## 6. Yetkiler ve uç noktalar

| Yetki | SY | BM | TM | DS | GM |
|---|---|---|---|---|---|
| `Catalog.Categories.View`, `Catalog.Models.View`, `Catalog.Kits.View` | | ✓ | ✓ | ✓ | ✓ |
| `Catalog.Categories.Create / Edit / Deactivate` | | | ✓ | | |
| `Catalog.Models.Create / Edit / Deactivate` | | | ✓ | | |
| `Catalog.Kits.Create / Edit / Deactivate` | | | ✓ | | |

Booking müdürü rider'ı okurken, depo sorumlusu stokta (1.5) katalogu görür.

| Yöntem ve adres | İşlem adı | Yetki | Not |
|---|---|---|---|
| `GET /api/v1/equipment-categories` | `ListEquipmentCategories` | `…Categories.View` | Sayfasız düz liste (üst kimlik, yol, aktif model sayısı); ön yüz ağaca çevirir (CT-02); `status` |
| `POST /api/v1/equipment-categories` | `CreateEquipmentCategory` | `…Categories.Create` | Ad, üst kategori |
| `PUT /api/v1/equipment-categories/{categoryId}` | `EditEquipmentCategory` | `…Categories.Edit` | Ad, üst kategori (taşıma); `If-Match` |
| `POST …/{categoryId}/deactivate`, `…/activate` | `DeactivateEquipmentCategory`, `ActivateEquipmentCategory` | `…Categories.Deactivate` | `If-Match`; pasif üst kategorinin altındaki kategori etkinleştirilemez |
| `GET /api/v1/equipment-models` | `ListEquipmentModels` | `…Models.View` | Sayfalı; `q`, `categoryId` (alt ağaç dahil), `trackingType`, `status`; sıralama `name`, `brand`, `category` |
| `GET /api/v1/equipment-models/{modelId}` | `GetEquipmentModel` | `…Models.View` | Kategori yolu, modeli içeren kitler; `ETag` |
| `POST /api/v1/equipment-models` | `CreateEquipmentModel` | `…Models.Create` | |
| `PUT /api/v1/equipment-models/{modelId}` | `EditEquipmentModel` | `…Models.Edit` | BR-EQP-001; `If-Match` |
| `POST …/{modelId}/deactivate`, `…/activate` | `DeactivateEquipmentModel`, `ActivateEquipmentModel` | `…Models.Deactivate` | `If-Match` |
| `GET /api/v1/kits` | `ListKits` | `…Kits.View` | Sayfalı; `q`, `status` |
| `GET /api/v1/kits/{kitId}` | `GetKit` | `…Kits.View` | Satırlar (model ya da alt kit adı, pasiflik), açılmış içerik, toplam ağırlık ve güç, eksik veri işareti; `ETag` |
| `POST /api/v1/kits` | `CreateKit` | `…Kits.Create` | Ad ve isteğe bağlı satırlar |
| `PUT /api/v1/kits/{kitId}` | `EditKit` | `…Kits.Edit` | Ad ve satırların tamamı (sıralı); BR-EQP-003; `If-Match` |
| `POST …/{kitId}/deactivate`, `…/activate` | `DeactivateKit`, `ActivateKit` | `…Kits.Deactivate` | `If-Match` |

Doğrulama: ad zorunlu (en çok 200), marka zorunlu (en çok 100), takip tipi zorunlu, ölçüler sıfırdan büyük; kit satırında adet 1–9999.

## 7. Sözleşme ve olaylar

- **Senkron sözleşme (Contracts):** `ICatalogDirectory.FindModelsAsync(ids)`, `FindCategoriesAsync(ids)`, `FindKitsAsync(ids)` → ad, kategori yolu (kökten yaprağa adlar), takip tipi, aktiflik. Venues ve Riders kayıtta hedefi doğrular, okurken adları alır (MD-02). Kategorinin alt ağacı ve kitin açılmış içeriği Planning'in ihtiyacıdır; 1.6'da sözleşmeye eklenir.
- **Dinlediği olay:** `StockCreatedForModel` (Inventory, 1.5).
- **Yayınladığı olay:** yok.

## 8. Ekranlar, hikayeler ve PR planı

| Ekran | Adres | Not |
|---|---|---|
| Kategoriler | `/catalog/categories` | Girintili ağaç: ad, aktif model sayısı, durum; satır menüsü (Alt kategori ekle, Düzenle, Pasifleştir / Etkinleştir, Geçmiş); kategori diyaloğu (ad, üst kategori) |
| Modeller | `/catalog/models` | Liste: marka, model, kategori yolu, takip tipi, ağırlık, güç; arama, kategori, takip tipi ve durum süzgeci adreste |
| Model detayı ve formu | `/catalog/models/{id}`, `/catalog/models/new`, `/catalog/models/{id}/edit` | Form sayfası (11 §2.4): marka, model adı, kategori (ağaçtan seçim), takip tipi (stok varsa pasif ve nedeni yazılı), ölçüler. Detay: bilgiler, modeli içeren kitler; Geçmiş sekmesi. |
| Kitler | `/catalog/kits`, `/catalog/kits/{id}` | Liste: ad, satır sayısı, toplam ağırlık ve güç (eksik veri işaretiyle). Detay: satır düzenleyici (model ya da kit seçimi, adet, sıra), açılmış içerik, toplamlar; Geçmiş sekmesi. Oluşturma diyaloğu yalnızca ad ister. |

| Hikaye | Uç noktalar ve ekranlar | Kurallar |
|---|---|---|
| US-EQP-001 Ekipman kategorileri | Kategori uçları; kategoriler | BR-SYS-001, BR-EQP-002, BR-EQP-011 |
| US-EQP-002 Ekipman modeli | Model uçları; modeller | BR-EQP-001, BR-EQP-012 |
| US-EQP-005 Kit tanımlama | Kit uçları; kitler | BR-EQP-003, BR-EQP-013 |

**Uygulama (PR 1b):** `/catalog/categories` bütün kategorileri bir kez okur (küçük ağaç, CT-02) ve durum süzgecini (adreste) ekranda uygular. Satırlar ağaç sırasıyla, derinliğe göre girintili gösterilir; üst kategorisi süzgece takılan kategori yoluyla görünür. Satır menüsü yetkiye göre: Alt kategori ekle (aktif kategoride), Düzenle, Pasifleştir (onaylı) / Etkinleştir, Geçmiş. Kategori diyaloğu adı ve üst kategoriyi sorar; üst kategori seçiminde yalnızca aktif kategoriler, düzenlemede kategorinin kendisi ve altındakiler hariç, yollarıyla listelenir. Alınmış ad (BR-EQP-011) ad alanının altında, ağaç kuralı reddi (BR-EQP-002) bildirimle gösterilir. Menüde yeni "Katalog" grubunun ilk öğesi "Kategoriler"dir; teknik müdür modeller ekranı gelene kadar bu ekrandan başlar (MD-07).

**Uygulama (PR 2b):** `/catalog/models` listesi arama, kategori (yolla seçilir, alt ağaç dahil), takip tipi ve durum süzgecini adreste tutar; satırda model adı detay sayfasına gider, kategori yolu, takip tipi, ağırlık ve güç Türkçe biçimle yazılır. Ondalıklar API'den metin gelir ve `formatDecimal` ile metinden biçimlenir (`lib/format.ts`). Form ayrı sayfadır (`/catalog/models/new`, `/catalog/models/{id}/edit`, CT-05): marka, model adı, kategori (aktif kategoriler ve modelin kendi kategorisi, yollarıyla), takip tipi (stoğu olan modelde pasif ve nedeni yazılı, BR-EQP-001), ölçüler. Ondalık alan virgül ya da noktayla, en çok üç basamak kabul eder ve sunucuya noktalı metin olarak gider. Alınmış marka ve ad (BR-EQP-012) ad alanının, pasif kategori reddi (BR-EQP-002) kategori alanının altında gösterilir. Detay sayfasının sekmeleri Genel ve Geçmiş'tir (`/catalog/models/{id}/history`); modeli içeren kitler kitlerle (3b) eklenir. Menüde "Katalog › Modeller" kategorilerin önündedir; teknik müdür geçici olarak bu ekrandan başlar (MD-07).

**Uygulama (PR 3b):** `/catalog/kits` listesi her kitin satır sayısını ve toplamlarını gösterir; değeri eksik model içeren toplam "eksik veri" rozeti taşır. "Kit ekle" yalnızca adı sorar ve yeni kitin sayfasını açar; satırlar orada eklenir. `/catalog/kits/{id}` sayfasında satır düzenleyici vardır: her satırda tür (model ya da kit), arayarak seçim (aktif modeller kategori yoluyla, aktif kitler kendisi hariç; MD-03), adet, yukarı / aşağı taşıma ve kaldırma. Değişiklikler kitin tamamı olarak, kitin sürümüyle kaydedilir (CT-04); hedefsiz ya da 1–9999 dışındaki adetli satır gönderilmez ve işaretlenir. Yanda açılmış içerik (modellere bağlantıyla) ve toplamlar durur. Adı değiştirme diyalogdadır; sekmeler Genel ve Geçmiş'tir. Model detayı modeli içeren kitleri listeler. Menüde "Katalog › Kitler" yer alır.

| # | PR | Kapsam |
|---|---|---|
| 1a | Catalog iskeleti ve kategoriler (sunucu) | Projeler, şema, rol, yetkiler ve rol matrisi; kategori tablosu ve uçları, BR-EQP-002, BR-EQP-011 |
| 1b | Kategoriler ekranı | `/catalog/categories`, menü grubu "Katalog" |
| 2a | Modeller (sunucu) | Model tablosu ve uçları, BR-EQP-001 alan kuralı, BR-EQP-012, `ICatalogDirectory` |
| 2b | Modeller ekranı | Liste, detay, form sayfası |
| 3a | Kitler (sunucu) | Kit tabloları ve uçları, BR-EQP-003, BR-EQP-013, toplam hesabı |
| 3b | Kitler ekranı | Liste, detay ve satır düzenleyici |

## 9. Kararlar

| No | Konu | Karar | Gerekçe |
|---|---|---|---|
| CT-01 | Benzersizlik ve pasif kayıtlar | Pasif kayıtlar da benzersizliğe girer | Pasif kaydın etkinleştirilmesi çakışma yaratmaz; aynı adlı iki model geçmişte karışmaz. |
| CT-02 | Kategori ağacı | Sayfasız düz liste, yol okurken üretilir; materyalize yol ya da `ltree` yok | S1'de yüzlerce kategori beklenir; taşımada alt ağacın yollarını güncellemek gerekmez. |
| CT-03 | Döngü denetimi | Alan kuralı + danışma kilidi (kategori taşıma, kit satırları) | Döngü iki toplu kökü birlikte ilgilendirir; iyimser kilit tek kökü korur, eşzamanlı iki değişikliği yakalamaz. |
| CT-04 | Kit satırları | Kitin tamamı tek istekle kaydedilir | Kit küçük bir toplu köktür; sıralama ve döngü denetimi bütün üzerinde yapılır. |
| CT-05 | Model formu | 11'deki gibi ayrı sayfa | Form ileride (S2 güç, S5 hacim) büyüyecek; 11 §3 onaylı envanterdir. |
| CT-06 | Alt kaydı olan kategorinin pasifleştirilmesi | Reddedilir; önce alttakiler taşınır ya da pasifleştirilir | S2; kazara toplu pasifleştirme olmaz. Kural BR-EQP-002'ye eklendi. |

## 10. Proje sahibine sorulanlar

| Soru | Seçenekler | Yanıt |
|---|---|---|
| S2 — Altında aktif alt kategori ya da aktif model bulunan bir kategori pasifleştirilmek istenirse ne olsun? | Reddedilsin, önce alttakiler taşınır ya da pasifleştirilir / alttakilerle birlikte pasifleşsin | **Reddedilsin** (CT-06) |

Soru 2026-10-02'de yanıtlandı; önerilen seçenek seçildi. US-EQP-001'in 3. kabul kriteri BR-SYS-001 ve yanıtla uyumlu hale getirildi: kategori hiç silinmez.

## 11. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-10-02 | v0.1 | İlk taslak |
| 2026-10-02 | v1.0 | S2 yanıtlandı (CT-06); onaylandı. |
| 2026-10-02 | v1.1 | §4: Catalog iskeleti ve kategorilerin uygulama ayrıntıları (PR 1a). |
| 2026-10-02 | v1.2 | §8: kategoriler ekranının uygulama ayrıntıları (PR 1b). |
| 2026-10-02 | v1.3 | §4: modellerin ve `ICatalogDirectory`'nin uygulama ayrıntıları (PR 2a). |
| 2026-10-02 | v1.4 | §8: modeller ekranlarının uygulama ayrıntıları (PR 2b). |
| 2026-10-02 | v1.5 | §4: kitlerin uygulama ayrıntıları (PR 3a). |
| 2026-10-02 | v1.6 | §8: kitler ekranlarının uygulama ayrıntıları (PR 3b); Catalog adımı tamam. |
