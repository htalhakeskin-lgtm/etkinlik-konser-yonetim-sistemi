# ADR-0028: Geliştirme iş akışı ve repo araçları

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [standards/git.md](../standards/git.md), [standards/definition-of-done.md](../standards/definition-of-done.md), [ADR-0005](0005-dependency-license-policy.md), [ADR-0019](0019-code-style-and-static-analysis-tools.md)

## Bağlam

- Proje tek geliştiricilidir ve uzun soluklu bir portfolyo çalışmasıdır. Değişikliklerin izlenebilir olması, geçmişin okunur kalması ve kalite kontrollerinin atlanamaması gerekir.
- Repo şu an GitHub'ın ücretsiz planında ve gizlidir. Bu planda dal koruma, zorunlu CI kontrolü, push protection ve CodeQL yalnızca herkese açık repolarda ücretsizdir; gizli repoda Actions dakikası ayda 2.000 ile sınırlıdır ([git §2](../standards/git.md#2-githubın-ücretsiz-planı-ve-bu-belgeye-etkisi)).
- Uygulamanın tek bir yayın sürümü vardır; eski sürümlere yama desteği gerekmez.
- Ön yüz sunucunun sürümünü `X-App-Version` başlığıyla karşılaştırır; sürüm numarasının her derlemede güvenilir biçimde üretilmesi gerekir.
- 2025'te hem npm paketlerine (`chalk` / `debug`, Shai-Hulud) hem GitHub Actions eylemlerine (`tj-actions/changed-files`) yönelik tedarik zinciri saldırıları yaşandı.

## Karar

- **Dal modeli:** GitHub Flow. Tek kalıcı dal `main`, iş başına kısa ömürlü dal, yalnızca squash birleştirme, doğrusal geçmiş.
- **Commit biçimi:** Conventional Commits 1.0.0, İngilizce, sabit kapsam listesi; commitlint ile denetlenir.
- **Görev takibi:** GitHub Issues; hikaye başına bir issue, sürüm başına kilometre taşı, tek pano.
- **Sürüm:** SemVer. Derlemedeki sürüm git etiketlerinden MinVer ile hesaplanır; sürüm PR'ı, etiket ve `CHANGELOG.md` release-please ile üretilir. S1 = `v1.0.0`.
- **Commit kancaları:** Lefthook; kök pnpm çalışma alanından kurulur. Kancalarda yalnızca hızlı kontroller (biçim, lint, gizli bilgi, commit mesajı) çalışır; derleme ve testler CI'dadır.
- **Bağımlılık güncellemeleri:** Dependabot; haftalık, gruplu, yeni sürümler için 3 gün (büyük sürümler 7 gün) bekleme. pnpm'de `minimumReleaseAge` 3 gün. GitHub Actions eylemleri tam commit kimliğiyle sabitlenir.
- **Gizli bilgi taraması:** gitleaks CLI; commit öncesi, her PR'da ve haftalık tüm geçmişte.
- **Repo görünürlüğü:** Faz 0 bitince, kod yazılmadan önce herkese açılır; öncesinde gizli bilgi, özel belge ve commit e-postası kontrolleri yapılır ([git §12](../standards/git.md#12-repo-herkese-açılmadan-önce)).
- **Lisans:** Lisans dosyası yok; tüm hakları saklı.
- **PR kullanımı:** Her değişiklik, belgeler dahil, PR ile girer; kural CI kurulduğunda yürürlüğe girer. `main`'e doğrudan push'u yerel kanca, repo açıldıktan sonra GitHub kural seti de engeller.

## Sonuçlar

**Olumlu:**
- `main`'in her commit'i bir PR'a karşılık gelir; geri almak ve hatanın hangi değişiklikle geldiğini bulmak kolaydır.
- Sürüm numarası ve değişiklik günlüğü elle tutulmaz, commit geçmişinden üretilir.
- Kalite kontrollerinin aynısı yerelde (hızlı olanlar) ve CI'da (tümü) çalışır; kanca atlanırsa CI yakalar.
- Yeni yayımlanmış kötü amaçlı paket sürümleri, fark edilip kaldırılmadan önce projeye girmez.
- Tüm araçlar ücretsiz ve serbest lisanslıdır (MIT, Apache 2.0) ya da GitHub'ın kendi özelliğidir.

**Olumsuz / bedeli:**
- Commit ve PR başlıklarında biçim disiplini gerekir; biçime uymayan commit kanca tarafından reddedilir.
- Squash birleştirme, dalın ara commit'lerini `main`'in geçmişinde tek commit'e indirir; ayrıntı PR sayfasında kalır.
- Bekleme süresi nedeniyle yeni sürümler birkaç gün geç gelir; acil güvenlik düzeltmeleri bu kuraldan muaftır.
- Repo açılana kadar kurallar GitHub tarafından zorlanamaz; bu dönemde yalnızca belgeler yazıldığı için etkisi küçüktür.
- Repo açılmadan önce commit geçmişi bir kez yeniden yazılır (e-posta gizleme); commit kimlikleri değişir.
- Lisanssız açık repo, kodun görülmesine izin verir ama başkalarının katkı yapmasını ve kodu kullanmasını hukuken belirsiz bırakır; tek geliştiricili bir portfolyo için bu kabul edilebilir.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Git Flow (`develop`, `release` dalları) | Aynı anda birden fazla sürümü destekleyen ürünler içindir; tek yayın sürümü olan bir uygulamada yalnızca ek birleştirme işi getirir. |
| Birleştirme commit'i ya da rebase ile birleştirme | Ara commit'ler `main`'e girer; geçmişte yarım kalmış, derlenmeyen durumlar oluşur ve geçmiş PR başlıklarından okunamaz. |
| GitVersion, Nerdbank.GitVersioning | GitVersion dal modeline bağlı ve karmaşık; Nerdbank sürümü dosyada tutuyor ve yama numarasını commit sayısından üretiyor. MinVer yalnızca etiketlere bakar. |
| semantic-release | Her birleştirmede otomatik yayın yapar; yayının bilinçli bir adım olması tercih edildi. |
| Husky + lint-staged | Kabuk betiği tabanlı; .NET ve ön yüzü aynı repoda çalıştırmak için dosya türüne göre ayrımı elle yazmak gerekir. Lefthook bunu yapılandırmayla ve paralel yapar. |
| Renovate | Daha esnek, ama barındırılan sürümü gizli repoya erişen üçüncü taraf bir uygulama; kendi çalıştırıcımızda çalıştırmak CI dakikası harcar. Dependabot ihtiyacı karşılıyor. |
| `gitleaks-action` | Eylem ayrı bir lisansla dağıtılıyor; kuruluş hesaplarında lisans anahtarı istiyor. Tarayıcının kendisi (MIT) doğrudan çalıştırılır. |
| GitHub Pro (gizli repoda dal koruması için) | Ücretli; ücretsiz bağımlılık ilkesine aykırı. |
| Repoyu S1 bitince ya da hiç açmamak | Kod yazılırken dal koruması, zorunlu CI, push protection ve CodeQL olmaz; CI dakikası sınırlı kalır. Açıldığında geçmiş yine görünür olacağı için gizli tutmanın kazancı küçüktür. |
| MIT ya da AGPL-3.0 lisansı | Verilen açık kaynak lisansı geri alınamaz; ticari ürün seçeneğini daraltır. Lisanssız başlamak her iki yolu da açık tutar. |
| Belgeleri PR'sız, kodu PR ile göndermek | Belge denetimleri (bağlantılar, kural referansları) değişiklik `main`'e girdikten sonra çalışır; iki farklı akış kural istisnası yaratır. |
