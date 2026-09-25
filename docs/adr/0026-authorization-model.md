# ADR-0026: Yetki modeli — kodda tanımlı yetki kataloğu ve sabit roller

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [standards/security.md §3](../standards/security.md#3-yetki-modeli), [ADR-0011](0011-authentication.md), BR-SYS-002, BR-SYS-003, BR-SYS-004, US-SYS-003

## Bağlam

- Kullanıcının yetkileri rollerinin birleşimidir ve her kontrol sunucuda yapılır (BR-SYS-002).
- S1'de roller sabittir; rol–yetki matrisi arayüzde salt okunur gösterilir (US-SYS-003).
- Yetkiler modüllere aittir, ama rolleri Identity modülü yönetir. Identity diğer modüllere referans veremez ([ADR-0002](0002-module-boundaries-and-layers.md)).
- Depo sorumlusunun yetkisi kayda göre değişir: okutma işlemlerini yalnızca bağlı olduğu depolarda yapabilir (BR-SYS-003).
- Genel müdür hiçbir şeyi değiştiremez (BR-SYS-004).

## Karar

- **Yetkiler** sahibi modülün kodunda sabit olarak tanımlanır (`Modül.KaynakÇoğul.Eylem`). Modül, yetkilerini modül tanımıyla Host'a bildirir; Host bunları yetki kataloğunda toplar ve Identity'ye verir.
- **Roller** Identity modülünün kodunda, yetki kodlarının kümesi olarak tanımlanır. Veritabanında yalnızca kullanıcıların rol ve depo atamaları tutulur.
- Her uç nokta tek bir yetki ister. Yetki kodu, katalogdan dinamik üretilen bir ASP.NET Core yetki politikasına dönüşür.
- Kayda bağlı kontroller (depo kapsamı) uygulama katmanındaki işleyicide yapılır; ihlal `403` ve kural numarası döner.
- Rol ve depo ataması değişince kullanıcının açık oturumlarındaki yetki bilgisi aynı işlemde güncellenir.
- Matrisin tutarlılığı testlerle korunur: sahipsiz yetki yok, var olmayan yetki kodu yok, genel müdürde yalnızca görüntüleme.

## Sonuçlar

**Olumlu:**
- Rol–yetki matrisi sürüm kontrolündedir; her değişiklik PR'da görünür ve testlerle doğrulanır.
- Yeni bir uç nokta, yetkisi katalogda ve en az bir rolde olmadan yayına çıkamaz.
- Identity modülü diğer modüllere bağımlı olmadan tüm yetkileri bilir.
- Genel müdürün salt okunurluğu bir test kuralıdır; yanlışlıkla değiştirme yetkisi verilemez.

**Olumsuz / bedeli:**
- Rolleri değiştirmek bir kod değişikliği ve yeni yayın gerektirir. S1 için bu, gereksinimle uyumludur (US-SYS-003).
- İleride düzenlenebilir roller istenirse rol tanımları veritabanına taşınır. Değişiklik Identity modülünün içinde kalır; uç noktalar ve yetki kodları değişmez.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Rol adıyla kontrol (`[Authorize(Roles = "...")]`) | Rol ile işlem birbirine bağlanır; yeni bir rol eklemek tüm uç noktaları değiştirmeyi gerektirir. Matris okunamaz. |
| Roller ve yetkiler baştan veritabanında, düzenlenebilir | S1'de gereksinim yok; hatalı bir düzenleme bir rolü sessizce yetkisiz ya da fazla yetkili bırakabilir. Kodda tanım testle korunur. |
| Kayda bağlı kontrolü de uç noktada yapmak | Hangi depo olduğu ancak istek okunup kayıt bulunduktan sonra bellidir; uç noktada iş verisi okumak gerekir. |
| Yetkileri çerezin içinde taşımak | Yetki değişikliği çerezin süresi dolana kadar geçerli olmaz; sunucu tarafı oturum bunu anında günceller. |
