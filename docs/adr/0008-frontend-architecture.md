# ADR-0008: Ön yüz: React + Vite tek sayfalı uygulama

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [07-tech-stack.md §3.3](../07-tech-stack.md#33-ön-yüz-frontend), [ADR-0009](0009-ui-components.md), [ADR-0011](0011-authentication.md)

## Bağlam

Projenin başında ön yüz için "React / Next.js" denmişti. Uygulamanın özellikleri:
- Yalnızca giriş yapmış şirket çalışanları kullanır; arama motorlarında görünmesi gerekmez.
- Ekranların çoğu veri yoğun: tablolar, formlar, anlık güncellenen listeler.
- Depo ekranları telefon kamerasıyla QR okur.
- Değişiklikler SignalR ile anlık olarak ekrana gelir.

Bunların hepsi tarayıcıda çalışan özelliklerdir; sunucuda sayfa üretmek bunlara bir şey katmaz.

## Karar

- **Uygulama türü:** React 19 ve Vite ile tek sayfalı uygulama (SPA), TypeScript katı modda.
- **Yönlendirme:** TanStack Router (tip güvenli yönlendirme ve adres çubuğu parametreleri).
- **Sunucu verisi:** TanStack Query. SignalR'dan gelen değişiklik bildirimleri ilgili sorguları geçersiz kılar ve veri yeniden alınır ([ADR-0012](0012-realtime-signalr.md)).
- **API istemcisi:** Orval, sunucunun OpenAPI belgesinden tipleri, TanStack Query kancalarını ve Zod şemalarını üretir. API tipleri elle yazılmaz.
- **Çok dillilik:** react-i18next. S1'de yalnızca Türkçe; tüm metinler baştan kaynak dosyalarındadır (K-02).
- **Tarih ve saat:** date-fns ve @date-fns/tz. Gösterim Europe/Istanbul saatine göredir.
- **Yayın:** Derlenen ön yüz, API ile aynı adresten sunulur. Böylece tek bir oturum çerezi kullanılır ve tarayıcıların kaynaklar arası istek kısıtlarıyla (CORS) uğraşılmaz. Geliştirmede Vite sunucusu API'ye yönlendirme yapar.
- **Klasör yapısı:** Özellik bazlıdır ve sunucudaki modüllerle eşleşir (ayrıntı Faz 0 C.2'de).

## Sonuçlar

**Olumlu:**
- Tek çalışma zamanı (.NET) ve tek adres. Ön yüz statik dosyalardan ibarettir.
- Kimlik doğrulama tek yerdedir: sunucunun çerezi.
- API değiştiğinde ön yüzdeki tipler yeniden üretilir; uyumsuzluk derleme sırasında yakalanır.

**Olumsuz / bedeli:**
- İlk açılışta uygulama paketinin indirilmesi gerekir. Kod bölme (route bazlı) ile hafifletilir.
- Next.js iş ilanlarında daha sık geçer. Bunun yerine seçim gerekçesi mülakatta anlatılabilir bir mühendislik kararıdır.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Next.js | Sunucu tarafı üretim ve arama motoru optimizasyonu için güçlüdür; ama bu uygulamada ikisine de gerek yoktur. İkinci bir sunucu çalışma zamanı (Node.js) ekler; oturum ve API yönlendirmesi iki sunucuya bölünür. QR okuma ve anlık güncelleme zaten tarayıcıda çalışır. |
| TanStack Start / React Router framework modu | Sunucu tarafı yetenekleri Next.js ile aynı gerekçeyle gereksizdir. |
| Blazor | Sunucu ve ön yüzde aynı dil (C#) büyük bir kolaylıktır. Ama WebAssembly sürümü depo telefonlarında büyük bir ilk indirme getirir; sunucu sürümü her kullanıcı için sürekli bağlantı ister. Bileşen ekosistemi React'e göre dardır. |
| Angular | Güçlü bir çatı ama daha fazla tören getirir; seçilen bileşen ekosistemiyle uyumlu değil. |
