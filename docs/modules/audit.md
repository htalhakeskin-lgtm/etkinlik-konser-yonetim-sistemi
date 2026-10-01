# Modül tasarımı: Audit — İşlem geçmişi

> **Durum:** v1.0 (onaylandı) · **Son güncelleme:** 2026-10-01
> **Adım:** Faz 1.2 ([12 §3](../12-implementation-plan.md#3-adımlar)) · **Kararlar:** [Bölüm 7](#7-kararlar)

## 1. Bu belge ne işe yarar

İşlem geçmişinin **okunma** tarafının tasarımıdır. Yazma tarafı Faz 1.1'de kuruldu: `audit.audit_entries` tablosu, her bağlamdaki eşleme ve kaydetme adımı 4'teki yazıcı ([building-blocks §6](building-blocks.md#6-i̇şlem-geçmişi-yazıcısı)).

**Kaynaklar:** [05 §5.2](../05-module-map.md#52-audit--i̇şlem-geçmişi), [06 §5.2](../06-erd-conceptual.md#52-audit--i̇şlem-geçmişi), [database §14.2](../standards/database.md#142-kayıtların-işlem-geçmişi), [11 §3](../11-screens.md#3-s1-ekran-envanteri); hikaye US-SYS-004; kural BR-SYS-010.

## 2. Yazma tarafına eklenenler

| Değişiklik | Neden |
|---|---|
| `actor_name varchar(200)` kolonu | Kullanıcı adı yazıldığı anki haliyle saklanır ([06 §5.2](../06-erd-conceptual.md#52-audit--i̇şlem-geçmişi)); okuma Identity'ye gitmez, pasifleşen kullanıcının adı da görünür. Yazıcı adı `ICurrentUser`'dan alır; sistem işlerinde ad "Sistem"dir. Mevcut satırlar yalnızca testlerde olduğu için geçiş boş değerle değil, kolonun zorunlu eklenmesiyle yapılır. |
| `ix_audit_entries_actor_id_occurred_at` | Genel ekranın kullanıcı süzgeci. 1.1'deki DT-03 istisnası (`actor_id`) kalkar. |
| `ix_audit_entries_occurred_at_id` | Genel ekran en yeniden eskiye, imleçle sayfalanır. |

`ICurrentUser`'a görünen ad (`DisplayName`) eklenir; Identity'nin uygulaması oturumdaki adı verir.

## 3. Uç noktalar

| Yöntem ve adres | İşlem adı | Yetki | Not |
|---|---|---|---|
| `GET /api/v1/audit-entries` | `ListAuditEntries` | `Audit.Entries.View` | İmleçli ([api §6](../standards/api.md#6-listeler)), en yeniden eskiye. Süzgeçler: `actorId`, `from`, `to` (İstanbul takvim günü), `module`, `entityType`, `entityId`. |

- Kaydın detay sayfasındaki geçmiş (US-SYS-004 kriter 1) aynı uç noktayı `entityType` ve `entityId` ile çağırır; ayrı bir uç nokta yoktur.
- Yanıt satırı: zaman, kullanıcı (kimlik ve ad), modül, kayıt türü ve kimliği, işlem, değişen alanların eski ve yeni değerleri (`changes`), iz kimliği.
- Uç nokta salt okurdur; tabloyu değiştiren bir uç nokta yoktur. Veritabanı rolü de yalnızca okur (DT-02).

## 4. Ekranlar

| Ekran | Adres | Not |
|---|---|---|
| İşlem geçmişi | `/audit` | İmleçli liste ("Daha fazla yükle"); kullanıcı, tarih aralığı ve kayıt türü süzgeci; satır açılınca alan alan eski → yeni değer |
| Kayıt geçmişi sekmesi | Detay sayfalarında | Ortak `HistoryTab` bileşeni; her modül kendi detay sayfasına ekler. 1.2'de kullanıcı detayında görünür. |

**Değerlerin gösterimi:** Alan adları modülün çeviri dosyasından gelir (`{modül}:{varlık}.{alan}`); çevirisi olmayan alan ham adıyla görünür. Enum değerleri de çevrilir; kimlikler (ör. depo kimliği) S1'de kimlik olarak kalır.

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
