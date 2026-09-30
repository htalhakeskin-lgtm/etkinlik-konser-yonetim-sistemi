# Ortak Yapı Taşları (BuildingBlocks) — Fiziksel Tasarım

> **Durum:** v1.14 (onaylandı) · **Son güncelleme:** 2026-09-30

## 1. Bu belge ne işe yarar

Faz 1.1'in ([12 §3](../12-implementation-plan.md#3-adımlar)) fiziksel tasarımıdır: modüllerin üzerine kurulacağı platformun **hangi sınıflarla, hangi tablolarla ve hangi sırayla** yazılacağını tanımlar.

Kuralların kendisi zaten standartlarda ve ADR'lerde karara bağlıdır; bu belge onları tekrar etmez, bağlantı verir. Belgenin katkısı üç şeydir:
- kararları somut tiplere, tablolara ve adımlara çevirmek (§3–§12),
- standartlarda açık kalan uygulama ayrıntılarını kararlaştırmak (§16, BB-01…),
- 1.1'i küçük, sınanabilir PR'lara bölmek (§14).

Proje sahibine sorulan üç soru ve yanıtları §15'tedir.

**Kaynaklar:** [05 — Modül haritası](../05-module-map.md), [08 — Mimari](../08-architecture.md) §3–§8, [database](../standards/database.md), [api](../standards/api.md), [security](../standards/security.md), [observability](../standards/observability.md), [configuration](../standards/configuration.md), [testing](../standards/testing.md); ADR [0003](../adr/0003-integration-events-and-outbox.md), [0006](../adr/0006-backend-platform.md), [0007](../adr/0007-data-access.md), [0010](../adr/0010-messaging-infrastructure.md), [0011](../adr/0011-authentication.md), [0012](../adr/0012-realtime-signalr.md), [0013](../adr/0013-scheduled-jobs.md), [0020](../adr/0020-entity-identifiers.md)–[0025](../adr/0025-idempotency-keys.md).

## 2. Kapsam

| 1.1'de | 1.1'de değil (nerede) |
|---|---|
| Modül kaydı, komut / sorgu altyapısı, dekoratörler, işlem birimi | Giriş, oturum tablosu, şifre (1.2, Identity) |
| Veritabanı rolleri ve şemaları, ortak EF kuralları, migration çalıştırma | Veri koruma anahtarlarının `identity` şemasındaki tablosu (1.2) |
| İşlem geçmişi yazıcısı ve `audit.audit_entries` tablosu (§15, S2) | İşlem geçmişi ekranı ve sorgusu (1.2, US-SYS-004) |
| Outbox, dağıtıcı, olay yolu, inbox | Hatalı olaylar ekranı (sistem yöneticisi arayüzü, 1.2) |
| Zamanlanmış iş temel sınıfı, danışma kilitleri | Modüllerin kendi zamanlanmış işleri (her modülde) |
| Problem Details, JSON ayarları, güvenlik başlıkları, `X-App-Version` | İstek sınırı (giriş uç noktasıyla birlikte, 1.2) |
| ETag / `If-Match`, tekrar güvenliği, CSRF | |
| OpenAPI şema kuralları, dosyaya üretim, Orval hattı | |
| SignalR hub'ı ve `resourceChanged` yayını | Grup yetkileri modüllere göre (her modülde) |
| Ön yüz: istek sarmalayıcısı, hata tipi, sürüm şeridi, SignalR istemcisi, ortak bileşenlerin ilk seti | Uygulama kabuğu ve menü (1.2) |
| Veritabanı testleri DT-01…05, mimari testler AT-05…AT-15, izlenebilirlik aracı | |

Tasarım değişkenleri, temalar, düğme ve durum rozeti Faz 1.0'da tamamlandı (PR #57, #58).

## 3. Projeler ve temel tipler

[08 §4](../08-architecture.md#4-ortak-yapı-taşları-buildingblocks)'teki dağılım korunur. Tip adları [naming §4.2](../standards/naming.md#42-mimari-yapı-taşlarının-adları)'e uyar.

| Proje | Klasör | Temel tipler |
|---|---|---|
| `BuildingBlocks.Domain` | `Entities/` | `Entity<TId>`, `AggregateRoot<TId>` (modül içi olay listesi, `Version`), `IAggregateRoot` (işlem biriminin kimlik tipinden bağımsız eriştiği yüz), `IAuditable` (`Created…`/`Updated…`), `IDeactivatable` |
| | `Rules/` | `BusinessRuleViolationException(ruleCode, message, kind, parameters)`; kural numarası ilk parametredir. `RuleKind`: Kısıt ve Geçiş 422, Yetki 403 döner ([api §8.3](../standards/api.md#83-i̇stisnaların-eşlenmesi)); Tetikleyici ve Hesaplama kuralları fırlatılmaz |
| | `Events/` | `IDomainEvent` |
| | `Time/` | `TimeRange` (yarı açık `[başlangıç, bitiş)`, çakışma ve kapsama), `IstanbulCalendar` (takvim günü dönüşümleri) |
| | `Monetary/` | `Money`, `Currency`, yuvarlama kuralı (V-08) |
| | `Identifiers/` | `IStronglyTypedId<TSelf>`: `Value` ve genel dönüştürücülerin kullandığı `static abstract From(Guid)`; yeni kimliği her tip kendi `New()` metoduyla üretir (`Guid.CreateVersion7()`) |
| `BuildingBlocks.Application` | `Messaging/` | `ICommand<TResult>`, `ICommandHandler<TCommand, TResult>`, `IQuery<TResult>`, `IQueryHandler<TQuery, TResult>`, `IDomainEventHandler<TEvent>`; `AddHandlersFrom(assembly)` (Scrutor taraması, iç tipler dahil) ve `DecorateHandlers()` (tüm modüller kaydolduktan sonra bir kez). `IIntegrationEventHandler<TEvent>`, `IIntegrationEvent`'le birlikte 6. PR'da gelir. |
| | `Behaviors/` | `LoggingCommandDecorator` (iz aralığı, süre, sonuç; [observability §4](../standards/observability.md#4-dağıtık-izleme)), `ValidationCommandDecorator`, sorgu karşılıkları (loglama, doğrulama); `UnitOfWorkCommandDecorator` veritabanına bağlı olduğu için Infrastructure'dadır (4. PR) |
| | `Users/` | `ICurrentUser`, `SystemUser` (sabit kimlik, [database §9](../standards/database.md#9-ortak-kolonlar)) |
| | `Errors/` | `NotFoundException`, `ConcurrencyConflictException`, `ValidationFailedException` (`ValidationError` listesi; gönderilen değer taşınmaz), `ErrorCodes` (beklenen sonuçların API kodları) |
| | `Paging/` | `PageRequest` + `PageRequestValidator`, `PagedResult<T>`, `CursorRequest` + `CursorRequestValidator`, `CursorResult<T>`, `SortSpec` / `SortField` ve `SortableBy(...)` doğrulama kuralı; liste sorgularının doğrulayıcıları bunları `SetValidator` ile kullanır. Kimliğin son sıralama alanı olarak eklenmesi ve imlecin kodlanması sorguyu kuran altyapı kodundadır. |
| `BuildingBlocks.Infrastructure` | `Modules/` | `IModuleDefinition`, `AddModules(...)` |
| | `Persistence/` | `ModuleDbContext` (şema, kurallar, kaydetme adımları), `AddModuleDbContext` / `UseModuleDatabase`, `DatabaseConnections`, `DatabaseBootstrapper`, `DatabaseMigrator`, EF kuralları (§5.3), `UnitOfWorkCommandDecorator` |
| | `Auditing/` | `AuditEntry`, `AuditAction`, `AuditEntryWriter` (5b); `[NotAudited]` alan tiplerinde kullanıldığı için `BuildingBlocks.Domain`'dedir (5b) |
| | `Messaging/` | `OutboxMessage`, `InboxMessage`, `Outbox` (`IOutbox`'un uygulaması; 6a); `OutboxDispatcher`, `IEventBus` ve süreç içi uygulaması, `InboxIntegrationEventDecorator` (6b) |
| | `Locking/` | `AdvisoryLocks` (işlem düzeyi ve oturum düzeyi) |
| | `Jobs/` | `ScheduledJob` (aralıklı ve cron; Cronos) |
| | `Http/` | Problem Details eşleyicisi, JSON ayarları, `IfMatchFilter`, `IdempotencyFilter`, CSRF ara katmanı, güvenlik başlıkları, `X-App-Version` |
| | `OpenApi/` | Şema ve işlem dönüştürücüleri |
| | `Realtime/` | `NotificationsHub`, `ResourceChangedPublisher` |
| `BuildingBlocks.Contracts` | — | `IIntegrationEvent`, `IntegrationEvent` temel kaydı: mesaj kimliği (`MessageId`), oluşma zamanı, sıra anahtarı. Olayın kendi kimliği `EventId` değil `MessageId`'dir, çünkü etkinlikle ilgili olaylar etkinliğin `EventId`'sini taşır. İlişki kurmak için ayrı bir kimlik yerine outbox'taki iz bağlamı (`trace_parent`) kullanılır. |

HTTP, OpenAPI ve SignalR kodu 08'deki tabloda Host'a yazılmıştı; hepsi modüllerin ortak kullandığı altyapı olduğu için `BuildingBlocks.Infrastructure`'a alınır, Host yalnızca bağlar (BB-01).

## 4. Bir komutun yolu

Sıra [08 §6](../08-architecture.md#6-bir-isteğin-yolculuğu)'dakidir; burada fiziksel karşılıkları vardır.

| Adım | Nerede | Ne olur |
|---|---|---|
| 1 | Ara katmanlar (Host) | İz kimliği, güvenlik başlıkları, kimlik doğrulama, CSRF |
| 2 | Uç nokta filtresi `IdempotencyFilter` | `Idempotency-Key` başlığı okunur ve isteğin kapsamına konur (§9.3) |
| 3 | Uç nokta filtresi `IfMatchFilter` | `If-Match` okunur ve kapsama konur (§9.2) |
| 4 | Uç nokta | İstek komuta çevrilir; işleyici arayüzü DI'dan alınır (`ICommandHandler<,>`, dekoratörlerle sarılı) |
| 5 | Loglama dekoratörü | İz aralığı (span), log kapsamı (modül, kullanıcı kimliği), süre; beklenen sonuçlar `Information`, diğer hatalar aralıkta `Error` |
| 6 | Doğrulama dekoratörü | FluentValidation; hata varsa `ValidationFailedException` |
| 7 | İşlem birimi dekoratörü | Npgsql yeniden deneme stratejisi içinde işlem açar ([database §11.4](../standards/database.md#114-i̇şlem-yalıtımı-ve-yeniden-deneme)); tekrar güvenliği satırını ekler; işleyiciyi çalıştırır; `SaveChanges`; tekrar güvenliği yanıtını yazar; işlemi tamamlar; dağıtıcıya sinyal verir |
| 8 | İşleyici | İş mantığı alan katmanında |

**`SaveChanges` adımları** (`ModuleDbContext`, tek işlem içinde):
1. Toplu köklerin modül içi olayları toplanır ve `IDomainEventHandler` dinleyicileri çalıştırılır; dinleyicilerin ürettiği yeni olaylar bitene kadar döngü sürer (en fazla 5 tur, sonsuz döngüye karşı).
2. Değişen toplu köklerin sürümü bir artırılır; kendisi değişmeyip alt varlığı değişen kök de dahil ([database §11.1](../standards/database.md#111-sürüm-numarasıyla-iyimser-kilit)).
3. `created_*` / `updated_*` alanları `ICurrentUser` ve `TimeProvider`'dan doldurulur.
4. İşlem geçmişi kayıtları üretilir (§6).
5. Entegrasyon olayları outbox'a yazılır (§7).
6. Veritabanına yazılır. Sürüm çakışması `ConcurrencyConflictException`'a, kurala eşlenmiş kısıt ihlali kural numaralı hataya çevrilir.

**Uygulama ayrıntıları** (4a):
- Adımlar, uygulama DI'ından alınan kapsamlı bir servistedir (`SaveChangesPipeline`); böylece modül bağlamlarının kurucusu yalnızca seçenekleri alır ve olay dinleyicileri isteğin bağlamını paylaşır.
- Alt varlığın toplu kökü, alt varlığın **basamaklı silinen** yabancı anahtarıyla bulunur: V-11'e göre yalnızca kökün kendi alt varlıkları basamaklıdır. Kökü yüklenmeden değiştirilen alt varlık hata verir.
- Yeni kökün sürümü 1'dir. Zaman damgaları mikrosaniyeye yuvarlanır; bellekteki değer veritabanındakiyle aynı kalır ([database §7.1](../standards/database.md#71-anlar)).
- Kısıt eşlemesi modül bağlamının `ConstraintRules` sözlüğündedir (kısıt adı → kural kodu).
- `SaveChanges` (senkron) desteklenmez; olay dinleyicileri kaydetmez, onları tetikleyen kayıt değişikliklerini de yazar.

**İşlem birimi dekoratörü** (4b):
- Komutun modülü ad alanından okunur (`FestOS.Modules.{Modül}.…`, `ModuleNames`); modülün bağlamı, `AddModuleDbContext`'in modül adıyla kaydettiği anahtarlı servistir. Modül dışındaki bir komut hata verir.
- İşleyici ve kayıt tek bir işlemde, yeniden deneme stratejisinin içinde çalışır. Yeniden denemede değişiklik takibi temizlenir ve işleyici baştan çalışır; bu yüzden işleyiciler verisini kendisi yükler ve dışarıya yan etki bırakmaz (dış etkiler outbox'la, §7).
- İşleyiciler normalde kaydetmez; kaydı dekoratör yapar. İşleyici kaydettikten sonra hata verirse işlem geri alınır.
- Dekoratörler içten dışa: işlem birimi, doğrulama, loglama. Doğrulama hatası işlem açmadan döner.

**İşleyicinin çağrılma biçimi:** Uç nokta, dekoratörlerle sarılmış işleyici arayüzünü doğrudan alır. Araya bir "mediator" (MediatR benzeri) konmaz: çağrı derleme anında bellidir, yansıma ile tip çözme yoktur ve IDE'de doğrudan işleyiciye gidilir (BB-02). Dekoratörler Scrutor ile kaydedilir ([07](../07-tech-stack.md)).

## 5. Veritabanı

### 5.1 Roller ve şemaların kurulumu

Roller ve yetkiler [database §4](../standards/database.md#4-roller-ve-yetkiler)'tedir. Kurulum iki parçadır:

| Parça | Ne yapar | Kim çalıştırır |
|---|---|---|
| **Veritabanı hazırlığı** (`bootstrap`) | Roller yoksa oluşturur ve parolalarını ayarlar (`festos_owner`, `festos_migrator`, modül rolleri, `festos_readonly`, `festos_monitor`); eklentileri (`btree_gist`, `pg_trgm`) kurar; rol düzeyi ayarları yazar ([database §17](../standards/database.md#17-bağlantı-ve-işletim-ayarları)). Tekrar çalıştırılabilir. | Yönetici bağlantısıyla, `migrate` komutunun ilk adımı |
| **Modül migration'ları** | Şemayı ve tabloları oluşturur, modül rolüne yetkileri verir (`GRANT`), değişmez tablolarda `UPDATE`/`DELETE` vermez | `festos_migrator` |

- Parolalar rol başına ayrı gizli bilgidir; kodda ya da migration'da yazmaz ([security §6](../standards/security.md#6-gizli-bilgiler)).
- **Geliştirmede** AppHost her rol için bir parola üretir (Aspire gizli parametresi, kullanıcı gizlileri deposunda saklanır) ve Host'a verir; her modül kendi rolüyle bağlanır (§15, S3).
- Yetkiler modülün migration'ındadır; yeni tablo eklendiğinde yetkisi de aynı migration'da verilir. DT-02 eksik ya da fazla yetkiyi yakalar.

### 5.2 Bağlantılar

- Tek bir temel bağlantı dizesi vardır: `ConnectionStrings:festos` (sunucu, port, veritabanı). İçinde kimlik bilgisi varsa bu yöneticinindir ve yalnızca hazırlık adımında kullanılır; geliştirmede Aspire verir. Demoda temel dize kimlik bilgisi taşımaz, yönetici `Database:AdminUsername` / `Database:AdminPassword` gizli bilgileriyle verilir.
- Her rolün parolası `Database:Passwords:{rol}` gizli bilgisidir (demoda `/run/secrets/Database__Passwords__festos_booking` gibi dosyalar). Her modül, temel dizeye kendi rolünün kullanıcı adı ve parolası yazılarak bağlanır (`DatabaseConnections.ForRole`); `Application Name=festos-{şema}`; havuz boyutu `Modules:{Modül}:Database:MaxPoolSize`. On modül için on ayrı bağlantı dizesi tutulmaz.
- Uygulama süreci yalnızca modül rollerinin parolalarını alır; yönetici ve migration rolünün parolalarını yalnızca `migrate` komutu alır (yayındaki en az yetki). Geliştirmede Host ikisini de yapar.
- Npgsql yeniden deneme stratejisi açıktır; işlem birimi onu bütün bir blok olarak çalıştırır ([database §11.4](../standards/database.md#114-i̇şlem-yalıtımı-ve-yeniden-deneme)).

### 5.3 Ortak EF kuralları

`ModuleDbContext.ConfigureConventions` ve `OnModelCreating` tüm modüllere tek yerden uygular:

| Kural | Uygulama |
|---|---|
| snake_case adlar | EFCore.NamingConventions, `InvariantCulture` ile ([naming §5](../standards/naming.md#5-veritabanı-adları)) |
| Varsayılan şema | Modülün şeması; migration geçmiş tablosu da o şemada |
| Tip güvenli kimlikler | `IStronglyTypedId<TSelf>` uygulayan her tipe tek bir genel değer dönüştürücüsü; anahtarlar `ValueGeneratedNever` |
| Silme davranışı | Varsayılan `RESTRICT`; `CASCADE` yalnızca açıkça (V-11) |
| Enum'lar | camelCase metin + otomatik `CHECK` kısıtı (V-06) |
| Sürüm | `AggregateRoot.Version` eşzamanlılık belirteci (V-09) |
| `decimal` | Varsayılan `numeric(19,4)`; farklısı açıkça |
| Metin uzunlukları | Belirtilmeyen metin kolonu derlemede hata (bir model testiyle denetlenir; [database §6.1](../standards/database.md#61-metin-uzunlukları)) |

- Kurallar `ModuleDbContext`'te uygulanır: `ConfigureConventions` (tip güvenli kimlik dönüştürücüleri, `decimal`, modül içi olayların eşlenmemesi) ve modülün tablo eşlemelerinden sonra `OnModelCreating` (anahtarlar, sürüm, silme davranışı, enum'lar). Tip güvenli kimlikler, bağlamın derlemesinde ve başvurduğu FestOS modül derlemelerinde (modülün Domain projesi) aranır.
- `UseModuleDatabase(...)` bağlantı ayarlarını tek yerde toplar; `AddModuleDbContext<T>` çalışma zamanında, `migrate` komutu ve tasarım zamanı fabrikaları da bunu kullanır.
- Tasarım zamanı fabrikalarının bağlantı dizesi çözümlenemeyen bir sunucuyu gösterir (`design-time.invalid`). Böylece yanlışlıkla çalıştırılan bir `dotnet ef` veritabanı komutu yerelde çalışan başka bir veritabanına ulaşamaz.
- Model kuralları `FestOS.DatabaseTests`'teki model testleriyle (AT-13'ün ad ve uzunluk kısmı, DT-01) tüm modüllerde denetlenir; yeni modül `ModuleContexts` listesine eklenir.

### 5.4 Migration'ların çalışması

- **Geliştirmede:** Host açılırken önce hazırlık, sonra tüm modüllerin migration'ları uygulanır ([database §16.2](../standards/database.md#162-uygulama)).
- **Demo ve yayında:** Aynı çalıştırılabilir dosyanın `migrate` komutu (`FestOS.Host migrate`); Host'un açılışı migration çalıştırmaz. Komut Host'la aynı modül listesini kullanır; hata olursa sıfır olmayan kodla çıkar ve yayın durur.
- Geliştirmede açılıştaki adım, temel bağlantı dizesi tanımlı değilse atlanır (Host'u Aspire olmadan açmak için).
- `migrate` komutu 1.1'de yazılır (hazırlık + migration'lar); diğer komutlar (`seed-demo`, `reset-demo` …) ihtiyaç duyuldukları adımda eklenir.

## 6. İşlem geçmişi yazıcısı

- Tablo [database §14.2](../standards/database.md#142-kayıtların-işlem-geçmişi)'deki gibi `audit.audit_entries`'dir ve Audit modülüne aittir. 1.1'de Audit modülünün yalnızca bu tabloyu ve rolünü kuran iskeleti yazılır; okuma tarafı 1.2'dedir (§15, S2).
- `AuditEntryWriter`, `SaveChanges`'in 4. adımında EF değişiklik izleyicisinden kayıtları üretir: `created`, `updated` (yalnızca değişen alanlar, eski ve yeni değer), `deleted`. Durum alanı değişen kayıtlar `statusChanged` olarak yazılır.
- Kayıtlar modülün kendi bağlantısıyla, aynı işlemde eklenir; modül rolünün bu tabloda yalnızca `INSERT` yetkisi vardır (V-02).
- `[NotAudited]` işaretli alanlar yazılmaz. `trace_id` o anki iz kimliğidir.

**Uygulama ayrıntıları** (5a):
- `AuditEntry` her modülün bağlamında aynı tabloya eşlenir, böylece kayıtlar değişiklikle aynı `SaveChanges`'te, aynı işlemde eklenir. Eşleme Audit dışındaki bağlamlarda migration dışıdır (`ExcludeFromMigrations`); tabloyu, indeksini ve yetkilerini yalnızca Audit modülünün migration'ı kurar.
- Modül listesi değişkendir, bu yüzden yetki bir grup rolüne verilir: hazırlık adımı giriş yapamayan `festos_audit_writer` rolünü kurar ve her modül rolünü üye yapar; Audit migration'ı bu gruba yalnızca `INSERT`, `festos_audit`'e yalnızca `SELECT` verir. Güncelleme ve silme yetkisi kimsede yoktur.
- `action` bir enum'dur (`AuditAction`); ortak kuralla camelCase metin ve `CHECK` kısıtıyla saklanır.

**Yazıcı** (5b):
- `changes`: yeni kayıtta her alanın yeni değeri, silinen kayıtta eski değeri, değişiklikte yalnızca gerçekten değişen alanların eski ve yeni değeri. Alan adları camelCase'dir; değerler veritabanındaki biçimleriyledir (kimlik UUID, enum camelCase metin).
- `Status` alanı değişen kayıt `statusChanged` olarak yazılır.
- Sürüm ve oluşturan / güncelleyen alanları yazılmaz; bunlar geçmiş satırının kendi zamanı ve kullanıcısıdır. Yalnızca bir alt varlığı değiştiği için sürümü artan kökün satırı yazılmaz; alt varlığın satırı yazılır.
- `[NotAudited]` alanda ya da varlıkta kullanılabilir; alan tiplerinde kullanıldığı için `BuildingBlocks.Domain`'dedir.
- `occurred_at` ve `actor_id` o kaydın damgalarıyla aynıdır; `module` modülün şemasıdır.

## 7. Olaylar: outbox, dağıtıcı, olay yolu, inbox

Kararlar [ADR-0010](../adr/0010-messaging-infrastructure.md)'da, tablolar [database §15](../standards/database.md#15-outbox-ve-inbox-tabloları)'tedir. Uygulama ayrıntıları:

| Konu | Karar |
|---|---|
| Yazma | Entegrasyon olayları, modül içi olay dinleyicilerinde `IOutbox.Add` ile eklenir ve `SaveChanges`'in 5. adımında kaydeden modülün `outbox_messages`'ına yazılır. `ordering_key` olayın `OrderingKey` alanıdır; `type` olay tipinin sürümsüz adıdır (`{tam ad}, {derleme}`); `payload` web varsayılanlarıyla (camelCase) JSON'dur. İşlem biriminin yeniden denemesi, önceki denemenin eklediklerini atar. |
| Uyanma | İşlem tamamlanınca işlem birimi dağıtıcıya süreç içi sinyal verir; yedek olarak 5 saniyede bir tarar (`Messaging:PollInterval`). |
| Dağıtıcı | Her modül için bir dağıtıcı döngüsü (arka plan hizmeti). Gönderilmemiş kayıtları `sequence` sırasıyla, `FOR UPDATE SKIP LOCKED` ile küçük gruplar halinde alır. |
| Teslim | Olay yolu, olayı abone olan tüm dinleyicilere paralel iletir. Her dinleyici **kendi modülünün** işlem biriminde ve inbox dekoratörüyle çalışır. |
| Başarı | Tüm dinleyiciler başarılıysa kayıt `dispatched_at` ile işaretlenir. Olay yolu, SignalR yayıncısı için de bir dinleyicidir. |
| Kısmi hata | Bir dinleyici başarısızsa kayıt gönderilmemiş kalır, `attempt_count` artar ve `next_attempt_at` artan aralıkla (5 sn, 30 sn, 2 dk, 10 dk, 1 sa) ileri alınır. Yeniden denemede tüm dinleyiciler yine çağrılır; başarılı olanlar inbox kaydı sayesinde işi tekrar yapmaz. Böylece hata yalnızca hatalı dinleyici için "yeniden denenmiş" olur (BB-03). |
| Sıra | Aynı `ordering_key`'e ait daha eski bir kayıt bekliyor ya da hatalıysa, sonraki kayıtları gönderilmez; farklı anahtarların kayıtları etkilenmez. |
| Hatalı olay | 8 denemeden sonra `failed_at` doldurulur ve kayıt otomatik denenmez. Sistem yöneticisi arayüzünden (1.2) yeniden denenebilir. Hatalı olay sayısı ölçüm olarak yayınlanır. |
| İz | `trace_parent` kaydedilir; dinleyicinin izi olayı doğuran isteğe bağlanır ([observability §4](../standards/observability.md#4-dağıtık-izleme)). |
| Ölçümler | [observability §5](../standards/observability.md#5-ölçümler)'teki adlarla: `festos.messaging.event.latency` (oluşma → işlenme), `festos.messaging.dead_letters`, `festos.messaging.outbox.pending` |
| Temizlik | Gönderilmiş outbox ve inbox kayıtları 30 günden eski olanlar, gecelik bir işle küçük gruplar halinde silinir. Hatalı olaylar silinmez. İş, zamanlanmış işler altyapısıyla (7. PR) gelir. |

`outbox_messages`'a [database §15](../standards/database.md#15-outbox-ve-inbox-tabloları)'teki kolonlara ek olarak `failed_at` eklenir.

**Teslimin uygulama ayrıntıları** (6b-1):
- `OutboxProcessor.ProcessBatchAsync(modül)` bir grup bekleyen kaydı modülün rolüyle, kendi işlem biriminde alır ve sırayla teslim eder. Sorgu şemasız yazılır; modül rolünün `search_path`'i modülün şemasıyla başlar, böylece ham SQL'e şema adı eklenmez.
- Olay yolu (`InProcessEventBus`) her dinleyiciyi **kendi DI kapsamında** ve paralel çalıştırır; farklı modüllerin dinleyicileri ayrı işlemlerdedir. Tüm dinleyiciler bitince hatalar tek bir `AggregateException`'da toplanır.
- Inbox dekoratörü dinleyiciyi, dinleyicinin ad alanından bulunan modülün işlem biriminde çalıştırır: inbox'ta kayıt varsa atlar, yoksa dinleyiciyi çalıştırır ve inbox satırını aynı işlemde ekler.
- Dinleyicilerin ve işleyicinin değişiklikleri sistem kullanıcısıyla kaydedilir (`ActingUser`; [database §9](../standards/database.md#9-ortak-kolonlar)). Komutlar, dinleyiciler ve işleyici aynı işlem birimi yardımcısını (`ModuleUnitOfWork`) kullanır.
- Kayıt yazarken ve hatada `Warning` (yeniden denenecek) ya da `Error` (hatalı olarak işaretlendi) loglanır ([observability §3.3](../standards/observability.md#33-ne-loglanır)).

**Dağıtıcı** (6b-2):
- `OutboxDispatcher` bir arka plan hizmetidir ve her modül için bir döngü çalıştırır: açılışta bekleyenleri teslim eder, dolu grup geldikçe devam eder, sonra bir sinyal ya da `Messaging:PollInterval` (varsayılan 5 sn, 1 sn–10 dk arası doğrulanır) gelene kadar bekler.
- İşlem birimi, commit ettiği kayıt outbox'a olay yazdıysa o modülün dağıtıcısını uyandırır (`OutboxSignals`); art arda gelen sinyaller tek sinyale indirgenir. Doğrudan `SaveChanges` sinyal vermez; o olayları yedek tarama alır.
- Bir turdaki hata `Warning` olarak loglanır ve döngü sürer. Temel bağlantı dizesi yoksa (Host'u veritabanısız açarken) dağıtıcı bir kez bilgi verip çalışmaz. `migrate` komutunun host'u başlatılmadığı için dağıtıcı orada çalışmaz.
- Her teslim `Deliver {Olay}` adlı bir iz aralığıdır ve olayı doğuran isteğin izine bağlanır (`trace_parent`).

**Sıra ve ölçümler** (6c):
- Bir kayıt, aynı `ordering_key`'e ait daha eski ve teslim edilmemiş (bekleyen ya da hatalı) bir kayıt varsa alınmaz. Böylece bir grupta her anahtardan en fazla bir kayıt bulunur ve her anahtarın olayları sırayla, bir öncekinin tesliminden sonra gider; diğer anahtarlar etkilenmez (BB-04). Kontrol `(ordering_key, sequence) WHERE dispatched_at IS NULL` indeksini kullanır.
- Ölçümler `FestOS.BuildingBlocks` ölçüm kaynağındadır; etiketleri `festos.module` ve `festos.event.type`'tır. Bekleyen olay sayısı dağıtıcının her turundan sonra güncellenir.
- Oluşmadan teslime geçen süre P-08'i (`Messaging:EventLatencyTarget`, varsayılan 300 ms) aşarsa `Warning` loglanır. P-15 üst sınırı gecikme ölçümü üzerindeki bir uyarıyla izlenir ([observability §7](../standards/observability.md#7-uyarılar)).

## 8. Zamanlanmış işler ve kilitler

- `ScheduledJob` temel sınıfı ([ADR-0013](../adr/0013-scheduled-jobs.md)): aralıklı işler `PeriodicTimer`, saatli işler Cronos; saatler Europe/Istanbul; zaman `TimeProvider`'dan.
- Her tur, ayrı bir bağlantıda oturum düzeyi danışma kilidiyle başlar (`{modül}:job:{iş adı}`); kilidi alamayan örnek turu atlar; kilit `finally` içinde bırakılır ([database §11.3](../standards/database.md#113-müsaitlik-kilidi)).
- İşin kendisi Application'daki bir komuttur; iş sınıfı yalnızca tetikler ([08 §3.4](../08-architecture.md#34-proje-içi-klasörler)).
- İşlem düzeyi kilit yardımcısı: `AdvisoryLocks.AcquireTransactionLocksAsync(keys)` anahtarları sıralayarak alır (kilitlenmeye karşı).

## 9. HTTP katmanı

### 9.1 Hata yanıtları ve JSON

- Tek bir `IExceptionHandler` [api §8.3](../standards/api.md#83-i̇stisnaların-eşlenmesi)'teki tabloyu uygular; `code`, `params`, `errors`, `traceId` alanlarını yazar.
- FluentValidation hataları JSON Pointer'a çevrilir (`/units/3/serialNumber`); doğrulama kodları kuralların adlarından eşlenir (`NotEmpty` → `required` gibi) ([api §8.2](../standards/api.md#82-doğrulama-hataları)).
- JSON ayarları [api §5.1](../standards/api.md#51-serileştirici-ayarları)'dedir: bilinmeyen ve tekrarlanan alan reddi, boş olabilirlik, `decimal` metin, süre tamsayı dakika, `null`'ların yazılması.
- Gelen metinlerin kırpılması ve NFC biçimi bir JSON dönüştürücüsüyle yapılır; `[Sensitive]` alanlar dışarıda kalır.

### 9.2 ETag ve `If-Match`

- Toplu kök döndüren uç noktalar `ETag: "{version}"` başlığını ekler (uç nokta sonucunun bir yardımcısıyla).
- `IfMatchFilter`, toplu kök değiştiren uç noktalarda başlık yoksa `428` (`versionRequired`) döner; varsa sürümü komutun kapsamına koyar. İşleyici, toplu kökü yükledikten sonra sürümü karşılaştırır; farklıysa `ConcurrencyConflictException` → `412` ([api §9](../standards/api.md#9-eşzamanlı-düzenleme)).
- Uç nokta metadatası (`RequiresVersion()`) OpenAPI'de zorunlu `If-Match` başlığını da üretir (AT-15).

### 9.3 Tekrar güvenliği

Akış [api §10](../standards/api.md#10-tekrar-güvenliği)'dadır. Fiziksel uygulama:

| Adım | Nerede |
|---|---|
| Başlık yoksa `400` (`idempotencyKeyMissing`); anahtarı ve istek parmak izini (yöntem, adres, gövdenin SHA-256 özeti) kapsama koyar | `IdempotencyFilter` |
| Anahtar satırını modülün `idempotency_keys` tablosuna **işlemin ilk adımı** olarak ekler | İşlem birimi dekoratörü |
| Aynı anahtar başka bir işlemde işleniyorsa PostgreSQL'in benzersizlik kontrolü o işlemin bitmesini bekler; `lock_timeout` aşılırsa `409` (`idempotencyKeyInProgress`) | Veritabanı |
| Anahtar zaten tamamlanmışsa: parmak izi aynıysa saklanan yanıt `Idempotency-Replayed: true` ile döner, farklıysa `422` (`idempotencyKeyReused`) | Filtre, dekoratörün fırlattığı özel istisnayla |
| İşleyici bitince yanıt (durum kodu + JSON gövde) aynı satıra, aynı işlemde yazılır | İşlem birimi dekoratörü |
| 24 saatten eski satırlar gecelik temizlik işiyle silinir | Zamanlanmış iş |

Anahtar denetimi `If-Match`'ten önce yapılır; filtre sırası bunu garanti eder.

### 9.4 CSRF, başlıklar, sürüm

- **CSRF** ([api §11](../standards/api.md#11-güvenlik-kuralları)): antiforgery belirteci değiştiren isteklerde `X-XSRF-TOKEN` başlığından ara katmanda doğrulanır; `Sec-Fetch-Site: cross-site` bildiren değiştiren istekler reddedilir. Belirteç çerezini veren uç nokta (`GET /api/v1/antiforgery`) 1.1'de, `/api/v1/me` ise 1.2'de yazılır.
- **Güvenlik başlıkları** ön yüz yanıtlarına [security §8](../standards/security.md#8-tarayıcı-güvenlik-başlıkları)'deki gibi, `/api` yanıtlarına `Cache-Control: no-store` ve `nosniff` eklenir. `style-src 'unsafe-inline'`'ın Base UI ile gerekip gerekmediği 1.1'de denenir.
- **`X-App-Version`:** Her API yanıtında, MinVer'in derleme sürümüyle.

## 10. OpenAPI ve API istemcisi

- Belge .NET'in dahili üreticisiyle OpenAPI 3.1 olarak üretilir ve derlemede `src/web/openapi/festos.json`'a yazılır (`Microsoft.Extensions.ApiDescription.Server`).
- Şema ve işlem dönüştürücüleri [api §14.1](../standards/api.md#141-openapi-belgesi)'deki garantileri uygular: metin enum, `uuid` kimlik, metin `decimal`, boş olabilirlik, zorunlu `If-Match` / `Idempotency-Key`, Problem Details hata şemaları.
- Orval (`tags-split`, TanStack Query + Zod) `src/web/src/api/`'ye üretir; istek sarmalayıcısı "mutator" olarak bağlanır.
- **Sözleşme testi:** Yalnızca testte açılan örnek uç noktalar (boş olabilen dizi, boş olabilen nesne, enum, ondalık, tip güvenli kimlik) üzerinden istemci üretilir ve tip denetiminden geçer ([api §14.1](../standards/api.md#141-openapi-belgesi)).
- **CI:** `backend` işinde üretilen belgenin repodakiyle aynı olduğu, `frontend` işinde Orval çıktısının güncel olduğu denetlenir.

## 11. Anlık bildirimler

- `NotificationsHub` (`/hubs/notifications`), çerezle kimlik doğrular. İstemci `JoinGroup(name)` ile gruba katılır; her grup türü için bir yetki denetimi modül tarafından kaydedilir (ör. `warehouses:{id}` → kullanıcının o depoya erişimi). Denetimi olmayan grup reddedilir.
- `ResourceChangedPublisher` olay yolunun bir dinleyicisidir; modüllerin kaydettiği eşlemelerle (olay → kaynak adı, kimlik, sürüm, gruplar) `resourceChanged` mesajını gönderir ([api §13](../standards/api.md#13-anlık-bildirimler)).
- Bağlantı sayısı `festos.signalr.connections` ölçümüyle yayınlanır.

## 12. Ön yüz platformu

| Parça | İçerik |
|---|---|
| İstek sarmalayıcısı (`lib/api-client.ts`) | Orval mutator: antiforgery başlığı, değiştiren isteklere `Idempotency-Key` (işlem başına bir kez üretilir, yeniden denemede aynı), `X-App-Version` karşılaştırması, Problem Details → `ApiError` tipi, yalnızca ağ hatasında aynı anahtarla yeniden deneme. `fetch` yasağının tek istisnası bu dosyadır. |
| Hata gösterimi | `ApiError` → `errors:{code}` çevirisi ve `params`; doğrulama hatalarının form alanlarına yerleşmesi için yardımcı ([ui §7.4](../standards/ui.md#74-sunucu-hatalarının-forma-yansıması)) |
| Sürüm şeridi | `VersionBanner`: sunucu sürümü farklıysa kapatılamayan "Yeni sürüm hazır — Yenile" ([api §12](../standards/api.md#12-sürümleme-ve-uyumluluk)) |
| SignalR istemcisi (`lib/realtime.ts`) | Tek bağlantı, otomatik yeniden bağlanma, grup katılımı, `resourceChanged` → TanStack Query geçersiz kılma, bağlantı durumu (`ConnectionIndicator` için) |
| Ortak bileşenlerin ilk seti | `EmptyState`, `ErrorState` (iz kimliğiyle), `SkeletonBlock`, `PageHeader`, `ConnectionIndicator`, `VersionBanner`, `ConfirmDialog`; her biri Storybook örneği ve erişilebilirlik testiyle ([ui §6.2](../standards/ui.md#62-ortak-bileşenler)) |

## 13. Testler

| Katman | Ne | Nerede |
|---|---|---|
| Birim | `TimeRange` (CsCheck özellik testleri), `Money` ve yuvarlama, dekoratörler, hata eşleyicisi, JSON ayarları | `tests/BuildingBlocks/FestOS.BuildingBlocks.UnitTests` |
| Platform entegrasyonu | Gerçek PostgreSQL üzerinde, bir **test modülüyle** (§15, S1): kaydetme adımları, sürüm artışı, işlem geçmişi, outbox → olay yolu → inbox, kısmi hata ve sıra, tekrar güvenliği (eşzamanlı iki istek dahil), ETag akışı, CSRF, Problem Details biçimi | `tests/BuildingBlocks/FestOS.BuildingBlocks.IntegrationTests` |
| Veritabanı | DT-01…DT-05 ([database §17.1](../standards/database.md#171-veritabanı-testleri)) | `tests/Database/FestOS.DatabaseTests` |
| Mimari | AT-05…AT-11, AT-13…AT-15 ([08 §12.2](../08-architecture.md#122-mimari-testler)); modül olmayan kurallar hemen, modül kuralları test modülü ve ilk modülle birlikte etkin | `tests/ArchitectureTests` |
| Ortak yardımcılar | PostgreSQL konteyneri (Testcontainers), veritabanı sıfırlama (Respawn), sahte zaman, istemci fabrikası | `tests/Testing/FestOS.Testing` |
| Ön yüz | İstek sarmalayıcısı (MSW ile), SignalR istemcisi, ortak bileşen örnekleri | `src/web` |
| İzlenebilirlik | `tools/docs/check_traceability.py` (TR-01…TR-06) ve CI'daki `traceability` işi | [testing §10](../standards/testing.md#10-kural-ve-geçiş-izlenebilirliği) |

CI'a `backend-integration` işi eklenir (Testcontainers, [ci §4](../standards/ci.md#4-pr-hattı)).

## 14. PR planı

Her PR tek bir davranışı testleriyle getirir; sıra bağımlılığa göredir. 2. ve 3. PR'lar, 400 satır sınırı için bölündü.

| # | PR | Kapsam |
|---|---|---|
| 1 | Alan temelleri | `Entity`, `AggregateRoot`, kurallar, `TimeRange`, `IstanbulCalendar`, `Money`, `IStronglyTypedId`; birim ve özellik tabanlı testler (CsCheck) |
| 2a | Komut / sorgu altyapısı | Arayüzler, loglama ve doğrulama dekoratörleri, hata tipleri, `ICurrentUser`, `SystemUser`; Scrutor kaydı |
| 2b | Sayfalama ve sıralama | `PageRequest`, `PagedResult<T>`, `CursorRequest`, `CursorResult<T>`, `SortSpec` ve doğrulayıcıları ([api §6](../standards/api.md#6-listeler)) |
| 3a | Veritabanı test altyapısı | `FestOS.Testing` (PostgreSQL konteyneri), `FestOS.DatabaseTests`, DT-05; CI `backend-integration` işi |
| 3b | Veritabanı hazırlığı | `bootstrap`: roller, eklentiler, rol düzeyi ayarları; testleri |
| 3c | Modül kaydı | `IModuleDefinition`, `AddModules` (kayıt sırası, dekoratörler bir kez), `MapModules` (`/api/v1`, modül etiketi), `ModuleCatalog`; Host'a bağlanması |
| 3d | Veritabanı bağlamı ve test modülü | `ModuleDbContext` ve EF kuralları (§5.3), test modülü ve migration'ı; DT-01 |
| 3e | `migrate` komutu | Hazırlık + tüm modüllerin migration'ları; AppHost'ta rol parolaları ve modül bağlantı dizeleri |
| 4a | Kaydetme adımları | `SaveChangesPipeline`: adımlar 1–3 ve 6, sürüm artışı, kısıt eşlemesi; `FestOS.BuildingBlocks.IntegrationTests`, Respawn |
| 4b | İşlem birimi dekoratörü | İşlem ve yeniden deneme stratejisi, modülün bağlamının seçimi (`ModuleNames`), örnek komutlar |
| 5a | İşlem geçmişi tablosu | Audit iskeleti, `audit_entries`, her bağlamda migration dışı eşleme, `festos_audit_writer`; yetki testleri |
| 5b | İşlem geçmişi yazıcısı | `AuditEntryWriter` (kaydetme adımı 4), `[NotAudited]` |
| 6a | Outbox'a yazma | Entegrasyon olayı tipleri, `IOutbox`, outbox ve inbox tabloları, kaydetme adımı 5 |
| 6b-1 | Teslim | Entegrasyon olayı dinleyicileri, inbox dekoratörü, olay yolu, `OutboxProcessor` (kısmi hata, artan aralıklı deneme, hatalı olaylar); AT-10 |
| 6b-2 | Dağıtıcı | Arka plan dağıtıcısı, işlem sonrası sinyal, yedek tarama (`Messaging:PollInterval`), iz bağlamı |
| 6c | Sıra ve ölçümler | Sıra anahtarı, ölçümler |
| 7 | Zamanlanmış işler ve kilitler | `ScheduledJob`, danışma kilitleri; sahte zamanla testler |
| 8 | Hata yanıtları ve JSON | `IExceptionHandler`, doğrulama eşlemesi, JSON ayarları, metin kırpma, `X-App-Version`, güvenlik başlıkları |
| 9 | ETag ve tekrar güvenliği | `IfMatchFilter`, `IdempotencyFilter`, `idempotency_keys`; eşzamanlılık testleri |
| 10 | CSRF | Antiforgery ara katmanı, `Sec-Fetch-Site`, belirteç uç noktası |
| 11 | OpenAPI ve Orval | Dönüştürücüler, dosyaya üretim, Orval, sözleşme testi, CI güncellik denetimleri; AT-14, AT-15 |
| 12 | Anlık bildirimler | Hub, grup yetkileri, `ResourceChangedPublisher` |
| 13 | Ön yüz platformu | İstek sarmalayıcısı, `ApiError`, sürüm şeridi, SignalR istemcisi |
| 14 | Ortak bileşenler | `EmptyState`, `ErrorState`, `SkeletonBlock`, `PageHeader`, `ConnectionIndicator`, `ConfirmDialog` |
| 15 | İzlenebilirlik | `check_traceability.py`, CI `traceability` işi, DT-03, DT-04 |

Paket sürümleri (FluentValidation, Scrutor, Cronos, EFCore.NamingConventions, Testcontainers, Respawn, CsCheck, Orval, @microsoft/signalr, MSW) her PR'da güncel kararlı sürümleri araştırılarak sabitlenir.

## 15. Proje sahibine sorulanlar

Üç soru 2026-09-30'da yanıtlandı; üçünde de önerilen seçenek seçildi ve kararlar tablosuna işlendi (BB-08…BB-10).

| Soru | Seçenekler | Yanıt |
|---|---|---|
| S1 — Platform gerçek modüllerden önce nasıl sınansın? | Test modülüyle / ilk gerçek modülle (1.2) | **Test modülüyle:** `tests` altında, yalnızca testlerde kullanılan küçük bir modül (kendi şeması, rolü ve basit bir toplu kökü). Platform bağımsız sınanır; yeni modüller için çalışan bir örnek olur. |
| S2 — İşlem geçmişi tablosu ne zaman kurulsun? | 1.1'de / 1.2'de | **1.1'de:** Audit modülünün yalnızca `audit_entries` tablosunu ve rolünü kuran iskeleti; okuma ve ekran 1.2'de. |
| S3 — Geliştirmede her modül kendi rolüyle mi bağlansın? | Evet, yayındaki gibi / tek yönetici bağlantısı | **Evet:** AppHost rol parolalarını üretir ve saklar; eksik yetki geliştirirken görülür. |

## 16. Kararlar

| No | Konu | Karar | Gerekçe |
|---|---|---|---|
| BB-01 | HTTP, OpenAPI ve SignalR altyapısının yeri | `BuildingBlocks.Infrastructure`; Host yalnızca bağlar | Modüller ortak kullanır; Host'ta kalsa test modülü ve modüller onu göremez. 08 §5 buna göre güncellenir. |
| BB-02 | İşleyicilerin çağrılması | Uç nokta, dekoratörlü işleyici arayüzünü doğrudan alır; mediator yok | Derleme anında belli çağrı, yansıma yok, gezinmesi kolay |
| BB-03 | Kısmi dinleyici hatası | Olay gönderilmemiş kalır; yeniden denemede başarılı dinleyiciler inbox sayesinde işi tekrarlamaz | Dinleyici başına ayrı teslim tablosu gerekmez; tek kaynak outbox + inbox |
| BB-04 | Sıra garantisi | Aynı sıra anahtarında bekleyen ya da hatalı eski kayıt, sonrakileri durdurur; diğer anahtarlar etkilenmez | ADR-0010'daki "aynı kaydın olayları sırayla" kuralı, hatada da korunur |
| BB-05 | Hatalı olay | 8 denemede `failed_at`; elle yeniden deneme | Sonsuz deneme yerine görünür durum; 1.2'de yönetim ekranı |
| BB-06 | Modül içi olay döngüsü | `SaveChanges` içinde en fazla 5 tur | Dinleyicinin yeni olay üretmesine izin verir, sonsuz döngüyü engeller |
| BB-07 | Rol parolaları | Rol başına ayrı gizli bilgi; geliştirmede AppHost üretir | Gizli bilgi kuralı (security §6); elle kurulum yok |
| BB-08 | Platform testleri | `tests` altında yalnızca testlerde kullanılan bir test modülüyle | S1; platform hataları iş kurallarından ayrı yakalanır |
| BB-09 | İşlem geçmişi tablosu | 1.1'de Audit iskeletiyle kurulur; okuma 1.2'de | S2; işlem geçmişi ilk modülden itibaren eksiksiz |
| BB-10 | Geliştirmede veritabanı rolleri | Yayındaki gibi modül başına rol | S3; yetki hataları geliştirirken görülür |

## 17. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-09-30 | v0.1 | İlk taslak: kapsam, tipler, komut yolu, veritabanı, işlem geçmişi, olaylar, zamanlanmış işler, HTTP, OpenAPI, SignalR, ön yüz platformu, testler, PR planı, üç soru. |
| 2026-09-30 | v0.2 | Üç soru yanıtlandı (önerilen seçenekler): BB-08…BB-10. |
| 2026-09-30 | v1.0 | Onaylandı. Uygulamada netleşenler: klasör adları `Monetary/` (ad alanı `Money` tipiyle çakışmasın) ve `Identifiers/` (Identity modülüyle karışmasın); `RuleKind`; `IStronglyTypedId<TSelf>` yalnızca `From` ister, çünkü arayüzdeki varsayılan statik metot uygulayan tipten çağrılamaz. |
| 2026-09-30 | v1.1 | 2. PR ikiye bölündü (2a komut / sorgu altyapısı, 2b sayfalama); iz aralığını loglama dekoratörü açar; `IIntegrationEventHandler` 6. PR'a, `UnitOfWorkCommandDecorator` Infrastructure'a taşındı; `ErrorCodes` ve `ValidationError` eklendi. |
| 2026-09-30 | v1.2 | `Paging/` satırı uygulamaya göre netleşti: doğrulayıcılar, `SortField`, `SortableBy`. |
| 2026-09-30 | v1.3 | 3. PR üçe bölündü: 3a veritabanı test altyapısı, 3b hazırlık, 3c modül kaydı ve veritabanı bağlamı. |
| 2026-09-30 | v1.4 | 3c ayrıca bölündü: 3c modül kaydı, 3d veritabanı bağlamı ve test modülü, 3e `migrate` komutu. |
| 2026-09-30 | v1.5 | §5.3 uygulamaya göre netleşti; Respawn 4. PR'a kaydı. |
| 2026-09-30 | v1.6 | §5.2 bağlantı düzeni: tek temel bağlantı dizesi ve rol parolaları; §5.4 `migrate` komutu (3e). |
| 2026-09-30 | v1.7 | §4 kaydetme adımlarının uygulama ayrıntıları; 4. PR ikiye bölündü (4a kaydetme adımları, 4b dekoratör). |
| 2026-09-30 | v1.8 | §4 işlem birimi dekoratörünün uygulama ayrıntıları (4b). |
| 2026-09-30 | v1.9 | §6 işlem geçmişi tablosunun uygulama ayrıntıları; 5. PR ikiye bölündü (5a tablo, 5b yazıcı). |
| 2026-09-30 | v1.10 | §6 yazıcının uygulama ayrıntıları (5b). |
| 2026-09-30 | v1.11 | §3 ve §7 outbox'a yazmanın uygulama ayrıntıları (`MessageId`, `IOutbox`); ölçüm adları gözlemlenebilirlik standardına uyduruldu; 6. PR üçe bölündü, temizlik 7. PR'a kaydı. |
| 2026-09-30 | v1.12 | §7 teslimin uygulama ayrıntıları (6b-1); 6. PR'ın kalanı yeniden bölündü (6b-1 teslim, 6b-2 dağıtıcı, 6c sıra ve ölçümler). |
| 2026-09-30 | v1.13 | §7 dağıtıcının uygulama ayrıntıları (6b-2). |
| 2026-09-30 | v1.14 | §7 sıra ve ölçümlerin uygulama ayrıntıları (6c). |
