# Ekran Tasarım Özeti (v0)

> **Durum:** v1.1 · **Son güncelleme:** 2026-09-26

## 1. Bu belge ne işe yarar

- S1 ekranlarının görsel tasarımını [v0](https://v0.app) ile çıkarmak için **hangi ekranın hangi sırayla** tasarlanacağını, **her isteme eklenecek genel kuralları** ve **ekran başına hazır istemleri** (prompt) verir.
- Yerleşim ve bilgi sırası [11 — Ekran şablonları](../11-screens.md)'ndaki çizimlerden, renk, boyut ve davranış kuralları [ui.md](../standards/ui.md)'den gelir. Tasarım bu iki belgeyle çelişirse ya tasarım düzeltilir ya da belge bilinçli olarak güncellenir.
- **v0'ın ürettiği kod projeye kopyalanmaz.** Tasarım görsel referanstır; kod, projenin bileşenleriyle (shadcn/ui'ın Base UI sürümü), tasarım değişkenleriyle ve çeviri dosyalarıyla yazılır ([ui §6.1](../standards/ui.md#61-kaynak)).

**Neden v0:** Tasarımları projede kullanılan bileşen kütüphanesiyle (shadcn/ui + Tailwind CSS) üretir. Ekranda görülen şey aynı bileşenlerle birebir kodlanabilir; başka bir araçtan gelen tasarımı bileşenlere çevirme adımı olmaz.

## 2. Çalışma yöntemi

1. [v0.app](https://v0.app)'e giriş yap (GitHub hesabıyla olabilir) ve `FestOS` adında bir proje aç.
2. Projede talimat alanı varsa (Instructions / Knowledge gibi) §3'teki **genel kurallar** bloğunu oraya bir kez yapıştır. Yoksa her yeni sohbetin ilk mesajına önce §3'ü, sonra ekranın istemini yapıştır.
3. **Önce §4.1'deki tasarım sistemi sayfasını** üret. Görünüm (renkler, düğmeler, rozetler) beğenilene kadar onu düzelt; sonraki ekranlarda "aynı görsel dili kullan" de.
4. Ekranları §4'teki sırayla üret. Her ekranda açık ve koyu temaya da bak (istemler tema düğmesi istiyor).
5. Düzeltmeleri kısa ve tek konulu iste: "Durum rozetlerini tablo satırında daha küçük yap" gibi. Ücretsiz planın kullanım hakkı sınırlı; ekran başına bir ana istem ve en fazla 2–3 düzeltme hedeflenir.
6. Beğenilen sonucun ekran görüntüsünü al ve `docs/design/screens/` klasörüne koy. Ad biçimi: `{sıra}-{ekran}-{tema}.png` (ör. `04-event-list-light.png`). v0'ın paylaşım bağlantısını da not et. Ekran görüntüleri bir PR ile eklenir; §5'teki kontrol listesiyle gözden geçirilir.
7. İstemlere gerçek kişi, firma, e-posta ya da telefon yazılmaz; örnek veriler uydurmadır.

## 3. Genel kurallar (her isteme eklenecek blok)

v0 İngilizce istemlerle daha tutarlı sonuç verdiği için istemler İngilizcedir; ekrandaki bütün metinler Türkçe istenir.

```text
FESTOS DESIGN RULES (apply to every screen)

Product: FestOS, a B2B web application (ERP) for a live-events company that runs concerts,
festivals and technical services. Office users (booking manager, technical manager, system
administrator, general manager) work on desktop; warehouse staff work on phones.

Components and type
- Build with shadcn/ui components and Tailwind CSS. Font: Inter. Use tabular numbers for
  dates, times, amounts and counts.
- Font weights only 400, 500 and 600. No ALL CAPS text.
- Office density: controls 36px high, table rows 40px, 14px text in tables and forms,
  page title 24px, section title 18px. Forms at most 640px wide; tables use the full width.

Color
- Brand color is violet. Light theme primary: oklch(0.54 0.28 293) with white text.
  Dark theme primary: oklch(0.70 0.18 294) with very dark text. Neutral gray surfaces.
- Dark theme background is not pure black; raised surfaces (cards, menus, dialogs) are lighter.
- Use only semantic colors (background, foreground, card, muted, primary, secondary, accent,
  destructive, border). No gradients, no illustrations, no emojis, no decorative imagery.
- Add a light/dark theme toggle so both themes can be reviewed.

Status
- A status is always a badge with a lucide icon AND text, never color alone. Seven tones:
  neutral (circle-dashed), info (circle-dot), active (play), success (circle-check),
  warning (triangle-alert), danger (octagon-alert), muted (circle-slash).
- Tone colors: info = light sky blue, active = teal, success = green, warning = amber,
  danger = red, neutral = gray with outline, muted = gray. Never use the brand violet for a
  status. All badges share one soft style: tinted surface, tinted border, darker tinted text.
- Event statuses: Talep = neutral; Opsiyonda, Müzakere, Hesaplaşma = info;
  Hazırlık, Kurulum, Canlı, Söküm = active; Onaylı = success; Kapandı, İptal = muted.
- Conditions that are not statuses (e.g. "Opsiyon süresi yaklaşıyor", "Açık çakışma") are small
  markers next to the badge, using the warning or danger tone.

Language and formats
- All UI text in Turkish. Dates dd.mm.yyyy, with short weekday for event planning
  (12.06.2027 Cmt). 24-hour time (14:30). Ranges: 12.06.2027 14:00 – 13.06.2027 02:00.
  Money ₺1.234,56. Counts with a thousands separator (1.250 adet). Percent %15.
- Never attach a Turkish suffix directly to a data value. Write "Etkinlik: Yaz Festivali 2027",
  not "Yaz Festivali 2027'nin ...".

Accessibility
- WCAG 2.2 AA: text contrast at least 4.5:1, visible keyboard focus ring, every click target
  at least 24x24px, form fields have visible labels.
- Input, select and date field outlines have at least 3:1 contrast against their background.

Data
- Use realistic but fictional sample data: events such as "Yaz Festivali 2027", "Kurumsal
  Lansman", venues such as "Açıkhava Tiyatrosu", warehouses IST-01 and ANK-01, equipment such
  as "Robe MegaPointe", "Shure SM58", "XLR 10 m". No real people or companies.
```

Ofis ekranlarında (§4.3–4.7) yukarıdaki bloğa ek olarak şu kabuk tarifi de eklenir:

```text
APP SHELL (office screens)
- Collapsible left sidebar, 240px wide, collapsing to a 56px icon rail. Logo text "FestOS" at the
  top. Groups and items: "Etkinlik" (Etkinlikler, Taraflar, Mekanlar, Sanatçılar), "Planlama"
  (Çakışmalar, Müsaitlik), "Depo" (Stok, Transferler), "Katalog" (Modeller, Kitler),
  "Yönetim" (Kullanıcılar, Depolar). A "Daralt" button at the bottom.
- Thin top bar: breadcrumb on the left; on the right a search button labeled "Ara" with a
  "Ctrl K" hint, a connection indicator (small green dot + "Bağlı"), and a user menu showing
  initials (e.g. "AK").
- Below 1024px the sidebar becomes a slide-over panel opened from a menu button in the top bar.
```

## 4. Ekranlar ve istemler

Sıra, geliştirme sırasını izler: önce görsel dil, sonra 1.2'de gelecek giriş ve kabuk, ardından demo senaryosunun ana ekranları.

| Sıra | Ekran | Yerleşim kaynağı | Hikayeler | Plan adımı |
|---|---|---|---|---|
| 1 | Tasarım sistemi sayfası | [ui §3](../standards/ui.md#3-tasarım-değişkenleri) | — | 2b |
| 2 | Giriş ve yeni şifre | [11 §2.8](../11-screens.md#28-giriş) | US-SYS-010, US-SYS-011 | 1.2 |
| 3 | Kabuk + etkinlik listesi | [11 §2.1](../11-screens.md#21-uygulama-kabuğu), [§2.2](../11-screens.md#22-liste-sayfası) | US-SYS-012, US-EVT-007 | 1.2, 1.4 |
| 4 | Etkinlik detayı (Genel) | [11 §2.3](../11-screens.md#23-detay-sayfası) | US-EVT-004…008 | 1.4 |
| 5 | İhtiyaç ve rezervasyon sekmesi | [11 §3](../11-screens.md#3-s1-ekran-envanteri) | US-MRP-001, US-MRP-002, US-MRP-003 | 1.6 |
| 6 | Çakışma paneli | [11 §2.5](../11-screens.md#25-panel) | US-MRP-004 | 1.6 |
| 7 | Stok görünümü | [11 §2.5](../11-screens.md#25-panel) | US-EQP-008 | 1.5 |
| 8 | Depo iş listesi (telefon) | [11 §2.6](../11-screens.md#26-depo-iş-listesi-telefon) | US-WHS-002 | 1.7 |
| 9 | Okutma ekranı (telefon) | [11 §2.7](../11-screens.md#27-depo-okutma-ekranı-telefon) | US-WHS-002, US-WHS-003, US-WHS-005 | 1.7 |

### 4.1 Tasarım sistemi sayfası

```text
Create a single "Tasarım sistemi" (design system) reference page for FestOS, following the
FESTOS DESIGN RULES. Sections, top to bottom:
1. Colors: swatches for background, foreground, card, muted, primary, secondary, accent,
   destructive, border, each with its name, in light and dark theme.
2. Typography: the scale 12/14/16/18/20/24/30px with sample Turkish text
   ("Işık, ses ve sahne ekipmanı; ğüşiöç İĞÜŞÖÇ") and tabular numbers ("1.250 adet", "₺1.234,56").
3. Buttons: primary, secondary, outline, ghost, destructive; normal, disabled and loading
   (spinner inside the button) states. Labels: "Kaydet", "Vazgeç", "İptal et", "Onayla".
4. Form fields: text input "Etkinlik adı", select "Kaynak depo", date + time inputs, a required
   field with an error message below it ("Bu alan zorunlu."), helper text.
5. Status badges: one badge per tone (neutral, info, active, success, warning, danger, muted)
   with the event statuses Talep, Opsiyonda, Hazırlık, Onaylı, Kapandı, plus markers
   "Opsiyon süresi yaklaşıyor" (warning) and "Açık çakışma" (danger).
6. A small table (5 rows) of events with columns Ad, Başlangıç, Mekan, Durum.
7. Feedback: a success toast ("Etkinlik kaydedildi."), an empty state ("Henüz etkinlik yok."
   with a "Yeni etkinlik" button), an error state ("Bu bölüm yüklenemedi." with "Yeniden dene").
```

### 4.2 Giriş ve yeni şifre

```text
Create the FestOS login screen following the FESTOS DESIGN RULES. It must work on desktop and on
a 390px-wide phone.
- Centered card, max 400px wide: "FestOS" wordmark, fields "E-posta" and "Şifre" (with a
  "Göster" toggle), primary full-width button "Giriş yap".
- Show three variants side by side or as tabs:
  a) default;
  b) wrong credentials: an alert below the button "E-posta ya da şifre hatalı." (do not say which
     field is wrong);
  c) locked account: "Hesabınız geçici olarak kilitlendi. Yeniden deneme saati: 14:45"
- Also create the "Yeni şifre belirleme" screen shown after logging in with a temporary password:
  fields "Yeni şifre" and "Yeni şifre (tekrar)", the password rules as helper text under the
  first field, button "Şifreyi kaydet".
- Phone inputs use at least 16px text.
```

Not: Kilit süresi ve şifre kuralı metni örnektir; kesin değerler güvenlik standardından gelir.

### 4.3 Kabuk ve etkinlik listesi

```text
Using the FESTOS DESIGN RULES and the APP SHELL, create the "Etkinlikler" (events) list page.
- Header: title "Etkinlikler", subtitle "312 etkinlik", primary button "+ Yeni etkinlik" on the right.
- Toolbar: search input "Ara…", "Durum" dropdown filter, "Tarih aralığı" filter,
  "Filtreler (2)" button. Below it, active filter chips "Durum: Onaylı ✕" and
  "Kaynak depo: IST-01 ✕" and a "Tümünü temizle" link.
- Table with sticky header, 25 rows, columns: Ad (link, sortable, sorted ascending),
  Başlangıç (e.g. "12.06.2027 Cmt"), Mekan, Durum (status badge), a narrow warning column that
  shows a count of flags with a tooltip listing them (e.g. "Opsiyon süresi yaklaşıyor",
  "Açık çakışma"), and a "⋯" row menu. Mix statuses across rows.
- Footer: "1–25 / 312", "Sayfa başına" select (25/50/100), numbered pagination.
- Also show three variants: loading (skeleton rows with the same height as real rows),
  no results for the filters ("Bu filtrelere uyan kayıt yok." + "Filtreleri temizle"),
  and first use ("Henüz etkinlik yok." + "Yeni etkinlik" button).
- Breadcrumb in the top bar: "Etkinlikler".
```

### 4.4 Etkinlik detayı (Genel sekmesi)

```text
Using the FESTOS DESIGN RULES and the APP SHELL, create the event detail page for
"Yaz Festivali 2027". Breadcrumb: "Etkinlikler › Yaz Festivali 2027".
- Header (stays visible on every tab): event name, status badge "Onaylı" (success), marker
  "Opsiyon son tarihi: 14.05.2027" (warning). Summary line:
  "12.06.2027 Cmt 14:00 – 13.06.2027 02:00 · Açıkhava Tiyatrosu · Kaynak depo: IST-01".
- Actions on the right: primary "Hazırlığa geç" shown DISABLED with the reason under it in small
  text ("Hazırlığa geç: rider versiyonu bağlanmamış."), outline destructive "İptal et",
  and a "⋯" menu.
- Tabs: Genel, Opsiyonlar, Rider, İhtiyaç ve rezervasyon, Depo, Geçmiş (Genel selected).
- Genel tab, two cards side by side:
  "Bilgiler": Tür (Konser / Promotör), Sanatçı, Prodüksiyon, Müşteri, Hazırlık payı (1 gün),
  Dönüş payı (1 gün), Kapı açılışı (18:00).
  "Durum geçmişi": a vertical timeline of badges with date and user initials:
  Onaylı 11.03.2027 A.K., Müzakere 02.03.2027 A.K., Opsiyonda 20.02.2027 A.K.,
  Talep 18.02.2027 A.K.
```

### 4.5 İhtiyaç ve rezervasyon sekmesi

```text
Using the FESTOS DESIGN RULES and the APP SHELL, create the "İhtiyaç ve rezervasyon" tab of the
event detail page (same header as the event detail page, this tab selected).
- Top row: badge "İhtiyaç hesabı: Güncel" (success) with "Son hesap: 12.05.2027 10:24 · A.K. ·
  Rider v3", and a secondary button "Yeniden hesapla". Also show the variant where the badge is
  "Güncel değil" (warning).
- Summary cards: "Net ihtiyaç 48 kalem", "Kaynak depodan 36", "Transfer önerisi 8",
  "Dış kiralama önerisi 4".
- Table, one row per rider line, columns: Rider satırı (e.g. "Hareketli başlık · Esnek"),
  Brüt ihtiyaç, Mekandan (venue equipment), Net ihtiyaç, then the sources:
  Kaynak depo (IST-01), Transfer (e.g. "ANK-01 → IST-01: 4"), Dış kiralama, and a
  Rezervasyon column with badge "Önerildi" (info) or "Onaylandı" (success).
  Numbers right-aligned with tabular digits. A totals row at the bottom.
- Rows with status "Önerildi" have checkboxes. When some are selected, a bar above the table shows
  "3 öneri seçildi" with a primary button "Seçilenleri onayla".
- One row marked "fazla" (warning marker) to show a confirmed reservation exceeding the new need.
```

### 4.6 Çakışma paneli

```text
Using the FESTOS DESIGN RULES and the APP SHELL, create the "Çakışmalar" (conflicts) panel page.
Breadcrumb: "Planlama › Çakışmalar".
- Four summary cards that also act as filters (one can be selected): "Açık 7" (danger icon),
  "Rakip talep 5", "Aşırı rezervasyon 2", "Kabul edildi 3" (warning icon).
- Filters: "Depo", "Tarih aralığı", "Etkinlik".
- Table columns: Model (e.g. "Robe MegaPointe"), Tür ("Rakip talep" / "Aşırı rezervasyon"),
  Depo, Aralık ("12–13.06.2027"), İstenen, Müsait, Eksik (bold), Etkilenen etkinlikler
  (1–2 event names, "+1" if more), Durum (Açık = danger, Kabul edildi = warning,
  Çözüldü = success), and a "›" link to the event's requirements tab.
- Row menu action: "Kabul edildi olarak işaretle" which opens a small dialog with a required
  "Not" field.
```

### 4.7 Stok görünümü

```text
Using the FESTOS DESIGN RULES and the APP SHELL, create the "Stok" (stock) panel page.
Breadcrumb: "Depo › Stok".
- Filters: "Kategori", "Depo" (IST-01, ANK-01), and an optional "Zaman aralığı".
- Table grouped by equipment model, with one sub-row per warehouse. Columns: Model / Depo,
  Depoda, Yolda, Etkinlikte, Bakımda, and — only when a time range is selected — Rezerve and
  Müsait. Numbers right-aligned with tabular digits.
- Serialized models have an expand chevron that reveals their units: Etiket (e.g. "E-10234"),
  Durum badge (Depoda = success, Etkinlikte = info, Yolda = active, Bakımda = warning,
  Kayıp = danger), Konum, Kasa.
- Show the page once without a time range and once with "12–13.06.2027" selected.
```

### 4.8 Depo iş listesi (telefon)

```text
Using the FESTOS DESIGN RULES, create a PHONE screen (390x844, portrait) for warehouse staff:
"Depo işleri". There is no sidebar.
- Top: "FestOS · Depo" and a warehouse selector "IST-01 ▾"; below it a connection indicator
  "● Bağlı".
- Sections "Bugün" and "Yarın" with large job cards (each card at least 72px tall, the whole card
  is tappable): "↑ Çıkış · Yaz Festivali 2027 · 34 / 52 kalem", "⇄ Transfer varışı ·
  ANK-01 → IST-01 · Planlanan: 14:00", "↓ Giriş · Kurumsal Lansman".
- A "Tümünü göster" link at the bottom.
- Minimum text 16px, touch targets at least 56x56px with 8px spacing. Main information uses
  high-contrast text (target 7:1), never light gray. No hover effects or tooltips.
- Respect the phone safe areas (notch and home indicator).
```

### 4.9 Okutma ekranı (telefon)

```text
Using the FESTOS DESIGN RULES, create a PHONE screen (390x844, portrait): the check-out scanning
screen "Çıkış · Yaz Festivali 2027". There is no sidebar.
- Header: back button "‹", title, progress "34 / 52 kalem", connection indicator "● Bağlı".
- Directly below: a large scan input "Etiketi okutun…" (always focused) with a "Kamera" button.
- Last result strip: "✓ Shure SM58 · E-10234" in the success tone.
- List "Kalanlar (18)": a case row "▣ Kasa: XLR 10 m × 20"; an item "Shure SM58 6 / 8"; a counted
  item "XLR 10 m" with a large quantity "18 / 20", the text "2 eksik", and "−" / "+" buttons of
  72x72px on both sides. A collapsed section "Tamamlananlar (34) ▾".
- Fixed bottom action bar in the thumb zone: full-width primary button "Çıkışı tamamla"
  (at least 72px tall).
- Also show these states as separate frames:
  a) processing: the scanned row shows a spinner for a moment;
  b) soft warning: "Bu etiket zaten okutuldu." as a dismissible strip;
  c) blocking error card that stops scanning and closes only with "Anladım":
     "✕ Bu birim zaten çıkışta · Shure SM58 · E-10234 · Çıkışı yapan: Ayşe Yılmaz ·
     Saat: 17:03";
  d) offline: a full-width banner "Bağlantı yok. Okutma alınmadı." and the indicator
     "● Bağlantı yok".
- Minimum text 16px, model names 18px, quantities 24–30px. Touch targets at least 56x56px,
  main actions 72x72px. No hover effects or tooltips.
```

## 5. Tasarımlar gözden geçirilirken bakılanlar

| Konu | Kontrol |
|---|---|
| Yerleşim | [11 — Ekran şablonları](../11-screens.md)'ndaki bilgi sırasına ve bölümlere uyuyor mu |
| Durum | Her durum simge + metinle mi; ton tablosu ([ui §3.3](../standards/ui.md#33-durum-tonları)) doğru mu |
| Kontrast | Metinde 4,5:1, arayüz öğelerinde 3:1; depo ekranlarında temel bilgide 7:1 hedefi |
| Yoğunluk | Ofiste 36 px kontroller ve 40 px satırlar; depoda 56 px dokunma hedefleri ve 72 px ana işlemler |
| Metin | Türkçe, tümü büyük harf yok, veri değerine ek yok, tarih ve sayı biçimleri [ui §13.5](../standards/ui.md#135-biçimler)'e uygun |
| Temalar | Açık ve koyu temada aynı bilgiler okunuyor mu; koyu tema zemini saf siyah değil |
| Durumlar | Yükleniyor, boş ve hata görünümleri [ui §10](../standards/ui.md#10-yükleniyor-boş-ve-hata-durumları)'daki gibi mi |

Kabul edilen tasarımdan çıkan kararlar (renk değerleri, aralıklar, bileşen görünümü) tasarım değişkenlerine (`src/web/src/styles/tokens.css`) ve gerekiyorsa ui.md'ye işlenir; ekran görüntüleri `docs/design/screens/` altında kalır.

## 6. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-09-26 | v1.0 | İlk sürüm: çalışma yöntemi, genel kurallar ve dokuz ekranın istemleri. |
| 2026-09-26 | v1.1 | Genel kurallara durum tonlarının renkleri ve alan kenarı kontrastı eklendi (ilk tasarım incelemesinden). |
