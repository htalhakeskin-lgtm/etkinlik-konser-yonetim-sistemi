# Arayüz Standardı

> **Durum:** v1.1 · **Son güncelleme:** 2026-09-26
> **Kararlar:** [Bölüm 19](#19-kararlar)

## 1. Bu belge ne işe yarar

Arayüzün nasıl görüneceğini ve nasıl davranacağını tanımlar:
- tasarım değişkenleri (renk, yazı, aralık, tema),
- yerleşim, gezinme ve bileşenler,
- formların, tabloların ve geri bildirimin davranışı,
- yükleniyor, boş ve hata durumları,
- anlık güncelleme ve eşzamanlı düzenlemenin kullanıcıya nasıl yansıdığı,
- metinler ve Türkçe biçimler,
- depo ekranları,
- erişilebilirlik ve performans.

Ekranların listesi ve şablonların çizimleri [11-screens.md](../11-screens.md)'dedir. Ön yüzün teknik yapısı [08 §11](../08-architecture.md#11-ön-yüz-yapısı)'de, kod kuralları [code-style §5](code-style.md#5-typescript-ve-react)'te, adlar [naming §7](naming.md#7-ön-yüz-adları)'dedir. Sunucuyla sözleşme (sayfalama, hata biçimi, eşzamanlılık, sürüm şeridi, anlık bildirimler) [api.md](api.md)'dedir; bu belge o sözleşmenin ekrandaki karşılığını tanımlar.

## 2. Temel ilkeler

1. **Sunucu gerçeğin kaynağıdır, ama ekran bekletmez.** Her tıklama 0,1 saniye içinde görünür bir tepki verir (düğmenin basılı hali, yükleniyor işareti); sonuç sunucudan geldiğinde ekran güncellenir. 1 saniyenin üstündeki beklemelerde ne olduğu yazılır ([kaynak](https://www.nngroup.com/articles/response-times-3-important-limits/)).
2. **Hata önlenir, sonra açıklanır.** Yapılamayacak işlem sunulmaz; yapılamıyorsa nedeni yazılır. Hata mesajı ne olduğunu ve ne yapılacağını söyler.
3. **Aynı iş her yerde aynı görünür.** Liste, detay, form ve okutma ekranları birkaç şablondan türetilir ([11](../11-screens.md)). Yeni bir ekran önce mevcut bir şablona oturtulur.
4. **Yoğun ama okunur.** Ofis ekranları çok veri gösterir; bu, sıkışık değil düzenli olmakla sağlanır: hizalı sütunlar, tablo rakamları, tutarlı aralıklar.
5. **Depo ekranı başka bir cihazdır.** Telefonda, tek elle, eldivenle, gürültülü ve aydınlığı değişken bir ortamda kullanılır. Kuralları ayrıdır (§14).
6. **Türkçe önce.** Metinler, biçimler, büyük / küçük harf ve sıralama Türkçe kurallarla tasarlanır (§13).
7. **Herkes kullanabilir.** Klavye, ekran okuyucu ve renk körlüğü tasarımın parçasıdır, sonradan eklenen bir kontrol değildir (§15).

## 3. Tasarım değişkenleri

### 3.1 Yapı

Tasarım kararları kodda tek yerde, CSS değişkenleri olarak tutulur (`src/web/src/styles/tokens.css`) ve Tailwind CSS 4'ün `@theme` bloğuyla sınıflara bağlanır ([kaynak](https://ui.shadcn.com/docs/theming)).

| Katman | Örnek | Kim kullanır |
|---|---|---|
| Ham değerler (palet) | `--palette-violet-540: oklch(0.54 0.28 293)` | Yalnızca anlamsal değişkenler |
| Anlamsal değişkenler | `--primary`, `--destructive`, `--status-warning` | Bileşenler |
| Bileşen değişkenleri | `--sidebar-width`, `--scan-target-size` | Yalnızca ilgili bileşen |

- Palet adındaki sayı OKLCH açıklığının 1000 katıdır: `--palette-gray-985` = `oklch(0.985 0 0)`. Ad, rengin ne kadar açık olduğunu doğrudan söyler.
- Renkler OKLCH ile yazılır. OKLCH'de açıklık değeri algıyla uyumlu olduğu için aynı açıklıktaki renkler gözde de aynı parlaklıkta görünür; kontrast ayarlamak kolaylaşır.
- **Bileşen kodunda ham renk yazılmaz.** Tailwind'in hazır renk paleti temada kapatılır (`--color-*: initial`); `bg-red-500` gibi sınıflar hiç üretilmez. Yalnızca anlamsal sınıflar vardır (`bg-primary`, `text-muted-foreground`, `bg-status-warning`). Kural böylece araçla kendiliğinden korunur.
- Değişkenler W3C tasarım değişkenleri biçimine (2025.10, ilk kararlı sürüm) uygun bir JSON dosyasından da okunabilecek şekilde adlandırılır; bir tasarım aracıyla eşitleme gerekirse dönüşüm doğrudan yapılır ([kaynak](https://www.w3.org/community/design-tokens/2025/10/28/design-tokens-specification-reaches-first-stable-version/)). S1'de kaynak CSS dosyasıdır.

### 3.2 Renkler

shadcn/ui'ın anlamsal değişkenleri temel alınır: her yüzey bir "üstündeki yazı" eşiyle gelir (`--primary` / `--primary-foreground`) ve eşin okunurluğu garanti edilir ([kaynak](https://21st.dev/blog/shadcn-theming-css-variables)).

| Değişken | Kullanım |
|---|---|
| `background` / `foreground` | Sayfa zemini ve ana metin |
| `card`, `popover` | Zeminin üstündeki yüzeyler |
| `primary` | Sayfadaki ana işlem düğmesi, seçili öğe, bağlantı. Marka rengi: **mor (menekşe)** ([U-03](#19-kararlar)). |
| `secondary` | İkincil düğmeler |
| `muted` / `muted-foreground` | Arka plan bölgeleri, yardımcı metin |
| `accent` | Üzerine gelinen ya da klavyeyle seçilen satır ve menü öğesi |
| `destructive` | Geri alınamaz işlem düğmesi (iptal, serbest bırakma) |
| `border`, `input`, `ring` | `border` ayırıcı çizgidir ve süs sayılır. `input` alan kenarıdır; zeminle en az 3:1 kontrast verir (WCAG 1.4.11). `ring` klavye odağıdır. |
| `status-neutral`, `status-info`, `status-active`, `status-success`, `status-warning`, `status-danger`, `status-muted` | Durum rozetleri ve uyarı kutuları (§3.3). Her birinin yazı ve simge (`--status-warning`), yüzey (`--status-warning-surface`) ve kenar (`--status-warning-border`) çeşidi vardır. |

**Marka rengi:** Mor, durum tonlarının renklerinden (yeşil, sarı, kırmızı, açık mavi) en uzak tondur; ana düğme bir durum rozetiyle karıştırılmaz. Başlangıç değerleri: açık temada `oklch(0.54 0.28 293)` üzerinde beyaz yazı, koyu temada `oklch(0.70 0.18 294)` üzerinde çok koyu yazı. Değerler `src/web/src/styles/tokens.css`'tedir; her yazı / zemin çifti kontrast testiyle (§18) ölçülür.

**Kontrast kuralları** (WCAG 2.2 AA):
- Metin ile zemini arasında en az **4,5:1**; 18 pt (24 px) ya da 14 pt kalın ve üstü metinde en az **3:1**.
- Alan kenarları, simgeler, odak halkası gibi arayüz öğelerinde en az **3:1** (1.4.11).
- Pasif (kullanılamaz) öğeler bu kuraldan muaftır, ama pasif olduğu yalnızca soluklukla değil, imleç ve ekran okuyucu bilgisiyle de anlaşılır.
- Her değişken çifti tasarım sistemi testlerinde otomatik olarak ölçülür (§18).

### 3.3 Durum tonları

Her durum bir **ton**, bir **simge** ve **adıyla** gösterilir. Renk tek başına anlam taşımaz (renk körlüğü; WCAG 1.4.1). Tonların anlamı tüm uygulamada aynıdır:

| Ton | Anlamı | Simge (lucide) | Renk |
|---|---|---|---|
| `neutral` | Başlangıç, henüz bir şey olmadı | `circle-dashed` | Gri, kenarlı |
| `info` | İlerliyor, bekleyen bir karar yok | `circle-dot` | Açık mavi |
| `active` | Şu an fiziksel olarak sürüyor (kurulum, yolda, sahada) | `play` | Turkuaz |
| `success` | İstenen duruma ulaşıldı | `circle-check` | Yeşil |
| `warning` | Dikkat ya da karar gerekiyor | `triangle-alert` | Kehribar |
| `danger` | Sorun var, müdahale gerekiyor | `octagon-alert` | Kırmızı |
| `muted` | Bitti ya da geçersiz; artık işlem beklemiyor | `circle-slash` | Gri |

Durumların tonları ([01](../01-glossary.md), [04](../04-state-machines.md)):

| Durum makinesi | neutral | info | active | success | warning | danger | muted |
|---|---|---|---|---|---|---|---|
| Etkinlik | Talep | Opsiyonda, Müzakere, Hesaplaşma | Hazırlık, Kurulum, Canlı, Söküm | Onaylı | — | — | Kapandı, İptal |
| Opsiyon | — | Aktif | — | Kesinleşti | — | — | Düştü |
| Rezervasyon | — | Önerildi | — | Onaylandı | — | — | Serbest bırakıldı, Tamamlandı |
| Transfer | — | Planlandı | Yolda | Tamamlandı | — | — | İptal |
| Dış kiralama siparişi | Taslak | Sipariş verildi | — | Teslim alındı | — | — | İade edildi, İptal |
| Birim | — | Etkinlikte | Yolda | Depoda | Bakımda | Kayıp | Hurda, Tedarikçiye iade edildi |
| Çakışma | — | — | — | Çözüldü | Kabul edildi | Açık | — |
| İhtiyaç hesabı | — | — | — | Güncel | Güncel değil | — | — |
| Kullanıcı hesabı | — | — | — | Aktif | Şifre değiştirmeli | — | Pasif |
| Ana veriler | — | — | Aktif (rozet gösterilmez) | — | — | — | Pasif |

**Durum dışı işaretler** (bir durum değil, bir koşul) aynı tonlarla, rozetin yanında küçük bir işaret olarak gösterilir: opsiyon süresi yaklaşıyor (`warning`) ya da geçti (`danger`), planlanan varışı geçmiş transfer (`warning`), eksik kasa (`warning`), kilitli hesap (`danger`), dış kiralama (`info`).

### 3.4 Yazı

| Konu | Karar |
|---|---|
| Yazı tipi | **Inter** (değişken yazı tipi), uygulamayla birlikte sunulur; dış kaynaktan yüklenmez ([security §8](security.md#8-tarayıcı-güvenlik-başlıkları)). Yedek: `system-ui`. |
| Neden Inter | Ekranda küçük boyutlarda okunması için tasarlandı; tüm rakamları eşit genişlikte gösteren tablo rakamları (`tnum`) var; Türkçe dahil 140'tan fazla dili destekliyor; değişken yazı tipi olarak tek dosyadır ([kaynak](https://rsms.me/inter/)). IBM Plex Sans da uygun bir alternatifti; Inter daha dar olduğu için yoğun tablolarda daha fazla bilgi sığdırır ([kaynak](https://fontfyi.com/blog/ibm-plex-vs-inter/)). |
| Lisans | SIL Open Font License 1.1. Yazı tipleri için [ADR-0005](../adr/0005-dependency-license-policy.md)'e eklenir ([ADR-0033](../adr/0033-design-system.md)). |
| PDF'ler | Aynı yazı tipi QuestPDF belgelerine gömülür ([ADR-0014](../adr/0014-documents-and-qr.md)); ekran ve belge aynı görünür. |
| Dosya | Yalnızca Latin ve Latin genişletilmiş karakter alt kümesi (Türkçe harfler dahil), WOFF2; ilk açılışta önceden yüklenir. |
| Tablo rakamları | Tablolarda, tutarlarda, adetlerde ve saatlerde `tabular-nums`; sütundaki sayılar alt alta hizalanır. |

**Ölçek** (ofis ekranları):

| Ad | Boyut / satır yüksekliği | Kullanım |
|---|---|---|
| `text-xs` | 12 / 16 px | Yalnızca yardımcı etiketler, tablo alt bilgisi |
| `text-sm` | 14 / 20 px | Tablo hücreleri, form alanları, çoğu arayüz metni |
| `text-base` | 16 / 24 px | Okunacak paragraflar, depo ekranlarında en küçük metin |
| `text-lg` | 18 / 28 px | Bölüm başlığı |
| `text-xl` | 20 / 28 px | Kart ve diyalog başlığı |
| `text-2xl` | 24 / 32 px | Sayfa başlığı; depo ekranında adetler |
| `text-3xl` | 30 / 36 px | Depo ekranında büyük sayaçlar |

- Ağırlıklar yalnızca 400 (normal), 500 (vurgulu) ve 600 (başlık).
- Telefonlarda metin giriş alanlarının yazı boyutu en az **16 px**'tir; daha küçük olursa iOS Safari alana dokunulunca sayfayı yakınlaştırır.
- Büyük harfle yazılmış metin (tümü büyük) kullanılmaz; okumayı yavaşlatır ve Türkçe "i / İ" dönüşümünü tarayıcının dil ayarına bağlar (§13.4).

### 3.5 Aralık, boyut ve yoğunluk

- Temel birim **4 px**'tir; aralıklar 4'ün katlarıdır (Tailwind'in varsayılan ölçeği).
- **Ofis kontrolleri** (düğme, alan, seçim): 36 px yükseklik (`h-9`); simge düğmeleri 36 × 36 px. Tablo satırı 40 px, sıkı görünümde 32 px. Hiçbir tıklama hedefi 24 × 24 px'ten küçük değildir (WCAG 2.5.8).
- **Depo kontrolleri:** §14.3.
- Formlar en fazla 640 px genişliğindedir; tablolar sayfanın tamamını kullanır.

### 3.6 Köşe, gölge, katman ve hareket

| Konu | Karar |
|---|---|
| Köşe yuvarlaklığı | Tek değişken (`--radius`, 8 px); diğerleri ondan türetilir. |
| Gölge | Yalnızca yüzen öğelerde (menü, açılır pencere, diyalog, bildirim). Sayfadaki kartlar gölge yerine kenar çizgisiyle ayrılır. |
| Katman sırası | Sabit ölçek: yapışkan başlık, açılır menü, yan panel, diyalog, bildirim, sürüm şeridi. Bileşenler rastgele `z-index` değeri yazmaz. |
| Hareket | Süreler 150–200 ms. İşletim sisteminde "hareketi azalt" açıksa (`prefers-reduced-motion`) geçişler kapanır; yalnızca anında renk değişimi kalır. Hareket bilgi taşır (açılan panelin yönü), süsleme için kullanılmaz. |

### 3.7 Simgeler

- [lucide](https://lucide.dev) (ISC); boyutlar 16, 20 ve 24 px; çizgi kalınlığı tüm uygulamada aynı.
- Yalnızca simgeden oluşan düğmenin erişilebilir bir adı (`aria-label`) ve üzerine gelince görünen bir ipucu vardır. Depo ekranlarında simgeli düğmelerin yanında her zaman metin de vardır.
- Aynı kavram her yerde aynı simgeyle gösterilir (ör. etkinlik `calendar`, depo `warehouse`, okutma `scan-line`); eşleme tasarım sistemi kataloğundadır (§18).

### 3.8 Temalar

**Açık ve koyu tema S1'den itibaren vardır** ([U-02](#19-kararlar)):
- Varsayılan, cihazın ayarıdır (`prefers-color-scheme`). Kullanıcı menüsünden "Açık", "Koyu" ya da "Sistem" seçilebilir; seçim cihaz başına saklanır. Aynı kişi ofiste açık, karanlık kuliste koyu kullanabilir.
- Tema, sayfa ilk kez çizilmeden önce uygulanır; yanlış temanın bir an görünüp değişmesi olmaz. Bunun için `<head>`'de küçük, ayrı bir betik dosyası çalışır. Güvenlik politikası satır içi betiğe izin vermediği için ayrı dosyadır ([security §8](security.md#8-tarayıcı-güvenlik-başlıkları)).
- Tüm renkler anlamsal değişkenlerden gelir; koyu tema yalnızca değişkenlerin yeni değerleridir, bileşen kodu değişmez.
- Koyu temada zemin saf siyah değildir; yüzeyler yükseldikçe (kart, menü, diyalog) açıklaşır. Durum tonlarının koyu tema çeşitleri aynı kontrast kurallarına uyar.
- QR kodu ve etiket önizlemeleri koyu temada da beyaz zemin üzerinde gösterilir.
- Her bileşen iki temada da katalogda görünür; kontrast testleri iki tema için de çalışır (§18).
- Depo ekranları da aynı temayı izler; iki temada da temel bilgiler için 7:1 kontrast hedeflenir (§14.3).
- İşletim sisteminin zorunlu renk kipi (Windows Yüksek Karşıtlık, `forced-colors`) desteklenir: durum bilgisi simge ve metinle de verildiği için bu kipte kaybolmaz; odak halkası sistem rengini kullanır.
- PDF belgeleri her zaman açık renklidir.

## 4. Yerleşim ve ekran boyutları

| Cihaz | Hedef | Kural |
|---|---|---|
| Ofis bilgisayarı | 1366 × 768 ve üstü; en iyi 1920 × 1080 | 1280 px genişlikte hiçbir işlev kaybolmaz. 1024 px'te kenar menüsü kapanır ve ekran kullanılabilir kalır. |
| Tablet | 768 px ve üstü | Ofis ekranları çalışır; tablete özel düzen yapılmaz. |
| Telefon | 360–430 px, dikey | Depo ekranları buna göre tasarlanır (§14). Ofis ekranları telefonda okunabilir ama iş akışları için tasarlanmaz. |

**Uygulama kabuğu** ([11 §2.1](../11-screens.md#21-uygulama-kabuğu)):
- Solda daraltılabilir kenar menüsü (240 px / 56 px); modüllere göre gruplanır ve yalnızca kullanıcının yetkisi olan ekranları gösterir (US-SYS-012).
- Üstte ince bir çubuk: sayfa yolu (breadcrumb), hızlı erişim (§5.3), bağlantı göstergesi (§11.4), kullanıcı menüsü.
- Sürüm şeridi ve bağlantı uyarısı, kabuğun en üstünde tüm genişlikte görünür (§9.5).
- Depo ekranlarının kabuğu farklıdır: kenar menüsü yoktur; üstte geri düğmesi ve ekran adı, altta sabit işlem çubuğu vardır (§14.2).

## 5. Gezinme ve adresler

### 5.1 Adresler ekranın durumunu taşır

- Her ekranın ve her önemli görünümün kendi adresi vardır: liste filtreleri, sıralama, sayfa, detay sekmesi, açık diyalog gerektirmeyen görünümler. Sayfa yenilendiğinde, bağlantı paylaşıldığında ya da geri tuşuna basıldığında aynı görünüm açılır ([code-style §5.4](code-style.md#54-react-kuralları)).
- Adresler İngilizce ve kebab-case'tir ([naming §7](naming.md#7-ön-yüz-adları)); örnekler [11 §3](../11-screens.md#3-s1-ekran-envanteri)'tedir.
- Kayıtlara giden bağlantılar gerçek bağlantıdır (`<a href>`): yeni sekmede açılabilir, adres kopyalanabilir.

### 5.2 Sayfa yolu ve geri dönüş

- Detay sayfalarında sayfa yolu vardır: `Etkinlikler › Yaz Festivali 2027`. Listeye dönüşte filtreler korunur (liste adresi hatırlanır).
- Diyaloglar tarayıcı geçmişine yazılmaz; geri tuşu diyalogu değil sayfayı değiştirir. İstisna: telefonda tam ekran açılan paneller (geri tuşuyla kapanması beklenir).

### 5.3 Hızlı erişim ve klavye kısayolları

- `Ctrl + K` (macOS'ta `⌘ + K`) hızlı erişim penceresini açar: ekran adına ve kayıtlara (etkinlik adı, etiket kodu, taraf adı) göre arama. Yalnızca yetkili ekranlar ve kayıtlar listelenir.
- Kısayollar az ve belgelidir: `Ctrl + K` hızlı erişim, `/` listedeki arama kutusu, `Ctrl + Enter` formu kaydetme, `Esc` açık paneli kapatma, `?` kısayol listesi. Tek harfli kısayollar kullanılmaz; Türkçe klavye düzeniyle ve ekran okuyucularla çakışır.

## 6. Bileşenler

### 6.1 Kaynak

- Bileşenler shadcn/ui'dan kaynak kodu olarak kopyalanır ([ADR-0009](../adr/0009-ui-components.md)) ve **Base UI** temelli sürümü kullanılır ([ADR-0033](../adr/0033-design-system.md)). Base UI, Radix'i geliştiren ekibin yeni kütüphanesidir; Aralık 2025'te kararlı sürüme ulaştı ve Temmuz 2026'dan beri shadcn/ui'ın varsayılanıdır ([kaynak](https://ui.shadcn.com/docs/changelog/2026-07-base-ui-default)).
- Kopyalanan bileşenler (`components/ui/`) tasarım değişkenlerine göre bir kez uyarlanır; sonra yalnızca bilinçli güncellemelerle değişir. Güncelleme ayrı bir PR'dır ve tasarım sistemi kataloğunda gözden geçirilir.
- Modül kodu `components/ui/` yerine mümkün olduğunca uygulamanın ortak bileşenlerini (`components/common/`) kullanır; ortak davranış (yükleniyor, hata, yetki) böylece tek yerde kalır.

### 6.2 Ortak bileşenler

| Bileşen | Görevi |
|---|---|
| `AppShell`, `PageHeader` | Kabuk; sayfa başlığı, sayfa yolu, sayfanın ana işlemleri |
| `DataTable` | Sunucu tarafı sayfalama, sıralama ve filtreli tablo (§8) |
| `FilterBar` | Arama kutusu, hızlı filtreler, etkin filtre etiketleri |
| `CursorList` | "Daha fazla yükle" düğmeli, imleçli liste (işlem geçmişi, mobil listeler) |
| `StatusBadge` | Ton + simge + ad (§3.3) |
| `DetailTabs`, `KeyValueList`, `SectionCard` | Detay sayfası düzeni |
| `StatusTimeline` | Durum geçmişi (BR-EVT-015) |
| `EntityCombobox` | Uzak aramalı seçim: taraf, mekan, model, depo (§7.9) |
| `MoneyField`, `QuantityField`, `DurationField` | Türkçe biçimli sayı girişleri (§7.8) |
| `DateField`, `DateTimeRangeField` | Yazarak ya da takvimden tarih ve saat; Europe/Istanbul (§7.8) |
| `ConfirmDialog` | Geri alınamaz işlemin onayı; gerekiyorsa neden alanıyla (§9.4) |
| `ConflictDialog` | `412` sonrası güncel kaydı ve kullanıcının girdiği değerleri gösterir (§11.3) |
| `ReauthDialog` | Oturum süresi dolunca ekran kaybolmadan yeniden giriş (§7.7) |
| `EmptyState`, `ErrorState`, `SkeletonBlock` | Boş, hatalı ve yükleniyor durumları (§10) |
| `ConnectionIndicator`, `VersionBanner` | Bağlantı ve yeni sürüm uyarıları (§9.5, §11.4) |
| `Can` | Yetkiye göre gösterme (§12) |
| `ScanField`, `ScanFeedback`, `CameraScanner`, `QuantityStepper`, `ActionBar` | Depo ekranları (§14) |

### 6.3 Düğmeler

- **Bir bölgede tek ana düğme** (`primary`). Diğerleri ikincil ya da metin düğmesidir. Sayfanın ana işlemi sayfa başlığının sağında, diyalogun ana işlemi sağ alttadır.
- **Geri alınamaz işlem** `destructive` düğmedir ve onay ister (§9.4).
- **Etiket fiildir** ve ne olacağını söyler: "Opsiyon ekle", "Çıkışı tamamla", "Rezervasyonları onayla". "Tamam", "Gönder", "Evet" kullanılmaz (§13.3).
- **Yükleniyor hali:** Düğme basıldığı anda pasifleşir, içinde dönen bir işaret görünür, genişliği değişmez. Aynı işlem iki kez gönderilemez.
- Yetkisi olmayan kullanıcıya düğme gösterilmez; koşulu sağlanmayan işlem nedeniyle birlikte pasif gösterilir (§12).

## 7. Formlar

### 7.1 Araç

Formlar React Hook Form ve OpenAPI'den üretilen Zod şemalarıyla yazılır ([ADR-0009](../adr/0009-ui-components.md)). React Compiler ile güvenli kullanım için ([kaynak](https://github.com/orgs/react-hook-form/discussions/12524)):
- alan değerini izlemek için `watch()` değil `useWatch({ control, name })`,
- form durumunu okumak için `formState` değil `useFormState({ control })`,
- tüm girişler `Controller` ile bağlanır (shadcn/ui'ın `Field` bileşeniyle).

Bu üç kural code-style'a eklenir ve lint kuralıyla denetlenir (`watch` ve `formState` kullanımı yasak sözdizimi olarak işaretlenir).

### 7.2 Düzen

- Tek sütun; etiket alanın üstünde. İki sütun yalnızca birlikte okunan kısa alan çiftlerinde (başlangıç / bitiş).
- **Zorunlu alanlar yıldızla** işaretlenir ve formun başında "* zorunlu alan" açıklaması bulunur. Yardımcı metin etiketin altında, hata mesajı alanın altındadır.
- Uzun formlar başlıklı bölümlere ayrılır. Yan yana sekmeler içinde form yazılmaz; kullanıcı bir sekmedeki hatayı görmez.
- **Diyalog mu sayfa mı:** 8 alana kadar olan küçük kayıtlar (taraf, depo, kategori) diyalogda ya da yan panelde; büyük kayıtlar (etkinlik, rider, model) kendi sayfasında.

### 7.3 Doğrulama zamanı

- Alan **ilk kez terk edildiğinde** doğrulanır, yazarken değil. Hatalı olduktan sonra her değişiklikte yeniden doğrulanır; kullanıcı düzeltince hata hemen kalkar (React Hook Form: `mode: "onTouched"`, `reValidateMode: "onChange"`). Bu zamanlama, yazarken ya da yalnızca gönderince doğrulamaya göre daha az hata ve daha kısa süre sağlar ([kaynak](https://www.72technologies.com/blog/form-validation-ux-when-to-show-errors)).
- Gönderince tüm alanlar doğrulanır; hata varsa **ilk hatalı alana odaklanılır**. Ekran boyunu aşan formlarda üstte hata özeti de gösterilir ve her madde ilgili alana götürür.
- İstemcideki doğrulama yalnızca kolaylıktır; kural her zaman sunucuda da denetlenir ([security §9](security.md#9-girdi-doğrulama-katmanları)).

### 7.4 Sunucu hatalarının forma yansıması

| Yanıt ([api §8](api.md#8-hata-yanıtları)) | Formda |
|---|---|
| `400` doğrulama, `errors[].pointer` | İşaretçi forma çevrilir (`/units/3/serialNumber` → `units.3.serialNumber`) ve hata o alanın altında gösterilir. Karşılığı olmayan işaretçi formun üstünde gösterilir. |
| `422` iş kuralı | Formun üstünde, ana düğmenin yakınında bir uyarı kutusu: `errors:{kod}` metni. Alanlar kilitlenmez; kullanıcı düzeltip yeniden gönderir. |
| `403` | Uyarı kutusu: "Bu işlem için yetkiniz yok." Kayda bağlı yetki kuralıysa kuralın metni. |
| `412` | `ConflictDialog` (§11.3) |
| `401` | `ReauthDialog` (§7.7) |
| `5xx`, ağ hatası | Uyarı kutusu: "Kaydedilemedi. Bağlantınızı kontrol edip yeniden deneyin." ve "Yeniden dene" düğmesi. Beklenmeyen hatada iz numarası "Ayrıntılar" altında kopyalanabilir. Girilen değerler korunur. |

### 7.5 Gönderme

- Gönderim sürerken ana düğme yükleniyor halindedir; form alanları değiştirilebilir kalır ama ikinci gönderim yapılamaz.
- Her gönderim denemesinin tekrar güvenliği anahtarı (`Idempotency-Key`) istek sarmalayıcısı tarafından üretilir; ağ hatasından sonra "Yeniden dene" aynı anahtarı kullanır. İşlem sunucuda gerçekleşmiş ama yanıt kaybolmuşsa tekrar, işlemi ikinci kez yapmaz ([ADR-0025](../adr/0025-idempotency-keys.md)).
- Başarıda kısa bir bildirim gösterilir (§9.3) ve kullanıcı kaydın detayına götürülür ya da diyalog kapanır. Başarı mesajı sonucu söyler: "Etkinlik oluşturuldu".

### 7.6 Kaydedilmemiş değişiklikler

Değiştirilmiş bir formdan ayrılmaya çalışılırsa (başka sayfaya geçiş, sekmeyi kapatma, diyalogu kapatma) onay istenir: "Kaydedilmemiş değişiklikler var. Çıkarsanız kaybolacak." Düğmeler: "Değişiklikleri at", "Düzenlemeye dön". Otomatik taslak kaydı S1'de yoktur.

### 7.7 Oturumun sona ermesi

Oturum süresi (P-03, P-16) dolduktan sonra yapılan ilk istek `401` döner. Kullanıcı giriş sayfasına **gönderilmez**; ekranın üstünde bir diyalogda yeniden giriş yapar. Diyalog kapanınca ekran, form içeriği ve okutma listesi olduğu gibi durur; kullanıcı işlemi yeniden gönderir. Yeniden giriş normal girişle aynı kurallara tabidir (hesap kilidi, geçici şifre). Farklı bir kullanıcıyla giriş yapılırsa sayfa baştan yüklenir ve eski kullanıcının girdileri atılır.

### 7.8 Sayı, tutar, tarih ve süre girişleri

| Giriş | Kural |
|---|---|
| Adet | Tam sayı; binlik ayıracı gösterilir (`1.250`). Klavyede rakam tuş takımı açılır (`inputmode="numeric"`). |
| Ondalıklı sayı, tutar | Ondalık ayıracı **virgül**, binlik ayıracı nokta (`1.234,56`). Kullanıcı binlik ayıracı yazmadan da girebilir (`1234,5`). Alan terk edilince biçimlenir. Sunucuya ondalık metin olarak gider (`"1234.50"`, [api §5](api.md#5-json-gövdesi)). Base UI'ın sayı alanı (`NumberField`, `locale="tr-TR"`) kullanılır; Türkçe girişin doğru ayrıştırıldığı bileşen testleriyle kanıtlanır. |
| Tutar | Para birimi alanın içinde sabit gösterilir (`₺`); S4'e kadar yalnızca TRY. |
| Tarih | Yazılarak (`gg.aa.yyyy`) ya da takvimden. Takvim Türkçe, hafta pazartesi başlar. |
| Saat | 24 saat (`14:30`); yazılarak ya da 15 dakikalık adımlarla listeden. |
| Tarih-saat aralığı | Başlangıç ve bitiş ayrı alanlar; bitiş başlangıçtan önce olamaz; gece yarısını geçen etkinlikte bitiş ertesi günü gösterir. Tüm saatler Europe/Istanbul ([00 §9](../00-scope.md#9-fonksiyonel-olmayan-varsayımlar)); tarayıcının saat dilimi kullanılmaz. |
| Süre | Saat ve dakika ayrı alanlar; sunucuya dakika olarak gider (hazırlık payı gibi, [api §5](api.md#5-json-gövdesi)). |
| Telefon | Uluslararası biçim, Türkiye varsayılan (`+90 5xx xxx xx xx`). |

### 7.9 Seçimler

| Liste | Bileşen |
|---|---|
| 7 seçeneğe kadar, sabit | Seçenek düğmeleri (radio) ya da düğme grubu |
| Sabit, uzun (ör. etkinlik türü) | Açılır liste (`Select`) |
| Veritabanından, çok sayıda (taraf, mekan, model, depo, etkinlik) | `EntityCombobox`: yazdıkça sunucuda arar (`q`, [api §6.3](api.md#63-filtre-ve-arama)); 250 ms bekleme; ilk 20 sonuç; Türkçe karakter ve büyük / küçük harf duyarsız. |

- **Pasif kayıtlar** seçim listelerinde görünmez; ama bir kayıtta zaten seçiliyse "Pasif" rozetiyle görünmeye devam eder (BR-SYS-001).
- Sık akışlarda listeden yeni kayıt açılabilir ("+ Yeni taraf"): küçük bir diyalog açılır, kayıt oluşunca seçilmiş olarak döner.

### 7.10 Toplu giriş

Seri numarası listesi gibi toplu girişlerde (US-EQP-003) metin kutusuna yapıştırılan satırlar önce **önizleme tablosunda** gösterilir: geçerli, mükerrer ve hatalı satırlar ayrı işaretlenir. Kaydet yalnızca önizlemeden sonra etkinleşir; sunucunun satır bazlı hataları ([api §8.2](api.md#82-doğrulama-hataları)) aynı tabloda gösterilir.

### 7.11 Klavye

- Diyalog açılınca ilk alana odaklanılır. `Enter` tek satırlık alanlarda formu gönderir; çok satırlı alanda `Ctrl + Enter`.
- Sekme sırası görsel sırayla aynıdır.
- Şifre alanlarına yapıştırmaya ve parola yöneticilerine izin verilir (WCAG 3.3.8); şifre alanında "göster" düğmesi vardır.

## 8. Tablolar ve listeler

| Konu | Kural |
|---|---|
| Veri | Sayfalama, sıralama ve filtre sunucudadır ([api §6](api.md#6-listeler)); tablo tüm veriyi indirmez. Durum adreste tutulur (§5.1). |
| Sayfa | Sayfa numaralı; varsayılan 25 satır, seçenekler 25 / 50 / 100; toplam kayıt sayısı gösterilir. Analiz ve karşılaştırma yapılan tablolarda sayfalama, sonsuz kaydırmadan daha kullanışlıdır ([kaynak](https://www.pencilandpaper.io/articles/ux-pattern-analysis-enterprise-data-tables)). |
| Büyüyen kayıtlar | İşlem geçmişi ve stok hareketleri imleçli listedir; "Daha fazla yükle" düğmesiyle. Kaydırınca kendiliğinden yükleme yapılmaz; kullanıcı konumunu kaybetmez ve sayfanın altına ulaşabilir. |
| Başlık | Yapışkan; tablo kaydırılırken sütun adları görünür kalır. Geniş tablolarda ilk sütun (kayıt adı) da sabitlenir. |
| Hizalama | Metin sola, sayı ve tutar sağa (tablo rakamlarıyla), durum ve simge ortaya. Tarih sütunları aynı biçimde (§13.5). |
| Satır | Kaydın adı detay sayfasına bağlantıdır; satırın tamamına tıklamak da detayı açar. Satırın işlemleri satır sonundaki "⋯" menüsündedir. |
| Toplu işlem | Yalnızca toplu işlemi olan tablolarda seçim kutuları vardır (ör. rezervasyon önerilerini toplu onaylama, etiket yazdırma). Seçim yapılınca tablonun üstünde seçili sayısı ve işlemler görünür. |
| Filtreler | Arama kutusu ve en sık 2–3 filtre görünür; diğerleri "Filtreler" panelinde. Etkin filtreler etiket olarak gösterilir, tek tıkla ya da "Tümünü temizle" ile kaldırılır. |
| Sütunlar ve yoğunluk | Kullanıcı sütunları gizleyebilir ve sıkı görünüme geçebilir; tercih tarayıcıda kullanıcı ve tablo başına saklanır. |
| Uzun metin | Tek satıra sığdırılır, sonu "…" ile kesilir; tamamı üzerine gelince ve detayda görünür. |
| Toplamlar | Anlamlı olduğunda (adetler, tutarlar) tablonun altında toplam satırı. |
| Uzun sayfalamasız listeler | Ekranda 200 satırdan fazla gösterilebilen sayfalamasız listelerde sanal kaydırma (TanStack Virtual). |

## 9. Geri bildirim

### 9.1 Hangi durumda hangi araç

| Araç | Ne zaman | Örnek |
|---|---|---|
| Alan hatası | Tek bir alandaki sorun | "Seri numarası bu modelde zaten var." |
| Sayfa içi uyarı kutusu | Bir bölümü ya da formu ilgilendiren, işi durduran sorun ya da önemli bilgi | İş kuralı ihlali, "İhtiyaç hesabı güncel değil" |
| Kısa bildirim (toast) | İşi durdurmayan, geçici sonuç bilgisi | "Etkinlik onaylandı" |
| Şerit (banner) | Tüm uygulamayı ilgilendiren, süren durum | Yeni sürüm, bağlantı yok |
| Diyalog | Kullanıcının karar vermesi gereken ya da geri alınamaz durum | Etkinliği iptal etme, sürüm çakışması, yeniden giriş |

İşi durduran bir hata **kısa bildirimle gösterilmez**; kısa bildirim kaybolur ve kullanıcı ne olduğunu kaçırabilir ([kaynak](https://www.nngroup.com/articles/indicators-validations-notifications/)).

### 9.2 Hata mesajları

- Ne olduğunu, biliniyorsa nedenini ve kullanıcının ne yapabileceğini söyler; kullanıcıyı suçlamaz, teknik terim içermez ([kaynak](https://www.nngroup.com/articles/error-message-guidelines/)).
- İş kuralı mesajları kural koduyla çeviri dosyasından gelir (`errors:BR-…`, [naming §7.1](naming.md#71-çeviri-anahtarları)); sunucunun gönderdiği parametrelerle doldurulur.
- Beklenmeyen hatalarda kullanıcı genel bir mesaj görür; "Ayrıntılar" altında iz numarası (`traceId`) kopyalanabilir. Hata bildirimi iz numarasıyla yapılır ([git §6](git.md#6-görev-takibi)).

### 9.3 Kısa bildirimler

- [Sonner](https://sonner.emilkowal.ski/) (MIT; shadcn/ui'ın bildirim bileşeni).
- Masaüstünde sağ altta, telefonda üstte (alt kısım işlem çubuğuna ayrılmıştır). Aynı anda en fazla 3.
- Başarı bildirimi 4 saniye görünür; üzerine gelinince süre durur. Bildirim ekran okuyuculara nazikçe duyurulur (`aria-live="polite"`).
- Okutma sonuçları kısa bildirim değildir; kendi geri bildirim alanları vardır (§14.5).

### 9.4 Onay diyalogları

- **Yalnızca geri alınamaz ya da başkalarını etkileyen** işlemler onay ister: etkinliği iptal etme, rezervasyonu serbest bırakma, birimi kayıp işaretleme, kullanıcıyı pasifleştirme. Geri alınabilir, sık yapılan işlemler onay istemez; gereksiz onay, kullanıcının onayları okumadan geçmesine yol açar.
- Başlık işlemi ve nesneyi adlandırır: "Yaz Festivali 2027 iptal edilsin mi?". Metin sonucu söyler: "Onaylı 42 rezervasyon serbest bırakılacak; bu geri alınamaz."
- Ana düğme işlemin adıdır ("Etkinliği iptal et"), `destructive` renktedir; diğer düğme "Vazgeç". Kuralın gerektirdiği neden alanı diyalogun içindedir.
- Diyalog açıldığında odak "Vazgeç"tedir; `Enter` yanlışlıkla geri alınamaz işlemi başlatmaz.

### 9.5 Şeritler

| Şerit | Görünüm | Davranış |
|---|---|---|
| Yeni sürüm hazır | `info` | Kapatılamaz; "Yenile" düğmesi. Sayfa zorla yenilenmez ([api §12](api.md#12-sürümleme-ve-uyumluluk)). |
| Bağlantı yeniden kuruluyor | `warning` | Kendiliğinden kalkar (§11.4). |
| Bağlantı yok | `danger` | Bağlantı gelince kalkar; veri yeniden okunur. |

Şeritler kabuğun en üstünde üst üste dizilir ve sayfanın içeriğini kaydırır; hiçbir öğenin üstünü örtmez (WCAG 2.4.11).

## 10. Yükleniyor, boş ve hata durumları

### 10.1 Yükleniyor

| Bekleme | Gösterilen |
|---|---|
| 300 ms'den kısa | Hiçbir şey; kısa beklemelerde yanıp sönen işaret rahatsız eder. |
| Sayfa ya da bölüm içeriği | İçeriğin şeklinde iskelet (skeleton). İskelet, dönen işarete göre daha hızlı algılanır ([kaynak](https://www.nngroup.com/articles/skeleton-screens/)). |
| Düğmeyle başlatılan işlem | Düğmenin içinde dönen işaret (§6.3) |
| 1 saniyeden uzun işlem (PDF üretimi, ihtiyaç hesabı) | Ne yapıldığını söyleyen metin: "Karşılama raporu hazırlanıyor…" |
| 10 saniyeden uzun işlem | İlerleme göstergesi ve "Vazgeç" düğmesi. S1'de beklenmez; olursa bu kuralla tasarlanır. |

- Veri yeniden okunurken (anlık güncelleme, filtre değişikliği) eski veri ekranda kalır ve yalnızca ince bir ilerleme çizgisi görünür; tablo boşalıp dolmaz, kaydırma konumu korunur.
- Yerleşim kayması yoktur: iskelet, gelecek içerikle aynı yüksekliktedir.

### 10.2 Boş durumlar

| Durum | Gösterilen |
|---|---|
| Henüz hiç kayıt yok | Neyin burada görüneceğinin kısa açıklaması ve yetkisi varsa ilk kaydı oluşturma düğmesi |
| Filtreye uyan kayıt yok | "Bu filtrelere uyan kayıt yok." ve "Filtreleri temizle" |
| Arama sonucu yok | "“{arama}” için sonuç yok." ve yazım ipucu |
| Tamamlanmış iş listesi | Olumlu durum: "Bu etkinliğin tüm çıkışları tamamlandı." |

### 10.3 Hata durumları

| Durum | Gösterilen |
|---|---|
| Bir bölüm yüklenemedi | O bölümde `ErrorState`: kısa açıklama, "Yeniden dene", ayrıntıda iz numarası. Sayfanın diğer bölümleri çalışmaya devam eder. |
| Sayfa yüklenemedi | Rota hata bileşeni ([code-style §5.4](code-style.md#54-react-kuralları)): aynı içerik tüm sayfada; kabuk ve menü çalışır. |
| Kayıt bulunamadı (`404`) | "Bu kayıt bulunamadı. Silinmiş ya da adres yanlış olabilir." ve listeye dönüş. Pasif kayıtlar bulunamadı değildir; "Pasif" rozetiyle açılır. |
| Yetki yok (`403`) | "Bu sayfayı görme yetkiniz yok." Menüden bu sayfaya zaten ulaşılamaz; bu durum eski bir bağlantı ya da rol değişikliğinden sonra görülür. |
| Uygulama çöktü | En dıştaki hata sınırı: "Beklenmeyen bir hata oluştu." ve "Sayfayı yenile". Hata telemetriye iz numarasıyla yazılır. |

## 11. Anlık güncelleme ve eşzamanlı düzenleme

### 11.1 Değişikliklerin ekrana gelmesi

- `resourceChanged` bildirimi ilgili sorguları geçersiz kılar ve veri yeniden okunur; ekrandaki kayıt zaten o sürümdeyse okunmaz ([api §13](api.md#13-anlık-bildirimler)).
- Başka biri tarafından değiştirilen satır ya da alan 1,5 saniye hafif bir arka plan rengiyle belirtilir ("hareketi azalt" açıksa renk hemen kaybolur). Kullanıcı neyin değiştiğini fark eder; dikkati dağılmaz.
- Değişiklik ekranın yapısını bozmaz: seçimler, açık menüler, kaydırma konumu ve sekmeler korunur.
- İşlemi yapan kullanıcının kendi ekranı bildirimi beklemez; komutun yanıtıyla hemen güncellenir ([05 §9.2](../05-module-map.md#92-olayların-teslimi)).

### 11.2 Düzenlerken kaydın değişmesi

Kullanıcı bir formu düzenlerken aynı kayıt başka biri tarafından değiştirilirse formun içeriği **ezilmez**. Formun üstünde bir uyarı görünür: "Bu kayıt siz düzenlerken değiştirildi." ve "Güncel hali göster" düğmesi. Kullanıcı kaydetmeye devam ederse sunucu `412` döner (§11.3).

### 11.3 Sürüm çakışması

`412` yanıtında `ConflictDialog` açılır: "Bu kayıt siz düzenlerken başka biri tarafından değiştirildi."
- Form, kaydın güncel haliyle yeniden doldurulur.
- Kullanıcının değiştirdiği her alanın altında kendi girdiği değer görünür ("Sizin girdiğiniz: …") ve tek tıkla yeniden uygulanabilir.
- Kullanıcı karşılaştırıp yeniden kaydeder. Hiçbir değer sessizce ezilmez (BR-SYS-011), kullanıcının emeği de kaybolmaz.

Durum geçişi gibi gövdesiz işlemlerde `412`, işlemin artık geçerli olmayabileceği anlamına gelir: kayıt yeniden okunur, "Kayıt siz bakarken değişti; güncel durumu kontrol edip yeniden deneyin." yazılır ve güncel geçerli işlemler gösterilir.

### 11.4 Bağlantı göstergesi

| Durum | Gösterilen |
|---|---|
| Bağlı | Hiçbir şey |
| Yeniden bağlanıyor | 2 saniyeden uzun sürerse `warning` şeridi: "Bağlantı yeniden kuruluyor…" |
| Bağlantı yok | `danger` şeridi: "Bağlantı yok. Ekrandaki bilgiler güncel olmayabilir." |
| Bağlantı geri geldi | Etkin tüm sorgular yeniden okunur; kısa bir bildirim: "Bağlantı yeniden kuruldu." |

Bu, hem SignalR bağlantısını hem tarayıcının çevrimdışı durumunu (`navigator.onLine`) kapsar (US-WHS-005).

## 12. Yetkiye göre arayüz

- Kullanıcının yetkileri oturum bilgisiyle gelir; arayüz yetki koduna göre karar verir (`<Can permission="Booking.Events.Confirm">`), role göre değil ([ADR-0026](../adr/0026-authorization-model.md)).
- **Yetki yoksa gösterilmez:** menü öğesi, sayfa ve işlem düğmesi (US-SYS-012).
- **Yetki var ama koşul sağlanmıyorsa** işlem pasif gösterilir ve nedeni görünür bir yardım metniyle yazılır (yalnızca üzerine gelince açılan ipucuyla değil; dokunmatik ekranda ipucu yoktur). Örnek: "Onayla — mekan için kesinleşmiş opsiyon yok." ([04 §2](../04-state-machines.md#2-genel-kurallar), madde 4).
- **Durum geçişleri:** Detay sayfası yalnızca o durumdan mümkün olan geçişleri düğme olarak sunar (US-EVT-007).
- **Genel müdür** her şeyi görür, hiçbir değiştirme düğmesi görmez (BR-SYS-004). Kullanıcı menüsünde "Salt okunur erişim" yazar.
- **Depo kapsamı:** Depo sorumlusunun okutma ekranlarında yalnızca bağlı olduğu depolar seçilebilir; tek deposu varsa seçim sorulmaz (BR-SYS-003).
- Arayüzdeki gizleme yalnızca kolaylıktır; her kontrol sunucuda yapılır (BR-SYS-002).

## 13. Metinler ve Türkçe

### 13.1 Ses ve hitap

- Kısa, açık, resmî ama soğuk değil. Kullanıcıya hitap gerekirse **"siz"** kullanılır: "Bu işlem için yetkiniz yok."
- Sonuç bildirimlerinde özne gerekmez: "Kaydedildi", "Etkinlik onaylandı".
- Terimler [sözlükteki](../01-glossary.md) Türkçe adlardır: opsiyon, rider, kasa, toplama listesi, hazırlık payı. Aynı kavram için eş anlamlı kullanılmaz.

### 13.2 Büyük / küçük harf

- Tüm etiketler, düğmeler, başlıklar ve menüler **cümle düzenindedir**: yalnızca ilk kelimenin ilk harfi büyük. "Yeni etkinlik", "Toplama listesi", "Çıkışı tamamla". ("Yeni Etkinlik" değil.)
- Özel adlar (kişi, mekan, marka) ve kısaltmalar (QR, PDF) olduğu gibi yazılır.

### 13.3 Düğme ve işlem adları

- Fiille ve emir kipiyle: "Kaydet", "Opsiyon ekle", "Rezervasyonları onayla", "Etiket yazdır".
- Onay diyaloglarında "Evet / Hayır" değil, işlemin adı ve "Vazgeç" (§9.4).
- Aynı işlem her yerde aynı adla: kayıt oluşturma "… ekle" ya da "Yeni …" tek biçimde (liste sayfasında "Yeni etkinlik", alt kayıtlarda "Opsiyon ekle").

### 13.4 Türkçeye özgü kurallar

| Kural | Neden |
|---|---|
| Sayfanın dili `tr` olarak işaretlenir (`<html lang="tr">`). | Tarayıcının büyük harf dönüşümü (`text-transform`), heceleme ve ekran okuyucunun telaffuzu dile göre çalışır; "i" ancak bu işaretle "İ"ye dönüşür ([kaynak](https://developer.mozilla.org/en-US/docs/Web/CSS/text-transform)). |
| **Değişken bir değere ek getirilmez.** "Ayşe Yılmaz'ın", "17:03'te", "Ankara'ya" gibi yapılar çeviri metninde kurulmaz. Yerine ek gerektirmeyen yapı: "Çıkışı yapan: Ayşe Yılmaz · Saat: 17:03", "Hedef depo: Ankara". | Türkçe ekler ünlü uyumuna ve kelimenin okunuşuna göre değişir; değişken bir değer için doğru eki çeviri dosyası bilemez. Yanlış ek ("Ankara'ye") profesyonel görünümü bozar. |
| Sayıdan sonra isim tekil kalır: "3 etkinlik" ("3 etkinlikler" değil). Çeviri anahtarları yine de sayı parametresi alır. | Türkçe dilbilgisi; i18next'in çoğul biçimleri Türkçe için aynı metni kullanır. |
| Metin alanları **%30 daha uzun** Türkçe metinlere göre tasarlanır; düğme ve sütun genişlikleri sabit metne göre ayarlanmaz. | Türkçe kelimeler ve eklerle birlikte cümleler uzundur; kesilen düğme metni anlamı bozar. |
| Arama, sıralama ve büyük / küçük harf dönüşümü `tr` yerel ayarıyla yapılır. | [code-style §5.5](code-style.md#55-türkçe-metin) |

### 13.5 Biçimler

Tüm biçimler tek bir yardımcı modülden gelir (`lib/format`); bileşenlerde biçim yazılmaz.

| Tür | Biçim | Örnek |
|---|---|---|
| Tarih | `gg.aa.yyyy` | `12.06.2027` |
| Tarih (etkinlik planlaması) | Tarih ve kısa gün adı | `12.06.2027 Cmt` |
| Saat | 24 saat | `14:30` |
| Tarih-saat | | `12.06.2027 14:30` |
| Aynı gün aralığı | | `12.06.2027 14:00–23:30` |
| Günler arası aralık | | `12.06.2027 14:00 – 13.06.2027 02:00` |
| Göreli zaman | Son 7 gün için, tam zaman üzerine gelince ve ekran okuyucuda | `5 dk önce` |
| Para | Sembol önde, iki ondalık | `₺1.234,56` |
| Eksi tutar | Gerçek eksi işareti | `−₺1.234,56` |
| Adet | Binlik ayıracıyla | `1.250 adet` |
| Yüzde | İşaret önde (Türkçe kuralı) | `%15` |
| Süre | | `2 sa 30 dk` |
| Ağırlık, güç | Birimle | `12,5 kg`, `1.200 W` |

- Tarih ve sayı biçimleri `Intl` ve date-fns'in `tr` yerel ayarıyla üretilir; saat dilimi her zaman Europe/Istanbul'dur ([code-style §5.5](code-style.md#55-türkçe-metin)). Tarayıcının dilinden ve saat diliminden bağımsızdır.
- Yurt dışı mekan (S5) geldiğinde mekanın yerel saati, saat dilimi kısaltmasıyla gösterilir.

## 14. Depo ekranları

### 14.1 Cihazlar ve okutma yöntemleri

| Cihaz | Okutma | Not |
|---|---|---|
| Android telefon, Chrome | Kamera; tarayıcının yerleşik barkod okuyucusu (`BarcodeDetector`) | Birincil cihaz |
| iPhone, Safari | Kamera; yedek kütüphane (WebAssembly) ([ADR-0014](../adr/0014-documents-and-qr.md)) | Safari yerleşik okuyucuyu Haziran 2026 itibarıyla desteklemiyor ([kaynak](https://caniuse.com/mdn-api_barcodedetector)); yedek kütüphane yalnızca gerektiğinde yüklenir. |
| Tümleşik okuyuculu dayanıklı Android cihaz (ör. Zebra) | Donanım okuyucu, klavye girdisi olarak | Cihazın okuyucusu okunan kodu tuş vuruşu olarak gönderir ve sonuna `Enter` ekler ([kaynak](https://techdocs.zebra.com/datawedge/11-1/guide/about/)). Uygulamada ek bir entegrasyon gerekmez. |
| Bluetooth ya da USB el okuyucu | Donanım okuyucu, klavye girdisi olarak | Aynı |
| Her cihaz | Elle etiket kodu girişi | Kamera ve okuyucu yoksa ya da etiket hasarlıysa (US-WHS-002) |

### 14.2 Yerleşim

- **Üstte** okutma alanı ve kamera düğmesi; **ortada** liste; **altta**, başparmağın ulaştığı bölgede sabit işlem çubuğu ("Tamamla" gibi).
- Ekranda tek bir iş vardır. Başka bir işe geçmek için geri dönülür.
- Üzerine gelme (hover) ve ipucu (tooltip) yoktur; her bilgi ekranda yazılıdır.
- Telefonun güvenli alanlarına uyulur (çentik ve alt çubuk, `env(safe-area-inset-*)`).
- Uygulama ana ekrana eklenebilir (web uygulaması bildirimi, tam ekran): tarayıcı çubukları kalkar, okutma için daha çok alan kalır.

### 14.3 Boyutlar ve okunurluk

| Öğe | En küçük | Neden |
|---|---|---|
| Her dokunma hedefi | **56 × 56 px** (yaklaşık 9 mm), aralarında en az 8 px | Genel mobil önerilerin (44–48 px) üstünde; eldivenle yanlış dokunmayı azaltır. |
| Ana işlemler ve +/− düğmeleri | **72 × 72 px** (yaklaşık 11,5 mm) | Eldivenle kullanımda kabul edilebilir en küçük boyut 11,43 mm'dir ([kaynak](https://www.w3.org/WAI/GL/mobile-a11y-tf/wiki/Summary_of_Research_on_Touch/Pointer_Target_Size)). |
| Metin | 16 px; model adı 18 px; adetler 24–30 px | Kol mesafesinden ve hareket halinde okunur. |
| Kontrast | Temel bilgi yardımcı gri tonla yazılmaz; 7:1 hedeflenir | Depo ve rampa aydınlığı değişkendir; soluk metin okunmaz. |

### 14.4 Okutma alanı

- Okutma ekranında okutma alanı her zaman odaktadır; bir işlemden sonra odak kendiliğinden geri döner. Donanım okuyucu doğrudan bu alana yazar.
- Donanım okuyucu kipinde alan ekran klavyesini açmaz (`inputmode="none"`); elle giriş düğmesi klavyeyi açar.
- `Enter` okutmayı tamamlar. Etiket kodunun biçimi önce istemcide denetlenir; biçime uymayan kod sunucuya gitmeden "Geçersiz etiket" olarak gösterilir.
- Aynı kod 1,5 saniye içinde ikinci kez okunursa yok sayılır (kameranın aynı etiketi art arda okuması).
- **Kamera:** Okutma boyunca açık kalan tam genişlikte önizleme; arka kamera; fener düğmesi (destekleyen cihazlarda); her okumadan sonra kısa bir duraklama. Kamera izni reddedilirse izni nasıl açacağını anlatan bir metin ve elle giriş seçeneği gösterilir.
- Okutma süresince ekranın kararması engellenir (Screen Wake Lock; tüm güncel tarayıcılarda destekleniyor, [kaynak](https://web.dev/blog/screen-wake-lock-supported-in-all-browsers)); ekrandan çıkınca bırakılır.

### 14.5 Okutma geri bildirimi

Geri bildirim, kod okunduktan sonra **200 ms içinde** başlar ([kaynak](https://medium.com/@stefan.karabin/7-ux-design-best-practices-for-warehouse-mobile-apps-b6e2a0a6940f)): önce "işleniyor" görünümü, sunucu yanıtıyla sonuç.

| Sonuç | Görsel | Ses | Titreşim | Kapanma |
|---|---|---|---|---|
| Başarılı | Yeşil çerçeve; listenin en üstüne eklenen satır ("✓ Shure SM58 · E-10234") | Kısa, tiz tek ton | 50 ms | Kendiliğinden |
| Hafif uyarı (zaten okutuldu, listede var ama adet dolu) | Sarı satır | Çift kısa ton | — | Bir sonraki okutmayla |
| Engelleyen hata (iş kuralı: başka depoda, zaten çıkışı yapılmış, listede olmayan model) | Kırmızı, tüm genişlikte kart; kuralın metni ve parametreleri ("Çıkışı yapan: Ayşe Yılmaz · Saat: 17:03") | Alçak, uzun ton | 3 kısa titreşim | Yalnızca "Anladım" düğmesiyle; bu sırada yeni okutma alınmaz |
| Onay gereken durum (listede olmayan model, US-WHS-002 madde 5) | Onay kartı | Çift ton | — | Kullanıcının seçimiyle |

- Sesler tarayıcıda üretilir (Web Audio); dosya indirilmez. Cihaz başına kapatılabilir; varsayılan açıktır. iOS'ta ses, kullanıcı ekrana ilk kez dokunduktan sonra çalabildiği için okutma ekranı "Okutmaya başla" dokunuşuyla açılır.
- Titreşim iOS Safari'de desteklenmez ([kaynak](https://www.lambdatest.com/web-technologies/vibration-safari)); bu yüzden asıl geri bildirim görsel ve seslidir, titreşim ektir.
- Sonuç ekran okuyucuya da duyurulur (`aria-live="assertive"` yalnızca engelleyen hatalarda).

### 14.6 Adetli kalemler

- Beklenen adet hazır gelir: çıkışta toplama listesindeki, dönüşte çıkan adet ([00 §7.7.1](../00-scope.md#771-adetli-kalem-yönetimi-ilkeleri)).
- Büyük − ve + düğmeleri (72 px); sayıya dokunulursa rakam klavyesiyle yazılır.
- Beklenenden farklı adet belirgin gösterilir ("18 / 20 — 2 eksik"). Dönüşte fark varsa neden seçimi istenir (sayım farkı, BR-WHS-008).
- Kasa okutulunca içeriği tek satır olarak eklenir; satıra dokunulunca içerik açılır.

### 14.7 İlerleme ve tamamlama

- Ekranın üstünde ilerleme: "34 / 52 kalem". Liste kalanlar ve tamamlananlar olarak iki bölümdür; kalanlar üstte.
- Başka cihazdan yapılan okutmalar da listeye anlık gelir (US-WHS-005).
- "Tamamla" her zaman kullanılabilir; eksik varsa özet gösterilir ve onay istenir (US-WHS-002 madde 8).
- Kısmi bırakılan iş kaybolmaz; ekrana dönüldüğünde kaldığı yerden devam eder.

### 14.8 Bağlantı kesildiğinde

**Okutma çevrimiçi yapılır** ([U-01](#19-kararlar)). "Aynı birim iki kez çıkamaz" kuralı (BR-WHS-006) her okutmada anında sunucuda denetlenir.
- Bağlantı durumu okutma ekranında her zaman görünür.
- Yanıtı gelmemiş okutma "gönderiliyor" olarak işaretli kalır ve bağlantı gelince aynı tekrar güvenliği anahtarıyla kendiliğinden yeniden gönderilir; sunucu aynı okutmayı iki kez işlemez ([ADR-0025](../adr/0025-idempotency-keys.md)). 2 dakika içinde gönderilemezse "Gönderilemedi" olarak işaretlenir ve "Yeniden gönder" düğmesi çıkar.
- **Bağlantı yokken yeni okutma alınmaz.** Okutma alanı "Bağlantı yok — okutma alınmıyor" yazar. Donanım okuyucular okumaya devam edebildiği için bağlantı yokken gelen her okutma engelleyen hata sesiyle ve "Okutma alınmadı, bağlantı yok" uyarısıyla karşılanır. Çalışan hangi kalemi yeniden okutacağını böylece bilir.
- Bağlantı gelince liste sunucudan yeniden okunur (başka cihazlardaki okutmalar dahil) ve okutma kaldığı yerden sürer.
- Telefonun mobil verisi, depo Wi-Fi'si için doğal bir yedektir.
- Çevrimdışı okutma kuyruğu ileride gerekirse sunucu değişmeden eklenebilir; tekrar güvenliği anahtarları bunu zaten destekler.

### 14.9 Oturum

Oturum bir vardiya boyunca sürer (P-03: 12 saat hareketsizlik). Süresi dolarsa yeniden giriş diyaloğu açılır, okutma listesi ekranda kalır (§7.7).

## 15. Erişilebilirlik

**Hedef:** Tasarım ve bileşenler **WCAG 2.2 AA** kriterlerine göre yapılır; resmî bir uygunluk denetimi hedeflenmez ([00 §9](../00-scope.md#9-fonksiyonel-olmayan-varsayımlar)). Uygulama şirket içi bir araçtır; Avrupa Erişilebilirlik Yasası tüketiciye açık hizmetleri kapsadığı için kapsam dışıdır ([kaynak](https://www.devlume.com/insights/european-accessibility-act-b2b-saas)), ama iyi bir iş uygulaması bu kriterleri zaten karşılar.

| Kriter | Uygulama |
|---|---|
| Klavye (2.1.1) | Her işlem klavyeyle yapılabilir; sürükle-bırak gerektiren işlemin düğmeyle alternatifi vardır (2.5.7; S2'deki takvim için de geçerli). |
| Odak görünür (2.4.7), örtülmez (2.4.11) | Tüm odaklanabilir öğelerde belirgin odak halkası (`ring`, 2 px, 3:1 kontrast). Yapışkan başlık ve şeritler odaklanan öğenin üstünü örtmez. |
| Kontrast (1.4.3, 1.4.11) | §3.2 |
| Renk tek başına değil (1.4.1) | Durumlar ton + simge + ad (§3.3); hatalı alan kırmızı kenar + simge + metin. |
| Dokunma hedefi (2.5.8) | Ofiste en az 24 px, uygulamada 36 px; depoda 56 px (§3.5, §14.3). |
| Etiketler (3.3.2) ve hatalar (3.3.1, 3.3.3) | Her alanın görünür etiketi; hata alanla ilişkilendirilir (`aria-describedby`) ve nasıl düzeltileceğini söyler. |
| Durum mesajları (4.1.3) | Kısa bildirimler, okutma sonuçları ve "kaydedildi" mesajları ekran okuyucuya duyurulur (`aria-live`). |
| Tekrar girişi (3.3.7) | Aynı süreçte daha önce girilen bilgi yeniden istenmez; önceki adımdan doldurulur. |
| Erişilebilir kimlik doğrulama (3.3.8) | Şifre alanına yapıştırma ve parola yöneticisi serbest; bilmece ya da hesaplama istenmez. |
| Sayfa yapısı | Sayfa başına tek `h1`; başlıklar sırayla; "İçeriğe geç" bağlantısı; sayfa değişince odak sayfa başlığına gider ve yeni sayfa ekran okuyucuya duyurulur. |
| Diyaloglar | Odak diyalogun içinde kalır; kapanınca açan öğeye döner (Base UI bunu sağlar). |
| Hareket | "Hareketi azalt" tercihine uyulur (§3.6). |

**Bilinçli sapma:** Ofis ekranları 320 px genişliğe sığma kriterini (1.4.10, yeniden akış) karşılamaz; veri yoğun tablolar için en küçük genişlik 1024 px'tir (§4). Depo ekranları bu kriteri karşılar.

**Denetim:** Tasarım sistemi bileşenlerinin her örneği otomatik erişilebilirlik testinden geçer (§18); uçtan uca testlerde ana ekranlar axe ile taranır ([testing §9.2](testing.md#92-erişilebilirlik)); her hikayede klavyeyle bir kez baştan sona denenir ([definition-of-done §4](definition-of-done.md#4-kullanıcı-hikayesi-için)).

## 16. Performans

| Ölçüt | Hedef | Nasıl |
|---|---|---|
| En büyük içeriğin görünmesi (LCP) | Orta seviye Android telefonda, 4G'de 2,5 sn'nin altı | Google'ın "iyi" eşiği ([kaynak](https://www.corewebvitals.io/core-web-vitals)) |
| Etkileşime yanıt (INP) | 200 ms'nin altı | Aynı kaynak; okutma geri bildirimi hedefiyle (§14.5) uyumlu |
| Yerleşim kayması (CLS) | 0,1'in altı | İskeletler gelecek içerikle aynı boyutta (§10.1) |
| İlk açılış JavaScript'i (sıkıştırılmış) | Kabuk + ilk ekran 250 KB'ın altı | Rota bazlı kod bölme; ağır parçalar (kamera yedek kütüphanesi, takvim, grafik) yalnızca gerektiğinde yüklenir |
| Yazı tipi | Tek değişken dosya, Latin alt kümesi, önceden yükleme | §3.4 |

- Paket boyutu CI'da [size-limit](https://github.com/ai/size-limit) (MIT) ile denetlenir; sınır aşılırsa `frontend` işi başarısız olur ([ci §4](ci.md#4-pr-hattı)).
- React Compiler gereksiz yeniden çizimleri azaltır ([code-style §5.4](code-style.md#54-react-kuralları)); yine de anlık güncellenen listelerde yalnızca değişen satırın yeniden çizildiği ölçülür.

## 17. Belgeler, yazdırma ve dosyalar

- Yazdırılacak her şey (toplama listesi, etiketler, karşılama raporu) sunucuda PDF olarak üretilir ([ADR-0014](../adr/0014-documents-and-qr.md)). Ekranların tarayıcıdan yazdırılması için ayrı bir düzen yapılmaz.
- PDF düğmesi yeni sekmede açar; üretim sürerken düğme yükleniyor halindedir (§10.1). Dosya adı içeriği söyler: `toplama-listesi-yaz-festivali-2027-ist-01.pdf`.

## 18. Tasarım sisteminin belgelenmesi ve testi

- **Katalog:** [Storybook](https://storybook.js.org/) (MIT), yalnızca `components/ui/` ve `components/common/` için. Her bileşenin tüm durumları (boyut, ton, yükleniyor, hata, pasif, uzun Türkçe metin) birer örnek (story) olarak yazılır. Tasarım değişkenleri, durum tonları ve simge eşlemesi de katalogda gösterilir.
- **Test:** Örnekler Vitest'in tarayıcı kipinde (Chromium) test olarak çalışır; her örnek otomatik erişilebilirlik taramasından geçer ([kaynak](https://storybook.js.org/docs/writing-tests/integrations/vitest-addon)). Modül ekranlarının davranış testleri [testing §7](testing.md#7-ön-yüz-testleri)'deki gibi kalır.
- **Kontrast testi:** Tasarım değişkenlerindeki her yazı / zemin çifti, bir birim testiyle §3.2'deki oranlara göre ölçülür; tema değişikliği kontrastı bozarsa test düşer.
- Katalog repoda derlenir; repo açıldıktan sonra GitHub Pages'te yayımlanabilir.

## 19. Kararlar

| No | Konu | Karar | Gerekçe |
|---|---|---|---|
| U-01 | Depoda bağlantı kesildiğinde | Çevrimiçi çalışma; yanıtı gelmemiş okutma aynı anahtarla kendiliğinden yeniden gönderilir; bağlantı yokken yeni okutma alınmaz ve her okutma uyarıyla karşılanır | Çift çıkış engeli (BR-WHS-006) her okutmada anında çalışır; çevrimdışı kuyruğun sonradan reddedilen okutmaları ve iPhone'da arka planda gönderim olmaması gibi sorunları S1'e girmez. §14.8 |
| U-02 | Tema | Açık ve koyu tema S1'den; varsayılan cihaz ayarı, kullanıcı menüsünden değiştirilebilir | Etkinlik sektöründe gece ve karanlık kulis çalışması yaygın; renkler değişkenlerle tanımlandığı için ek kod az. §3.8 |
| U-03 | Marka rengi (`primary`) | Mor (menekşe) | Sahne ışıklarını çağrıştırır, kurumsal görünümü korur ve durum renklerinden en uzak tondur. §3.2 |
| U-04 | Temel bileşen kütüphanesi | Base UI (shadcn/ui'ın varsayılanı) | Radix'i yapan ekibin aktif geliştirdiği yeni kütüphane; kararlı ve shadcn/ui'ın varsayılanı. Radix de desteklenmeye devam ediyor; seçim geri alınabilir ([ADR-0033](../adr/0033-design-system.md)). |
| U-05 | Form kütüphanesi | React Hook Form kalır; React Compiler için `useWatch`, `useFormState` ve `Controller` zorunlu | Mayıs 2026 itibarıyla bu kalıplarla uyumlu ([kaynak](https://github.com/orgs/react-hook-form/discussions/12524)). TanStack Form'da derleyiciyle ilgili açık hata bildirimi var ([kaynak](https://github.com/TanStack/form/discussions/968)); değiştirmenin kazancı belirsiz. |
| U-06 | Ham renk kullanımı | Tailwind'in hazır paleti temada kapatılır; yalnızca anlamsal renkler | Kural araçla kendiliğinden korunur. |
| U-07 | Yazı tipi | Inter, kendi sunucumuzdan; PDF'lerde de | Yoğun ekranlar için tasarlanmış, tablo rakamları, Türkçe desteği. |
| U-08 | Durumların gösterimi | 7 ton + simge + ad; tüm durum makineleri için tek tablo | Renk körlüğü; tüm ekranlarda aynı anlam. |
| U-09 | Doğrulama zamanı | Alan terk edilince; hatalıysa her değişiklikte | En az hata ve en kısa süre veren zamanlama. |
| U-10 | Oturum sona erince | Ekrandan çıkmadan yeniden giriş diyaloğu | Form ve okutma listesi kaybolmaz. |
| U-11 | Sürüm çakışması | Güncel kayıt + kullanıcının girdiği değerler yan yana | Ne ezme ne emek kaybı. |
| U-12 | Değişken değerlere Türkçe ek | Yasak; ek gerektirmeyen cümle yapısı | Yanlış ek, profesyonel görünümü bozar ve çeviri dosyası doğru eki bilemez. |
| U-13 | Depo dokunma hedefleri | 56 px; ana işlemler 72 px | Eldivenle kullanım araştırmalarındaki alt sınırlar. |
| U-14 | Donanım okuyucu | Klavye gibi davranan okuyucular (dayanıklı cihazlar, Bluetooth) kamerayla birlikte desteklenir | Ek maliyeti yok; profesyonel depolarda yaygın. |
| U-15 | Okutma geri bildirimi | 200 ms; üç düzey (başarılı, hafif uyarı, engelleyen hata); görsel + ses, titreşim ek | Ekrana bakmadan çalışma; iOS'ta titreşim yok. |
| U-16 | Erişilebilirlik hedefi | WCAG 2.2 AA kriterleri tasarım hedefi; resmî denetim yok; 1.4.10 ofis ekranlarında bilinçli sapma | Şirket içi araç; yine de herkes kullanabilmeli. |
| U-17 | Tasarım sistemi kataloğu | Storybook; örnekler tarayıcıda test ve erişilebilirlik taraması | Tasarım sistemi görünür ve sınanır; portfolyoda gösterilebilir. |
| U-18 | Paket boyutu | 250 KB sınırı, CI'da size-limit ile | Depo telefonlarında hızlı açılış. |

## 20. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-09-25 | v0.1 | İlk taslak |
| 2026-09-25 | v1.0 | U-01 (çevrimiçi okutma, otomatik tekrar), U-02 (açık ve koyu tema), U-03 (mor marka rengi) kararlaştırıldı; ADR-0033 ve ADR-0034 kabul edildi. |
| 2026-09-26 | v1.1 | Tasarım değişkenleri koda geçti: durum tonlarının renkleri (mor hiçbir tonda kullanılmaz), durum değişkenlerinin adları, `border` / `input` ayrımı, palet adlandırması (Faz 1.0). |
