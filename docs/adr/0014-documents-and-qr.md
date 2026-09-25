# ADR-0014: PDF üretimi ve QR

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** US-WHS-001, US-WHS-002, US-EQP-007, US-MRP-006, BR-EQP-004, [ADR-0005](0005-dependency-license-policy.md)

## Bağlam

S1'de üretilen PDF'ler:
- toplama listesi,
- rider karşılama raporu,
- QR etiket sayfası.

S2'de bunlara day sheet, run of show ve call sheet eklenecek. Etiketlerde ve toplama listesinde QR kod bulunur; depo ekranları telefon kamerasıyla QR okur. Belgelerde Türkçe karakterler doğru görünmelidir.

## Karar

- **PDF:** QuestPDF. Belge yerleşimi C# ile yazılır. Türkçe karakterleri destekleyen yazı tipi (arayüzle aynı Inter, [ADR-0033](0033-design-system.md)) belgeye gömülür.
- **Lisans:** QuestPDF'in topluluk lisansı; bireyler, açık kaynak projeler ve yıllık geliri 1 milyon ABD dolarının altındaki şirketler için ücretsizdir. Bu, [ADR-0005](0005-dependency-license-policy.md)'teki istisna kuralıyla kabul edilir ve **bu projede hiçbir ücret ödenmez**. Proje ileride bu eşiği aşan bir şirkete ürün olarak sunulursa lisans satın alınmaz; PDF üretimi MIT lisanslı PDFsharp / MigraDoc'a taşınır. PDF üretimi tek bir arayüzün arkasında tutulduğu için bu değişiklik yalnızca o arayüzün uygulamasını etkiler.
- **QR üretimi:** QRCoder. QR yalnızca etiket kodunu içerir (BR-EQP-004).
- **Tarayıcıda QR okuma:** Tarayıcının yerleşik `BarcodeDetector` arayüzü kullanılır. Bu arayüzü desteklemeyen tarayıcılar (ör. iOS Safari) için aynı arayüzü sağlayan barcode-detector yedek kütüphanesi yüklenir. Kamera erişimi HTTPS gerektirir. Kamera kullanılamazsa etiket kodu elle girilir (US-WHS-002).

## Sonuçlar

**Olumlu:**
- PDF'ler tip güvenli C# koduyla, hızlı ve sunucuda ek bir tarayıcı çalıştırmadan üretilir.
- QR okuma, destekleyen tarayıcılarda yerleşik ve hızlıdır; diğerlerinde aynı kodla çalışır.

**Olumsuz / bedeli:**
- QuestPDF'in lisansı gelir eşiğine bağlıdır (yukarıda yönetildi).
- Belge tasarımı HTML/CSS yerine C# ile yapılır.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| HTML'den PDF (sunucuda başsız Chromium) | Şablonlar HTML/CSS ile kolay yazılır; ama sunucuda ağır bir tarayıcı çalıştırmak gerekir, üretim yavaş ve kaynak tüketimi yüksektir. |
| PDFsharp / MigraDoc | MIT lisanslı; ama daha düşük seviyeli ve yerleşim yazması daha zahmetli. |
| html5-qrcode | Yaygın kullanılmış ama bakımı zayıfladı; standart `BarcodeDetector` arayüzü daha geleceğe dönük. |
