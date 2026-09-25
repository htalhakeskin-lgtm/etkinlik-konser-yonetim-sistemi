# Standartlar

Kod yazılırken uyulan kurallar. Her standart, kuralın **ne** olduğunu, **neden** seçildiğini ve **hangi araç ya da testle** denetlendiğini yazar. Bir kural değişecekse önce ilgili belge, sonra araç yapılandırması aynı değişiklikte güncellenir.

| Belge | Kapsam | Durum |
|---|---|---|
| [naming.md](naming.md) | Koddaki, veritabanındaki, API'deki ve ön yüzdeki adlar; sözlükten diğer biçimlere dönüşüm | v1.0 |
| [code-style.md](code-style.md) | Kod biçimi, yazım kuralları, analizörler ve lint araçları | v1.0 |
| `database.md` | Kimlik tipi, ortak kolonlar, para hassasiyeti, zaman, eşzamanlılık, migration kuralları | C.4'te yazılacak |
| `api.md` | Adres yapısı, sürümleme, hata biçimi, sayfalama, filtreleme, sıralama | C.5'te yazılacak |
| `git.md` | Dal düzeni, commit biçimi, PR kontrol listesi | D bölümünde yazılacak |

Standartlar [08-architecture.md](../08-architecture.md)'deki yapıya ve [07-tech-stack.md](../07-tech-stack.md)'deki araç seçimlerine dayanır.
