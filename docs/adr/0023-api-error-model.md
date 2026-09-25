# ADR-0023: API hata modeli

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [standards/api.md §4.2, §8](../standards/api.md#8-hata-yanıtları), [03-business-rules.md §3](../03-business-rules.md#3-biçim-ve-kullanım), K-02

## Bağlam

- İş kuralları numaralıdır ve kullanıcıya gösterilen hata mesajında numarasıyla anılır ([03 §3](../03-business-rules.md#3-biçim-ve-kullanım)).
- Arayüz metinleri ön yüzün çeviri dosyalarındadır (K-02); sunucunun Türkçe metin üretmesi, metinlerin iki yerde tutulması demektir.
- Doğrulama hataları alan bazında gösterilmelidir; toplu girişte (yapıştırılan seri numarası listesi) satır bazında.
- Bazı hatalar bağlam taşır: aynı birim iki cihazda okutulduğunda ilk okutanın adı ve zamanı gösterilir (BR-WHS-006).

## Karar

- Tüm hatalar RFC 9457 Problem Details biçimindedir.
- Ek alanlar:
  - `code`: kural numarası (`BR-EVT-009`) ya da sabit teknik kod (`validation`, `concurrencyConflict` …),
  - `params`: mesajın ihtiyaç duyduğu değerler,
  - `errors`: doğrulama hataları; her öğe JSON Pointer, kod ve parametre taşır,
  - `traceId`: OpenTelemetry iz kimliği.
- `type` alanı `urn:festos:problem:{tür}` biçimindedir. `title` ve `detail` İngilizcedir ve geliştirici içindir; kullanıcı metni ön yüzde `code` üzerinden çevrilir.
- Durum kodları: biçim ve alan doğrulama hataları `400`, tüm iş kuralı ihlalleri `422`, sürüm çakışması `412`, sürüm başlığı eksikliği `428`, tekrar güvenliği çakışması `409` / `422`.
- Eşleme tek bir yerde, Host'taki hata işleyicide yapılır. Veritabanı kısıt ihlalleri de kural koduna eşlenir.

## Sonuçlar

**Olumlu:**
- Ön yüz her hatayı tek bir yöntemle işler: `code` → çeviri anahtarı, `params` → metnin değişkenleri.
- Kural numarası API'den ekrana kadar kaybolmaz; destek talebinde "hangi kural?" sorusu hemen cevaplanır.
- Standart biçim; OpenAPI ve araçlar tarafından tanınır.
- `traceId` sayesinde kullanıcının gördüğü hata, sunucu loglarında doğrudan bulunur.

**Olumsuz / bedeli:**
- Her yeni kural ve doğrulama kodu için ön yüzde bir çeviri anahtarı gerekir. Eksik anahtar, tip denetimi ve bir test ile yakalanır.
- İş kuralı ihlallerinin hepsinin `422` olması, durum kodundan "şimdi mi, hiç mi geçersiz" ayrımını okumayı engeller. Bu ayrım kural numarasında zaten vardır.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Sunucunun Türkçe mesaj döndürmesi | Metinler sunucu ve ön yüzde iki kez tutulur; K-02'deki kaynak dosyası ilkesine aykırı. |
| ASP.NET'in varsayılan doğrulama biçimi (`errors: { alan: [mesajlar] }`) | Mesaj metni taşır, kod ve parametre taşımaz; iç içe alanları (satır numarası) JSON Pointer kadar açık göstermez. |
| İş kuralı ihlallerini 409 ve 422 arasında bölmek | 90'dan fazla kural için tek tek tartışmaya açık bir ayrım; ön yüz zaten koda göre davranır. |
| Kendi hata biçimimiz (`{ success: false, error: … }`) | Standart dışı; araç desteği yok. |
