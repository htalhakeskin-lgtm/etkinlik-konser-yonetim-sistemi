# Kod Stili ve Statik Analiz

> **Durum:** v1.5 · **Son güncelleme:** 2026-09-25
> **Kararlar:** [Bölüm 10](#10-kararlar)

## 1. Bu belge ne işe yarar

Kodun nasıl biçimlendirileceğini, hangi yazım kurallarına uyacağını ve bunları hangi araçların denetleyeceğini tanımlar. İlke şudur: **biçim tartışılmaz, araç karar verir.** Biçimlendirici kodu her kaydedişte aynı hale getirir; analizörler hataya yol açan kalıpları derlemede yakalar. Kod incelemesi yalnızca tasarıma ve iş mantığına odaklanır.

Adlandırma kuralları [naming.md](naming.md)'dedir. Araç seçimlerinin gerekçesi ve alternatifleri [ADR-0019](../adr/0019-code-style-and-static-analysis-tools.md)'dadır.

## 2. Tüm dosyalar için ortak kurallar

Ortak kurallar kök dizindeki `.editorconfig` dosyasındadır; editörler ve biçimlendiriciler bu dosyayı okur.

| Kural | Değer | Neden |
|---|---|---|
| Karakter kodlaması | UTF-8, BOM'suz | Windows araçları (ör. PowerShell 5.1) varsayılan olarak BOM ekler; BOM bazı araçlarda dosyanın ilk karakterini bozar. |
| Satır sonu | LF | Depo zaten LF ile saklar (`.gitattributes`); Windows'ta da aynı kalır. |
| Dosya sonu | Tek boş satır | Farklarda (diff) gereksiz satır değişikliği çıkmaz. |
| Satır sonu boşlukları | Silinir (Markdown hariç) | Markdown'da satır sonundaki iki boşluk anlamlıdır. |
| Girinti | C#: 4 boşluk; TS, JSON, YAML, CSS, XML: 2 boşluk | Her dilin topluluk alışkanlığı. |
| Satır uzunluğu | C#: 120; TS: 100 | C#'ta uzun genel tip adları yaygındır. |
| Kod içi yorumların ve XML belgelerinin dili | İngilizce ([S-11](#10-kararlar)) | Kod İngilizcedir (K-01); API belgesi XML belgelerinden üretilir. |

## 3. Araçlar

| Dil | Görev | Araç | Lisans |
|---|---|---|---|
| C# | Biçim (girinti, satır kırma, boşluk) | CSharpier | MIT |
| C# | Yazım kuralları ve adlandırma | `.editorconfig` + .NET'in yerleşik kod stili analizörleri | MIT |
| C# | Hata kalıpları, güvenlik, performans, kültür | .NET analizörleri (önerilen küme) + Meziantou.Analyzer | MIT |
| C# | Yasak API'ler | Microsoft.CodeAnalysis.BannedApiAnalyzers | MIT |
| TS / React | Biçim | Prettier + Tailwind sınıf sıralama eklentisi | MIT |
| TS / React | Hata kalıpları ve kurallar | ESLint 10 + typescript-eslint (tip bilgili kurallar) + eklentiler ([§5.3](#53-lint-eslint)) | MIT |
| TS | Tip denetimi | TypeScript 6.x (`tsc --noEmit`) | Apache 2.0 |
| Tümü | Ortak editör ayarları | EditorConfig | — |

## 4. C#

### 4.1 Derleme ayarları

Tüm projelerin ortak ayarları `Directory.Build.props`'tadır:

| Ayar | Değer | Açıklama |
|---|---|---|
| `Nullable` | `enable` | Boş (null) olabilecek değerler tipte açıkça belirtilir. |
| `ImplicitUsings` | `enable` | Sık kullanılan ad alanları otomatik eklenir. |
| `AnalysisLevel` | `latest-recommended` | .NET analizörlerinin önerilen kümesi. |
| `EnforceCodeStyleInBuild` | `true` | `.editorconfig`'deki yazım kuralları yalnızca editörde değil, derlemede de denetlenir. |
| `GenerateDocumentationFile` | `true` | İki nedenle: kullanılmayan `using` kuralı (IDE0005) derlemede yalnızca bu ayarla çalışır ([kaynak](https://github.com/dotnet/roslyn/issues/53720)); ayrıca .NET 10, XML belgelerini OpenAPI açıklamalarına dönüştürür ([kaynak](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/openapi-comments?view=aspnetcore-10.0)). |
| `NoWarn` | `CS1591` (Contracts ve IntegrationEvents projeleri hariç) | Belgesiz genel üye uyarısı yalnızca modüllerin dışa açık sözleşmelerinde zorunludur. |
| `TreatWarningsAsErrors` | Sürekli entegrasyonda ve Release derlemesinde `true` | Yerelde denemeler uyarıyla derlenebilir; ama uyarılı kod depoya giremez ([S-03](#10-kararlar)). |

### 4.2 Biçim

Biçimi yalnızca CSharpier belirler. CSharpier, Prettier gibi görüş sahibi (opinionated) bir biçimlendiricidir: ayar sayısı bilerek azdır ve kod her seferinde aynı biçime gelir. Satır uzunluğunu `.editorconfig`'den (`max_line_length = 120`) okur. `.csproj` ve `.props` dosyalarını da biçimlendirir.

.NET'in kendi biçim kuralı (IDE0055) kapatılır. Açık kalırsa CSharpier ile çelişir ve kaydedilen dosya bir biçimden diğerine gidip gelir.

### 4.3 Yazım kuralları

Microsoft'un C# kuralları temel alınır ([kaynak](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)). Aşağıdakiler derlemede denetlenir:

| Kural | Seçim | Kural kodu |
|---|---|---|
| Ad alanı bildirimi | Dosya kapsamlı (`namespace X;`) | IDE0161 |
| Ad alanı = klasör yolu | Zorunlu | IDE0130 |
| Süslü parantez | Tek satırlık `if` dahil her zaman | IDE0011 |
| `var` kullanımı | Tip sağ taraftan açıkça anlaşılıyorsa `var`, anlaşılmıyorsa açık tip | IDE0007, IDE0008 |
| `this.` | Kullanılmaz | IDE0003 |
| Yerleşik tip adları | `int`, `string` (`Int32`, `String` değil) | IDE0049 |
| Boş kontrolü | `is null` / `is not null` | IDE0041 |
| Kullanılmayan `using` | Silinir | IDE0005 |
| Kullanılmayan parametre ve özel üye | Silinir | IDE0060, IDE0051, IDE0052 |
| Koleksiyon oluşturma | Koleksiyon ifadesi (`[]`, `[a, b]`) | IDE0300–IDE0305 |
| Desen eşleme | Tip kontrolü ve dönüşümde desen eşleme; çoklu dalda `switch` ifadesi | IDE0019, IDE0020, IDE0066 |
| İfade gövdeli üye | Özellik ve tek satırlık metotlarda | IDE0022, IDE0025 |
| `readonly` alan | Yalnızca kurucuda atanan alan `readonly` olur | IDE0044 |
| `using` sırası | `System` önce, alfabetik | — |

### 4.4 Tasarım kuralları

- **Varsayılan erişim en dar olandır.** Tipler `internal`'dır; modül dışına yalnızca Contracts ve IntegrationEvents projelerindeki tipler açılır ([08 §3.4](../08-architecture.md#34-proje-içi-klasörler), AT-11).
- **Sınıflar varsayılan olarak `sealed`'dır.** Kalıtım bilinçli bir tasarım kararıdır; gerekiyorsa `sealed` kaldırılır ve nedeni yazılır (CA1852).
- **Kayıtlar (record).** Komut, sorgu, sonuç, okuma modeli ve olay tipleri `sealed record`'dur.
- **API'ye açılan tiplerde zorunlu alanlar `required` ile işaretlenir.** Alanın boş olup olamayacağı C# tipinde (`string` / `string?`) yazılır. Bu iki bilgi JSON şemasına, oradan OpenAPI belgesine, oradan da ön yüzün TypeScript tiplerine aynen geçer. Böylece "sunucuda zorunlu, ön yüzde isteğe bağlı" türü uyumsuzluklar ortadan kalkar. Serileştirici ayarları (`RespectNullableAnnotations`, `RespectRequiredConstructorParameters`) C.5'te bu kurala göre açılacak.
- **Değişmezlik.** Olaylar ve okuma modelleri değiştirilemez (`init`). Varlıkların durumu yalnızca kendi metotlarıyla değişir; dışarıya açık `set` yoktur.
- **Birincil kurucular** (`class X(IFoo foo)`) bağımlılık alan sınıflarda (işleyici, servis) kullanılabilir; parametreye yeniden değer atanmaz. Varlık ve değer tiplerinde kullanılmaz, çünkü bunlar kurucuda kural denetimi yapar.
- **Sihirli değer yok.** İş kuralı parametreleri (P-01…P-15) ayar sınıflarından, kural numaraları `…RuleCodes` sabitlerinden, yetki kodları `…Permissions` sabitlerinden gelir ([naming §4.2](naming.md#42-mimari-yapı-taşlarının-adları)).
- **Hatalar.** İş kuralı ihlalinde kural numarasını taşıyan `BusinessRuleViolationException` fırlatılır. İstisnalar akış kontrolü için kullanılmaz. Hata modelinin ayrıntısı C.6'dadır.
- **Veri erişimi.** Sorgular değişiklik takibi olmadan (`AsNoTracking`) okur ve doğrudan okuma modeline (`Select`) dönüştürür; varlık tipi modül dışına ya da API'ye sızmaz. Tembel yükleme (lazy loading) kullanılmaz.

### 4.5 Asenkron kod

- Veritabanı, ağ ve dosya işlemleri asenkrondur. Asenkron kodu senkron bekletmek (`.Result`, `.Wait()`, `GetAwaiter().GetResult()`) yasaktır (MA0042, yasak API listesi).
- `async void` yasaktır.
- `CancellationToken` her asenkron metoda son parametre olarak iletilir. İç metotlarda varsayılan değer (`= default`) verilmez; böylece iletmeyi unutmak derleme hatası olur (CA2016, MA0040).
- `ConfigureAwait(false)` yazılmaz. ASP.NET Core'da senkronizasyon bağlamı yoktur; bu çağrı yalnızca gürültüdür (CA2007 ve MA0004 kapalı).

### 4.6 Kültür ve metin

Türkçe kültürde `"I".ToLower()` sonucu `"ı"`, `1234.5` ise `"1234,5"` olur. Bu yüzden kültüre bağlı her dönüşüm açıkça yazılır ([08 §9](../08-architecture.md#9-türkçe-karakter-güvenliği)).

| Kural | Kod | Önem |
|---|---|---|
| Kültür belirtmeden büyük / küçük harf dönüşümü | CA1304, CA1311 | Hata |
| Biçim sağlayıcı belirtmeden biçimlendirme (`ToString`, `Parse`, `string.Format`) | CA1305, MA0011 | Hata |
| Karşılaştırma türü belirtmeden metin karşılaştırma | CA1307, CA1309, CA1310, MA0006, MA0074 | Hata |
| Metin içine yazılan sayı ve tarihin (`$"{amount}"`) örtük kültürle biçimlenmesi | MA0076 | Hata |

Son satırdaki kuralı .NET'in kendi analizörleri yakalamaz. Sunucu sabit kültürle çalışsa bile migration, test ve araçlar geliştiricinin Türkçe bilgisayarında çalışır; bu yüzden kurallar derlemede zorlanır ([kaynak](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/MA0076.md)).

Kullanıcıya giden metinler (PDF, e-posta) açıkça `tr-TR` kültürüyle biçimlenir. Bu nedenle uygulamada `InvariantGlobalization` açılmaz ve konteyner imajı ICU ile saat dilimi verisini içeren `chiseled-extra` sürümüdür ([09 §5](../09-environments-and-deployment.md#5-konteyner-imajı)).

### 4.7 Loglama

- Log mesajı sabit bir şablondur; değerler şablon alanlarıyla verilir: `logger.LogInformation("Hold {VenueHoldId} expired", holdId)`. Metin birleştirme ve `$"..."` yasaktır (CA2254). Böylece loglar alanlarına göre aranabilir.
- Şablon alanlarının adları [naming §9](naming.md#9-telemetri-ve-log-adları)'a uyar; tek başına `{Id}` ve `{Name}` kullanılmaz.
- Kişisel veri (şifre, oturum anahtarı, kimlik numarası) loga yazılmaz. Ayrıntısı C.6'dadır.

### 4.8 Yorumlar ve XML belgeleri

- Yorum, kodun **neden** öyle yazıldığını anlatır; ne yaptığını kodun kendisi anlatır.
- Yorum satırına alınmış (commented-out) kod depoya girmez; geçmiş git'tedir.
- Yapılacak işler iş takibindeki numarayla yazılır: `// TODO(#42): …`. Numarasız TODO kalmaz.
- XML belgesi (`/// <summary>`) şu yerlerde zorunludur: Contracts ve IntegrationEvents projelerindeki genel tipler (modülün dış sözleşmesi) ve API uç noktaları. Uç nokta belgeleri OpenAPI'ye, oradan ön yüzdeki üretilen kodun açıklamalarına geçer.
- **Uç nokta işleyicileri satır içi lambda değil, adlı metottur.** Metot, uç nokta grubunun sınıfında durur ve işlem adını taşır (`VenueHoldEndpoints.PlaceVenueHold`). .NET 10, XML belgesini yalnızca adlı metotlardan OpenAPI'ye aktarır ([kaynak](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/openapi-comments?view=aspnetcore-10.0)); ayrıca hata izlerinde lambda yerine anlamlı bir ad görünür. Açıklaması eksik uç noktayı AT-14 yakalar.

### 4.9 Analizörler

| Analizör | Kullanım |
|---|---|
| .NET analizörleri | `latest-recommended` kümesi. Kültür kuralları ve CA2254 hata seviyesinde. |
| Meziantou.Analyzer | Varsayılan kurallar açık. Kapatılanlar: MA0004 (`ConfigureAwait`), MA0051 (metot uzunluğu), MA0026 (TODO; §4.8'deki kural bunu karşılar). Hata seviyesine çıkarılanlar: MA0006, MA0011, MA0042, MA0048, MA0074, MA0076. |
| BannedApiAnalyzers | §4.10'daki liste |

Meziantou, .NET'in kendi analizörlerinin kaçırdığı kültür ve asenkron hatalarını yakaladığı için eklendi ([ADR-0019](../adr/0019-code-style-and-static-analysis-tools.md)). Kuralların tam önem listesi `.editorconfig`'de tutulur.

### 4.10 Yasak API'ler

`BannedSymbols.txt`'deki API'ler kullanılırsa derleme hata verir. Her satırda neyin yerine ne kullanılacağı yazılıdır:

| Yasak | Yerine | Neden |
|---|---|---|
| `DateTime.Now`, `DateTime.UtcNow`, `DateTime.Today`, `DateTimeOffset.Now`, `DateTimeOffset.UtcNow` | `TimeProvider` | Zamana bağlı kurallar testte zaman ilerletilerek sınanır ([ADR-0017](../adr/0017-time-and-money-types.md)). |
| `Task.Wait()`, `Task<T>.Result` | `await` | Kilitlenme ve iş parçacığı tükenmesi |
| `Thread.Sleep` | `await Task.Delay(…, timeProvider, cancellationToken)` | Aynı neden, ayrıca zaman testte kontrol edilemez |
| `Console.WriteLine` | `ILogger` | Loglar yapılandırılmış ve izlenebilir olmalı |
| `Guid.NewGuid()` | `Guid.CreateVersion7()` | Rastgele (sürüm 4) kimlik indeksi dağıtır ([database §5](database.md#5-kimlik)) |
| Yöntem belirtmeyen `Math.Round(decimal…)` ve `decimal.Round(…)` | `Money` tipinin yuvarlama yöntemi | .NET varsayılanı bankacı yuvarlamasıdır; proje kuralı sıfırdan uzağa ([database §8.2](database.md#82-yuvarlama)) |
| `FromSqlRaw`, `ExecuteSqlRaw`, `SqlQueryRaw` | `FromSql`, `ExecuteSql`, `SqlQuery` (enterpolasyonlu, otomatik parametreli) | SQL enjeksiyonu ([database §17](database.md#17-bağlantı-ve-işletim-ayarları)) |
| `ExecuteUpdate`, `ExecuteDelete` (yalnızca Domain ve Application projelerinde) | Değişiklik takibiyle güncelleme | İşlem geçmişini atlar ([database §14.2](database.md#142-kayıtların-işlem-geçmişi)) |

## 5. TypeScript ve React

### 5.1 TypeScript ayarları

TypeScript 6 katı modu (`strict`) varsayılan olarak açar. Buna ek olarak:

| Ayar | Değer | Neden |
|---|---|---|
| `noUncheckedIndexedAccess` | `true` | `list[0]` ve `map[key]` "yok olabilir" kabul edilir; en sık çalışma zamanı hatalarından biri derlemede yakalanır. |
| `noImplicitOverride` | `true` | Üst sınıf metodunu ezen metot `override` ile işaretlenir. |
| `noFallthroughCasesInSwitch` | `true` | `switch` dalları istemeden alttakine geçmez. |
| `verbatimModuleSyntax` | `true` | Yalnızca tip olan içe aktarımlar `import type` ile yazılır; derleme çıktısı öngörülebilir olur. |
| `erasableSyntaxOnly` | `true` | Çalışma zamanı kodu üreten TypeScript söz dizimi (`enum`, `namespace`, kurucu parametre özellikleri) yasaktır ([kaynak](https://www.totaltypescript.com/erasable-syntax-only)). |
| `exactOptionalPropertyTypes` | kapalı | Form ve tablo kütüphaneleriyle sürekli çatışır; kazancı bu maliyete değmez. |

**Sürüm:** TypeScript 7 (Go ile yeniden yazılmış derleyici) Temmuz 2026'da çıktı. Ancak henüz programatik API sunmuyor ve typescript-eslint onu kullanamıyor. API'nin TypeScript 7.1 ile gelmesi bekleniyor ([kaynak](https://github.com/typescript-eslint/typescript-eslint/issues/10940)). Bu yüzden proje **TypeScript 6.x**'e sabitlenir; typescript-eslint 7.x'i desteklediğinde geçiş planlanır ([S-05](#10-kararlar)).

### 5.2 Biçim

Prettier varsayılan ayarlarıyla kullanılır; yalnızca satır uzunluğu (`printWidth: 100`) değiştirilir. Tailwind sınıfları `prettier-plugin-tailwindcss` ile Tailwind'in önerdiği sıraya dizilir. Tailwind 4'te eklentiye stil dosyasının yolu (`tailwindStylesheet`) verilir.

Prettier'ın dışında tutulanlar (`.prettierignore`): `docs/` (Prettier Markdown tablolarını hizalar; uzun tablolarda her küçük değişiklik tüm tabloyu değiştirir), üretilen API istemcisi (`src/web/src/api/`), kilit dosyaları.

### 5.3 Lint (ESLint)

Yapılandırma tek dosyadadır (`eslint.config.js`; ESLint 10'da tek desteklenen biçim).

| Paket | Ne yakalar |
|---|---|
| `@eslint/js` (önerilen) | Temel JavaScript hataları |
| `typescript-eslint` (`strictTypeChecked`, `stylisticTypeChecked`) | Tip bilgisiyle çalışan kurallar: beklenmeyen promise'ler (`no-floating-promises`), yanlış yerde kullanılan promise'ler, eksik `switch` dalları, gereksiz koşullar |
| `eslint-plugin-react-hooks` (önerilen) | Hook kuralları ve React Compiler'ın doğrulama kuralları ([kaynak](https://react.dev/reference/eslint-plugin-react-hooks)) |
| `eslint-plugin-react-refresh` | Vite'in anlık yenilemesini bozan dışa aktarımlar (TanStack Router'ın `Route` dışa aktarımı izinli) |
| `@tanstack/eslint-plugin-query` | TanStack Query'nin yanlış kullanımları (ör. sorgu anahtarında eksik bağımlılık) |
| `@tanstack/eslint-plugin-router` | TanStack Router yönlendirme tanımlarındaki sıra hataları |
| `eslint-plugin-jsx-a11y` | Erişilebilirlik hataları (etiketsiz alan, klavyeyle erişilemeyen düğme) |
| `eslint-plugin-boundaries` | Ön yüz modül sınırları: bir modül başka bir modülün yalnızca `index.ts`'inden içe aktarabilir ([08 §11](../08-architecture.md#11-ön-yüz-yapısı)) |
| `eslint-plugin-simple-import-sort` | İçe aktarımların sırası (otomatik düzeltilir) |
| `@eslint-community/eslint-plugin-eslint-comments` | Kural kapatma yorumlarının gerekçesiz ya da gereksiz olması |

Biçimle ilgili kural içeren eklenti kullanılmaz. Bu yüzden Prettier ile ESLint çakışmaz ve `eslint-config-prettier` gerekmez.

**Projeye özgü kurallar:**

| Kural | Nasıl | Neden |
|---|---|---|
| Adlandırma | `@typescript-eslint/naming-convention` | [naming §7](naming.md#7-ön-yüz-adları)'deki büyük / küçük harf ve boolean önekleri |
| Yalnızca ASCII adlar | `id-match` | Türkçe karakterli değişken adı olmaz |
| Varsayılan dışa aktarım yasağı | `no-restricted-exports` (yapılandırma dosyaları hariç) | Adlı dışa aktarım yeniden adlandırmada güvenlidir |
| Tip tanımı | `consistent-type-definitions: type`, `consistent-type-imports` | Tek biçim |
| Yerel ayarsız harf dönüşümü ve sıralama yasağı | `no-restricted-syntax`: argümansız `toLocaleLowerCase()`, `toLocaleUpperCase()`, `localeCompare(x)` | Tarayıcının diline göre değişen sonuç; bkz. §5.5 |
| Sunucuya yalnızca üretilen istemciyle gidilir | `no-restricted-globals: fetch` (istemci sarmalayıcısı hariç) | Tüm istekler aynı hata işleme, kimlik doğrulama ve tip katmanından geçer |
| `console` | Yalnızca `console.warn` ve `console.error` | Geliştirme artığı log kalmaz |
| HTML'i doğrudan basma | `no-restricted-syntax`: `dangerouslySetInnerHTML` yasak | XSS ([security §9](security.md#9-girdi-doğrulama-katmanları)) |
| Eşitlik | `eqeqeq` | `==`'nin tip dönüştürme sürprizleri |

Üretilen kod (`src/api/`) ve shadcn/ui bileşenleri (`src/components/ui/`) lint dışında tutulur.

### 5.4 React kuralları

- **React Compiler kullanılır** ([S-06](#10-kararlar)). Compiler, bileşenleri otomatik olarak önbelleğe alır (memoization). Bu yüzden `useMemo`, `useCallback` ve `memo` varsayılan olarak yazılmaz; yalnızca ölçülmüş bir performans sorunu varsa eklenir. Vite 8'de React eklentisi Babel'i kendi içinden çıkardığı için Compiler, ayrı bir Babel eklentisiyle (`@rolldown/plugin-babel` + `reactCompilerPreset()`) etkinleştirilir ([kaynak](https://react.dev/blog/2025/10/07/react-compiler-1)).
- Yalnızca fonksiyon bileşenleri yazılır. Hata sınırları TanStack Router'ın rota hata bileşeniyle (`errorComponent`) kurulur.
- Bileşen özellikleri fonksiyon imzasında ayrıştırılır ve `…Props` tipiyle yazılır. `React.FC` kullanılmaz.
- Sunucu verisi yalnızca üretilen TanStack Query kancalarıyla alınır. `useEffect` içinde veri çekilmez ([08 §11](../08-architecture.md#11-ön-yüz-yapısı)).
- Liste filtreleri, sıralama ve sayfa numarası adres çubuğunda (TanStack Router arama parametreleri, Zod ile doğrulanmış) tutulur. Böylece sayfa yenilendiğinde ya da bağlantı paylaşıldığında aynı görünüm açılır, geri tuşu beklendiği gibi çalışır.
- Formlar React Hook Form + Zod ile yazılır. Zod şemaları mümkünse OpenAPI'den üretilir; elle yazılan şema, üretilen tiple uyumlu olmak zorundadır.
- React Compiler ile uyum için formlarda alan değeri `useWatch({ control, name })`, form durumu `useFormState({ control })` ile okunur ve tüm girişler `Controller` ile bağlanır. `watch()` ve `formState` nesnesinden doğrudan okuma lint kuralıyla yasaktır ([ui §7.1](ui.md#71-araç), [U-05](ui.md#19-kararlar)).
- Tüm kullanıcı metinleri çeviri dosyalarındadır; bileşende sabit Türkçe metin yazılmaz (K-02).

### 5.5 Türkçe metin

| İş | Doğru | Yanlış |
|---|---|---|
| Kullanıcı metnini arama için küçük harfe çevirme | `text.toLocaleLowerCase('tr-TR')` | `text.toLocaleLowerCase()` (tarayıcının diline bağlı) |
| Teknik anahtarı küçük harfe çevirme | `key.toLowerCase()` (dile bağlı değildir) | — |
| Sıralama | `a.localeCompare(b, 'tr')` ya da `new Intl.Collator('tr')` | `a.localeCompare(b)`, `a < b` |
| Sayı ve para biçimi | `Intl.NumberFormat('tr-TR', …)` | `value.toString()` |
| Tarih ve saat | date-fns + `tr` yerel ayarı, saat dilimi `Europe/Istanbul` (@date-fns/tz) | `new Date().toLocaleString()` |

## 6. Diğer dosyalar

| Dosya | Kural |
|---|---|
| Markdown (`docs/`) | Otomatik biçimlendirilmez. Bağlantılar ve kural numaraları `tools/docs` betikleriyle denetlenir. |
| JSON, YAML | Prettier |
| `.csproj`, `.props` | CSharpier |
| Elle yazılan SQL (migration içindeki özel kısıtlar, görünümler) | SQL anahtar kelimeleri büyük harf, adlar küçük harf snake_case: `ALTER TABLE booking.venue_holds ADD CONSTRAINT …` |

## 7. Kurallar nerede denetlenir

| Aşama | Ne çalışır |
|---|---|
| Editör | Kaydederken CSharpier ve Prettier biçimlendirir. Analizör ve ESLint uyarıları yazarken görünür. |
| Commit öncesi | Değişen dosyalarda biçim ve lint denetimi; Lefthook ile ([git §8](git.md#8-commit-öncesi-kontroller)). |
| Sürekli entegrasyon | `dotnet build -c Release` (uyarılar hata), `dotnet csharpier check .`, mimari testler, `pnpm lint`, `pnpm format:check`, `pnpm typecheck`, üretilen API istemcisinin güncelliği. Ayrıntısı [ci.md](ci.md)'dedir. |

## 8. Kuraldan sapma

- Bir analizör ya da lint kuralı yalnızca **gerekçesiyle** kapatılır ve kapsam en dar tutulur (tek satır):
  - C#: `#pragma warning disable MA0076 // Invariant format is intended: value is a machine-readable code.` ya da `[SuppressMessage(…, Justification = "…")]`.
  - TypeScript: `// eslint-disable-next-line no-console -- Startup diagnostics before logger is ready.` Gerekçesiz kapatma lint hatasıdır.
- Bir kural sık sık kapatılıyorsa kural bu belgede tartışılır ve gerekirse genel olarak değiştirilir; tek tek kapatma alışkanlığa dönüşmez.
- Gereksiz hale gelmiş kapatmalar hata verir (IDE0079, `eslint-comments/no-unused-disable`).

## 9. Kuralları değiştirme

Bu belgedeki bir kural değiştirilecekse önce belge güncellenir, sonra araç yapılandırması (`.editorconfig`, `eslint.config.js`) aynı PR'da değiştirilir ve tüm kod yeni kurala göre düzeltilir. Belge ile yapılandırma birbirinden ayrışmaz.

## 10. Kararlar

| No | Konu | Karar | Gerekçe |
|---|---|---|---|
| S-01 | C# biçimlendirici | CSharpier; .NET'in biçim kuralı (IDE0055) kapalı | `dotnet format` satır kırmaz, uzun satırları olduğu gibi bırakır. CSharpier her dosyayı tek bir biçime getirir; biçim tartışması biter. |
| S-02 | Ek C# analizörü | Meziantou.Analyzer (MIT), seçilmiş kurallarla | Türkçe kültürde en tehlikeli hata olan metin içi örtük biçimlendirmeyi (MA0076) .NET'in kendi analizörleri yakalamıyor. Dosya adı = tip adı kuralı da buradan gelir. |
| S-03 | Uyarılar hata mı | Sürekli entegrasyonda ve Release'te evet, yerelde uyarı | Yerelde deneme yapmak engellenmez; ama uyarılı kod ana dala giremez. [08 §10](../08-architecture.md#10-derleme-ayarları) buna göre güncellendi. |
| S-04 | Ön yüz lint ve biçim aracı | ESLint 10 + typescript-eslint + Prettier; Biome değil | Biome hızlıdır ama React Compiler kuralları, TanStack eklentileri ve modül sınırı kuralı yok; tip bilgili kuralları typescript-eslint'in yaklaşık %85'i düzeyinde ([kaynak](https://reintech.io/blog/typescript-biome-vs-eslint-linting-formatting-comparison-2026)); Tailwind sınıf sıralaması yarım ([kaynak](https://biomejs.dev/linter/rules/use-sorted-classes/)). İki aracı birlikte kullanmak "aynı işe tek araç" ilkesine aykırı. |
| S-05 | TypeScript sürümü | 6.x'e sabit; typescript-eslint desteklediğinde 7.x'e geçiş | TypeScript 7.0 programatik API sunmadığı için tip bilgili lint kuralları onunla çalışmıyor. |
| S-06 | React Compiler | Kullanılır | Kararlı sürümde (1.0). Elle önbellekleme gerektirmeden gereksiz yeniden çizimleri azaltır; anlık güncellenen depo ve okutma ekranlarında akıcılığa katkı verir. Lint kuralları zaten Compiler'a göre yazılmıştır. |
| S-07 | Dışa aktarım biçimi | Yalnızca adlı dışa aktarım | Yeniden adlandırma güvenli, arama tek adla. |
| S-08 | TypeScript `enum` | Yasak (`erasableSyntaxOnly`) | Orval zaten sabit nesne üretir; enum'un çalışma zamanı kodu ve tip tuhaflıkları gereksizdir. |
| S-09 | API tiplerinde zorunluluk ve boş değer | `required` + C# nullable bilgisi tek kaynak; OpenAPI ve TypeScript tipleri buradan türer | Sunucu ile ön yüz arasındaki en yaygın uyumsuzluk sınıfı derleme aşamasında kapanır. |
| S-10 | Prettier'ın kapsamı | `docs/` dışarıda | Markdown tablolarının yeniden hizalanması belge farklarını okunmaz hale getirir. |
| S-11 | Kod içi yorumların ve XML belgelerinin dili | İngilizce | K-01 kodu İngilizce tanımlıyor; yorumlar da kodun parçası. OpenAPI açıklamaları XML belgelerinden üretildiği için API belgesi de aynı dilde olur. Portfolyoyu yabancı okuyucular da inceleyebilir. |

## 11. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-09-25 | v0.1 | İlk taslak |
| 2026-09-25 | v1.0 | S-11: kod içi yorumlar ve XML belgeleri İngilizce. ADR-0019 kabul edildi. |
| 2026-09-25 | v1.1 | Yasak API listesine veritabanı standardından gelen kurallar eklendi (C.4). |
| 2026-09-25 | v1.2 | `dangerouslySetInnerHTML` yasağı eklendi (C.6). |
| 2026-09-25 | v1.3 | Commit öncesi kanca aracı: Lefthook (D.1). |
| 2026-09-25 | v1.4 | Konteyner imajı ve CI ayrıntısı bağlandı (D.3). |
| 2026-09-25 | v1.5 | React Hook Form için derleyici uyumlu kalıplar zorunlu (D.4). |
