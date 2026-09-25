# ADR-0033: Tasarım sistemi

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [standards/ui.md](../standards/ui.md), [ADR-0009](0009-ui-components.md) (bu ADR onu genişletir), [ADR-0005](0005-dependency-license-policy.md), [ADR-0014](0014-documents-and-qr.md), [standards/code-style.md §5.4](../standards/code-style.md#54-react-kuralları)

## Bağlam

- [ADR-0009](0009-ui-components.md) shadcn/ui, Tailwind CSS 4, React Hook Form ve lucide'ı seçti; tasarım sistemini (renk, yazı, aralık) Faz 0 D bölümüne bıraktı.
- shadcn/ui Temmuz 2026'da varsayılan temel bileşen kütüphanesini Radix'ten **Base UI**'a çevirdi. Base UI, Radix'i geliştiren ekibin yeni kütüphanesidir; Aralık 2025'te kararlı sürüme ulaştı. Radix kullanımdan kaldırılmıyor ([kaynak](https://ui.shadcn.com/docs/changelog/2026-07-base-ui-default)).
- React Compiler benimsendi ([S-06](../standards/code-style.md#10-kararlar)). React Hook Form'un bazı kullanımları derleyicinin varsayımlarıyla çelişiyor; Mayıs 2026 itibarıyla belirli kalıplarla uyumlu ([kaynak](https://github.com/orgs/react-hook-form/discussions/12524)).
- Yazı tipleri SIL Open Font License ile dağıtılır; bu lisans bağımlılık politikasındaki listede yoktu.
- 11 durum makinesinin durumlarının tüm ekranlarda aynı anlamla ve renk körlüğüne uygun gösterilmesi gerekiyor.

## Karar

- **Temel bileşenler:** shadcn/ui'ın Base UI temelli sürümü.
- **Tasarım değişkenleri:** CSS değişkenleri, OKLCH renkler, Tailwind 4 teması; ham değerler → anlamsal değişkenler → bileşen değişkenleri. Tailwind'in hazır renk paleti kapatılır; bileşenler yalnızca anlamsal renk kullanabilir.
- **Durum tonları:** 7 ton (nötr, bilgi, sürüyor, başarılı, dikkat, sorun, bitti); her durum ton + simge + adla gösterilir; tüm durum makineleri tek tabloda eşlenir.
- **Yazı tipi:** Inter, uygulamayla birlikte sunulur ve PDF'lere gömülür. Yazı tipleri için SIL Open Font License 1.1, [ADR-0005](0005-dependency-license-policy.md)'teki izin verilen lisanslara eklenir.
- **Formlar:** React Hook Form kalır; `useWatch`, `useFormState` ve `Controller` zorunludur, `watch()` ve doğrudan `formState` okuma lint kuralıyla yasaklanır.
- **Katalog ve test:** Storybook; ortak bileşenlerin tüm durumları örnek olarak yazılır ve tarayıcıda test ile erişilebilirlik taramasından geçer. Renk çiftlerinin kontrastı birim testiyle ölçülür.
- **Tema:** Açık ve koyu tema S1'den; varsayılan cihaz ayarı, kullanıcı değiştirebilir; tema ilk çizimden önce uygulanır.
- **Marka rengi:** Mor (menekşe); durum tonlarından en uzak renk.

## Sonuçlar

**Olumlu:**
- Ekranlar arasında görünüm ve davranış tutarlıdır; yeni ekran mevcut parçalardan kurulur.
- Renk, yazı ve aralık kararları tek dosyadan değişir; ikinci bir tema bileşen kodunu değiştirmez.
- Ham renk kullanımı araçla engellendiği için tasarım sistemi zamanla aşınmaz.
- Durumlar renk körü kullanıcılar ve zorunlu renk kipi için de anlaşılır.
- Erişilebilirlik ve kontrast hataları, bileşen yazılırken yakalanır.
- Tasarım sistemi kataloğu portfolyoda gösterilebilir.

**Olumsuz / bedeli:**
- Storybook örneklerinin bakımı gerekir.
- Base UI, Radix'e göre daha yeni bir kütüphanedir; örnek ve kaynak sayısı daha azdır. shadcn/ui iki kütüphaneyi de desteklediği için gerekirse geri dönülebilir.
- React Hook Form'un derleyiciyle uyumu belirli kalıplara bağlıdır; kalıpların dışına çıkan kod sessizce hatalı çalışabilir. Bu yüzden lint kuralı ve form testleri zorunludur.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Radix temelli shadcn/ui | Olgun ve destekleniyor; ama yeni bileşenler ve topluluk tercihi Base UI yönünde. İki sürüm aynı arayüzle geldiği için seçim sonradan değiştirilebilir. |
| TanStack Form | TanStack ailesiyle uyumlu ve derleyici için baştan tasarlanmış; ama derleyiciyle ilgili açık hata bildirimi var ([kaynak](https://github.com/TanStack/form/discussions/968)) ve React Hook Form artık belirli kalıplarla uyumlu. Değiştirmenin kazancı belirsiz. |
| Sistem yazı tipi (`system-ui`) | İndirme gerektirmez; ama Windows, Android ve iOS'ta farklı yazı tipleri ve farklı rakam genişlikleri çıkar, PDF'lerle ekran farklı görünür. |
| IBM Plex Sans | Kurumsal arayüzler için güçlü bir alternatif; Inter daha dar olduğu için yoğun tablolara daha uygun. |
| Katalogsuz, yalnızca uygulama içinde bileşen geliştirmek | Bileşenlerin tüm durumları hiçbir yerde birlikte görünmez; tutarlılık ve erişilebilirlik sorunları geç fark edilir. |
| Yalnızca açık tema | Test ve tasarım yükü yarıya iner; ama gece ve karanlık kulis çalışmasında göz yorar. Koyu temayı sonradan eklemek, tüm bileşenleri yeniden gözden geçirmeyi gerektirir. |
| İndigo, nötr ya da turkuaz marka rengi | İndigo bilgi rozetindeki açık maviye, turkuaz başarı yeşiline yakın; nötr (siyah) marka kimliği bırakmaz. |
| Ladle | Daha hafif bir katalog aracı; ama Storybook'un Vitest ile tarayıcı testleri ve erişilebilirlik eklentisi yok. |
