# 12 — Uygulama Planı (Faz 1: S1)

> **Durum:** v1.0 · **Son güncelleme:** 2026-09-26

## 1. Bu belge ne işe yarar

Faz 0'da belgelenen tasarımın koda dönüşme sırasını tanımlar. **Faz 1, S1'in (MVP) geliştirilmesidir** ve S1'in bitti tanımı karşılandığında biter ([definition-of-done §6](standards/definition-of-done.md#6-sürüm-için)): demo senaryosu ([00 §6.1](00-scope.md#61-bitti-kriteri-mvp-demo-senaryosu)) otomatik testlerle baştan sona geçer, S1'in tüm kuralları ve geçişleri testlidir, `v1.0.0` demo ortamında yayındadır. Sonraki sürümler (S2–S6) kendi fazlarında aynı yöntemle planlanır.

Adımların ilerlemesi GitHub'daki "S1" kilometre taşından ve panodan izlenir ([git §6](standards/git.md#6-görev-takibi)). Bu belge sıralamayı ve yöntemi tutar; bir adım değişirse burası güncellenir.

## 2. Çalışma yöntemi

1. **Tasarım önce.** Her modül adımı, modülün fiziksel tasarım belgesiyle başlar (`docs/modules/{modül}.md`): tablolar ve kısıtlar, uç noktalar, olaylar ve yerel kopyalar, ekranlar, hikaye ve kural eşlemesi. Belge Faz 0'daki gibi araştırmaya dayanır ve karar gerektiren soruları içerir. Kodlamaya proje sahibinin onayından sonra geçilir ([06 §1](06-erd-conceptual.md#1-bu-belge-ne-işe-yarar)).
2. **Dikey dilimler.** Uygulama küçük PR'larla ilerler; her PR bir davranışı uç nokta, ekran ve testleriyle birlikte getirir ([git §3.3](standards/git.md#33-bitmemiş-işler), [definition-of-done B-01](standards/definition-of-done.md#7-kararlar)). Hedef boyut 400 satırın altıdır.
3. **Bitti tanımı her PR'da.** CI yeşil olmadan PR birleşmez; kurallar ve geçişler numaralı testlerle izlenir ([testing §10](standards/testing.md#10-kural-ve-geçiş-izlenebilirliği)).
4. **Birleştirme.** PR'ın özeti sohbette sunulur; proje sahibi onaylayınca squash ile birleştirilir.
5. **Adım sonu.** Her adım bir özet ve çalışan özelliklerin gösterimiyle kapanır.

## 3. Adımlar

| Adım | Kapsam | Hikayeler | Bitince |
|---|---|---|---|
| **1.0 Geliştirme altyapısı** | Kurulumlar; araçların güncel kararlı sürümlerinin araştırılması ve sabitlenmesi; solution ve proje iskeleti, merkezi paket sürümleri, analizörler, yasak API listesi; Host, AppHost, ServiceDefaults; boş BuildingBlocks ve test projeleri, mimari testler; ön yüz çalışma alanı (Vite, React, Tailwind, shadcn/ui, TanStack, i18n, ESLint, Prettier, Vitest, Storybook); Playwright iskeleti; commit kancaları, Dependabot, release-please, PR ve issue şablonları; CI iş akışları ve lisans denetimi | — | Uygulama tek komutla açılır; CI yeşil. `main` kural setine PR zorunluluğu ve zorunlu kontroller eklenir ([git R-03](standards/git.md#13-kararlar)). S1 issue'ları, kilometre taşı ve pano açılır. |
| **1.1 Ortak yapı taşları** | Modül kaydı; komut ve sorgu işleyicileri, dekoratörler, işlem birimi; UUIDv7 ve zaman; veritabanı rolleri ve şemaları, işlem geçmişi yazıcısı, outbox ve inbox, danışma kilidi, zamanlanmış işler; Problem Details, ETag / If-Match, tekrar güvenliği, CSRF, güvenlik başlıkları, OpenAPI → Orval hattı, SignalR; veritabanı testleri; izlenebilirlik betiği; ön yüzde istek sarmalayıcısı, SignalR istemcisi, tasarım değişkenleri ve temalar, ortak bileşenlerin ilk seti | — | Platform testleri yeşil; tasarım sistemi kataloğu çalışıyor. |
| **1.2 Identity, Audit, depolar** | Giriş ve oturum, şifre, kullanıcılar, rol matrisi, genel müdürün salt okunur erişimi, işlem geçmişi, depolar ve depo atamaları; uygulama kabuğu, yetkiye göre menü, yeniden giriş diyaloğu | US-SYS-001, 002, 003, 004, 005, 006, 010, 011, 012 | Giriş yapılıyor; menü role göre değişiyor; işlem geçmişi doluyor. |
| **1.3 Ana veriler** | Parties; Catalog (kategoriler, modeller, kitler); Venues (mekanlar, mekan ekipmanı); Riders (sanatçılar, prodüksiyonlar, rider versiyonları, karşılaştırma) | US-PTY-001, 002; US-EQP-001, 002, 005; US-VEN-001, 002; US-ART-001; US-RDR-001, 002 | Demo senaryosunun ana verileri arayüzden girilebiliyor. |
| **1.4 Booking** | Etkinlik talebi, opsiyonlar ve sıra yükselmesi, durum makinesi, otomatik operasyon geçişleri, iptal, liste ve detay sayfası, etkinlik varsayılanları, etkinliğe rider bağlama | US-EVT-001–008; US-SYS-007; US-RDR-003, 004, 005 | Demo adımları 1–4 |
| **1.5 Inventory: stok** | Seri no'lu birimler, adetli stok, kasalar, QR etiketleri, stok görünümü, birim durumları, kayıp görünümü | US-EQP-003, 004, 006, 007, 008, 009; US-WHS-006 | Depolarda stok var; etiketler basılabiliyor. |
| **1.6 Planning** | İhtiyaç hesabı, müsaitlik, rezervasyon önerisi ve onayı, transfer önerisi, çakışma paneli, müsaitlik sorgusu, karşılama raporu, hazırlık ve dönüş payı | US-MRP-001, 002, 003, 004, 006, 007, 008 | Demo adımları 5–8 ve 10 |
| **1.7 Depo işlemleri** | Toplama listesi, telefondan çıkış ve giriş okutma, transfer çıkışı ve varışı, anlık güncelleme ve eşzamanlı okutma | US-WHS-001–005 | Demo adımları 7 ve 11–13 |
| **1.8 Procurement** | Dış kiralama siparişleri; dış kiralanan ekipmanın QR ile takibi ve tedarikçiye iadesi | US-MRP-005; US-WHS-007 | Demo adımı 9 |
| **1.9 S1 kapanışı** | Demo verisi; demo senaryosunun uçtan uca testi; performans, erişilebilirlik ve mutasyon testleri; güvenlik gözden geçirmesi; demo ortamının kurulumu; `v1.0.0` | — | S1 bitti tanımı karşılanıyor. |

S1'in 52 hikayesinin hepsi bir adıma atanmıştır. Demo adımı 14 (işlem geçmişi) 1.2'de başlar ve her adımda genişler.

**Sıranın gerekçesi:** Modüller, [modül haritasındaki](05-module-map.md#41-katmanlar) katmanlara göre aşağıdan yukarıya yazılır; bir modül yalnızca kendisinden önce yazılmış modüllerin sözleşmelerini kullanır. İki istisna demo akışından gelir: depolar, kullanıcılara depo atanabilsin diye 1.2'de; Inventory ise iki parçadır. Stok (1.5), müsaitlik hesabı stok verisine dayandığı için Planning'den önce; okutma işlemleri (1.7), rezervasyonlara dayandığı için Planning'den sonra gelir.

## 4. Proje sahibinden gerekenler

| Ne zaman | Ne |
|---|---|
| 1.0 | Kurulumların onayı (yönetici izni isteyen pencereler); GitHub bağlantısının erişim anahtarının yenilenmesi |
| Her modül adımının başı | Tasarım belgesinin onayı ve sorulara cevap |
| Her PR | Sohbetteki özetin onayı |
| 1.9 | Oracle Cloud, deSEC ve Grafana Cloud hesaplarının açılması ([09 §4.3](09-environments-and-deployment.md#43-sunucu)) |

## 5. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-09-26 | v1.0 | Faz 1 planı onaylandı. |
