# ADR-0006: Sunucu platformu: .NET 10 LTS ve ASP.NET Core Minimal API

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [07-tech-stack.md §3.1](../07-tech-stack.md#31-sunucu-backend), [ADR-0001](0001-modular-monolith.md), [ADR-0005](0005-dependency-license-policy.md)

## Bağlam

Sunucu tarafı için .NET, projenin başlangıcında seçilmişti. Seçilmesi gerekenler: hangi .NET sürümü, API'nin hangi biçimde yazılacağı, komut ve sorguların nasıl işleneceği.

.NET 10 Kasım 2025'te çıkan uzun destekli (LTS) sürümdür ve Kasım 2028'e kadar desteklenir. .NET 11 Kasım 2026'da kısa destekli (STS) olarak çıkacak; desteği de .NET 10 ile aynı tarihte bitecek.

## Karar

- **Platform:** .NET 10 LTS, C# 14. .NET 11 atlanır; sıradaki geçiş .NET 12 LTS'e yapılır.
- **API:** ASP.NET Core Minimal API. Her modül kendi uç noktalarını kendi projesinde tanımlar; ana uygulama (Host) yalnızca modüllerin uç nokta gruplarını bağlar.
- **Komut ve sorgular:** Kendi basit arayüzlerimizle yazılır (`ICommandHandler<TCommand, TResult>`, `IQueryHandler<TQuery, TResult>`). Doğrulama, işlem birimi, işlem geçmişi ve loglama, işleyicilerin etrafına DI dekoratörleriyle eklenir (Scrutor).
- **Doğrulama:** Giriş verisinin biçim doğrulaması FluentValidation ile yapılır. İş kuralları alan (domain) katmanında uygulanır ve ihlalde kural numarasını taşıyan bir hata üretir.
- **Nesne eşleme:** Elle yazılır. Tekrarlayan eşlemeler çoğalırsa Mapperly kullanılır.
- **API belgesi:** ASP.NET Core'un dahili OpenAPI üretimi. Arayüz olarak Scalar. Bu belge ön yüzün API istemcisinin kaynağıdır ([ADR-0008](0008-frontend-architecture.md)).

## Sonuçlar

**Olumlu:**
- Üç yıllık destek penceresiyle başlanır.
- Minimal API, modül başına uç nokta gruplamayla modüler yapıya doğal uyar ve controller yapısından daha az tören gerektirir.
- Komut hattı tamamen bizim kodumuzdur; lisans riski yoktur ve dekoratörlerin sırası açıkça görülür.

**Olumsuz / bedeli:**
- MediatR gibi bir kütüphanenin hazır sunduğu boru hattı (pipeline) birkaç yüz satırlık kendi kodumuzla yazılır.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| .NET 11 ile başlamak | Kısa destekli; desteği .NET 10 ile aynı anda bitiyor. Projenin başında sürüm çıkmamış olacak. |
| Controller tabanlı API | Çalışır ama modül başına daha fazla tören getirir; Minimal API yeterlidir. |
| MediatR | Ücretli lisansa geçti ([ADR-0005](0005-dependency-license-policy.md)). |
| Wolverine | Komut işleme, mesajlaşma ve outbox'ı birlikte sunan güçlü bir çatı. Ama projenin öğretmeyi amaçladığı çekirdeği gizler ve projeyi tek bir çatıya bağlar ([ADR-0010](0010-messaging-infrastructure.md)). |
| FastEndpoints | İyi bir kütüphane ama Minimal API'nin üstüne ek bir çatı katmanı ekler; buna gerek yoktur. |
