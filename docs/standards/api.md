# API Standardı

> **Durum:** v1.0 · **Son güncelleme:** 2026-09-25
> **Kararlar:** [Bölüm 15](#15-kararlar)

## 1. Bu belge ne işe yarar

Sunucunun HTTP API'sinin sözleşmesini tanımlar:
- adres yapısı, HTTP yöntemleri ve durum kodları,
- JSON gövdesinin biçimi,
- listeler (sayfalama, sıralama, filtre),
- hata yanıtları,
- eşzamanlı düzenleme ve tekrar güvenliği,
- güvenlik kuralları, sürümleme, anlık bildirimler,
- OpenAPI belgesi, istemci üretimi ve uç nokta yazım kuralları.

Adların yazımı (kebab-case adres, camelCase JSON, işlem adı) [naming §6](naming.md#6-api-adları)'dadır. Oturum ve yetki modeli [ADR-0011](../adr/0011-authentication.md)'dedir; yetki kataloğu, loglama ve gizli bilgiler C.6'da tanımlanacak.

## 2. Temel ilkeler

1. **Tek istemci, yine de kesin sözleşme.** API'yi yalnızca kendi ön yüzümüz kullanır, ikisi aynı adresten birlikte yayınlanır. Yine de sözleşme OpenAPI belgesinde tektir ve ön yüzün API kodu bu belgeden üretilir; elle yazılmış istek kodu yoktur ([08 §10](../08-architecture.md#10-derleme-ayarları)).
2. **Görev odaklı API.** Uç noktalar tablo satırı düzenlemek (CRUD) için değil, iş işlemleri için tasarlanır. Durum geçişleri kendi uç noktalarındadır; durum alanı genel bir güncellemeyle değiştirilemez. Google'ın API kılavuzu da aynı gerekçeyi verir: güncelleme yan etkisiz beklenir ve durum her değere serbestçe atanabilir görünür, oysa durum bir yaşam döngüsünü izler ([kaynak](https://google.aip.dev/216)).
3. **Hatalar makinece okunur.** Her hata bir kod ve gerekiyorsa parametreler taşır. Kullanıcıya gösterilen Türkçe metin ön yüzün çeviri dosyasında kod üzerinden bulunur (K-02); sunucu Türkçe metin üretmez.
4. **Hiçbir değişiklik sessizce ezilmez.** Toplu kök değiştiren her istek, istemcinin gördüğü sürümü taşır (BR-SYS-011, [§9](#9-eşzamanlı-düzenleme)).
5. **Her değiştiren istek güvenle tekrarlanabilir.** Zayıf bağlantıda yeniden gönderilen bir okutma iki kez işlenmez ([§10](#10-tekrar-güvenliği)).

## 3. Adres yapısı

| Öğe | Kural | Örnek |
|---|---|---|
| Kök | `/api/v1` ([A-03](#15-kararlar)) | — |
| Koleksiyon | `/api/v1/{kaynakÇoğul}`; kaynak adı sözlükteki varlık adıdır | `/api/v1/events`, `/api/v1/equipment-models` |
| Tek kayıt | `/{kaynakÇoğul}/{kimlik}` | `/api/v1/events/{eventId}` |
| Alt kaynak | Yalnızca toplu kökün kendi parçaları için, en fazla bir seviye | `/api/v1/events/{eventId}/approvals` |
| Durum geçişi ve iş işlemi | `POST /{kaynakÇoğul}/{kimlik}/{fiil}` | `POST /api/v1/events/{eventId}/confirm` |
| Koleksiyon düzeyinde işlem | `POST /{kaynakÇoğul}/{fiil}` | `POST /api/v1/requirement-calculations/run` |
| Hesaplanan bilgi | Ad olarak kaynak, parametreler sorguda | `GET /api/v1/availability?modelId=…&warehouseId=…&from=…&to=…` |
| Oturumdaki kullanıcı | `/api/v1/me` | Kullanıcı bilgisi ve yetkileri |
| Dosya çıktısı | Son bölüm dosya adı ve uzantısı | `GET /api/v1/events/{eventId}/pick-list.pdf` |

**Kurallar:**
- **Adreste modül adı yoktur.** Kaynak adları sözlükten geldiği ve sözlükte her kavramın tek adı olduğu için sistem genelinde tekildir. Modül bilgisi OpenAPI etiketindedir ([naming §6](naming.md#6-api-adları)). Aksi halde `/api/v1/venues/venues/{venueId}` gibi tekrarlı adresler oluşurdu. Bir adres kalıbının yalnızca bir modüle ait olduğunu AT-14 denetler.
- **İlişkili kayıtlar filtreyle alınır.** Başka bir toplu köke ait kayıtlar iç içe adresle değil, filtreyle alınır: `/api/v1/venue-holds?eventId=…`. İç içe adres yalnızca toplu kökün kendi parçaları içindir.
- **Durum geçişi fiilleri** sözlükteki işlem adlarıdır ve kebab-case yazılır: `confirm`, `cancel`, `deactivate`, `reactivate`, `check-out`.
- **Pasifleştirme `DELETE` değildir.** Kayıt silinmediği için `POST /{kaynak}/{kimlik}/deactivate` kullanılır (BR-SYS-001). `DELETE` yalnızca gerçekten silinebilen modül içi kayıtlarda kullanılır ([database §10.2](database.md#102-silme)).
- **Neden `/confirm`, `:confirm` değil:** Google ve Microsoft kılavuzları işlem için `…/{id}:confirm` biçimini önerir ([kaynak](https://google.aip.dev/136)). Ama `:` karakteri ASP.NET Core rota şablonlarında kısıt ayracıdır (`{id:guid}`). Alt yol biçimi (`/confirm`) Stripe gibi yaygın API'lerde de kullanılır ve araçlarda sorun çıkarmaz.
- **Türkçe karakterli dosya adları:** İndirilen dosyanın adı (ör. `Toplama listesi - Çanakkale.pdf`) `Content-Disposition` başlığında hem ASCII hem UTF-8 (`filename*`) biçiminde gönderilir. ASP.NET Core'un dosya yanıtı bunu kendisi yapar.

## 4. HTTP yöntemleri ve durum kodları

### 4.1 Yöntemler

| Yöntem | Kullanım | Başarılı yanıt |
|---|---|---|
| `GET` | Okuma; hiçbir yan etkisi yoktur | `200` + kayıt ya da liste |
| `POST` (koleksiyona) | Oluşturma | `201 Created` + `Location` başlığı + `{ id, version }` |
| `POST` (işlem) | Durum geçişi ve iş işlemi | `200` + güncel kayıt (`…Details`) |
| `PUT` | Bir kaydın düzenlenebilir alanlarının tamamını değiştirme | `200` + güncel kayıt |
| `DELETE` | Gerçekten silinebilen modül içi kayıt | `204 No Content` |

- **`PATCH` kullanılmaz.** Kısmi güncellemede "alan gönderilmedi" ile "alan boşaltıldı" ayrımı (JSON Merge Patch) hataya açıktır. Düzenleme ekranları tüm alanları gönderir (`PUT`); tek alanlık iş değişiklikleri kendi işlem uç noktasındadır.
- Değişiklik yapan yanıtlar güncel kaydı döndürür. Böylece işlemi yapan kullanıcının ekranı, anlık bildirimi beklemeden hemen güncellenir (BR-SYS-012).

### 4.2 Durum kodları

| Kod | Ne zaman | Hata kodu ([§8](#8-hata-yanıtları)) |
|---|---|---|
| `400 Bad Request` | Bozuk JSON, yanlış tip, bilinmeyen alan, alan doğrulama hatası (zorunlu alan, uzunluk, biçim) | `validation`, `malformedRequest` |
| `401 Unauthorized` | Oturum yok ya da süresi dolmuş | `unauthenticated` |
| `403 Forbidden` | Oturum var, yetki yok | `forbidden` |
| `404 Not Found` | Kayıt yok | `notFound` |
| `409 Conflict` | Aynı tekrar güvenliği anahtarıyla bir istek hâlâ işleniyor | `idempotencyKeyInProgress` |
| `412 Precondition Failed` | Kayıt, istemcinin gördüğü sürümden sonra değişmiş | `concurrencyConflict` |
| `422 Unprocessable Content` | İş kuralı ihlali; tekrar güvenliği anahtarının başka içerikle yeniden kullanımı | Kural numarası (ör. `BR-EVT-009`), `idempotencyKeyReused` |
| `428 Precondition Required` | Sürüm gerektiren istekte `If-Match` başlığı yok | `versionRequired` |
| `429 Too Many Requests` | İstek sınırı aşıldı; `Retry-After` başlığıyla | `rateLimited` |
| `500 Internal Server Error` | Beklenmeyen hata; ayrıntı verilmez, iz kimliği verilir | `internalError` |
| `503 Service Unavailable` | Bakım ya da bağımlılık kullanılamıyor | `serviceUnavailable` |

**400 ile 422 ayrımı:** İsteğin biçimi yanlışsa (şema, tip, zorunlu alan) `400`; biçim doğru ama iş kuralı izin vermiyorsa `422` ([kaynak](https://milanjovanovic.tech/blog/rest-api-http-status-codes)). Ön yüz dallanmayı durum koduna göre değil, **hata koduna** göre yapar; durum kodu yalnızca genel sınıfı belirtir.

**Tüm iş kuralı ihlalleri 422'dir.** "Şu an geçerli değil" (409) ile "hiç geçerli değil" (422) ayrımı, 90'dan fazla kural için tek tek tartışmaya açıktır. Kural numarası zaten ayrıntıyı taşır. `409` yalnızca tekrar güvenliği için, `412` yalnızca sürüm çakışması için kullanılır.

## 5. JSON gövdesi

### 5.1 Serileştirici ayarları

.NET'in web varsayılanlarına (camelCase alan adları) ek olarak .NET 10'un katı ayarları açılır ([kaynak](https://duendesoftware.com/blog/20260430-harden-your-dotnet-json-deserialization)):

| Ayar | Değer | Neden |
|---|---|---|
| Aynı alanın iki kez gelmesi (`AllowDuplicateProperties`) | Reddedilir | Aynı alanın iki değeri farklı katmanlarda farklı okunabilir; güvenlik açığıdır. |
| Bilinmeyen alan (`UnmappedMemberHandling`) | Reddedilir (`400`) | Yanlış yazılmış bir alan adı sessizce yok sayılmaz. |
| Boş olabilirlik (`RespectNullableAnnotations`) | Uygulanır | C#'ta boş olamayan alana `null` gönderilemez. |
| Zorunlu kurucu parametreleri (`RespectRequiredConstructorParameters`) | Uygulanır | Eksik zorunlu alan `400` verir. |
| Alan adı büyük / küçük harf duyarsızlığı | Kapalı | Tek biçim: camelCase. |
| Enum | camelCase metin; sayı kabul edilmez | [naming §6](naming.md#6-api-adları) |
| `null` değerli alanlar | Yanıtta **her zaman yazılır**, atlanmaz | TypeScript tipi `T \| null` olarak kesinleşir; "alan yok" ile "değer yok" karışmaz ([code-style S-09](code-style.md#10-kararlar)). |

### 5.2 Değer biçimleri

| Değer | JSON biçimi | Örnek |
|---|---|---|
| Kimlik | Metin (UUID) | `"0192f0c5-…"` |
| An (`DateTimeOffset`, UTC) | ISO 8601 metin, UTC | `"2027-06-12T16:00:00+00:00"` |
| Takvim günü (`DateOnly`) | `yyyy-MM-dd` | `"2027-06-12"` |
| Günün saati (`TimeOnly`) | `HH:mm:ss` | `"23:00:00"` |
| Gelecekteki duvar saati | Yerel tarih-saat (ofsetsiz) + ayrı `timeZone` alanı; yanıtta türetilen UTC anı da bulunur | `"doorsAtLocal": "2027-06-12T19:00:00"`, `"timeZone": "Europe/Istanbul"`, `"doorsAt": "2027-06-12T16:00:00+00:00"` |
| Süre | **Dakika cinsinden tamsayı**, ad `…Minutes` ile biter | `"prepBufferMinutes": 600` |
| Ondalık sayı (`decimal`: tutar, kur, yüzde, ölçü) | **Metin** | `"weightKilograms": "12.500"` |
| Tutar (`Money`) | Nesne | `{ "amount": "1250.00", "currency": "TRY" }` |
| Tamsayı | Sayı | `"quantity": 20` |

**Neden ondalık sayılar metin:** JavaScript sayıları ikili kayan noktadır; ondalık bir değer JSON sayısı olarak okunduğunda kesinliğini kaybedebilir ve hesapta `0.1 + 0.2 = 0.30000000000000004` gibi sonuçlar verir ([kaynak](https://uuid.medium.com/serialising-monetary-amounts-7409acd6f38d)). Metin, değeri bayt bayt korur. Ön yüz ondalık değerleri `Intl.NumberFormat` ile doğrudan metinden biçimler; hesaplama gerekirse sunucu yapar. Tek kural (her `decimal` metindir) alan alan karar vermeyi ortadan kaldırır.

**Neden süreler dakika:** .NET `TimeSpan`'i JSON'da `"1.02:00:00"` biçiminde yazar; bu biçimi TypeScript'te okumak özel kod gerektirir. Süreler zaten dakika hassasiyetindedir ([ADR-0017](../adr/0017-time-and-money-types.md)); tamsayı dakika her iki tarafta da açıktır.

### 5.3 Gövde kuralları

- **Tek kayıt zarfsız döner.** `GET /events/{id}` doğrudan etkinlik nesnesini döndürür; `{ "data": … }` sarmalayıcısı yoktur. Listeler [§6](#6-listeler)'daki yapıdadır.
- **Gelen metinler** kırpılır ve Unicode NFC biçimine getirilir ([database §13](database.md#13-metin-sıralama-ve-arama)). Şifre gibi dokunulmaması gereken alanlar `[Sensitive]` özniteliğiyle bu işlemin dışında tutulur.
- **Gövde boyutu:** JSON istekleri en fazla 1 MB'tır. Dosya yükleme S2'de ayrıca tanımlanır.

## 6. Listeler

### 6.1 Sayfalama

İki biçim kullanılır ([A-01](#15-kararlar)):

| Biçim | Nerede | İstek | Yanıt |
|---|---|---|---|
| **Sayfa numaralı** | Ofis ekranlarındaki tablolar (etkinlikler, modeller, taraflar) | `?page=2&pageSize=25` | `{ "items": [...], "page": 2, "pageSize": 25, "totalCount": 312 }` |
| **İmleçli** (cursor) | Büyüyen ve yalnızca eklenen kayıtlar (işlem geçmişi, stok hareketleri) ve mobilde "daha fazla yükle" listeleri | `?after={imleç}&limit=50` | `{ "items": [...], "nextCursor": "…" }` (son sayfada `null`) |

- **Sayfa numaralı:** `page` 1'den başlar. `pageSize` varsayılanı 25, üst sınırı 100'dür. TanStack Table toplam sayıyla sayfa numaralarını ve son sayfaya atlamayı gösterir ([kaynak](https://tanstack.com/table/latest/docs/framework/react/guide/pagination)). Toplam sayı bu veri hacminde ucuzdur.
- **İmleçli:** İmleç, istemcinin içini yorumlamadığı opak bir metindir (sıralama alanı ve kimliğin kodlanmış hali). Kayıt eklense bile sayfa kayması olmaz; büyük tabloda sayfa numarasının yavaşlığı yaşanmaz ([kaynak](https://designgurus.substack.com/p/api-pagination-guide-cursor-vs-offset)). `limit` üst sınırı 100'dür.
- Bir uç nokta tek bir sayfalama biçimi kullanır. Sayfalamasız liste yalnızca doğası gereği küçük ve sınırlı listelerde (ör. bir rider'ın satırları, seçim listeleri) kullanılır.

### 6.2 Sıralama

- `?sort=startsAt` artan, `?sort=-startsAt` azalan; birden fazla alan virgülle: `?sort=-startsAt,name`.
- Her uç nokta sıralanabilir alanları açıkça listeler. Listede olmayan alan `400` verir; kullanıcı girdisi doğrudan SQL'e gitmez.
- Varsayılan sıralama her liste uç noktasında tanımlıdır ve sonuna her zaman kimlik eklenir. Böylece aynı değerdeki kayıtların sırası sayfadan sayfaya değişmez.
- Metin alanlarında sıralama Türkçe kuralla yapılır ([database §13](database.md#13-metin-sıralama-ve-arama)).

### 6.3 Filtre ve arama

- Filtreler, adlandırılmış sorgu parametreleridir: `?status=confirmed&warehouseId=…&from=2027-06-01`. Genel bir sorgu dili (OData, `filter=` ifadeleri) kullanılmaz; her uç nokta desteklediği filtreleri OpenAPI'de açıkça gösterir.
- Çoklu değer parametre tekrarıyla verilir: `?status=confirmed&status=advancing`.
- Aralık filtreleri `from` / `to` çiftidir ve yarı açıktır: `[from, to)` ([database §7.3](database.md#73-zaman-aralıkları)).
- Serbest metin araması `q` parametresidir ve arama anahtarı kolonunda çalışır: büyük / küçük harf ve Türkçe karakter duyarsızdır ([database §13](database.md#13-metin-sıralama-ve-arama)).

## 7. Belgeler ve dosya çıktıları

- PDF çıktıları (toplama listesi, QR etiketleri) sunucuda üretilir ve `GET` ile indirilir. Çok sayıda kimlik gerektiren çıktılar (ör. seçilen 200 birimin etiketi) `POST /api/v1/labels/print` ile, kimlikler gövdede gönderilerek alınır.
- Yanıt `Content-Type: application/pdf` ve `Content-Disposition: attachment` başlıklarıyla döner.
- Oturum çerezle taşındığı için bağlantı (`<a href>`) ile indirme de çalışır.

## 8. Hata yanıtları

### 8.1 Biçim

Tüm hatalar RFC 9457 "Problem Details" biçimindedir (`Content-Type: application/problem+json`; [kaynak](https://www.rfc-editor.org/rfc/rfc9457.html)). Standart alanlara projeye özgü ek alanlar eklenir:

| Alan | İçerik |
|---|---|
| `type` | Hata türünün kalıcı kimliği: `urn:festos:problem:{tür}` (ör. `urn:festos:problem:business-rule`). Sahip olmadığımız bir alan adı kullanmamak için URN seçildi; RFC çözümlenemeyen URI'lere izin verir. |
| `title` | Türün kısa, İngilizce, sabit başlığı (geliştirici için) |
| `status` | HTTP durum kodu |
| `detail` | İngilizce, geliştiriciye yönelik açıklama; kişisel ya da gizli veri içermez |
| `instance` | İstek yöntemi ve adresi |
| `code` | **Makinece okunan hata kodu**: kural numarası (`BR-EVT-009`) ya da teknik kod (`validation`, `concurrencyConflict` …) |
| `params` | Hata mesajının ihtiyaç duyduğu değerler (ör. `{ "performedBy": "…", "performedAt": "…" }`) |
| `errors` | Yalnızca doğrulama hatalarında: alan başına hata listesi |
| `traceId` | OpenTelemetry iz kimliği; destek ve log araması için |

**İş kuralı ihlali örneği:** Aynı birim iki cihazda aynı anda okutuldu (BR-WHS-006):

```json
{
  "type": "urn:festos:problem:business-rule",
  "title": "Business rule violated",
  "status": 422,
  "detail": "Unit was already checked out by another user.",
  "instance": "POST /api/v1/check-outs",
  "code": "BR-WHS-006",
  "params": { "performedBy": "Ayşe Yılmaz", "performedAt": "2027-06-12T14:03:11+00:00" },
  "traceId": "4bf92f3577b34da6a3ce929d0e0e4736"
}
```

Ön yüz `errors:BR-WHS-006` anahtarıyla Türkçe metni bulur ve `params` ile doldurur: "Bu birim 17:03'te Ayşe Yılmaz tarafından çıkışı yapıldı." ([naming §7.1](naming.md#71-çeviri-anahtarları)).

### 8.2 Doğrulama hataları

`errors`, RFC 9457'deki örneği izler: her öğe hatalı alanı JSON Pointer ile gösterir ([kaynak](https://www.rfc-editor.org/rfc/rfc9457.html#name-extension-members)). Mesaj yerine kod ve parametre taşır:

```json
{
  "type": "urn:festos:problem:validation",
  "title": "Validation failed",
  "status": 400,
  "code": "validation",
  "errors": [
    { "pointer": "/name", "code": "required", "params": {} },
    { "pointer": "/units/3/serialNumber", "code": "duplicate", "params": {} },
    { "pointer": "/notes", "code": "maxLength", "params": { "max": 2000 } }
  ],
  "traceId": "…"
}
```

- Doğrulama kodları FluentValidation kurallarından eşlenir ve `validation:{kod}` çeviri anahtarına karşılık gelir.
- Toplu girişte (ör. yapıştırılan seri numarası listesi, US-EQP-003) işaretçi satırı da gösterir (`/units/3/serialNumber`); ön yüz hatalı satırları işaretler.

### 8.3 İstisnaların eşlenmesi

Hata eşlemesi Host'taki tek bir hata işleyicidedir (`IExceptionHandler`; [kaynak](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling-api?view=aspnetcore-10.0)). Uç noktalar hata yanıtını elle kurmaz; istisna fırlatır ya da doğrulama sonucunu döndürür.

| Kaynak | Durum | `code` |
|---|---|---|
| Doğrulama (FluentValidation) | 400 | `validation` |
| JSON okuma ve bağlama hatası | 400 | `malformedRequest` |
| `BusinessRuleViolationException` | 422 | Kural numarası |
| Veritabanı kısıt ihlali (kural koduna eşlenmiş) | 422 | Eşlenen kural numarası ([database §12.1](database.md#121-kısıtlar)) |
| `NotFoundException` | 404 | `notFound` |
| Sürüm çakışması | 412 | `concurrencyConflict` |
| Beklenmeyen her şey | 500 | `internalError`; `detail` genel bir metindir, yığın izi (stack trace) asla dönmez |

## 9. Eşzamanlı düzenleme

BR-SYS-011 (sessiz ezme yok) HTTP üzerinde standart koşullu isteklerle uygulanır ([ADR-0024](../adr/0024-http-optimistic-concurrency.md)):

| Adım | Ne olur |
|---|---|
| Okuma | Toplu kökü döndüren `GET` yanıtında `ETag: "7"` başlığı ve gövdede `"version": 7` bulunur. |
| Değiştirme | Toplu kökü değiştiren her istek (`PUT`, işlem `POST`'u, `DELETE`) `If-Match: "7"` gönderir. |
| Sürüm aynıysa | İşlem yapılır; yanıtta yeni `ETag` ve güncel kayıt döner. |
| Sürüm değişmişse | `412 Precondition Failed`, `code: concurrencyConflict`; ön yüz kaydın güncel halini gösterir. |
| `If-Match` yoksa | `428 Precondition Required`, `code: versionRequired` |

- Sürüm, [database §11.1](database.md#111-sürüm-numarasıyla-iyimser-kilit)'deki toplu kök sürümüdür. ETag güçlü bir doğrulayıcıdır ve yalnızca bu sayıdan oluşur.
- Oluşturma isteklerinde `If-Match` yoktur.
- **Neden başlık, gövde alanı değil:** `If-Match` ve `412` HTTP'nin kendi mekanizmasıdır; durum geçişi gibi gövdesi boş isteklerde de aynı biçimde çalışır ve aradaki katmanlar bu başlıkları tanır ([kaynak](https://sookocheff.com/post/api/optimistic-locking-in-a-rest-api/)). Sürümün gövdede de dönmesi, ön yüzün yanıt başlığını okumadan sürümü bilmesi içindir.
- Sürüm gerektiren uç noktalar OpenAPI'de `If-Match` başlığını zorunlu parametre olarak gösterir. Üretilen istemci bu yüzden sürümü unutmaya izin vermez.

## 10. Tekrar güvenliği

Depo çalışanı zayıf bağlantıda okuttuğunda istek sunucuya ulaşıp işlenebilir ama cevap kaybolabilir. Telefon isteği yeniden gönderir; adetli bir düzeltme (+2 kablo) iki kez işlenir. Bunu önlemek için değiştiren istekler bir **tekrar güvenliği anahtarı** taşır ([ADR-0025](../adr/0025-idempotency-keys.md)). Yöntem, IETF'in `Idempotency-Key` taslağını izler ([kaynak](https://www.ietf.org/archive/id/draft-ietf-httpapi-idempotency-key-header-07.html)). Taslak henüz RFC olmasa da Stripe gibi API'lerin fiili standardıdır.

| Kural | Açıklama |
|---|---|
| Kapsam | **Tüm değiştiren istekler** (`POST`, `PUT`, `DELETE`) `Idempotency-Key` başlığı taşır. |
| Anahtarı kim üretir | Ön yüzün istek sarmalayıcısı, her kullanıcı işlemi için otomatik (`crypto.randomUUID()`). Aynı işlemin yeniden denemeleri aynı anahtarı kullanır. Geliştirici elle bir şey yapmaz. |
| Anahtar yoksa | `400`, `code: idempotencyKeyMissing` |
| Aynı anahtar, aynı içerik, işlem tamamlanmış | İlk yanıt aynen döner; `Idempotency-Replayed: true` başlığıyla |
| Aynı anahtar, farklı içerik | `422`, `code: idempotencyKeyReused` |
| Aynı anahtarla ilk istek hâlâ işleniyor | İkinci istek ilkinin bitmesini bekler ve onun yanıtını alır. Bekleme kilit zaman aşımını geçerse `409`, `code: idempotencyKeyInProgress` |
| Saklama süresi | 24 saat. Kapsam kullanıcı + anahtardır. |

**Nasıl çalışır:**
- Anahtar, modülün şemasındaki `idempotency_keys` tablosuna, komutun **kendi işlem biriminin ilk adımı** olarak yazılır. Yanıt da aynı satıra, aynı işlemde kaydedilir. Böylece işlem ya sonucuyla birlikte kalıcı olur ya da hiç olmaz.
- Aynı anahtarla eşzamanlı gelen ikinci istek, benzersizlik kısıtında ilkinin bitmesini bekler. İlki tamamlanınca saklanan yanıtı döndürür; ilki başarısız olduysa kendisi çalışır.
- İş kuralı hatasıyla biten istek geri alındığı için anahtar saklanmaz; yeniden deneme yeniden çalışır.
- Karşılaştırma için isteğin parmak izi (yöntem, adres ve gövdenin özeti) saklanır.
- **Anahtar denetimi, sürüm denetiminden (`If-Match`) önce yapılır.** Aksi halde tekrarlanan başarılı istek, saklanan yanıta ulaşmadan `412` alır.

**Hayalet çakışmayı da önler:** Başarılı bir `PUT`'un cevabı kaybolup istek yeniden gönderilirse, kayıt artık yeni sürümdedir ve `If-Match` tutmaz. Anahtar olmasaydı kullanıcı **kendi değişikliği yüzünden** "başkası değiştirdi" hatası görürdü. Anahtar sayesinde saklanan başarılı yanıt döner. Bu yüzden kapsam yalnızca oluşturma değil, tüm değiştiren isteklerdir.

## 11. Güvenlik kuralları

Oturum [ADR-0011](../adr/0011-authentication.md)'deki gibi sunucu tarafı oturum ve çerezle taşınır. API'ye özgü kurallar:

| Konu | Kural |
|---|---|
| Oturum çerezi | `HttpOnly`, `Secure`, `SameSite=Strict` |
| İstek sahteciliği (CSRF) | Üç katman ([kaynak](https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html)): (1) `SameSite=Strict` çerez; (2) değiştiren isteklerde antiforgery belirteci `X-XSRF-TOKEN` başlığında, Host'taki ara katmanda doğrulanır; (3) tarayıcının `Sec-Fetch-Site: cross-site` bildirdiği değiştiren istekler reddedilir. Belirteç, giriş ve `/api/v1/me` yanıtlarında JavaScript'in okuyabildiği bir çerezle verilir; istek sarmalayıcısı onu başlığa kopyalar. ASP.NET Core, JSON gövdeli Minimal API uç noktalarında belirteci kendiliğinden doğrulamaz; bu yüzden doğrulama açıkça ara katmanda yapılır ([kaynak](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0)). |
| CORS | Kapalı. Ön yüz ve API aynı adrestedir; başka kaynaktan çağrıya izin verilmez. |
| Kimliği doğrulanmamış istek | `401` ve Problem Details döner; giriş sayfasına yönlendirme (302) yapılmaz. |
| Anonim uç noktalar | Yalnızca giriş, antiforgery belirteci alma ve sağlık kontrolleri (AT-09) |
| İstek sınırı | Giriş uç noktasında IP başına ve hesap başına ayrı sınır; aşılınca `429` ve `Retry-After` ([kaynak](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0)). Hesap kilidi (BR-SYS-005) alan kuralı olarak ayrıca işler. |
| Yanıt başlıkları | Tüm `/api` yanıtlarında `Cache-Control: no-store` (kişisel ve güncel veri tarayıcıda ya da arada önbelleğe alınmaz) ve `X-Content-Type-Options: nosniff` |
| HTTPS | Yalnızca HTTPS; yönlendirme ve HSTS ters proxy'de (D bölümü) |

## 12. Sürümleme ve uyumluluk

- Adres kökü `/api/v1`'dir ([A-03](#15-kararlar)). Sürüm kütüphanesi (Asp.Versioning) şimdilik eklenmez. `v2` ancak kaçınılmaz bir kırıcı değişiklikte açılır; o zaman kütüphane eklenir ([kaynak](https://devblogs.microsoft.com/dotnet/api-versioning-in-dotnet-10-applications/)).
- **Eklemeler kırıcı değildir:** yeni uç nokta, yanıtta yeni alan, isteğe bağlı yeni istek alanı. Bunlar sürüm değiştirmez.
- **Kırıcı değişiklik iki yayında yapılır:** önce yeni alan ya da uç nokta eklenir ve ön yüz ona geçer; eski olan bir sonraki yayında kaldırılır. Kaldırılacak uç nokta yanıtlarında `Deprecation` başlığı bulunur.
- **Açık kalan eski sekmeler:** Yeni sürüm yayınlandığında kullanıcının açık sekmesi eski ön yüz koduyla çalışmaya devam eder ve yeni API'yi yanlış çağırabilir ([kaynak](https://www.codemzy.com/blog/clients-reload-single-page-application-update)). Bu yüzden:
  - Her API yanıtı `X-App-Version` başlığında sunucunun sürümünü taşır; ön yüz kendi derleme sürümüyle karşılaştırır.
  - Sürüm farklıysa ekranın üstünde kapatılamayan bir şerit gösterilir: "Yeni sürüm hazır — Yenile". Sayfa zorla yenilenmez; kullanıcı elindeki işi (form, okutma) bitirip kendisi yeniler ([A-02](#15-kararlar)).
  - Kırıcı değişikliklerin iki yayına bölünmesi, eski sekmenin bir yayın boyunca çalışmaya devam etmesini sağlar.

## 13. Anlık bildirimler

[ADR-0012](../adr/0012-realtime-signalr.md)'deki "bildir, sonra yeniden oku" yaklaşımının sözleşmesi:

- Hub adresi `/hubs/notifications`, gruplar [naming §8.3](naming.md#83-anlık-bildirim-signalr)'tedir.
- İstemciye tek bir mesaj türü gider: `resourceChanged`.

```json
{ "resource": "events", "id": "0192f0c5-…", "version": 8 }
```

- Mesaj **veri taşımaz**, yalnızca neyin değiştiğini söyler. İstemci ilgili sorguları geçersiz kılar ve API'den yeniden okur. Böylece yetki kontrolü her zaman API'de yapılır; bildirim, kullanıcının görmemesi gereken veriyi sızdıramaz.
- `version` alanı, istemcinin elindeki kayıt zaten o sürümdeyse (ör. değişikliği kendisi yaptıysa) gereksiz yeniden okumayı atlamasını sağlar.
- Bağlantı koparsa ekran uyarı gösterir; bağlantı geri gelince etkin tüm sorgular yeniden okunur (BR-SYS-012).

## 14. OpenAPI, istemci üretimi ve uç nokta yazımı

### 14.1 OpenAPI belgesi

- Belge .NET 10'un dahili üreticisiyle, OpenAPI **3.1** biçiminde üretilir ve derlemede dosyaya yazılır ([08 §10](../08-architecture.md#10-derleme-ayarları)).
- Şema dönüştürücüleri (BuildingBlocks) şunları garanti eder:
  - enum'lar metin (`type: string`) ve camelCase değerli,
  - tip güvenli kimlikler `string` / `uuid`,
  - `decimal` değerler `string` / `decimal`,
  - `required` ve boş olabilirlik C# tipinden birebir,
  - sürüm gerektiren uç noktalarda zorunlu `If-Match`, değiştiren tüm uç noktalarda zorunlu `Idempotency-Key` başlığı,
  - tüm hata yanıtları Problem Details şemasıyla.
- **Sözleşme testi:** .NET'in 3.1 çıktısındaki boş olabilen dizi gibi bazı biçimler istemci üreteçlerinde sorun çıkarabiliyor ([kaynak](https://github.com/cyclosproject/ng-openapi-gen/issues/410)). Bu yüzden sürekli entegrasyonda örnek bir uç nokta kümesinden (boş olabilen dizi, boş olabilen nesne, enum, ondalık, tip güvenli kimlik) istemci üretilir ve tip denetiminden geçirilir. Üreteç 3.1'de sorun çıkarırsa belge tek satırlık bir ayarla 3.0'a çevrilir.

### 14.2 Ön yüz istemcisi

- Orval, OpenAPI belgesinden TanStack Query kancalarını ve Zod şemalarını üretir; modül etiketine göre klasörler (`tags-split`).
- Tüm istekler tek bir istek sarmalayıcısından (Orval "mutator") geçer. Sarmalayıcı:
  - antiforgery belirtecini ve değiştiren isteklere tekrar güvenliği anahtarını ekler,
  - `X-App-Version` başlığını kontrol eder,
  - Problem Details yanıtlarını tipli bir hata nesnesine çevirir,
  - yalnızca ağ hatalarında aynı anahtarla yeniden dener.
- Üretilen kod depoya eklenir ve elle değiştirilmez; sürekli entegrasyon güncelliğini denetler.

### 14.3 Uç nokta yazım kuralları

- Her modül uç noktalarını kendi grubunda tanımlar ([08 §3.4](../08-architecture.md#34-proje-içi-klasörler)); grup OpenAPI etiketini ve yetki gereksinimini taşır.
- İşleyici adlı bir metottur ([code-style §4.8](code-style.md#48-yorumlar-ve-xml-belgeleri)) ve yalnızca şunu yapar: isteği komut ya da sorguya çevirir, gönderir, sonucu HTTP yanıtına çevirir. İş mantığı içermez.
- Dönüş tipi tipli sonuç birleşimidir (`Results<Ok<EventDetails>, NotFound>`). Böylece olası yanıtlar OpenAPI'de otomatik görünür.
- Sorgu parametreleri tek bir kayıtta (`[AsParameters]`) toplanır.
- İptal belirteci (`CancellationToken`) her işleyiciye alınır ve komuta iletilir; istemci bağlantıyı kapatınca iş durur.

### 14.4 Denetim

| Kural | Denetleyen |
|---|---|
| Benzersiz işlem adı, modül etiketi, açıklama (AT-14); adres kalıbı + yöntemin tekilliği ve kalıbın tek modüle ait olması | Mimari test AT-14 (genişletildi) |
| Toplu kök değiştiren uç noktada `If-Match`, değiştiren her uç noktada `Idempotency-Key` bildirimi | Mimari test AT-15 |
| Hata yanıtlarının biçimi (Problem Details, `code`, `traceId`) | Entegrasyon testleri |
| Üretilen istemcinin güncelliği ve tip denetimi | Sürekli entegrasyon |

## 15. Kararlar

| No | Konu | Karar | Gerekçe |
|---|---|---|---|
| A-01 | Liste sayfalama biçimi | Karma: ofis tablolarında sayfa numaralı (toplam sayıyla), büyüyen kayıtlarda ve mobil listelerde imleçli | §6.1 |
| A-02 | Yeni sürüm yayınlandığında açık sekme | Kapatılamayan bir şerit: "Yeni sürüm hazır — Yenile"; zorla yenileme yok | §12 |
| A-03 | Adreste sürüm | `/api/v1`; sürüm kütüphanesi şimdilik yok | §12 |
| A-04 | Adreste modül adı | Yok; kaynak adları sözlükten ve tekil | §3 |
| A-05 | Durum geçişi biçimi | `POST /{kaynak}/{kimlik}/{fiil}` | §3 |
| A-06 | Kısmi güncelleme | `PATCH` yok; `PUT` + işlem uç noktaları | §4.1 |
| A-07 | Hata biçimi | RFC 9457 Problem Details + `code`, `params`, `errors`, `traceId` ([ADR-0023](../adr/0023-api-error-model.md)) | §8 |
| A-08 | Durum kodları | Biçim hatası 400; tüm iş kuralı ihlalleri 422; sürüm çakışması 412; tekrar güvenliği 409 / 422 | §4.2 |
| A-09 | HTTP üzerinden iyimser kilit | `ETag` / `If-Match`, `412` / `428` ([ADR-0024](../adr/0024-http-optimistic-concurrency.md)) | §9 |
| A-10 | Tekrar güvenliği | Tüm değiştiren isteklerde `Idempotency-Key`, istemci sarmalayıcısında otomatik ([ADR-0025](../adr/0025-idempotency-keys.md)) | §10 |
| A-11 | Ondalık sayılar JSON'da | Metin | §5.2 |
| A-12 | Süreler JSON'da | Dakika cinsinden tamsayı | §5.2 |
| A-13 | JSON katılığı | Bilinmeyen ve tekrarlanan alan reddedilir; `null` her zaman yazılır | §5.1 |
| A-14 | Filtre dili | Adlandırılmış parametreler; genel sorgu dili yok | §6.3 |
| A-15 | CSRF | `SameSite=Strict` + antiforgery başlığı + `Sec-Fetch-Site` kontrolü | §11 |
| A-16 | Anlık bildirim içeriği | Yalnızca kaynak, kimlik, sürüm; veri yok | §13 |
| A-17 | OpenAPI sürümü | 3.1 + istemci sözleşme testi; sorun çıkarsa 3.0 | §14.1 |

## 16. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-09-25 | v0.1 | İlk taslak |
| 2026-09-25 | v1.0 | A-01, A-02, A-03 kararlaştırıldı; ADR-0023, ADR-0024, ADR-0025 kabul edildi. |
