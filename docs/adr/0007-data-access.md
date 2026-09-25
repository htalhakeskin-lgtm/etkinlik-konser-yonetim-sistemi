# ADR-0007: Veri erişimi: PostgreSQL 18 ve EF Core 10

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [07-tech-stack.md §3.2](../07-tech-stack.md#32-veri), [05-module-map.md §13](../05-module-map.md#13-kod-ve-veritabanına-yansıması), [ADR-0002](0002-module-boundaries-and-layers.md)

## Bağlam

- Veritabanı olarak PostgreSQL projenin başında seçilmişti.
- Her modülün kendi şeması olacak ve modüller birbirinin tablolarına erişmeyecek.
- İşlem geçmişi gibi değişken yapılı veriler var; bunlar için JSON desteği gerekiyor.
- Birden fazla uygulama örneği çalıştığında zamanlanmış işlerin tek bir kez çalışması gerekiyor; bunun için bir kilit mekanizması gerekli.

## Karar

- **Veritabanı:** PostgreSQL 18.
- **ORM:** Entity Framework Core 10, Npgsql sağlayıcısıyla.
- **Modül başına bağlam:** Her modülün kendi `DbContext`'i vardır ve varsayılan şeması modülün şemasıdır. Bir bağlam yalnızca kendi şemasındaki tabloları eşler. Tek istisna işlem geçmişi yazıcısıdır ([05 §5.2](../05-module-map.md#52-audit--i̇şlem-geçmişi)).
- **Migration'lar:** Her modülün kendi migration seti ve kendi migration geçmişi tablosu vardır. Modüller birbirinden bağımsız olarak şema değiştirebilir.
- **Okuma sorguları:** Karmaşık okuma ve hesaplama sorguları (ör. müsaitlik) EF Core üzerinden ham SQL ile yazılabilir. Dapper gibi ek bir kütüphane ancak ihtiyaç kanıtlanırsa eklenir.
- **Kullanılan PostgreSQL özellikleri:** değişken yapılı veri için `jsonb`, zamanlanmış işlerin tek çalışması için advisory lock ([ADR-0013](0013-scheduled-jobs.md)).

Tablo, kolon, indeks ve kısıt adları [standards/naming.md](../standards/naming.md#5-veritabanı-adları)'dedir. Kimlik tipi, ortak kolonlar, roller, eklentiler ve diğer fiziksel kurallar [standards/database.md](../standards/database.md)'dedir.

## Sonuçlar

**Olumlu:**
- PostgreSQL ücretsizdir; Linux'ta sorunsuz çalışır ve barındırma maliyeti düşüktür.
- JSON, zaman aralığı ve kilit gibi bu projede gereken özellikleri yerleşik olarak sunar.
- Modül başına bağlam ve migration, modül sınırını veritabanı seviyesinde de korur.

**Olumsuz / bedeli:**
- 10 ayrı bağlam ve migration seti yönetilir; migration'ların doğru sırayla uygulanması için bir başlangıç adımı gerekir.
- Modüller arası sorgu yazılamaz (bu bilinçli bir kısıttır).

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| SQL Server | Lisans ve barındırma maliyeti; PostgreSQL'e göre bu projeye ek bir getirisi yok. |
| MongoDB gibi belge veritabanları | Alan güçlü biçimde ilişkisel; tutarlılık kuralları ilişkisel modelde daha güvenli uygulanır. |
| Tüm modüller için tek bağlam | Modül sınırı veritabanı seviyesinde kaybolur; her modül her tabloyu görebilir. |
| Yalnızca Dapper | Tam kontrol sağlar ama eşleme, değişiklik takibi ve migration'lar elle yazılır; tek kişilik proje için fazla iş. |
