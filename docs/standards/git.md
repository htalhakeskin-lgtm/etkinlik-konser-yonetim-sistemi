# Git ve İş Akışı Standardı

> **Durum:** v1.5 · **Son güncelleme:** 2026-09-26
> **Kararlar:** [Bölüm 13](#13-kararlar)

## 1. Bu belge ne işe yarar

Kodun bilgisayardan ana dala nasıl ulaştığını tanımlar:
- dal düzeni ve dal adları,
- commit mesajları,
- çekme istekleri (PR) ve birleştirme,
- görevlerin takibi,
- sürüm numaraları ve değişiklik günlüğü,
- commit öncesi kontroller,
- bağımlılık güncellemeleri ve gizli bilgi taraması,
- GitHub repo ayarları.

Bir işin ne zaman "bitti" sayılacağı [definition-of-done.md](definition-of-done.md)'de, hangi testlerin yazılacağı [testing.md](testing.md)'de tanımlıdır. Sürekli entegrasyon hattı [ci.md](ci.md)'de, yayın [09 §7](../09-environments-and-deployment.md#7-yayın-akışı)'dedir.

Proje tek geliştiricilidir. Kurallar buna göre seçildi: ekip içi onay adımları yok, ama her değişiklik ana dala girmeden önce otomatik kontrollerden geçer ve izlenebilir kalır.

## 2. GitHub'ın ücretsiz planı ve bu belgeye etkisi

Repo şu an gizlidir (private) ve GitHub'ın ücretsiz planındadır. Bu planda bazı koruma özellikleri yalnızca **herkese açık** (public) repolarda ücretsizdir:

| Özellik | Gizli repo (ücretsiz plan) | Açık repo (ücretsiz plan) |
|---|---|---|
| Dal koruma ve kural setleri (rulesets): ana dala doğrudan yazmayı, zorla yazmayı (force push) engelleme | Yok | Var |
| Birleştirmeden önce zorunlu CI kontrolü | Yok | Var |
| Gizli bilgi içeren push'un GitHub tarafında engellenmesi (push protection) | Yok | Var |
| Kod güvenlik taraması (CodeQL) | Yok | Var |
| GitHub Actions dakikası | Ayda 2.000 dakika | Standart çalıştırıcılarda sınırsız |
| Dependabot uyarıları ve güncelleme PR'ları | Var | Var |
| Issues, Projects, PR ve issue şablonları, birleştirme ayarları | Var | Var |

Kaynaklar: [dal koruma](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches), [kural setleri](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/available-rules-for-rulesets), [Actions ücretleri](https://github.com/resources/insights/2026-pricing-changes-for-github-actions), [push protection](https://docs.github.com/en/code-security/concepts/secret-security/push-protection), [CodeQL](https://docs.github.com/en/code-security/code-scanning/troubleshooting-code-scanning/cannot-enable-codeql-in-a-private-repository), [Dependabot](https://docs.github.com/en/code-security/concepts/supply-chain-security/dependabot-security-updates).

**Etkisi:**
- Gizli repoda bu belgedeki kurallar GitHub tarafından **zorlanamaz**. Kurallar yerel commit kancalarıyla (§8) ve CI raporlarıyla korunur; CI kırmızıyken birleştirmeyi engelleyen bir şey yoktur, bu bir öz disiplin kuralı olur.
- Gizli repoda CI dakikası sınırlıdır. Kabaca bir tahmin: PR başına bir çalıştırma yaklaşık 10–12 dakika (derleme, birim testleri, veritabanlı testler, ön yüz denetimleri). Günde 3–5 push ile ayda 700–1.300 dakika eder; gece çalışan uçtan uca testler de eklenince sınıra yaklaşılır. Yalnızca belge değişikliklerinde ağır adımları atlamak ve aynı dala yeni push gelince eski çalıştırmayı iptal etmek bu yüzden baştan uygulanır (D.3).
- Ücretli plan (GitHub Pro) ücretsiz bağımlılık ilkesine aykırıdır ve değerlendirilmedi.

**Karar ([R-01, R-02](#13-kararlar)):** Repo Faz 0 bitince, kod yazılmaya başlamadan önce herkese açılır; öncesinde §12'deki kontroller yapılır. Açık repoya lisans dosyası eklenmez: kod görülebilir, ama tüm hakları saklıdır. Böylece §11'deki korumalar ilk kod satırından itibaren GitHub tarafından zorlanır; gizli dönemde yalnızca belgeler yazıldığı için sınırlamaların etkisi küçüktür.

## 3. Dal düzeni

### 3.1 Ana dal

- Tek kalıcı dal `main`'dir. `main` her zaman derlenir, testleri geçer ve demo ortamına yayınlanabilir durumdadır.
- `develop`, `release/…` gibi uzun ömürlü dallar yoktur (Git Flow kullanılmaz). Sürümler `main` üzerindeki commit'lere konan etiketlerdir (§7).
- `main`'in geçmişi doğrusaldır: birleştirme commit'i yoktur, her PR tek bir commit olarak girer (§5.5).

Bu düzen, küçük ekipler ve tek geliştirici için en çok önerilen GitHub Flow'dur ([kaynak](https://env.dev/guides/git-branching-strategies), [kaynak](https://www.blog.brightcoding.dev/2026/09/04/best-git-and-github-practices-for-developers)). Git Flow, aynı anda birden fazla sürümü destekleyen ürünler içindir; burada yalnızca bir yayın sürümü vardır.

### 3.2 Çalışma dalları

Her iş kendi dalında yapılır. Dal adı:

```
{tür}/{kimlik}-{kısa-açıklama}
```

| Parça | Kural | Örnek |
|---|---|---|
| `tür` | Commit türlerinden biri (§4.2) | `feat`, `fix`, `docs`, `chore` |
| `kimlik` | Kullanıcı hikayesi numarası (küçük harf) ya da issue numarası | `us-evt-003`, `42` |
| `kısa-açıklama` | İngilizce, küçük harf, kelimeler `-` ile ayrılır, ASCII | `place-venue-hold` |

Örnekler: `feat/us-evt-003-place-venue-hold`, `fix/42-etag-after-retry`, `docs/d3-ci-pipeline`, `chore/update-dotnet-sdk`.

- Dal ömrü kısadır: hedef en fazla 2–3 iş günüdür. Uzayan iş, kendi başına çalışan daha küçük parçalara bölünür (§3.3). Araştırmalar kısa ömürlü dalların birleştirme çakışmalarını ve gizli entegrasyon riskini azalttığını gösteriyor ([kaynak](https://www.blog.brightcoding.dev/2026/09/04/best-git-and-github-practices-for-developers)).
- Dal birleştirilince silinir (GitHub bunu otomatik yapar, §11).

### 3.3 Bitmemiş işler

S1'de özellik bayrağı yoktur ([configuration §7](configuration.md#7-özellik-bayrakları)). Büyük bir iş şöyle parçalanır:

- **Dikey dilimler.** Her parça kendi başına çalışan bir davranıştır (ör. önce "opsiyon ekle", sonra "opsiyonu düşür"), katman değildir (ör. "önce tüm tablolar"). Her parça `main`'e girdiğinde uygulama çalışır durumda kalır.
- **Ekran bağlantısı son parçada.** Henüz kullanıcıya açılmaması gereken bir ekran menüye bağlanmaz. Menü bağlantısı, işi tamamlayan parçayla eklenir.
- **Veritabanı değişiklikleri adım adım.** Geriye uyumsuz değişiklikler genişlet / daralt (expand / contract) yöntemiyle birden fazla PR'a bölünür ([database §16](database.md#16-migrationlar)).

### 3.4 Dalı güncel tutma

- Dal `main`'in gerisinde kalırsa `main`'in üzerine yeniden yerleştirilir (`git rebase main`). Çalışma dalı tek kişiye ait olduğu için bu güvenlidir; dal, `git push --force-with-lease` ile güncellenir.
- `main`'e zorla yazılmaz (force push) ve `main`'in geçmişi değiştirilmez.

## 4. Commit mesajları

### 4.1 Biçim

Commit mesajları [Conventional Commits 1.0.0](https://www.conventionalcommits.org/en/v1.0.0/) biçimindedir ve İngilizcedir (K-01):

```
{tür}({kapsam}): {özet}

{gövde: neden yapıldığı, gerekirse}

{alt bilgi: BREAKING CHANGE, Refs}
```

| Parça | Kural |
|---|---|
| Özet | Emir kipinde, küçük harfle başlar, sonda nokta yok, başlık satırı en fazla 72 karakter. "add", "fix", "remove" gibi fiille başlar. |
| Kapsam | İsteğe bağlı; §4.3'teki listeden. |
| Gövde | Ne yapıldığını değil **neden** yapıldığını anlatır; ne yapıldığı farkta (diff) görünür. |
| Alt bilgi | Geriye uyumsuz değişiklikte `BREAKING CHANGE: …`; isteğe bağlı olarak `Refs: US-EVT-003, BR-EVT-003`. |

Bu biçim sürüm numarasının ve değişiklik günlüğünün otomatik üretilmesini sağlar (§7) ve `git log` çıktısını türe göre okunur kılar.

### 4.2 Türler

| Tür | Ne zaman | Sürüm etkisi (v1.0.0 sonrası) | Değişiklik günlüğünde |
|---|---|---|---|
| `feat` | Kullanıcının görebildiği yeni davranış | Küçük sürüm (1.**4**.0) | Özellikler |
| `fix` | Hata düzeltmesi | Yama (1.4.**1**) | Düzeltmeler |
| `perf` | Davranışı değiştirmeyen performans iyileştirmesi | Yama | Performans |
| `refactor` | Davranışı değiştirmeyen kod düzenlemesi | — | — |
| `test` | Yalnızca test ekleme ya da düzeltme | — | — |
| `docs` | Yalnızca belge | — | — |
| `build` | Derleme ayarı, bağımlılık | — | — |
| `ci` | Sürekli entegrasyon iş akışları | — | — |
| `chore` | Yukarıdakilere girmeyen bakım işleri | — | — |
| `revert` | Önceki bir commit'in geri alınması | Yama | Geri alınanlar |

`style` türü kullanılmaz; biçim araçlar tarafından otomatik uygulandığı için yalnızca biçim değiştiren bir commit olmaz.

### 4.3 Kapsamlar

Kapsam isteğe bağlıdır; yazılırsa aşağıdaki listeden biri olur. Liste commit mesajı denetiminde (§8) tanımlıdır.

| Kapsam | Ne için |
|---|---|
| `identity`, `audit`, `parties`, `catalog`, `venues`, `riders`, `booking`, `planning`, `inventory`, `procurement` | O modülün sunucu **ve** ön yüz kodu. Dikey dilim tek kapsamla yazılır. |
| `host` | Ana uygulama, modül kaydı, ara katmanlar |
| `building-blocks` | Ortak yapı taşları |
| `web` | Ön yüzün modüle ait olmayan kısmı: kabuk, ortak bileşenler, API istemcisi |
| `database` | Modüller arası veritabanı düzeni: roller, eklentiler |
| `tests` | Ortak test altyapısı, uçtan uca testler |
| `deps`, `deps-dev` | Bağımlılık güncellemeleri (Dependabot bunları kullanır) |
| `adr`, `standards` | `docs` türüyle: ADR'ler ve standartlar |
| `release` | Sürüm PR'ları (§7) |

### 4.4 Geriye uyumsuz değişiklik

Tür ya da kapsamdan sonra `!` konur ve alt bilgiye `BREAKING CHANGE:` yazılır. Bu projede geriye uyumsuz sayılanlar:
- API sözleşmesini bozan değişiklik (adres, alan ya da durum kodu kaldırma, anlam değiştirme; [api §12](api.md#12-sürümleme-ve-uyumluluk)),
- yayında elle adım gerektiren migration,
- ayar anahtarının adının ya da anlamının değişmesi.

### 4.5 Örnekler

```
feat(booking): place venue hold with queue rank

fix(inventory): reject second checkout of the same unit

Two concurrent scans could both pass the status check before either
committed. The unit row is now locked with FOR UPDATE.

Refs: BR-WHS-006

docs(standards): add git and workflow standard

build(deps): bump Npgsql from 10.0.1 to 10.0.2

feat(booking)!: require If-Match on event confirmation

BREAKING CHANGE: POST /api/v1/events/{id}/confirm now returns 428
without an If-Match header.
```

## 5. Çekme istekleri (PR)

### 5.1 Akış

1. Görev için dal açılır (§3.2).
2. İş küçük commit'lerle yapılır; commit öncesi kontroller her commit'te çalışır (§8).
3. Dal GitHub'a gönderilir ve PR açılır. İş bitmemişse **taslak** (draft) PR açılır; CI erken çalışır.
4. CI yeşil olur.
5. PR, GitHub'daki "Files changed" görünümünde satır satır kendi kendine incelenir (§5.4) ve bitti tanımının kontrol listesi doldurulur.
6. PR squash ile birleştirilir; dal otomatik silinir; bağlı issue otomatik kapanır.

**Her değişiklik PR ile girer** ([R-03](#13-kararlar)); belge değişiklikleri de. Kural, CI iş akışları kurulduğunda (Faz 1'in ilk adımı) yürürlüğe girer. O zamana kadar Faz 0 belgeleri doğrudan `main`'e gönderilir, çünkü PR'da çalışacak bir kontrol henüz yoktur.

### 5.2 Boyut

- Bir PR tek bir hikayeyi ya da tek bir tutarlı değişikliği içerir.
- Hedef, üretilmiş kod (migration, OpenAPI, API istemcisi) hariç **400 satırın altıdır**. Cisco'da 2.500 kod incelemesi üzerinde yapılan çalışma, bir seferde 200–400 satırdan fazlası incelendiğinde hata bulma oranının belirgin düştüğünü gösterdi ([kaynak](https://smartbear.com/learn/code-review/best-practices-for-peer-code-review/)). Tek geliştiricide inceleyen de yazan da aynı kişidir; küçük PR bu yüzden daha da önemlidir.
- Büyük bir hikaye birden fazla PR'a bölünür (§3.3).

### 5.3 Başlık ve açıklama

- **Başlık** commit mesajının başlık satırıyla aynı kurala uyar (§4.1), çünkü squash birleştirmede `main`'e giren commit'in başlığı olur. CI başlığı, yerel commit'lerle aynı commitlint yapılandırmasıyla denetler (`pr-title` iş akışı).
- **Açıklama** PR şablonundan gelir (`.github/pull_request_template.md`):

```markdown
## Ne ve neden
<!-- Bir iki cümle. -->

## Referanslar
Closes #…  ·  Hikaye: US-…  ·  Kurallar: BR-…  ·  Geçişler: T-…  ·  ADR: …

## Nasıl test edildi
<!-- Eklenen testler; elle denenen senaryo. -->

## Ekran görüntüleri
<!-- Arayüz değiştiyse: masaüstü ve (depo ekranlarında) telefon görünümü. -->

## Bitti tanımı
- [ ] …  (definition-of-done.md §3'teki liste)
```

### 5.4 Kendi kendine inceleme

Tek geliştiricide inceleme adımı atlanmaz, biçim değiştirir:
- İnceleme, kod yazıldıktan sonra ve CI yeşilken, GitHub'ın fark görünümünde yapılır. Editörde görülmeyen şeyler (unutulmuş hata ayıklama kodu, istenmeden eklenen dosya, eksik test) burada görünür.
- Bitti tanımı kontrol listesinin her maddesi işaretlenir ya da neden uygulanmadığı yazılır.
- İsteğe bağlı olarak otomatik bir kod inceleme aracından ikinci görüş alınabilir; bu, kontrol listesinin yerini tutmaz.

### 5.5 Birleştirme

- Yalnızca **squash** birleştirme açıktır. Dalın tüm commit'leri `main`'e tek commit olarak girer; commit başlığı PR başlığıdır, sonuna PR numarası eklenir (`feat(booking): place venue hold (#12)`).
- Squash commit'inin gövdesi dalın commit başlıklarının listesidir.
- `main`'in geçmişi doğrusal kalır; her commit tek bir mantıksal değişikliktir. Geri almak (`git revert`) ve hatayı bulmak (`git bisect`) kolaylaşır ([kaynak](https://env.dev/guides/git-branching-strategies)).

### 5.6 Dil

- PR başlığı İngilizcedir, çünkü commit mesajı olur (K-01).
- PR açıklaması ve issue'lar Türkçe yazılabilir; bağlandıkları hikayeler, kurallar ve belgeler Türkçedir.

## 6. Görev takibi

Görevler **GitHub Issues** ile takip edilir.

| Öğe | Kural |
|---|---|
| Hikaye | Her kullanıcı hikayesi bir issue'dur. Başlık hikaye numarası ve adıdır: `US-EVT-003 · Opsiyon ekleme`. Açıklamada hikayenin belgesine bağlantı vardır; kabul kriterleri kopyalanmaz, belge tek kaynaktır. |
| Hata | Hata şablonuyla açılır: adımlar, beklenen, gerçekleşen, ekran görüntüsü ve varsa hata yanıtındaki **iz numarası** (`traceId`, [api §8](api.md#8-hata-yanıtları)). İz numarası, hatanın loglarını ve izlerini doğrudan bulmayı sağlar. |
| Teknik iş | Hikayeye bağlı olmayan işler (araç kurulumu, iyileştirme) ayrı bir şablonla açılır. |
| Etiketler | `module:{modül}` (ör. `module:booking`); `type:story`, `type:bug`, `type:tech`, `type:docs`; hatalar için `severity:critical`, `severity:high`, `severity:medium`, `severity:low`. |
| Kilometre taşı (milestone) | Sürüm başına bir tane: `S1`, `S2`, … Bir sürümün ilerlemesi buradan görünür. |
| Pano | GitHub Projects'te tek pano: Yapılacak → Yapılıyor → İncelemede → Bitti. |
| Kapanış | PR açıklamasındaki `Closes #…` ile; PR birleştirilince issue kendiliğinden kapanır. Kapanan hikaye, bitti tanımını karşılamış sayılır. |

Hata önem seviyeleri:

| Seviye | Anlamı |
|---|---|
| `critical` | Veri kaybı ya da bozulması, güvenlik açığı, demo senaryosunun ilerleyememesi |
| `high` | Bir iş akışı çalışmıyor ve geçici bir yol yok |
| `medium` | İş akışı çalışıyor ama yanlış ya da zahmetli |
| `low` | Görünüm, metin, küçük rahatsızlık |

## 7. Sürüm numaraları ve etiketler

| Konu | Kural |
|---|---|
| Biçim | [Anlamsal sürüm numarası](https://semver.org/) (SemVer): `MAJOR.MINOR.PATCH`. Etiketler `v` önekiyle yazılır: `v1.4.0`. |
| S1 öncesi | `0.x` sürümleri. S1 geliştirilirken geriye uyumsuz değişiklik küçük sürümü artırır. |
| S1 | MVP demo senaryosu bitti tanımını karşıladığında **`v1.0.0`** yayınlanır ([definition-of-done §6](definition-of-done.md#6-sürüm-için)). |
| S1 sonrası | `feat` küçük sürümü, `fix` ve `perf` yamayı artırır (§4.2). Büyük sürüm yalnızca geriye uyumsuz değişiklikte (§4.4) artar. |
| Derlemedeki sürüm | [MinVer](https://github.com/adamralph/minver) (Apache 2.0) sürümü git etiketlerinden hesaplar: etiketli commit'te `1.4.0`, sonraki commit'lerde `1.4.1-alpha.0.3` gibi. CI, sürümün sonuna commit kimliğini ekler (`+a1b2c3d`). Sürüm numarası hiçbir dosyada elle tutulmaz. |
| Uygulamadaki sürüm | Sunucu `X-App-Version` başlığında bu sürümü döner ([api §12](api.md#12-sürümleme-ve-uyumluluk)); ön yüz aynı derlemede aynı sürümü alır. Sürümler farklıysa kullanıcı "Yeni sürüm hazır" uyarısını görür. |
| Sürüm yayınlama | [release-please](https://github.com/googleapis/release-please-action) (Apache 2.0) `main`'e giren commit'lerden bir **sürüm PR'ı** hazırlar ve günceller: sürüm numarası ve `CHANGELOG.md`. Sürüm PR'ı birleştirildiğinde etiket ve GitHub sürüm sayfası (release) oluşur. Sürüm oluşunca demo ortamına otomatik yayınlanır ([09 §7](../09-environments-and-deployment.md#7-yayın-akışı)). |
| Değişiklik günlüğü | `CHANGELOG.md` elle yazılmaz; commit başlıklarından üretilir. Bu yüzden commit başlıkları kullanıcıya anlamlı yazılır. |

**Neden MinVer:** Yalnızca etiketlere bakar; dal adından ya da ayar dosyasından sürüm çıkarmaz. GitVersion daha karmaşıktır ve dal modeline bağlıdır. Nerdbank.GitVersioning sürümü bir dosyada tutar ve yama numarasını commit sayısından üretir; her commit yayınlanmadığında numaralarda boşluk olur ([kaynak](https://github.com/adamralph/minver)).

**Neden release-please:** Sürüm numarasına karar vermeyi commit türlerine bırakır ve yayını bilinçli bir adım (sürüm PR'ını birleştirmek) olarak tutar. Her birleştirmede otomatik yayın yapan araçlar (semantic-release) bu projede gerekmiyor.

## 8. Commit öncesi kontroller

Kancalar [Lefthook](https://github.com/evilmartians/lefthook) (MIT) ile yönetilir. Kök klasördeki `package.json`'a geliştirme bağımlılığı olarak eklenir; kökte `pnpm install` çalıştırmak kancaları kurar.

**İlke:** Kancalar hızlı bir kolaylıktır, yetki kaynağı değildir. Aynı kontroller CI'da tekrar çalışır; kanca atlanırsa CI yakalar.

| Kanca | Ne çalışır | Hedef süre |
|---|---|---|
| `pre-commit` | Yalnızca commit'e eklenen dosyalarda: CSharpier (C#, `.csproj`, `.props`), ESLint ve ardından Prettier (düzeltebildiklerini düzeltir ve commit'e ekler); gitleaks ile gizli bilgi taraması | Birkaç saniye |
| `commit-msg` | [commitlint](https://commitlint.js.org/) (MIT): Conventional Commits biçimi ve §4.3'teki kapsam listesi | Anlık |
| `pre-push` | `main`'e doğrudan push'u reddeder ([R-03](#13-kararlar)). Repo açılınca aynı kuralı GitHub'daki kural seti de zorlar; kanca, hatanın sunucuya gitmeden yerelde görülmesini sağlar. | Anlık |

Derleme ve testler kancalarda çalışmaz; saniyeler yerine dakikalar süren kontroller kanca atlama alışkanlığı yaratır. Bunlar CI'dadır.

Yapılandırma kökteki `lefthook.yml` dosyasındadır. İki ayrıntı:

- ESLint ile Prettier aynı dosyayı yeniden yazdığı için paralel çalışmaz; önce ESLint, en son Prettier çalışır. Diğer kontroller paraleldir.
- `pre-push` kontrolü bir Lefthook komutu değil, betiktir (`tools/git/pre-push/protect-main.sh`). Lefthook, yeni dosya getirmeyen push'larda komutları atlar; betikleri atlamaz. Betik, push'un hedef dallarını okur; böylece `git push origin ozellik:main` gibi başka bir daldan yapılan push da yakalanır.

**Neden Lefthook:** Tek bir çalıştırılabilir dosyadır, kontrolleri paralel çalıştırır ve alt klasörlere göre ayrı komut tanımlamayı destekler. .NET ve ön yüzün aynı repoda olduğu bu yapıya Husky'den daha uygundur. Husky yalnızca kabuk betiği çalıştırır; hangi dosyada hangi aracın çalışacağını betiğe elle yazmak gerekir ([kaynak](https://www.andymadge.com/2026/03/10/git-hooks-comparison/), [kaynak](https://www.pkgpulse.com/guides/husky-vs-lefthook-vs-lint-staged-git-hooks-nodejs-2026)).

**Kök çalışma alanı:** Ön yüz (`src/web`) ve uçtan uca testler (`tests/e2e`) kökteki bir pnpm çalışma alanında (`pnpm-workspace.yaml`) toplanır. Kök `package.json` yalnızca Lefthook, commitlint ve Prettier'ı taşır. Böylece tek `pnpm install` tüm JavaScript bağımlılıklarını ve kancaları kurar.

## 9. Bağımlılık güncellemeleri

Güncellemeler [Dependabot](https://docs.github.com/en/code-security/reference/supply-chain-security/dependabot-options-reference) ile PR olarak gelir (`.github/dependabot.yml`).

| Konu | Kural |
|---|---|
| Kapsam | NuGet (`Directory.Packages.props`), npm (pnpm çalışma alanı), GitHub Actions, .NET SDK (`global.json`), Compose dosyalarındaki konteyner imajları (Caddy, Alloy, PostgreSQL temel imajı). Uygulama imajının temel imajı her derlemede güncel yamasıyla çekilir ([09 §5](../09-environments-and-deployment.md#5-konteyner-imajı)). |
| Sıklık | Haftalık, pazartesi. Güvenlik güncellemeleri beklemeden gelir. |
| Gruplama | Her ekosistemde küçük sürüm ve yama güncellemeleri tek PR'da; büyük sürümler ayrı ayrı |
| Bekleme süresi | Yeni bir sürüm yayımlandıktan **3 gün** sonra önerilir, büyük sürümler 7 gün sonra (`cooldown`). Güvenlik güncellemeleri beklemez. |
| Commit başlığı | `build(deps): …` ve `build(deps-dev): …` (§4.3) |
| Birleştirme | Elle, CI yeşilken. Büyük sürümde sürüm notları okunur; kırıcı değişiklik varsa kod aynı PR'da uyarlanır. |

**Bekleme süresi neden var:** Son yıllardaki paket saldırılarının çoğu, ele geçirilen bir hesapla kötü amaçlı bir sürüm yayımlayıp otomatik kurulumların onu hemen çekmesine dayanıyor. Eylül 2025'teki `chalk` / `debug` saldırısı yaklaşık 2,5 saatte, Shai-Hulud saldırısı yaklaşık 12 saatte fark edildi; birkaç günlük bekleme ikisini de engellerdi ([kaynak](https://socket.dev/blog/pnpm-11-adds-new-supply-chain-protection-defaults), [kaynak](https://pnpm.io/supply-chain-security)). Aynı koruma pnpm'de de açılır: `pnpm-workspace.yaml`'da `minimumReleaseAge: 4320` (3 gün, dakika cinsinden). Acil bir güvenlik düzeltmesi bu sınırdan tek tek muaf tutulabilir (`minimumReleaseAgeExclude`).

**Kurulum betikleri:** Bağımlılıkların kurulum sırasında çalıştırdığı betikler yalnızca `pnpm-workspace.yaml`'daki `allowBuilds` listesinde açıkça izin verilirse çalışır (pnpm 10'dan beri varsayılan). Ele geçirilmiş bir paketin en yaygın saldırı yolu kurulum betiğidir. Her izin gerekçesiyle yazılır; betiği gerekmeyen pakete izin verilmez (ör. Lefthook'un betiği yalnızca kancaları kurar, bunu kökteki `prepare` betiği zaten yapar).

**GitHub Actions sabitleme:** İş akışlarındaki dış eylemler (actions) sürüm etiketiyle değil, tam commit kimliğiyle (SHA) sabitlenir; yanına okunur sürüm yorum olarak yazılır (`uses: actions/checkout@<sha> # v5.0.0`). Mart 2025'te `tj-actions/changed-files` eylemi ele geçirildi ve saldırgan tüm sürüm etiketlerini kötü amaçlı koda yönlendirdi; commit kimliğiyle sabitlenmiş iş akışları etkilenmedi ([kaynak](https://www.cisa.gov/news-events/alerts/2025/03/18/supply-chain-compromise-third-party-tj-actionschanged-files-cve-2025-30066-and-reviewdogaction), [kaynak](https://docs.github.com/en/actions/reference/security/secure-use)). Dependabot sabitlenmiş kimlikleri de günceller.

**Neden Renovate değil:** Renovate daha fazla ekosistemi ve daha esnek gruplamayı destekliyor ([kaynak](https://appsecsanta.com/sca-tools/dependabot-vs-renovate)). Ama barındırılan sürümü, gizli repoya erişim izni verilen üçüncü taraf bir uygulamadır; kendi çalıştırıcımızda çalıştırmak ise CI dakikası harcar. Bu projenin ihtiyaçlarını (merkezi NuGet sürümleri, tek pnpm çalışma alanı, gruplama, bekleme süresi) Dependabot karşılıyor ve GitHub'ın içinde çalışıyor.

## 10. Gizli bilgi taraması

Parola, anahtar ya da bağlantı dizesinin repoya girmesi [gitleaks](https://github.com/gitleaks/gitleaks) (MIT) ile engellenir.

| Nerede | Ne |
|---|---|
| Commit öncesi | Commit'e eklenen değişiklikler taranır (§8). gitleaks geliştirici bilgisayarına bir kez kurulur (`winget install Gitleaks.Gitleaks`). |
| CI, her PR'da | PR'ın commit'leri taranır. |
| CI, haftalık | Tüm geçmiş taranır; yeni eklenen kurallar eski commit'lerde de denenmiş olur. |
| GitHub | Repo açıksa push protection ayrıca açılır (§2). |

- gitleaks CLI doğrudan çalıştırılır. Resmi GitHub eylemi (`gitleaks-action`) ayrı bir lisansla dağıtılıyor: kişisel hesaplarda ücretsiz, kuruluş hesaplarında lisans anahtarı istiyor ([kaynak](https://github.com/gitleaks/gitleaks-action)). Tarayıcının kendisi MIT'dir ve bu koşula bağlı değildir.
- Test verisindeki bilerek sahte değerler `.gitleaks.toml`'da tek tek muaf tutulur; klasör bazında genel muafiyet verilmez.
- **Gizli bilgi sızarsa** önce değeri geçersiz kılıp yenisini üret (rotation), sonra repodan kaldır. Commit geçmişe girdiği anda değer ele geçmiş sayılır; geçmişi yeniden yazmak tek başına yeterli değildir. Yenileme yordamı [10 §6](../10-operations.md#6-gizli-bilgilerin-yenilenmesi)'dadır.

## 11. GitHub repo ayarları

Repo açılışında bir kez yapılır ve bu listeyle doğrulanır:

| Ayar | Değer |
|---|---|
| Birleştirme yöntemleri | Yalnızca "Allow squash merging"; birleştirme commit'i ve rebase kapalı |
| Squash commit mesajı | "Pull request title and commit details" |
| Dalı otomatik silme | Açık ("Automatically delete head branches") |
| Dependabot | Bağımlılık grafiği, uyarılar ve güvenlik güncellemeleri açık |
| Actions izinleri | `GITHUB_TOKEN` varsayılanı salt okunur; her iş akışı ihtiyacı olan izni açıkça ister. Actions'ın PR açmasına izin verilir ("Allow GitHub Actions to create and approve pull requests"); release-please sürüm PR'ını bununla açar (§7). |
| Şablonlar | `.github/pull_request_template.md`; `.github/ISSUE_TEMPLATE/` altında hikaye, hata ve teknik iş şablonları |

Repo açıksa ek olarak:

| Ayar | Değer |
|---|---|
| `main` için kural seti | Repo açılınca: doğrusal geçmiş zorunlu, zorla yazma ve dal silme engelli. CI kurulunca (Faz 1'in ilk adımı, [R-03](#13-kararlar)): PR zorunlu (onay sayısı 0, tek geliştirici) ve zorunlu kontroller `ci-result`, `pr-title` ([ci §11](ci.md#11-zorunlu-kontroller)). |
| Gizli bilgi taraması | Açık, push protection açık |
| Kod taraması | CodeQL varsayılan kurulum (C#, JavaScript / TypeScript); taranacak kod geldiğinde (Faz 1) |
| Güvenlik açığı bildirimi | Özel bildirim (private vulnerability reporting) açık; `SECURITY.md` dosyası |
| Dış katkılar | Fork'lardan gelen PR'larda iş akışları onaysız çalışmaz |
| Repo tanıtımı | Açıklama ve konu etiketleri (topics) doldurulur; README'deki İngilizce özetle uyumlu |

## 12. Repo herkese açılmadan önce

Repo Faz 0 bitince açılır ([R-01](#13-kararlar)). Açılmadan önce şu kontroller yapılır:

1. **Gizli bilgi:** gitleaks ile tüm geçmiş taranır; bulgu olmamalıdır.
2. **Özel belge ve kişisel veri:** Repoda proje dışı bir kuruma ait belge, kişi adı, IP adresi ya da iç bilgi bulunmadığı kontrol edilir. 2026-09-25 itibarıyla geçmişe yalnızca Markdown belgeleri, `.gitattributes`, `.gitignore` ve iki belge betiği girmiştir.
3. **Commit e-postası:** Mevcut commit'lerin hepsinde kişisel e-posta adresi yazılıdır ve repo açılınca herkes görebilir. Adres gizlenir:
   - geçmiş, GitHub'ın gizli adresine (`<kimlik>+<kullanıcı>@users.noreply.github.com`) [git filter-repo](https://github.com/newren/git-filter-repo) ile çevrilir ve `main` bir kereliğine zorla güncellenir (commit kimlikleri değişir; tek geliştirici olduğu için sorun olmaz, §3.4'teki yasağın tek istisnasıdır);
   - repo için `git config user.email` gizli adrese ayarlanır;
   - GitHub'da "Keep my email address private" ve "Block command line pushes that expose my email" açılır ([kaynak](https://github.blog/news-insights/product-news/private-emails-now-more-private/)).
4. **README:** İngilizce özet güncel teknoloji seçimleriyle uyumlu olur; lisans durumu yazılır: "Copyright © 2026. All rights reserved." ([R-02](#13-kararlar)). `LICENSE` dosyası eklenmez.
5. **Ayarlar:** §11'deki "repo açıksa" ayarları yapılır.

## 13. Kararlar

| No | Konu | Karar | Gerekçe |
|---|---|---|---|
| R-01 | Repo görünürlüğü | Faz 0 bitince, kod yazılmadan önce herkese açılır; öncesinde §12 uygulanır (commit e-postası gizli adrese çevrilir) | Açık repoda dal koruması, zorunlu CI, push protection ve CodeQL ücretsizdir; CI dakikası sınırsızdır (§2). Korumalar ilk kod satırından itibaren geçerli olur. Portfolyo hedefiyle uyumludur. |
| R-02 | Açık repodaki lisans | Lisans dosyası yok; tüm hakları saklı | Kod görülebilir ama izinsiz kullanılamaz; ileride ticari ürün seçeneği açık kalır. Lisans sonradan eklenebilir, ama açık kaynak lisansıyla verilmiş bir sürüm geri alınamaz. |
| R-03 | Değişiklikler ana dala nasıl girer | Her değişiklik PR ile, belgeler dahil; CI kurulduğunda yürürlüğe girer | CI, değişiklik `main`'e girmeden önce çalışır; kendi kendine inceleme için bir an oluşur; portfolyoda süreç görünür. |
| R-04 | Dal modeli | GitHub Flow: tek kalıcı `main` + kısa ömürlü dallar | Tek yayın sürümü olan bir uygulama için en sade model; Git Flow'un sürüm dalları gereksiz. |
| R-05 | Birleştirme yöntemi | Yalnızca squash; doğrusal geçmiş | Her PR tek commit olur; geri alma ve hata arama kolaylaşır; sürüm ve değişiklik günlüğü PR başlıklarından üretilir. |
| R-06 | Commit biçimi | Conventional Commits 1.0.0, İngilizce, sabit kapsam listesi | Sürüm numarası ve değişiklik günlüğü otomatik üretilir; geçmiş türe ve modüle göre okunur. |
| R-07 | Görev takibi | GitHub Issues: hikaye başına bir issue, sürüm başına kilometre taşı, tek pano | Ücretsiz ve repoyla bütünleşik; PR'dan issue'ya, issue'dan belgeye izlenebilirlik. |
| R-08 | Sürüm numarası ve yayın | SemVer; sürüm etiketlerden (MinVer); sürüm PR'ı ve değişiklik günlüğü release-please ile; S1 = `v1.0.0` | Sürüm numarası elle tutulmaz; yayın bilinçli bir adımdır. |
| R-09 | Commit kancası aracı | Lefthook; kök pnpm çalışma alanından kurulur | Çok dilli repoda paralel ve dosya türüne göre çalışır. |
| R-10 | Bağımlılık güncellemeleri | Dependabot; haftalık; gruplu; 3 / 7 gün bekleme; pnpm `minimumReleaseAge`; Actions commit kimliğiyle sabit | Tedarik zinciri saldırılarına karşı bekleme; üçüncü taraf uygulamaya repo erişimi gerekmez. |
| R-11 | Gizli bilgi taraması | gitleaks CLI: commit öncesi, her PR'da ve haftalık tüm geçmişte | Ücretsiz ve repo görünürlüğünden bağımsız çalışır. |
| R-12 | PR ve issue dili | PR başlığı İngilizce; açıklama ve issue'lar Türkçe olabilir | Başlık commit olur (K-01); içerik Türkçe belgelere bağlanır. |

## 14. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-09-25 | v0.1 | İlk taslak |
| 2026-09-25 | v1.0 | R-01 (Faz 0 sonunda herkese açık), R-02 (lisans yok, tüm hakları saklı), R-03 (her değişiklik PR ile) kararlaştırıldı; ADR-0028 kabul edildi. |
| 2026-09-25 | v1.1 | CI, yayın ve gizli bilgi yenileme belgelerine bağlandı (D.3). |
| 2026-09-25 | v1.2 | Kural setinin aşamaları (repo açılınca / CI kurulunca), Actions'ın PR açma izni, CodeQL zamanı ve repo tanıtımı netleşti. |
| 2026-09-26 | v1.3 | Bağımlılık kurulum betiklerine yalnızca açık izinle izin verilmesi (pnpm `allowBuilds`). |
| 2026-09-26 | v1.4 | Commit kancalarının kesin hâli: ESLint ve Prettier sırayla çalışır; `main` koruması betikle yapılır (Faz 1.0). |
| 2026-09-26 | v1.5 | PR başlığı denetimi commitlint'le yapılır; ayrı bir eylem kullanılmaz (Faz 1.0). |
