# ADR-0004: Taahhüt ile olgunun ayrılması

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [05-module-map.md §2, §6](../05-module-map.md#6-tartışmalı-sahiplik-kararları), [06-erd-conceptual.md §7](../06-erd-conceptual.md#7-toplu-kökler-ve-eşzamanlılık), BR-MRP-002, BR-MRP-013, BR-MRP-017

## Bağlam

Müsaitlik iki şeyden hesaplanır:
- **Olgular:** Hangi ekipman şu an nerede ve ne durumda?
- **Taahhütler:** Hangi ekipman, hangi zaman aralığında, kime söz verildi?

Aynı ekipmanın aynı zaman aralığında iki kez söz verilmesi (çifte rezervasyon) sistemin en pahalı hatasıdır. Bunu kesin olarak önlemek için, taahhütler yazılırken müsaitlik aynı kilit altında yeniden hesaplanmalıdır.

## Karar

- **Tüm taahhütler Planning modülündedir.** S1'de bunlar rezervasyon ve transfer planıdır; S2'de crew çağrısı, S5'te araç ataması eklenir.
- **Tüm olgular Inventory modülündedir:** birim, adetli stok, kasa, okutma ve stok hareketi.
- Bir taahhüt yazılırken ilgili her (model, depo) çifti için kısa süreli bir **müsaitlik kilidi** alınır ve müsaitlik yeniden hesaplanır.
- Planning, müsaitlik hesabı için Inventory olaylarından bir **stok kopyası** tutar.
- Olgulardaki sonradan değişiklikler (ör. bir birimin bakıma girmesi) aşırı rezervasyon kontrolüyle yakalanır ve çakışma açılır (BR-MRP-017).

## Sonuçlar

**Olumlu:**
- Çifte rezervasyon, tek modül ve tek kilit içinde kesin olarak engellenir.
- Stok yönetimi ile planlama (MRP), ERP'deki gibi ayrı katmanlar olarak kalır.
- Aynı yaklaşım ileride ekip ve araç planlamasına da uygulanır.

**Olumsuz / bedeli:**
- Planning, stoğun bir kopyasını tutar ve bu kopya olaylarla birkaç yüz milisaniye gecikmeyle güncellenir.
- Transferin planı Planning'de, okutmaları Inventory'dedir. Transferin durumu iki modülün olaylarıyla ilerler.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Stok ve planlamayı tek modülde toplamak | Çifte rezervasyonu önlemek daha kolay olurdu. Ama bu modül S1'in yaklaşık üçte ikisini kapsar ve S2 ile S5'te büyümeye devam ederdi. |
| Rezervasyonu Inventory'de, transfer planını Planning'de tutmak | İki tür taahhüt farklı modüllerde olurdu; aralarındaki çifte ayırma tek kilitle önlenemezdi. |
