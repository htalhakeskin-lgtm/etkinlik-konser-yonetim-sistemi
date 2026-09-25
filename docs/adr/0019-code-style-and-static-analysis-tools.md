# ADR-0019: Kod biçimi ve statik analiz araçları

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [standards/code-style.md](../standards/code-style.md), [standards/naming.md](../standards/naming.md), [ADR-0005](0005-dependency-license-policy.md), [ADR-0006](0006-backend-platform.md), [ADR-0008](0008-frontend-architecture.md)

## Bağlam

Kod yazılmaya başlamadan önce biçim ve yazım kurallarının hangi araçlarla uygulanacağı belirlenmelidir. Kurallar sonradan eklenirse var olan kodun tamamı yeniden biçimlenir ve büyük, okunmaz farklar oluşur.

Proje tek kişi tarafından geliştirilir. Bu yüzden kuralların insan dikkatine değil, editöre, derlemeye ve sürekli entegrasyona emanet edilmesi gerekir.

Türkçe bölge ayarlı geliştirme bilgisayarı, kültüre bağlı hataları (büyük / küçük harf dönüşümü, sayı biçimi) sıradan bir risk olmaktan çıkarıp günlük bir risk yapar.

## Karar

**C#:**
- Biçim: CSharpier. .NET'in biçim kuralı (IDE0055) kapatılır.
- Yazım kuralları: `.editorconfig` + .NET kod stili analizörleri, derlemede denetlenir (`EnforceCodeStyleInBuild`).
- Hata kalıpları: .NET analizörleri (`latest-recommended`), Meziantou.Analyzer (seçilmiş kurallar) ve BannedApiAnalyzers.
- Uyarılar sürekli entegrasyonda ve Release derlemesinde hata sayılır.

**TypeScript / React:**
- Biçim: Prettier + `prettier-plugin-tailwindcss`.
- Lint: ESLint 10 (tek dosyalı yapılandırma) + typescript-eslint (tip bilgili, `strictTypeChecked`) + React Hooks / React Compiler, TanStack Query, TanStack Router, erişilebilirlik, modül sınırı, içe aktarım sırası ve kural-kapatma-yorumu eklentileri.
- TypeScript 6.x'e sabitlenir. typescript-eslint TypeScript 7.x'i desteklediğinde geçiş planlanır.
- React Compiler kullanılır.

Tüm araçlar MIT ya da Apache 2.0 lisanslıdır ([ADR-0005](0005-dependency-license-policy.md)).

## Sonuçlar

**Olumlu:**
- Biçim, kod incelemesinde hiç konuşulmaz; editör kaydederken düzeltir, sürekli entegrasyon denetler.
- Türkçe kültürden kaynaklanan hatalar (örtük biçimlendirme dahil) derlemede yakalanır.
- Tip bilgili lint kuralları, TanStack Query ile yaygın olan "beklenmeyen promise" hatalarını yakalar.
- Ön yüz modül sınırları, sunucu tarafındaki mimari testlerin karşılığı olarak lint ile korunur.

**Olumsuz / bedeli:**
- ESLint'in tip bilgili kuralları yavaştır; büyük projede lint süresi saniyelerden onlarca saniyeye çıkabilir. Proje boyutunda bu kabul edilebilir.
- TypeScript 7'nin derleme hızından, typescript-eslint desteği gelene kadar yararlanılamaz.
- Meziantou ve .NET analizörlerinin bazı kuralları örtüşür; önem düzeyleri `.editorconfig`'de bilinçli olarak ayarlanmalıdır.
- React Compiler, Vite 8'de ayrı bir Babel adımı gerektirir; bu, üretim derlemesini biraz yavaşlatır.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Yalnızca `dotnet format` (CSharpier yok) | Satır kırmaz; uzun satırlar ve parametre listeleri kişiden kişiye farklı biçimde kalır. |
| StyleCop.Analyzers | Çok sayıda biçim kuralı CSharpier ile çakışır; ana sürümü uzun süredir ön sürümde. |
| SonarAnalyzer | Kasım 2024'ten beri açık kaynak olmayan, kaynak kodu erişilebilir bir lisansla (SSALv1) dağıtılıyor ([kaynak](https://community.sonarsource.com/t/a-new-sonar-license-for-sonarqube-analyzers/130731)); ihtiyaç duyulan kurallar Meziantou ve .NET analizörleriyle karşılanıyor. |
| Biome (lint + biçim tek araç) | React Compiler kuralları, TanStack eklentileri ve modül sınırı kuralı yok; tip bilgili kuralları typescript-eslint'ten dar; Tailwind sınıf sıralaması yarım. |
| Biome + ESLint birlikte | Aynı işi yapan iki araç ([07 §2](../07-tech-stack.md#2-seçim-ilkeleri), ilke 4). |
| Oxlint | Tip bilgili lint henüz olgun değil; eklenti ekosistemi dar. |
| TypeScript 7'ye hemen geçmek | typescript-eslint'in tip bilgili kuralları çalışmaz; lint'in en değerli kısmı kaybolur. |
| React Compiler olmadan elle `useMemo` / `useCallback` | Elle önbellekleme hataya açıktır ve kodu kalabalıklaştırır; Compiler kararlı sürümdedir. |
