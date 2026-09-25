# ADR-0001: Modüler monolit

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [05-module-map.md](../05-module-map.md), [ADR-0002](0002-module-boundaries-and-layers.md)

## Bağlam

- Proje tek bir geliştirici tarafından yürütülüyor.
- Alan karmaşık: S1'de 10 modül var, S6'da bu sayı 17'ye çıkıyor. Modüller arasında sıkı iş kuralları bulunuyor.
- Ölçek küçük: yaklaşık 20 eşzamanlı kullanıcı, 3 depo, yılda 300 etkinlik.
- Değişikliklerin ekranlara 300 milisaniyede yansıması hedefleniyor.
- Portfolyo açısından modül sınırlarının kodda açıkça görünmesi isteniyor.

## Karar

Sistem **tek bir dağıtılabilir uygulama** ve **tek bir PostgreSQL veritabanı** olarak kurulur. İçeride modüller, ayrı servislermiş gibi sınırlarla ayrılır:

- Her modülün kendi veritabanı şeması vardır.
- Başka bir modüle yalnızca onun `Contracts` projesi üzerinden erişilir.
- Modüller arası yan etkiler entegrasyon olaylarıyla taşınır.

## Sonuçlar

**Olumlu:**
- Tek dağıtım ve tek veritabanı olduğu için işletim yükü azdır.
- Hata ayıklamak kolaydır; bir isteğin tüm akışı tek süreçte izlenebilir.
- Modüller arası çağrılar ağ üzerinden değil süreç içinde gerçekleşir; anlık güncelleme hedefi buna dayanır.
- Sınırlar net olduğu için ileride bir modül ayrı bir servise dönüştürülebilir.

**Olumsuz / bedeli:**
- Sınırları koruma disiplini gerekir. Bu disiplin mimari testlerle derleme sırasında zorlanır ([ADR-0015](0015-testing-tools.md)).
- Tüm modüller aynı süreçte çalışır; bir modüldeki bellek sızıntısı ya da aşırı yük diğerlerini de etkiler.
- Modüller bağımsız ölçeklenemez. Bu ölçekte gerek de yoktur.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Mikroservisler | Ayrı dağıtım, ağ üzerinden haberleşme, dağıtık işlem ve izleme tek kişilik bir proje için ağır bir işletim yükü getirir. Bu ölçekte bağımsız ölçeklemenin getirisi yoktur. |
| Klasik katmanlı monolit (tek proje, modül sınırı yok) | Başta hızlıdır ama her kod her koda erişebildiği için bağımlılıklar zamanla düğümlenir. S6'ya kadar büyüyecek bir sistemde bakımı zorlaşır. |
