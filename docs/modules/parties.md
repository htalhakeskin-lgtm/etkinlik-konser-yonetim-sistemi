# Modül tasarımı: Parties — Taraflar

> **Durum:** v1.2 (onaylandı) · **Son güncelleme:** 2026-10-02
> **Adım:** Faz 1.3 ([12 §3](../12-implementation-plan.md#3-adımlar)) · **Kararlar:** [Bölüm 11](#11-kararlar)

## 1. Bu belge ne işe yarar

Parties modülünün fiziksel tasarımıdır: tablolar ve kısıtlar, kurallar, yetkiler, uç noktalar, senkron sözleşme, ekranlar, hikaye ve kural eşlemesi, PR planı. Faz 1.3'ün dört modülüne ortak kararlar da buradadır ([Bölüm 2](#2-adımın-ortak-kararları)); diğer belgeler bunlara bağlantı verir.

**Kaynaklar:** [05 §5.3](../05-module-map.md#53-parties--taraflar), [06 §5.3](../06-erd-conceptual.md#53-parties--taraflar), [01 §3.2](../01-glossary.md#32-kişi-ve-firma), [11 §3](../11-screens.md#3-s1-ekran-envanteri); hikayeler US-PTY-001, 002, US-ART-001 (sanatçı ve ajans kısmı); kurallar BR-SYS-001, BR-PTY-001…004.

Aynı adımın diğer belgeleri: [catalog.md](catalog.md) (kategoriler, modeller, kitler), [venues.md](venues.md) (mekanlar ve mekan ekipmanı), [riders.md](riders.md) (prodüksiyonlar ve rider versiyonları).

## 2. Adımın ortak kararları

Dört modül de Identity ve Inventory'nin kalıbını izler: komutlar Application'da depo arayüzüyle, sorgular Infrastructure'da EF izdüşümüyle ([identity ID-01](identity.md#12-kararlar)); pasifleştirme ortak kolonlarla ([database §10.1](../standards/database.md#101-pasifleştirme)); adlar arama anahtarıyla aranır ve Türkçe sırayla sıralanır ([database §13](../standards/database.md#13-metin-sıralama-ve-arama)); değiştiren uçlar `If-Match` ister. Bunlara ek olarak:

| No | Konu | Karar | Gerekçe |
|---|---|---|---|
| MD-01 | Alt varlıkların işlem geçmişi | Geçmiş satırı, değişen varlığın yanında **kök kaydı** da taşır (`root_type`, `root_id`). Kayıt geçmişi ve genel ekrandaki kayıt türü süzgeci köke göre çalışır. Kök, alt varlığın `CASCADE` yabancı anahtar zinciri izlenerek bulunur; toplu kökte kök kendisidir. Var olan satırlar migration'da kendileriyle doldurulur. | Yazıcı her varlığa ayrı satır yazar ([building-blocks §6](building-blocks.md#6-i̇şlem-geçmişi-yazıcısı)). Tarafın iletişim bilgisi, kitin satırı ya da mekanın ekipmanı değişince satır alt varlığın kimliğiyle yazılır ve kaydın "Geçmiş" sekmesinde görünmezdi. 1.2'nin kayıtlarında alt varlık olmadığı için sorun çıkmadı. |
| MD-02 | Başka modüldeki kaydın adı | Yanıtlar, başvurulan kaydın adını sahibi modülün senkron sözleşmesinden toplu olarak alıp döner (ör. mekan yanıtında işletmecinin adı). Yerel kopya tutulmaz. | Katmanlar buna izin verir ([05 §4.3](../05-module-map.md#43-i̇zin-verilen-senkron-çağrılar)); ön yüz ikinci bir istek atmaz ve kullanıcının diğer modülü görme yetkisine bakılmaz. Ad her okumada günceldir. |
| MD-03 | Seçim listeleri | Seçim kutuları ayrı `…Option` uçları yerine liste uçlarını `status=active` ve `q` ile çağırır. Yeni ortak bileşen: yazdıkça arayan seçim kutusu (Base UI `Combobox`). | Uç sayısı artmaz; pasif kayıt yeni işlemde seçilemez (BR-SYS-001). İlk kullanım mekan işletmecisi ve iletişim kişisi seçimidir. |
| MD-04 | Rol matrisindeki kodlar | Identity'nin `RoleCatalog`'u bu adımın yetkilerini yazıyla verir, Audit'teki gibi. | Identity yalnızca Inventory'nin Contracts'ına başvurabilir ([08 §3.3](../08-architecture.md#33-modüller-arası-referanslar)); kodun katalogda olduğunu matris testi denetler. |
| MD-05 | Alan doğrulamasını tekrarlayan kısıtlar | Kural taşıyan kontrol, benzersizlik ve dışlama kısıtları kurala eşlenir. Yalnızca alan doğrulamasını tekrarlayan `CHECK`'ler (adet ≥ 1, ölçü > 0) DT-04 istisna listesine gerekçesiyle girer. Basit benzersizlikler, Identity ve Inventory'deki gibi numaralı kural olur. | DT-04 her kısıtın bir kural koduna eşlenmesini ister ([database §17.1](../standards/database.md#171-veritabanı-testleri)); kuralsız alan doğrulaması için yeni kural numarası açmak kataloğu şişirirdi. |
| MD-06 | Anlık bildirim | Yalnızca bütünleşme olayı olan değişiklikler duyurulur (bu adımda `VenueEquipmentChanged`, `RiderVersionCreated`). Diğer ekranlar kendi değişikliklerinden sonra yeniden okur. | Duyuru olay yolundan çıkar ([building-blocks §11](building-blocks.md#11-anlık-bildirimler)); depolardaki kararla aynıdır. |
| MD-07 | Menü ve başlangıç ekranı | Yeni menü grupları: **Ana veriler** (Taraflar, Sanatçılar, Mekanlar) ve **Katalog** (Modeller, Kategoriler, Kitler). Booking müdürü geçici olarak Taraflar'a, teknik müdür Modeller'e yönlenir; asıl başlangıç ekranları (Etkinlikler, Çakışmalar) gelince değişir. | 11 §4'teki ekranlar 1.4 ve 1.6'dadır; o zamana kadar boş ana sayfa yerine iş ekranı açılır. |

## 3. Kapsam

| Var | Yok (sonraki sürüm) |
|---|---|
| Kişi ve firma tarafları, roller, iletişim bilgileri, iletişim kişileri, temsil (ajans–sanatçı) | Crew (S2) ve Sponsor (S3) rolleri, mükerrer kayıt birleştirme (S6) |
| Arama, rol süzgeci, pasifleştirme ve yeniden etkinleştirme | Vergi ve fatura bilgileri (S4) |
| `IPartyDirectory` sözleşmesi ve BR-PTY-004 denetimi | Olay yayınlama (S1'de yok) |
| Taraflar ve sanatçılar ekranları | Prodüksiyonlar Riders'tadır ([riders.md](riders.md)) |

## 4. Projeler

`FestOS.Modules.Parties.{Domain, Application, Contracts, Infrastructure}`. S1'de olay yayınlamadığı için `IntegrationEvents` projesi yoktur. Şema `parties`, veritabanı rolü `festos_parties`.

## 5. Veritabanı (`parties` şeması)

| Tablo | Kolonlar (ortak kolonlar hariç) | Kısıtlar ve indeksler |
|---|---|---|
| `parties` | `id`, `kind` (`person` / `organization`), `name varchar(200)`, `first_name varchar(100) null`, `last_name varchar(100) null`, `legal_name varchar(200) null`, `roles text[]`, `search text`, `deactivated_at`, `deactivated_by`, `version` | `ck_parties_kind_names`: kişide ad ve soyad dolu, unvan boş; firmada ad ve soyad boş; `ck_parties_roles_present`: en az bir rol (ikisi de BR-PTY-001'e eşlenir). `ix_parties_roles` (GIN, rol süzgeci), `ix_parties_search` (`pg_trgm` GIN) |
| `contact_points` | `id`, `party_id`, `kind` (`phone` / `email` / `address`), `value varchar(500)`, `label varchar(100) null`, `is_primary bool`, `sort_order int` | FK `parties` `CASCADE`; tek birincil toplu kökte korunur (BR-PTY-002, PT-08) |
| `organization_contacts` | `id`, `organization_id`, `person_id`, `title varchar(100) null`, `sort_order int` | FK `organization_id` → `parties` `CASCADE` (firmanın parçası), FK `person_id` → `parties` `RESTRICT` |
| `artist_representations` | `id`, `artist_id`, `agency_id`, `description varchar(200) null` | FK `artist_id` → `parties` `CASCADE` (sanatçının parçası), FK `agency_id` → `parties` `RESTRICT` |

- `Party` tek toplu köktür ([06 §7](../06-erd-conceptual.md#7-toplu-kökler-ve-eşzamanlılık)). Kişi ve firma ayrı tablolar değil, aynı tablonun türe göre dolan kolonlarıdır (PT-01). İletişim kişileri firmanın, temsiller sanatçının toplu köküne aittir.
- **Ad:** `name` listede ve seçim kutularında görünen addır. Kişide formda ad ve soyaddan önerilir, değiştirilebilir (sahne adı: "Tarkan"); firmada kısa addır, ticari unvan `legal_name`'dedir (PT-02).
- **Roller** `text[]` dizisidir (`customer`, `supplier`, `artist`, `agency`, `venueOperator`, `contact`; sözcük [01 §3.2](../01-glossary.md#32-kişi-ve-firma)'dedir). Identity'deki roller gibi küçük ve sabit bir kümedir; işlem geçmişi değişikliği tek alanda eski → yeni gösterir ([identity ID-10](identity.md#12-kararlar), PT-03). database §6'daki "iş verisinde dizi kullanılmaz" kuralına bu iki kullanım istisna olarak eklenir.
- **Arama anahtarı** (`search`): ad, unvan ve iletişim bilgilerinin değerlerinden üretilir; telefonların yalnızca rakamlardan oluşan biçimi de eklenir ("5321112233" yazan "+90 532 111 22 33"ü bulur). Alan katmanı her değişiklikte yeniden üretir; işlem geçmişine yazılmaz (PT-04).
- İletişim bilgisinin değeri türüne göre doğrulanır: telefon en çok 32 karakter, rakam, boşluk, `+`, `-`, `(`, `)` ve 7–15 rakam; e-posta geçerli biçim, en çok 254 karakter, küçük harfe çevrilir; adres en çok 500 karakter.

## 6. Kurallar

| Kural | Uygulama |
|---|---|
| BR-SYS-001 Silme yerine pasifleştirme | Taraf silinmez; pasifleştirilir ve yeniden etkinleştirilebilir. Pasif taraf yeni iletişim kişisi, temsil ve başka modüllerdeki seçimlerde seçilemez; bağlı kayıtlarda "Pasif" rozetiyle görünmeye devam eder. İletişim bilgisi, iletişim kişisi bağı ve temsil modül içi kayıtlardır, kaldırılabilir. |
| BR-PTY-001 Taraf rolleri | En az bir rol; aynı rol bir kez (alan kuralı). Tür oluşturulduktan sonra değişmez: düzenleme ucu türü almaz. Başka rolü olmayan iletişim kişisi **İletişim kişisi** rolündedir (PT-07); iletişim kişisi diyaloğunda açılan yeni kişi bu rolle gelir. |
| BR-PTY-002 Birincil iletişim bilgisi | Kaydı olan her türde tam olarak bir birincil kayıt bulunur: türün ilk kaydı kendiliğinden birincil olur; birincil kaldırılınca aynı türün sıradaki kaydı birincil olur; aynı türde iki birincil gönderilirse istek reddedilir. Kural toplu kökün içindedir; tarafın sürümü eşzamanlı iki düzenlemeyi ayırır (PT-08). |
| BR-PTY-003 İletişim kişisi | Yalnızca firma türündeki tarafa, yalnızca kişi türündeki ve aktif bir taraf bağlanır; aynı kişi bir firmaya bir kez bağlanır. Komut iki tarafı da yükler (aynı modül). |
| BR-PTY-004 Rol gerektiren seçimler | Temsilde sanatçı **Sanatçı**, ajans **Ajans** rolünde ve aktif olmalıdır; aynı ajans bir sanatçıya bir kez bağlanır. Temsili olan sanatçıdan Sanatçı, temsil ettiği sanatçısı olan ajanstan Ajans rolü kaldırılamaz (kurala eklenir). Diğer modüllerdeki seçimler (mekan işletmecisi, prodüksiyonun sanatçısı) `IPartyDirectory` ile aynı kuralı uygular; o modüldeki kayıt, rol sonradan kaldırılsa da değişmez. |

## 7. Yetkiler ve roller

| Yetki | SY | BM | TM | DS | GM |
|---|---|---|---|---|---|
| `Parties.Parties.View` | | ✓ | ✓ | | ✓ |
| `Parties.Parties.Create` | | ✓ | | | |
| `Parties.Parties.Edit` | | ✓ | | | |
| `Parties.Parties.Deactivate` | | ✓ | | | |

Teknik müdür, mekan ve sanatçı ekranlarında tarafların adını ve iletişim bilgilerini görür. Depo sorumlusu, tedarikçiyi göreceği dış kiralama adımında (1.8) eklenir. İletişim kişisi ve temsil işlemleri tarafı düzenlemek sayılır (`Edit`).

## 8. Uç noktalar

| Yöntem ve adres | İşlem adı | Yetki | Not |
|---|---|---|---|
| `GET /api/v1/parties` | `ListParties` | `Parties.Parties.View` | Sayfalı; `q` (ad, unvan, iletişim bilgisi), `role`, `kind`, `status` (`active` varsayılan); sıralama `name`, `createdAt` |
| `GET /api/v1/parties/{partyId}` | `GetParty` | `Parties.Parties.View` | İletişim bilgileri, iletişim kişileri, temsiller ve karşı tarafların adları; ajansta temsil ettiği sanatçılar, kişide çalıştığı firmalar (salt okunur); `ETag` |
| `POST /api/v1/parties` | `CreateParty` | `Parties.Parties.Create` | Tür, adlar, roller, iletişim bilgileri; `201` + `Location` |
| `PUT /api/v1/parties/{partyId}` | `EditParty` | `Parties.Parties.Edit` | Adlar, roller ve iletişim bilgilerinin tamamı; var olan iletişim bilgisi kimliğiyle gelir, geçmişte silinip eklenmiş değil değişmiş görünür; `If-Match` |
| `POST /api/v1/parties/{partyId}/deactivate` | `DeactivateParty` | `Parties.Parties.Deactivate` | `If-Match` |
| `POST /api/v1/parties/{partyId}/activate` | `ActivateParty` | `Parties.Parties.Deactivate` | `If-Match` |
| `POST /api/v1/parties/{partyId}/contact-persons` | `AddContactPerson` | `Parties.Parties.Edit` | Kişi ve görevi; BR-PTY-003; `If-Match` (firmanın sürümü) |
| `PUT /api/v1/parties/{partyId}/contact-persons/{contactId}` | `EditContactPerson` | `Parties.Parties.Edit` | Görev; `If-Match` |
| `DELETE /api/v1/parties/{partyId}/contact-persons/{contactId}` | `RemoveContactPerson` | `Parties.Parties.Edit` | Bağ kaldırılır, kişi kalır; `If-Match` |
| `POST /api/v1/parties/{partyId}/representations` | `AddRepresentation` | `Parties.Parties.Edit` | Ajans ve açıklama; BR-PTY-004; `If-Match` (sanatçının sürümü) |
| `PUT /api/v1/parties/{partyId}/representations/{representationId}` | `EditRepresentation` | `Parties.Parties.Edit` | Açıklama; `If-Match` |
| `DELETE /api/v1/parties/{partyId}/representations/{representationId}` | `RemoveRepresentation` | `Parties.Parties.Edit` | `If-Match` |

Doğrulama: tür zorunlu; kişide ad ve soyad zorunlu (en çok 100); görünen ad zorunlu (en çok 200); en az bir rol; iletişim bilgisi en çok 20; değer türüne göre (§5).

## 9. Sözleşme ve olaylar

- **Senkron sözleşme (Contracts):** `IPartyDirectory.FindAsync(ids)` → `PartySummary(Id, Name, Kind, Roles, IsActive)`. `PartySelection.Ensure(summary, role, field)` kaydı yoksa, pasifse ya da rolü yoksa `422 BR-PTY-004` fırlatır; `params` alanı ve beklenen rolü söyler. Venues ve Riders bu adımda kullanır; Booking ve Procurement sonraki adımlarda.
- **Olaylar:** S1'de yayınladığı ve dinlediği olay yoktur ([05 §5.3](../05-module-map.md#53-parties--taraflar)).

## 10. Ekranlar, hikayeler ve PR planı

| Ekran | Adres | Not |
|---|---|---|
| Taraflar | `/parties` | Liste: ad, tür, roller (rozet), birincil telefon ve e-posta; arama, rol, tür ve durum süzgeci adreste. "Taraf ekle" diyaloğu: tür seçimi, adlar, roller, iletişim bilgileri (satır ekle, birincil işaretle). |
| Taraf detayı | `/parties/{id}` | Başlık: ad, tür, roller, durum, işlemler (Düzenle, Pasifleştir / Etkinleştir). Bölümler: İletişim bilgileri; İletişim kişileri (firmada; kişi seçimi ya da aynı diyalogda yeni kişi); Temsil (sanatçıda ajanslar, ajansta temsil ettiği sanatçılar); kişide çalıştığı firmalar. Sekmeler: Genel, Geçmiş. |
| Sanatçılar | `/artists`, `/artists/{id}` | Sanatçı rolündeki taraflar. Liste `ListParties`'i `role=artist` ile çağırır; "Sanatçı ekle" taraf diyaloğunu Sanatçı rolü seçili açar. Detay; taraf bilgileri, ajanslar ve Riders'ın prodüksiyonlarını bir araya getirir ([riders.md](riders.md)). |

| Hikaye | Uç noktalar ve ekranlar | Kurallar |
|---|---|---|
| US-PTY-001 Taraf oluşturma | `CreateParty`, `AddContactPerson`; taraf diyaloğu, detay | BR-PTY-001, 002, 003 |
| US-PTY-002 Arama, düzenleme, pasifleştirme | `ListParties`, `EditParty`, `DeactivateParty`, `ActivateParty`; liste, Geçmiş sekmesi | BR-SYS-001 |
| US-ART-001 Sanatçı (sanatçı ve ajans kısmı) | `AddRepresentation`; sanatçılar | BR-PTY-004 |

| # | PR | Kapsam |
|---|---|---|
| 0 | Alt varlıkların işlem geçmişi | BuildingBlocks yazıcısında kök bulma, Audit migration'ı (`root_type`, `root_id`, indeks), liste süzgeçleri, ön yüzde kayıt geçmişinin köke göre okunması (MD-01) |
| 1a | Parties iskeleti | Projeler, şema, rol, tablolar, yetkiler ve rol matrisi, `Party` alan kuralları (BR-PTY-001, 002) |
| 1b | Taraflar (sunucu) | `ListParties`, `GetParty`, oluşturma, düzenleme, pasifleştirme, etkinleştirme, arama anahtarı |
| 1c | İletişim kişileri ve temsil (sunucu) | Bağ uçları, BR-PTY-003, BR-PTY-004, `IPartyDirectory` |
| 2a | Taraflar ekranı | `/parties` listesi, taraf diyaloğu, menü grubu "Ana veriler" |
| 2b | Taraf detayı | `/parties/{id}`, yazdıkça arayan seçim kutusu (MD-03), iletişim kişisi ve temsil diyalogları, Geçmiş sekmesi |
| 2c | Sanatçılar ekranı | `/artists` listesi ve detayın taraf kısmı; prodüksiyon bölümü Riders'ın PR'ıyla gelir |

## 11. Kararlar

| No | Konu | Karar | Gerekçe |
|---|---|---|---|
| PT-01 | Kişi ve firma | Tek tablo, türe göre dolan kolonlar ve `CHECK` | 06'daki `Person` ve `Organization` ikişer nitelik taşır; ayrı tablolar her okumada birleştirme getirirdi. Tür değişmediği için (BR-PTY-001) kolonlar tutarlı kalır. |
| PT-02 | Görünen ad | Her tarafta zorunlu `name`; kişide ad ve soyad ayrıca, firmada unvan isteğe bağlı | Sanatçının sahne adı yasal adından farklıdır; listeler ve seçim kutuları tek bir adla çalışır. |
| PT-03 | Roller | `parties.roles text[]` | Identity'nin ID-10 gerekçesi: küçük, sabit küme; tek alanlı geçmiş. Rol süzgeci GIN indeksle çalışır. |
| PT-04 | İletişim bilgisine göre arama | Tarafta, ad ve iletişim bilgilerinden üretilen tek arama anahtarı | US-PTY-002 iletişim bilgisiyle arama istiyor; tek kolon tek `pg_trgm` indeksiyle aranır, alt tabloya birleştirme gerekmez. |
| PT-05 | Telefon biçimi | Serbest yazım, izin verilen karakterler ve rakam sayısıyla doğrulanır; uluslararası numara kütüphanesi kullanılmaz | S1'de arama ve görüntüleme yeterli; bağımlılık eklemeye değmez. |
| PT-06 | Taraf ve iletişim bilgileri tek formda | İletişim bilgileri tarafın oluşturma ve düzenleme isteğinin parçasıdır; iletişim kişileri ve temsiller ayrı uçlardır | İletişim bilgileri tarafın kendi verisidir ve hikayede birlikte girilir. İletişim kişisi ve temsil başka bir tarafa bağdır; detay sayfasında tek tek eklenir. |
| PT-07 | İletişim kişisinin rolü | Yeni rol: **İletişim kişisi** (`contact`) | S1; BR-PTY-001 değişmez, iletişim kişileri rol süzgeciyle bulunur. Kişi başka bir rol de alabilir (ör. hem iletişim kişisi hem tedarikçi). |
| PT-08 | Birincil iletişim bilgisinin veritabanı kısıtı | Yok; kural toplu kökte | Birincili bir kayıttan diğerine taşımak aynı kayıtta iki satırı günceller; koşullu benzersiz indeks ara durumda ihlal verir ve ertelenemez ([database §12.1](../standards/database.md#121-kısıtlar)). Taraf her zaman bütün olarak ve sürümüyle kaydedildiği için kural tek yerde güvenle korunur. |

## 12. Proje sahibine sorulanlar

Soru 2026-10-02'de yanıtlandı; önerilen seçenek seçildi.

| Soru | Seçenekler | Yanıt |
|---|---|---|
| S1 — Firmanın iletişim kişisi (ör. mekan işletmecisinin teknik sorumlusu) hiçbir taraf rolüne uymuyor; BR-PTY-001 ise en az bir rol istiyor. Ne yapalım? | Yeni "İletişim kişisi" rolü / firmaya bağlı kişide rol şartı kalksın / iletişim kişisi taraf olmasın | **Yeni rol** (PT-07) |

## 13. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-10-02 | v0.1 | İlk taslak; adımın ortak kararları (MD-01…07). |
| 2026-10-02 | v1.0 | S1 yanıtlandı (PT-07, İletişim kişisi rolü); onaylandı. |
| 2026-10-02 | v1.1 | MD-01 uygulandı (PR 0); ayrıntılar [audit §2](audit.md#2-yazma-tarafına-eklenenler). API süzgeçleri `rootType`, `rootId`. |
| 2026-10-02 | v1.2 | §5, §6: PR 1a'nın uygulama ayrıntıları; birincil iletişim bilgisi için veritabanı indeksi yerine toplu kök (PT-08). |
