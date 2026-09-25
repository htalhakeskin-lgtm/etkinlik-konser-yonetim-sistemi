# ADR-0009: Arayüz bileşenleri: shadcn/ui ve Tailwind CSS

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **Genişletildi:** [ADR-0033](0033-design-system.md) — Base UI temelli bileşenler, tasarım değişkenleri, Inter, durum tonları, Storybook; React Hook Form için derleyici kuralları
- **İlgili:** [07-tech-stack.md §3.3](../07-tech-stack.md#33-ön-yüz-frontend), [ADR-0008](0008-frontend-architecture.md), [00-scope.md §7.7.1](../00-scope.md#771-adetli-kalem-yönetimi-ilkeleri)

## Bağlam

Bir ERP'nin ekranlarının çoğu tablo, form, filtre, tarih seçici ve iletişim kutusudur. Depo ekranları ise telefonda tek elle, eldivenle kullanılacak büyüklükte olmalıdır ([00 §7.7.1](../00-scope.md#771-adetli-kalem-yönetimi-ilkeleri)). Bileşen kütüphanesi, erişilebilir olmalı ve kurumsal görünüme kolayca uyarlanabilmelidir. Tasarım sistemi (renkler, tipografi, aralıklar) [standards/ui.md](../standards/ui.md)'de ve [ADR-0033](0033-design-system.md)'tedir.

## Karar

- **Bileşenler:** shadcn/ui. Bileşenler bir paket olarak değil, kaynak kodu olarak projeye kopyalanır ve erişilebilir, stilsiz temel bileşenler üzerine kuruludur. Böylece her bileşen projenin tasarım sistemine göre değiştirilebilir.
- **Stil:** Tailwind CSS 4. Renk, yazı ve aralık değerleri tasarım değişkenleri olarak tanımlanır.
- **Tablolar:** TanStack Table; sıralama, filtreleme, sayfalama ve uzun listeler için sanal kaydırma.
- **Formlar:** React Hook Form ve Zod. Zod şemaları OpenAPI'den üretildiği için sunucu ve ön yüz doğrulaması aynı tanıma dayanır.
- **Simgeler:** lucide.
- **Mobil depo ekranları:** Aynı bileşenlerin büyük dokunma alanlı çeşitleri kullanılır. Ayrı bir mobil kütüphane eklenmez.

## Sonuçlar

**Olumlu:**
- Bileşenlerin kodu projede olduğu için hiçbir tasarım ya da davranış kütüphane sürümüne kilitli değildir.
- Güncel React ekosisteminde en yaygın kullanılan yaklaşımlardan biridir; örnek ve kaynak boldur.
- Tailwind ile tasarım sistemi değişkenleri tek yerden yönetilir.

**Olumsuz / bedeli:**
- Hazır "her şey dahil" bir kütüphaneye göre bazı bileşenler (ör. gelişmiş veri tablosu, tarih-saat aralığı seçici) birkaç parça birleştirilerek kurulur.
- S2'deki kaynak takvimi (Gantt) gibi özel bileşenler ayrıca seçilecek.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Mantine | Tarih seçiciler, bildirimler, formlar dahil çok zengin, MIT lisanslı bir kütüphane. Başlangıç daha hızlı olurdu. Ama görünümü kendi tasarım diline daha bağlıdır ve özelleştirmesi shadcn/ui kadar serbest değildir. İkinci güçlü seçenek olarak kalır (T-02). |
| MUI | Material tasarım diline bağlı; gelişmiş veri tablosu özellikleri ücretli sürümde. |
| Ant Design | Kurumsal tablolar için güçlü; ama paket boyutu büyük ve tasarım dili değiştirmesi zor. |
