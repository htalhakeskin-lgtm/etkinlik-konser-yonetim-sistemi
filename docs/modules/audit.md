# Modül tasarımı: Audit — İşlem geçmişi

> **Durum:** v1.4 (onaylandı) · **Son güncelleme:** 2026-10-02
> **Adım:** Faz 1.2; kök kayıt 1.3 ([12 §3](../12-implementation-plan.md#3-adımlar)) · **Kararlar:** [Bölüm 7](#7-kararlar)

## 1. Bu belge ne işe yarar

İşlem geçmişinin **okunma** tarafının tasarımıdır. Yazma tarafı Faz 1.1'de kuruldu: `audit.audit_entries` tablosu, her bağlamdaki eşleme ve kaydetme adımı 4'teki yazıcı ([building-blocks §6](building-blocks.md#6-i̇şlem-geçmişi-yazıcısı)).

**Kaynaklar:** [05 §5.2](../05-module-map.md#52-audit--i̇şlem-geçmişi), [06 §5.2](../06-erd-conceptual.md#52-audit--i̇şlem-geçmişi), [database §14.2](../standards/database.md#142-kayıtların-işlem-geçmişi), [11 §3](../11-screens.md#3-s1-ekran-envanteri); hikaye US-SYS-004; kural BR-SYS-010.

## 2. Yazma tarafına eklenenler

| Değişiklik | Neden |
|---|---|
| `actor_name varchar(200)` kolonu | Kullanıcı adı yazıldığı anki haliyle saklanır ([06 §5.2](../06-erd-conceptual.md#52-audit--i̇şlem-geçmişi)); okuma Identity'ye gitmez, pasifleşen kullanıcının adı da görünür. Yazıcı adı `ICurrentUser`'dan alır; sistem işlerinde ad "Sistem"dir. Mevcut satırlar yalnızca testlerde olduğu için geçiş boş değerle değil, kolonun zorunlu eklenmesiyle yapılır. |
| `ix_audit_entries_actor_id_occurred_at` | Genel ekranın kullanıcı süzgeci. 1.1'deki DT-03 istisnası (`actor_id`) kalkar. |
| `ix_audit_entries_occurred_at_id` | Genel ekran en yeniden eskiye, imleçle sayfalanır. |
| `root_type varchar(200)`, `root_id uuid` (1.3) | Satırın ait olduğu toplu kök ([parties MD-01](parties.md#2-adımın-ortak-kararları)): iletişim bilgisinin satırı tarafı, dönemin satırı mekanı gösterir. Kayıt geçmişi köke göre okunur, alt varlıkların değişiklikleri de görünür. |
| `ix_audit_entries_root_id_occurred_at_id` (1.3) | Kayıt geçmişi; 1.1'deki `(entity_type, entity_id, occurred_at)` indeksinin yerini alır. |

`ICurrentUser`'a görünen ad (`DisplayName`) eklenir; Identity'nin uygulaması oturumdaki adı verir.

**Uygulama (1.3, PR 0):** Yazıcı kökü EF modelinden bulur: toplu kökte kök kendisidir; alt varlıkta `CASCADE` yabancı anahtar izlenir ([database §10.2](../standards/database.md#102-silme)), ara varlıkta (mekan ekipmanı satırı) üst kayıt izleyicide aranır. Hiçbir köke bağlı olmayan kayıt kendi köküdür. Var olan satırlar migration'da kendileriyle doldurulur; o güne kadar yalnızca toplu kökler yazılmıştı. `entity_id` artık indeksli değildir, DT-03 istisnası gerekçesiyle güncellendi.

**Uygulama (PR 1):** `ICurrentUser.DisplayName` ve kayıt adımının `ActingUser`'ı adı verir; istekte oturumdaki ad (`ClaimTypes.Name`), dinleyici ve işlerde `SystemUser.Name` ("Sistem"). Yazıcı adı her satıra `actor_name` olarak yazar. Tablo Audit'in migration'ındadır; diğer modüllerin bağlamları tabloyu eşleyip oluşturmadığı için onların migration'ları yalnızca model anlık görüntüsünü günceller (boş `Up`).

## 3. Uç noktalar

| Yöntem ve adres | İşlem adı | Yetki | Not |
|---|---|---|---|
| `GET /api/v1/audit-entries` | `ListAuditEntries` | `Audit.Entries.View` | İmleçli ([api §6](../standards/api.md#6-listeler)), en yeniden eskiye. Süzgeçler: `actorId`, `from`, `to` (İstanbul takvim günü), `module`, `rootType`, `rootId` (1.2'de `entityType`, `entityId`). |

- Kaydın detay sayfasındaki geçmiş (US-SYS-004 kriter 1) aynı uç noktayı `rootType` ve `rootId` ile çağırır; ayrı bir uç nokta yoktur.
- Yanıt satırı: zaman, kullanıcı (kimlik ve ad), modül, değişen kaydın türü ve kimliği, ait olduğu kökün türü ve kimliği, işlem, değişen alanların eski ve yeni değerleri (`changes`), iz kimliği.
- Uç nokta salt okurdur; tabloyu değiştiren bir uç nokta yoktur. Veritabanı rolü de yalnızca okur (DT-02).

**Uygulama (PR 2):** Modül salt okunur ve tek projelidir (Infrastructure); sorgu, sonucu ve işleyicisi oradadır. İmleç, son satırın zamanı ve kimliğidir (`TimeAndIdCursor`, BuildingBlocks); bir sonraki dilim `(occurred_at, id) < (…)` satır değeri karşılaştırmasıyla alınır ve `ix_audit_entries_occurred_at_id` indeksini kullanır. Tanınmayan imleç `400 invalidCursor` döner. `from` / `to` İstanbul günleridir ve aralık yarı açıktır (`to` dahil değil, api §6.3). `changes` alan başına `old` / `new` değerli bir nesnedir. `Audit.Entries.View` yetkisi Audit'in kendi sabitindedir; rol matrisi (Identity) Audit'e başvuramadığı için kodu metin olarak yazar, ikisinin aynı kalmasını mimari test denetler (BR-SYS-010: yalnızca sistem yöneticisi ve genel müdür).

## 4. Ekranlar

| Ekran | Adres | Not |
|---|---|---|
| İşlem geçmişi | `/audit` | İmleçli liste ("Daha fazla yükle"); kullanıcı, tarih aralığı ve kayıt türü (kök) süzgeci; satır açılınca alan alan eski → yeni değer; alt varlığın satırı değişen parçanın türünü de yazar |
| Kayıt geçmişi sekmesi | Detay sayfalarında | Ortak `HistoryTab` bileşeni; her modül kendi detay sayfasına ekler. 1.2'de kullanıcı detayında görünür. |

**Değerlerin gösterimi:** Alan adları modülün çeviri dosyasından gelir (`{modül}:{varlık}.fields.{alan}`, ör. `identity:User.fields.roles`); çevirisi olmayan alan ham adıyla görünür. Enum değerleri de çevrilir (`{modül}:{varlık}.values.{alan}.{değer}`); kimlikler (ör. depo kimliği) S1'de kimlik olarak kalır. Doğru / yanlış "Evet / Hayır", boş değer "—", zaman damgası İstanbul saatiyle yazılır.

**Uygulama (PR 3):** Ortak `HistoryList` imleçli listeyi `useInfiniteQuery` ile yükler; "Daha fazla yükle" bir sonraki dilimi ister, kaydırınca kendiliğinden yüklemez (ui §8). Satırda zaman, kullanıcı, işlem rozeti ve (genel ekranda) kayıt türü ile kimliği vardır; "Değişiklikleri göster" alan alan eski → yeni tablosunu açar. `/audit` süzgeçleri adreste tutar; bitiş günü ekranda dahildir, sunucuya bir sonraki gün olarak gider. Kullanıcı süzgeci, kullanıcıları görme yetkisi olana görünür. 1.2'de kullanıcı ve depo için ayrı detay sayfası olmadığından kayıt geçmişi satır menüsündeki "Geçmiş" ile `HistoryDialog`'da açılır; detay sayfaları geldiğinde aynı içerik `HistoryTab` olarak sekmeye girer. Menüde "Yönetim › İşlem geçmişi" yer alır.

## 5. Hikaye ve kural eşlemesi

| Hikaye | Karşılığı | Kural |
|---|---|---|
| US-SYS-004 İşlem geçmişi | Kriter 1: `HistoryTab`; kriter 2–3: `/audit` ve süzgeçleri; kriter 4: rol yetkisi ve tablonun değişmezliği (1.1); kriter 5: `Audit.Entries.View` yalnızca SY ve GM | BR-SYS-010 |

## 6. PR planı

| # | PR | Kapsam |
|---|---|---|
| 1 | Kullanıcı adı ve indeksler | `actor_name`, `ICurrentUser.DisplayName`, iki indeks, migration'lar |
| 2 | İşlem geçmişi uç noktası | `ListAuditEntries` ve süzgeçleri |
| 3 | İşlem geçmişi ekranları | `/audit`, `HistoryTab`, kullanıcı detayında geçmiş |

## 7. Kararlar

| No | Konu | Karar | Gerekçe |
|---|---|---|---|
| AU-01 | Kullanıcı adının kaynağı | Yazarken anki adı kaydetmek | 06'daki karar; okuma modüller arası çağrı gerektirmez |
| AU-02 | Kayıt geçmişi uç noktası | Ayrı uç nokta yok; genel uç noktanın süzgeçleri | Tek sorgu, tek yetki, tek test seti |
| AU-03 | Sayfalama | İmleç (`occurred_at`, `id`) | Tablo yalnızca büyür; sayfa numarası derin sayfalarda yavaşlar ve yeni kayıtla kayar |

Bu belgede proje sahibine soru yoktur.

## 8. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-10-01 | v0.1 | İlk taslak |
| 2026-10-01 | v1.0 | Onaylandı. |
| 2026-10-02 | v1.1 | §2: kullanıcı adı ve indekslerin uygulama ayrıntıları (PR 1). |
| 2026-10-02 | v1.2 | §3: işlem geçmişi uç noktasının uygulama ayrıntıları (PR 2). |
| 2026-10-02 | v1.3 | §4: işlem geçmişi ekranlarının uygulama ayrıntıları (PR 3); çeviri anahtarlarının biçimi. |
| 2026-10-02 | v1.4 | §2–§4: satırın kök kaydı (`root_type`, `root_id`), kayıt geçmişi köke göre (Faz 1.3, parties MD-01). |
