# Bitti Tanımı

> **Durum:** v1.1 · **Son güncelleme:** 2026-09-25
> **Kararlar:** [Bölüm 7](#7-kararlar)

## 1. Bu belge ne işe yarar

Bir işin ne zaman "bitti" sayılacağını tanımlar. Scrum rehberindeki tanımla aynı anlamdadır: bitti tanımı, ürünün kalite ölçütlerini karşılayan işin durumunun resmi tarifidir; bu tanımı karşılamayan iş ürünün parçası sayılmaz ([kaynak](https://scrumguides.org/scrum-guide.html)).

İki kavram birbirinden ayrılır:

| Kavram | Neyi söyler | Nerede |
|---|---|---|
| Kabul kriterleri | **Bu** hikayenin neyi yapması gerektiğini | Her hikayenin kendisinde ([02](../02-user-stories/README.md)) |
| Bitti tanımı | **Her** işin hangi kalite koşullarını karşılaması gerektiğini | Bu belge |

Bir hikaye, kabul kriterlerinin tamamı **ve** bu belgedeki koşullar karşılandığında biter. Bu belgenin PR'a ilişkin kısmı (§3), PR şablonundaki kontrol listesidir ([git §5.3](git.md#53-başlık-ve-açıklama)).

## 2. Düzeyler

| Düzey | Ne zaman uygulanır | Bölüm |
|---|---|---|
| PR | Her PR'da | [§3](#3-her-pr-için) |
| Kullanıcı hikayesi | Hikayenin son PR'ında; issue kapanmadan önce | [§4](#4-kullanıcı-hikayesi-için) |
| Hata düzeltmesi | Hata issue'su kapanmadan önce | [§5](#5-hata-düzeltmesi-için) |
| Sürüm | Sürüm etiketi konmadan önce | [§6](#6-sürüm-için) |

## 3. Her PR için

- [ ] **Başlık ve referanslar.** PR başlığı Conventional Commits biçiminde ([git §4](git.md#4-commit-mesajları)); açıklamada ilgili issue, hikaye, kural, geçiş ve ADR numaraları yazılı.
- [ ] **CI yeşil.** Derleme (uyarılar hata), biçim, lint, tip denetimi, tüm testler, izlenebilirlik denetimi ([testing §10](testing.md#10-kural-ve-geçiş-izlenebilirliği)), API istemcisinin güncelliği, gizli bilgi taraması.
- [ ] **Kendi kendine inceleme.** Fark GitHub'da satır satır okundu; unutulmuş hata ayıklama kodu, istenmeden eklenmiş dosya, yorum satırına alınmış kod yok ([git §5.4](git.md#54-kendi-kendine-inceleme)).
- [ ] **Testler.** Değişikliğin türüne göre gereken testler eklendi ([testing §4](testing.md#4-hangi-değişiklik-hangi-testi-ister)); kurallar ve geçişler numarasıyla etiketli.
- [ ] **Önce sözlük.** Yeni bir kavram, koda girmeden önce [sözlüğe](../01-glossary.md) eklendi; adlar sözlükten türetildi ([naming §2](naming.md#2-temel-ilkeler)).
- [ ] **Belgeler güncel.** Kural metni değiştiyse [03](../03-business-rules.md) ve değişiklik kaydı; durum geçişi değiştiyse [04](../04-state-machines.md); yeni bir mimari karar varsa ADR; modülün fiziksel tasarımı (`docs/modules/`) değiştiyse o belge. Belge denetim betikleri temiz.
- [ ] **Veritabanı.** Model değişikliği migration'la geldi; modelde migration'a dökülmemiş değişiklik yok; geriye uyumsuz değişiklik genişlet / daralt yöntemiyle bölündü ([database §16](database.md#16-migrationlar)).
- [ ] **Kural kapatma yok.** Yeni bir analizör ya da lint kuralı kapatıldıysa gerekçesi yazılı ve kapsamı tek satır ([code-style §8](code-style.md#8-kuraldan-sapma)).
- [ ] **Metinler kaynak dosyasında.** Kullanıcıya görünen metinler çeviri dosyalarında; yeni kural kodlarının `errors:` çevirisi var (K-02).
- [ ] **Ekran görüntüsü.** Arayüz değiştiyse masaüstü görünümü; depo ekranlarında telefon görünümü de eklendi.

## 4. Kullanıcı hikayesi için

§3'e ek olarak:

- [ ] **Kabul kriterleri.** Hikayenin tüm kabul kriterleri karşılanıyor.
- [ ] **Kurallar ve geçişler.** Hikayenin "Kurallar" satırındaki her kural ve hikayeye ait geçişler uygulandı ve testli; izlenebilirlik raporunda hikayenin eksik kuralı yok (TR-05).
- [ ] **Dikey dilim.** Hikaye, ilgili rolle arayüzden baştan sona yapılabiliyor: uç nokta, ekran ve testler birlikte bitti. Kullanıcı arayüzü olmayan hikayeler (otomatik geçişler, zamanlanmış işler) bunun dışındadır.
- [ ] **Yetki.** Yeni uç noktaların yetkisi katalogda ve doğru rollerde; genel müdür salt okunur kalıyor (BR-SYS-004); arayüz, kullanıcının yetkisi olmayan işlemleri göstermiyor.
- [ ] **Hatalar anlaşılır.** Hikayenin kural ihlalleri kullanıcıya Türkçe ve ne yapması gerektiğini söyleyen bir mesajla gösteriliyor.
- [ ] **İşlem geçmişi.** Hikayenin değiştirdiği kayıtlar işlem geçmişinde görünüyor.
- [ ] **Anlık güncelleme.** Başka kullanıcıların açık ekranında görünen bir kaydı değiştiriyorsa, o ekranlar sayfa yenilenmeden güncelleniyor (BR-SYS-012).
- [ ] **Mobil.** Depo ve saha ekranları telefonda, tek elle kullanılabiliyor (ayrıntılar arayüz standartlarında, D.4).
- [ ] **Klavye.** Ofis ekranlarının işlemleri klavyeyle yapılabiliyor ([00 §9](../00-scope.md#9-fonksiyonel-olmayan-varsayımlar)).
- [ ] **Demo.** Hikaye MVP demo senaryosunun bir adımıysa, uçtan uca testi ve demo verisi güncel.
- [ ] **Gözlemlenebilirlik.** Yeni iş olayları ve zamanlanmış işler [observability](observability.md) standardına göre loglanıyor ve ölçülüyor.

## 5. Hata düzeltmesi için

§3'e ek olarak:

- [ ] **Önce test.** Hatayı yeniden üreten test düzeltmeden önce başarısız oldu, sonra geçti.
- [ ] **Kök neden.** PR açıklamasında hatanın nedeni bir iki cümleyle yazılı.
- [ ] **Belge boşluğu.** Hata bir kuralın eksik ya da belirsiz yazılmasından doğduysa kural metni düzeltildi.

## 6. Sürüm için

S1'in bitti tanımı MVP demo senaryosudur ([00 §6.1](../00-scope.md#61-bitti-kriteri-mvp-demo-senaryosu)). Bu bölüm senaryonun nasıl kanıtlanacağını ve sürümün yayına hazır olmasını tanımlar. Sonraki sürümler aynı listeyi kendi senaryolarıyla kullanır.

- [ ] **Demo senaryosu otomatik.** Senaryonun 14 adımı uçtan uca testlerle, demo verisiyle ve temiz bir veritabanında baştan sona geçiyor.
- [ ] **Tüm kurallar ve geçişler.** Sürümün tüm kurallarının ve geçişlerinin kaynak kodu ve testi var; izlenebilirlik raporunda sürüme ait eksik yok (TR-06). Sonraki sürümlerde değişecek kurallar ([03 §6](../03-business-rules.md#6-sonraki-sürümlerde-değişecek-kurallar)) S1'deki halleriyle testli.
- [ ] **Açık hata yok.** `critical` ve `high` seviyesinde açık hata yok.
- [ ] **Gece testleri yeşil.** Tüm tarayıcılarda uçtan uca testler, performans testleri (P-08, P-09, P-15) ve erişilebilirlik taraması son çalıştırmada geçti.
- [ ] **Mutasyon testi.** Hesaplama motorlarında Stryker.NET çalıştırıldı ve yakalanmayan mutasyonlar incelendi ([testing §13](testing.md#13-kod-kapsamı)).
- [ ] **Güvenlik.** Bilinen yüksek ya da kritik açığı olan bağımlılık yok; gizli bilgi taraması tüm geçmişte temiz; güvenlik başlıkları testleri geçiyor.
- [ ] **Yayında.** Sürüm demo ortamında çalışıyor; migration'lar yayın betiğinde uygulandı ([09 §7](../09-environments-and-deployment.md#7-yayın-akışı)); son geri yükleme tatbikatı başarılı ([09 §8.3](../09-environments-and-deployment.md#83-geri-yükleme-tatbikatı)).
- [ ] **Sürüm kaydı.** Sürüm etiketi ve değişiklik günlüğü release-please ile oluştu; S1 için `v1.0.0` ([git §7](git.md#7-sürüm-numaraları-ve-etiketler)).
- [ ] **Belgeler.** README'deki durum tablosu ve [işletim el kitabı](../10-operations.md) güncel.

## 7. Kararlar

| No | Konu | Karar | Gerekçe |
|---|---|---|---|
| B-01 | Hikayenin kapsamı | Dikey dilim: bir hikaye, uç noktası, ekranı ve testleriyle birlikte biter | Her biten hikaye kullanılabilir ve gösterilebilir; depo ekranlarının kullanılabilirliği erken denenir. "Önce tüm sunucu, sonra tüm ekranlar" yaklaşımında arayüz sorunları en sona kalır. |
| B-02 | Sürümün bitti kanıtı | Demo senaryosunun uçtan uca testlerle otomatik geçmesi | Senaryonun elle bir kez denenmesi, sonraki değişikliklerde bozulmadığını göstermez. |
| B-03 | Kontrol listesinin yeri | PR şablonunda; madde uygulanmıyorsa nedeni yazılır | Liste her PR'da görünür; atlanan maddenin nedeni kayıt altında kalır. |
| B-04 | Hikayenin kapanışı | Hikayenin son PR'ı birleşince issue kendiliğinden kapanır; kapanan hikaye bitmiş sayılır | Tek bir "bitti" anı; pano ile repo durumu ayrışmaz. |

## 8. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-09-25 | v0.1 | İlk taslak |
| 2026-09-25 | v1.0 | Kesinleşti. |
| 2026-09-25 | v1.1 | Yayın ve işletim belgelerine bağlandı (D.3). |
