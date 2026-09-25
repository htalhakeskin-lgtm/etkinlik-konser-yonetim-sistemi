# Standartlar

Kod yazılırken uyulan kurallar. Her standart, kuralın **ne** olduğunu, **neden** seçildiğini ve **hangi araç ya da testle** denetlendiğini yazar. Bir kural değişecekse önce ilgili belge, sonra araç yapılandırması aynı değişiklikte güncellenir.

| Belge | Kapsam | Durum |
|---|---|---|
| [naming.md](naming.md) | Koddaki, veritabanındaki, API'deki ve ön yüzdeki adlar; sözlükten diğer biçimlere dönüşüm | v1.3 |
| [code-style.md](code-style.md) | Kod biçimi, yazım kuralları, analizörler ve lint araçları | v1.6 |
| [database.md](database.md) | Roller, kimlik tipi, kolon tipleri, zaman, para, ortak kolonlar, eşzamanlılık, kısıtlar, işlem geçmişi, migration'lar | v1.3 |
| [api.md](api.md) | Adres yapısı, yöntemler ve durum kodları, JSON biçimi, listeler, hata yanıtları, eşzamanlı düzenleme, tekrar güvenliği, güvenlik, sürümleme, anlık bildirimler, OpenAPI | v1.3 |
| [security.md](security.md) | Yetki modeli ve roller, oturum ve şifre, gizli bilgiler, veri koruma anahtarları, tarayıcı güvenlik başlıkları, girdi doğrulama, kişisel veri | v1.3 |
| [observability.md](observability.md) | Loglar, izler, ölçümler, sağlık kontrolleri, uyarılar | v1.1 |
| [configuration.md](configuration.md) | Ortamlar, ayar kaynakları, ayar doğrulama, parametre eşlemesi, dosya saklama, dış servis çağrıları | v1.2 |
| [git.md](git.md) | Dal düzeni, commit mesajları, PR ve birleştirme, görev takibi, sürüm numaraları, commit kancaları, bağımlılık güncellemeleri, gizli bilgi taraması, repo ayarları | v1.3 |
| [testing.md](testing.md) | Test katmanları, değişiklik türüne göre gereken testler, kural izlenebilirliği, test verisi, kararsız testler, kod kapsamı | v1.2 |
| [ci.md](ci.md) | Sürekli entegrasyon: iş akışları, PR hattı, gece ve haftalık işler, yayın hattı, iş akışı güvenliği, lisans ve açık denetimi | v1.2 |
| [definition-of-done.md](definition-of-done.md) | PR, hikaye, hata düzeltmesi ve sürüm için bitti tanımı | v1.2 |
| [ui.md](ui.md) | Arayüz: tasarım değişkenleri, durum tonları, yazı, yerleşim, gezinme, bileşenler, formlar, tablolar, geri bildirim, durumlar, anlık güncelleme, yetki, Türkçe metin ve biçimler, depo ekranları, erişilebilirlik, performans | v1.0 |

Standartlar [08-architecture.md](../08-architecture.md)'deki yapıya ve [07-tech-stack.md](../07-tech-stack.md)'deki araç seçimlerine dayanır.
