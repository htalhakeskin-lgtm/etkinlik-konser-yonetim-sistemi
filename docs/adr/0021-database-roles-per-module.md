# ADR-0021: Modül başına veritabanı rolü

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [standards/database.md §4](../standards/database.md#4-roller-ve-yetkiler), [ADR-0002](0002-module-boundaries-and-layers.md), [ADR-0007](0007-data-access.md)

## Bağlam

Modül sınırları kod tarafında proje referansları ve mimari testlerle korunuyor ([ADR-0002](0002-module-boundaries-and-layers.md)). Ama bir modülün altyapı katmanında yazılan ham SQL, başka modülün şemasındaki tabloya ulaşabilir; mimari testler bunu göremez.

Ayrıca bazı kurallar "hiçbir koşulda" diye yazılmıştır:
- İşlem geçmişi değiştirilemez ve silinemez (BR-SYS-010).
- Stok hareketi, rider versiyonu gibi kayıtlar değişmezdir ([06 §8](../06-erd-conceptual.md#8-değişmez-kayıtlar)).
- Ana veriler silinmez (BR-SYS-001).

Bu kurallar yalnızca kodla korunursa, koddaki tek bir hata onları çiğneyebilir.

## Karar

- Her modül veritabanına kendi rolüyle bağlanır (`festos_booking`, `festos_planning` …). Rol yalnızca kendi şemasına erişir.
- Modül rolleri `audit.audit_entries` tablosuna yalnızca `INSERT` yapabilir.
- Değişmez tablolarda modül rolünün `UPDATE` ve `DELETE` yetkisi, silinmeyen ana veri tablolarında da `DELETE` yetkisi yoktur.
- Şemayı yalnızca migration rolü (`festos_migrator`) değiştirir; modül rolleri şema değiştiremez.
- Roller ve yetkiler migration'larla oluşturulur; doğruluklarını bir veritabanı testi (DT-02) denetler.

## Sonuçlar

**Olumlu:**
- Modül sınırı veritabanında da garanti altındadır; ham SQL bile sınırı aşamaz.
- Değişmezlik ve silinmezlik kuralları veritabanında zorlanır; koddaki bir hata veriyi bozamaz.
- `pg_stat_activity`'de her bağlantının hangi modüle ait olduğu görünür.
- En az yetki ilkesi uygulanır: bir modülün kimlik bilgisi sızsa bile yalnızca o modülün verisi risk altındadır.

**Olumsuz / bedeli:**
- 10 ayrı bağlantı dizesi ve parola yönetilir. Yerel geliştirmede bir kurulum betiği, yayında ortam gizlileri (C.6) bunu karşılar.
- 10 ayrı bağlantı havuzu vardır; toplam bağlantı sayısı `max_connections` içinde planlanmalıdır.
- Yeni tablo eklenirken yetkileri de (özellikle değişmez tablolarda) doğru verilmelidir; DT-02 eksik ya da fazla yetkiyi yakalar.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Tek uygulama rolü, sınır yalnızca kodda | Basittir; ama ham SQL ile sınır aşılabilir ve değişmezlik yalnızca koda güvenir. |
| Tek bağlantı + her işlemde `SET ROLE` | Tek havuz kullanılır; ama bağlantı havuza dönerken rol sıfırlandığı için her kullanımda ek komut gerekir ve rolün unutulması güvenliği sessizce kaldırır. |
| Değişmezliği tetikleyiciyle (trigger) zorlamak | İş kuralını veritabanı koduna taşır; yetki vermemek daha basit ve daha güçlüdür. |
| Satır düzeyi güvenlik (RLS) | Tek şirketli bir sistemde satır bazında ayrım ihtiyacı yok. |
