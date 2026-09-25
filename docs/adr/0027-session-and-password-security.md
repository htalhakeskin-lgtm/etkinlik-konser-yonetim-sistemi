# ADR-0027: Oturum ve şifre güvenliği ayrıntıları

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [ADR-0011](0011-authentication.md) (bu ADR onu genişletir), [standards/security.md §4–7](../standards/security.md#4-kimlik-doğrulama-ve-oturum), BR-SYS-005, BR-SYS-006, BR-SYS-007, BR-SYS-008

## Bağlam

[ADR-0011](0011-authentication.md) sunucu tarafı oturum, çerez ve ASP.NET Core Identity'nin şifre özetleyicisini seçti. Ama güvenliği belirleyen ayrıntılar kararlaştırılmamıştı:
- oturumun mutlak süresi,
- oturum anahtarının nasıl saklanacağı,
- şifre kuralları ve özetleme gücü,
- veri koruma anahtarlarının nerede tutulacağı.

Bu ayrıntılar sonradan değişirse tüm kullanıcıların şifresini ya da oturumunu etkiler.

Üç kaynak güncellendi:
- NIST'in 2025'te yayımlanan rehberi (SP 800-63B-4) şifre kurallarını baştan değiştirdi: karışım kuralı ve düzenli değiştirme zorunluluğu yok, uzunluk ve sızmış şifre kontrolü var.
- OWASP, PBKDF2-HMAC-SHA512 için ASP.NET'in varsayılanının (100.000 tur) iki katından fazlasını öneriyor.
- ASP.NET'in veri koruma anahtarları varsayılan olarak konteynerle birlikte kayboluyor.

## Karar

**Oturum:**
- Çerez `__Host-festos_session`; `HttpOnly`, `Secure`, `SameSite=Strict`.
- Veritabanında oturum anahtarının yalnızca SHA-256 özeti tutulur.
- Hareketsizlik süresi P-03 (12 saat); ek olarak mutlak süre P-16 (24 saat).
- Her girişte yeni oturum açılır; çıkışta sunucu kaydı silinir; şifre değişince diğer oturumlar biter.
- E-posta kayıtlı değilse de şifre özetleme yapılır; yanıt süresinden hangi e-postaların kayıtlı olduğu anlaşılamaz.

**Şifre:**
- En kısa uzunluk P-04 (15 karakter), en uzun 128 karakter. Boşluk karakterleri kullanılamaz; diğer tüm karakterler serbesttir (NFC biçimine getirilerek).
- Karışım kuralı ve düzenli değiştirme zorunluluğu yok.
- Yaygın şifre listesi, e-posta ve ürün adı kontrolü dış servis olmadan yapılır.
- PBKDF2-HMAC-SHA512, 210.000 tur; eski ayarla özetlenmiş şifreler girişte otomatik yeniden özetlenir.

**Veri koruma anahtarları:**
- Veritabanında saklanır; uygulama adı sabittir (`FestOS`).
- Demo ve yayında anahtarlar X.509 sertifikasıyla şifrelenir.

## Sonuçlar

**Olumlu:**
- Çalınmış bir çerez sınırsız kullanılamaz; veritabanı sızıntısı tek başına oturum ele geçirmeye yetmez.
- Şifre politikası güncel standartla uyumludur; kullanıcıyı kısa ama karmaşık, unutulan şifrelere zorlamaz.
- Yayın ve yeniden başlatmalar kullanıcıları oturumdan düşürmez; açık sekmelerdeki formlar bozulmaz.
- Özetleme gücü ileride kullanıcıya hissettirmeden artırılabilir.

**Olumsuz / bedeli:**
- Giriş başına özetleme süresi artar (onlarca milisaniye); giriş sık yapılan bir işlem olmadığı için kabul edilebilir.
- Mutlak süre dolunca, etkin çalışan kullanıcı da yeniden giriş yapar.
- Sertifika bir gizli bilgi olarak yönetilmeli ve süresi dolmadan yenilenmelidir (D bölümü).

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Yalnızca hareketsizlik süresi | Sürekli kullanılan (ya da çalınıp kullanılan) bir oturum hiç sona ermez. |
| Oturum anahtarını veritabanında düz saklamak | Veritabanı ya da yedek sızarsa tüm açık oturumlar ele geçirilebilir. |
| Karışım kuralları ve 90 günde bir değiştirme | NIST bunları yasakladı; kullanıcıyı tahmin edilebilir şifrelere iter. |
| Sızmış şifre kontrolünü dış servisle yapmak (Have I Been Pwned) | Her şifre değişikliği dış bir servise bağlanır; servis erişilemezse karar verilemez. Yerel liste yeterli koruma sağlar. |
| Argon2id | OWASP'ın ilk önerisidir, ama .NET'te yerleşik değildir; ek kütüphane gerekir. PBKDF2, yeterli tur sayısıyla kabul edilen bir seçenektir. |
| Veri koruma anahtarlarını kalıcı bir dosya biriminde tutmak | Birden fazla uygulama örneğinde paylaşılamaz ve yedekleme ayrıca düşünülmelidir. Veritabanı her ikisini de karşılar. |
