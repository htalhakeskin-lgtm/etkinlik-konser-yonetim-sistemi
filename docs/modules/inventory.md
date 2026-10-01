# Modül tasarımı: Inventory — Stok ve depo

> **Durum:** v1.3 (onaylandı) · **Son güncelleme:** 2026-10-01
> **Adım:** Faz 1.2 (depolar); stok 1.5'te, depo işlemleri 1.7'de bu belgeye eklenir ([12 §3](../12-implementation-plan.md#3-adımlar)) · **Kararlar:** [Bölüm 8](#8-kararlar)

## 1. Bu belge ne işe yarar

Inventory modülünün fiziksel tasarımıdır. Modül üç adımda yazılır; bu sürüm yalnızca **depoları** kapsar. Depolar 1.2'dedir, çünkü kullanıcılara depo atanabilmesi gerekir ([12 §3](../12-implementation-plan.md#3-adımlar), sıranın gerekçesi). Stok, birimler, kasalar ve etiketler 1.5'te; okutma ve stok hareketleri 1.7'de bu belgeye eklenir.

**Kaynaklar:** [05 §5](../05-module-map.md#5-modül-kartları), [06](../06-erd-conceptual.md) (`Warehouse`), [11 §3](../11-screens.md#3-s1-ekran-envanteri); hikaye US-SYS-005; kurallar BR-SYS-001, BR-SYS-013, BR-SYS-016 (yeni).

## 2. Projeler

`FestOS.Modules.Inventory.{Domain, Application, Contracts, IntegrationEvents, Infrastructure}`; veri erişim kalıbı Identity'dekiyle aynıdır ([identity ID-01](identity.md#12-kararlar)). Şema `inventory`, veritabanı rolü `festos_inventory`.

## 3. Veritabanı

| Tablo | Kolonlar (ortak kolonlar hariç) | Kısıtlar ve indeksler |
|---|---|---|
| `warehouses` | `id`, `name varchar(100)`, `city varchar(100)`, `address varchar(500)`, `is_active bool`, `version` | `ux_warehouses_name` → BR-SYS-016 (pasif depolarda da tekil); ad karşılaştırması büyük-küçük harf duyarsızdır ([database §13](../standards/database.md#13-metin-sıralama-ve-arama)) |

`Warehouse` toplu köktür; işlem geçmişine yazılır.

**Uygulama (PR 1a):** Pasiflik, `is_active` yerine ortak kalıpla tutulur: `deactivated_at`, `deactivated_by` ([database §10.1](../standards/database.md#101-pasifleştirme)); Identity'deki kullanıcılarla aynıdır. Adın tekilliği, adın arama anahtarı olan `name_search` kolonundaki `ux_warehouses_name` indeksiyle sağlanır (BR-SYS-016): büyük-küçük harf ve Türkçe işaret farkı ikinci bir ad yapmaz ("Işıklar Depo" ile "ISIKLAR DEPO" aynıdır). Yetki sabitleri (`InventoryPermissions`) Contracts'tadır, çünkü onları veren rol matrisi Identity'dedir; Identity Inventory'nin Contracts'ına başvurabilir ([08 §3.3](../08-architecture.md#33-modüller-arası-referanslar)).

## 4. Kurallar

| Kural | Uygulama |
|---|---|
| BR-SYS-001 Silme yerine pasifleştirme | Depo silinmez; pasifleştirilir ve yeniden etkinleştirilebilir. Pasif depo yeni işlemlerde seçilemez. |
| BR-SYS-013 Depo kısıtları | **1.2:** son aktif depo pasifleştirilemez (alan kuralı; aktif depo sayısı aynı işlemde danışma kilidiyle sayılır). **1.5 ve 1.6:** stoğu, açık transferi ya da açık rezervasyonu olan depo da pasifleştirilemez; bu denetimler o adımlarda eklenir ve kural testleri genişler. |
| BR-SYS-016 Tekil depo adı (yeni) | Depo adı, büyük-küçük harf farkı gözetmeden tekildir; pasif depoların adı da yeniden kullanılamaz. US-SYS-005'in kabul kriteriydi; veritabanı kısıtına bağlanabilmesi (DT-04) için numaralı kural olur. |

## 5. Uç noktalar

| Yöntem ve adres | İşlem adı | Yetki | Not |
|---|---|---|---|
| `GET /api/v1/warehouses` | `ListWarehouses` | `Inventory.Warehouses.View` | Sayfalı; aktiflik süzgeci (varsayılan: aktif) |
| `GET /api/v1/warehouses/{warehouseId}` | `GetWarehouse` | `Inventory.Warehouses.View` | `ETag` |
| `POST /api/v1/warehouses` | `CreateWarehouse` | `Inventory.Warehouses.Create` | Ad, şehir, adres zorunlu |
| `PUT /api/v1/warehouses/{warehouseId}` | `EditWarehouse` | `Inventory.Warehouses.Edit` | `If-Match` |
| `POST /api/v1/warehouses/{warehouseId}/deactivate` | `DeactivateWarehouse` | `Inventory.Warehouses.Deactivate` | BR-SYS-013; `WarehouseDeactivated` yayınlar |
| `POST /api/v1/warehouses/{warehouseId}/activate` | `ActivateWarehouse` | `Inventory.Warehouses.Deactivate` | |

## 6. Sözleşmeler ve olaylar

- **Senkron sözleşme (Contracts):** `IWarehouseDirectory.FindActiveAsync(ids)`; Identity, depo atamasında depoların var ve aktif olduğunu doğrular ([05 §3](../05-module-map.md#3-modüller), BR-SYS-014). Uygulaması Inventory'nin Infrastructure'ındadır ve kendi rolüyle kendi şemasını okur.
- **Yayınladığı olaylar (IntegrationEvents):** `WarehouseDeactivated { WarehouseId }`, sıra anahtarı depo kimliği. 1.2'de Identity dinler.
- **Anlık bildirim:** `warehouses` liste grubu ve `warehouses:{id}` kayıt grubu; değişiklik `resourceChanged` olarak duyurulur (building-blocks §11). Gruba her oturum açmış kullanıcı katılabilir, çünkü depo listesi herkese açıktır.

**Uygulama (PR 1b):** Uçlar §5'teki gibidir; değiştirenler `If-Match` ister ve yanıtta deponun kaydedilmiş halini yeni `ETag` ile döner, oluşturma `201` ve `Location` ile döner. Liste adın arama anahtarında arar (`q`), `status` (`active` varsayılan, `inactive`, `all`) ile süzer, `name` ve `city`'ye göre Türkçe sırayla sıralar. BR-SYS-013: pasifleştirme, `inventory:warehouses:active` danışma kilidini aldıktan sonra başka aktif depo arar (IN-02); zaten pasif olan depoda bir şey yapmaz. Pasifleşen depo `WarehouseDeactivatedDomainEvent` üretir, aynı kayıtta giden kutusuna `WarehouseDeactivatedIntegrationEvent` yazılır ve `warehouses` ile `warehouses:{id}` gruplarına `resourceChanged` duyurulur. Oluşturma ve düzenleme bir bütünleşme olayı üretmediği için şimdilik duyurulmaz; ekran kendi değişikliğinden sonra listeyi yeniler. `IWarehouseDirectory` Inventory'nin Infrastructure'ında, kendi bağlamı ve rolüyle uygulanır.

## 7. Ekranlar ve PR planı

| Ekran | Adres | Not |
|---|---|---|
| Depolar | `/admin/warehouses` | Liste + oluşturma / düzenleme diyaloğu; pasifleştirme `ConfirmDialog` ile |

| # | PR | Kapsam |
|---|---|---|
| 1a | Inventory iskeleti | Projeler, şema, rol, `warehouses`, yetkiler ve rol matrisi, BR-SYS-016 |
| 1b | Depolar (sunucu) | Uç noktalar, BR-SYS-013 (son aktif depo), `WarehouseDeactivated`, `IWarehouseDirectory` |
| 2 | Depolar ekranı | `/admin/warehouses` |

**Uygulama (PR 2):** `/admin/warehouses` kullanıcılar ekranının kalıbını izler: arama, durum süzgeci, sıralama ve sayfa adreste; satır sonundaki "⋯" menüsü yalnızca yetkili işlemleri gösterir; pasifleştirme onay ister, etkinleştirme istemez. Alınmış ad (BR-SYS-016) ad alanının altında, son aktif depo reddi (BR-SYS-013) kısa bildirimle gösterilir. Menüde "Yönetim › Depolar" olarak yer alır; depoları görme her rolde olduğundan menü öğesi herkese görünür, yönetim düğmeleri yalnızca sistem yöneticisine. Ön yüz kodu `modules/inventory`, metinler `locales/tr/inventory.json`'dadır.

## 8. Kararlar

| No | Konu | Karar | Gerekçe |
|---|---|---|---|
| IN-01 | Depoların yeri | Inventory modülü, 1.2'de yalnızca depolarla açılır | Depo stoğun ve okutmanın sahibidir ([05](../05-module-map.md)); Identity'ye taşımak modül sınırını bozardı |
| IN-02 | "Son aktif depo" denetimi | Danışma kilidi altında aktif depo sayısı | İki depo aynı anda pasifleşirse ikisi de "başka aktif depo var" görmesin |
| IN-03 | Yeniden etkinleştirme | Pasif depo geri açılabilir | Silme olmadığı için hata düzeltilebilir olmalı |

Bu belgede proje sahibine soru yoktur.

## 9. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-10-01 | v0.1 | İlk taslak (depolar) |
| 2026-10-01 | v1.0 | Onaylandı. |
| 2026-10-01 | v1.1 | §3: PR 1a'nın uygulama ayrıntıları (ortak pasifleştirme kolonları, ad için arama anahtarı); §7: PR 1 ikiye bölündü. |
| 2026-10-01 | v1.2 | §6: depo uçlarının uygulama ayrıntıları (PR 1b). |
| 2026-10-01 | v1.3 | §7: depolar ekranının uygulama ayrıntıları (PR 2). |
