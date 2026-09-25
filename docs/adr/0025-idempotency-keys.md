# ADR-0025: Tekrar güvenliği anahtarları

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [standards/api.md §10](../standards/api.md#10-tekrar-güvenliği), [ADR-0024](0024-http-optimistic-concurrency.md), BR-WHS-002, BR-WHS-008

## Bağlam

Depo çalışanları telefonla, çoğu zaman zayıf bağlantıda okutur ve her okutma anında kaydedilir (BR-WHS-002). Bir istek sunucuda işlenip cevabı kaybolursa, istemci aynı isteği yeniden gönderir:
- Seri no'lu birimde ikinci okutma durum kontrolüne takılır; zararsızdır ama kullanıcıya yanlış bir hata gösterir.
- Adetli kalemde (+2 kablo) düzeltme iki kez uygulanır ve stok yanlış olur (BR-WHS-008).
- Başarılı bir düzenleme yeniden gönderilirse kayıt artık yeni sürümdedir. Kullanıcı kendi değişikliği yüzünden "başkası değiştirdi" hatası görür ([ADR-0024](0024-http-optimistic-concurrency.md)).

IETF, bu sorun için `Idempotency-Key` başlığını tanımlayan bir taslak üzerinde çalışıyor. Taslak henüz RFC değil ama Stripe gibi API'lerde yaygın olarak kullanılan yöntemi tarif ediyor.

## Karar

- Tüm değiştiren istekler (`POST`, `PUT`, `DELETE`) `Idempotency-Key` başlığı taşır. Başlığı ön yüzün istek sarmalayıcısı her kullanıcı işlemi için otomatik üretir ve yeniden denemelerde aynı anahtarı kullanır.
- Sunucu anahtarı, komutun işlem biriminin ilk adımında modül şemasındaki `idempotency_keys` tablosuna yazar. Yanıtı aynı satıra, aynı işlemde kaydeder. Saklama süresi 24 saat, kapsam kullanıcı + anahtardır.
- Davranış taslağı izler:
  - tamamlanmış istek tekrarlanırsa saklanan yanıt döner,
  - aynı anahtar farklı içerikle gelirse `422`,
  - başlık yoksa `400`.
- Eşzamanlı ikinci istek, benzersizlik kısıtında ilkini bekler ve onun sonucunu alır; bekleme zaman aşımına uğrarsa `409`.
- Anahtar denetimi, sürüm denetiminden (`If-Match`) önce yapılır.

## Sonuçlar

**Olumlu:**
- Zayıf bağlantıda yeniden gönderilen okutma ve düzeltmeler bir kez işlenir.
- Kaybolan cevaplar yanlış çakışma hatasına dönüşmez.
- Anahtar ve işlemin sonucu aynı işlemde kalıcı olur; "işlendi ama kaydedilmedi" gibi ara durum yoktur.
- Geliştirici için şeffaftır; sarmalayıcı anahtarı kendisi yönetir.
- İleride çevrimdışı okutma kuyruğu (bağlantı gelince gönderme) eklenirse aynı mekanizma çift işlemeyi önler.

**Olumsuz / bedeli:**
- Her değiştiren istek için bir satır yazılır; 24 saat sonra silinir. Bu hacimde önemsizdir.
- İş kuralı hatasıyla biten istek geri alındığı için anahtarı saklanmaz; yeniden deneme yeniden çalışır. Bu bilinçli bir sadeleştirmedir: aynı istek aynı kurala yine takılır.
- Taslak RFC olmadan değişebilir. Değişirse davranış, bu projenin kendi tanımı olarak kalır.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Anahtarı yalnızca oluşturma isteklerinde kullanmak | Adetli düzeltmeler ve yanlış çakışma sorunu çözülmez. |
| İstemcide yeniden denemeyi kapatmak | Zayıf bağlantıda kullanıcı işlemi kendisi tekrarlar; sorun kaybolmaz, kullanıcıya geçer. |
| Her komutu alan seviyesinde tekrar güvenli tasarlamak | Adetli düzeltme doğası gereği tekrar güvenli değildir; her komut için ayrı çözüm gerekir. |
| Anahtarları bellekte (önbellekte) tutmak | Uygulama yeniden başlayınca kaybolur; işlemle aynı anda kalıcı olmaz. |
| Microsoft'un `Repeatability-Request-ID` başlıkları | Yalnızca Azure'da kullanılıyor; IETF taslağı daha genel. |
