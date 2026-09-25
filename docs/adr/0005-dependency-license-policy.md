# ADR-0005: Bağımlılık lisans politikası

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [07-tech-stack.md §5](../07-tech-stack.md#5-kullanılmayan-kütüphaneler)

## Bağlam

2025–2026'da .NET dünyasında yaygın kullanılan birçok kütüphane (MediatR, AutoMapper, MassTransit, FluentAssertions) ücretli lisansa geçti. Bir kütüphanenin lisansı proje ortasında değişirse, ya ücret ödenir ya da kütüphane pahalı bir şekilde sökülür. Proje portfolyo olarak herkese açılabilir ve ileride ticari bir ürüne dönüşebilir; bu yüzden lisans riski baştan yönetilmelidir.

## Karar

- **İzin verilen lisanslar:** MIT, Apache 2.0, BSD, ISC, PostgreSQL lisansı. MPL 2.0 yalnızca kütüphane değiştirilmeden kullanılıyorsa kabul edilir.
- **İzin verilmeyen lisanslar:** GPL, AGPL ve kaynak kodu açma yükümlülüğü getiren diğer lisanslar; kullanım sınırı ya da gelir eşiği olan ticari lisanslar.
- **İstisnalar:** Gelir eşikli topluluk lisansları (ör. QuestPDF), yalnızca ayrı bir ADR ile ve eşik aşıldığında ne yapılacağı yazılarak kabul edilir ([ADR-0014](0014-documents-and-qr.md)).
- **Lisansı değişen bağımlılık:** Son ücretsiz sürümde kalınır, alternatif değerlendirilir ve yeni bir ADR yazılır.
- **Denetim:** Bağımlılıkların lisansları sürekli entegrasyon hattında otomatik kontrol edilir (Faz 0 D bölümü).

## Sonuçlar

**Olumlu:**
- Proje, lisans değişiklikleri yüzünden kilitlenmez.
- Portfolyo olarak açıldığında ya da ürüne dönüştüğünde lisans sürprizi yaşanmaz.

**Olumsuz / bedeli:**
- Bazı işler hazır kütüphane yerine elle yazılır (ör. komut işleyicileri, nesne eşleme).

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Ücretli kütüphanelerin son ücretsiz sürümünü kullanmak | Güvenlik yamaları ve yeni .NET sürümlerine uyum zamanla kesilir. |
| Topluluk lisanslarını genel olarak kabul etmek | Gelir eşikleri ve koşullar değişebilir; her birinin bilinçli olarak onaylanması gerekir. |
