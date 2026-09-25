# ADR-0029: Test stratejisi ve kural izlenebilirliği

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** [standards/testing.md](../standards/testing.md), [standards/definition-of-done.md](../standards/definition-of-done.md), [ADR-0015](0015-testing-tools.md) (bu ADR onu genişletir), [03-business-rules.md §3](../03-business-rules.md#3-biçim-ve-kullanım)

## Bağlam

- [ADR-0015](0015-testing-tools.md) test araçlarını seçti ve stratejiyi Faz 0 D bölümüne bıraktı.
- 91 iş kuralı ve 62 durum geçişi var. İş kuralları kataloğu her kuralın en az bir otomatik testi olmasını istiyor ([03 §3](../03-business-rules.md#3-biçim-ve-kullanım)); bunu elle takip etmek, kural sayısı arttıkça güvenilmez hale gelir.
- Uygulamanın en riskli kısımları hesaplama motorları (ihtiyaç hesabı, müsaitlik, çakışma) ve eşzamanlılıktır (aynı birimin iki kez çıkışı). Hesaplama motorlarında örnek tabanlı testler, akla gelmeyen sınır durumlarını kaçırabilir.
- Sunucu değişmez kültürle çalışıyor ve analizörler kültürsüz metin işlemlerini yasaklıyor. Yine de Türkçe "I" sorunu, gözden kaçtığında veri bozan bir hata sınıfıdır.
- Gizli repoda CI dakikası sınırlıdır; tüm tarayıcılarda uçtan uca test ve performans testi her PR'da çalıştırılamaz.

## Karar

- **Dağılım:** Testlerin çoğu entegrasyon katmanındadır: tüm uygulama bellek içinde açılır, uç noktalara HTTP ile gidilir, gerçek PostgreSQL, gerçek oturum ve modülün kendi veritabanı rolü kullanılır. Hesaplama ve durum kuralları birim testlerinde yoğunlaşır. Uçtan uca testler demo senaryosu ve kritik akışlarla sınırlıdır.
- **İzlenebilirlik kapısı:** Kaynak kodda geçen her kural ve geçiş numarasının, o numarayla etiketlenmiş en az bir testi olması CI'da zorunludur. Kural numaralarının belgelerde tanımlı olduğu ve API'nin döndürebildiği her kural kodunun Türkçe çevirisinin bulunduğu da aynı araçla denetlenir ([testing §10](../standards/testing.md#10-kural-ve-geçiş-izlenebilirliği)).
- **Hesaplama motorları:** Örnek tabanlı testlere ek olarak özellik tabanlı testler (CsCheck, Apache 2.0) ve sürüm öncesi mutasyon testi (Stryker.NET, Apache 2.0).
- **Kültür:** Tüm .NET testleri `tr-TR` kültüründe çalışır.
- **Kod kapsamı:** Ölçülür ve raporlanır (coverlet, MIT); birleştirmeyi engelleyen bir eşik değildir.
- **Kararsız testler:** .NET testlerinde otomatik tekrar yok; uçtan uca testlerde en fazla bir tekrar ve raporlama; karantina en fazla 2 hafta.
- **Çalışma zamanı:** Her PR'da birim, entegrasyon, veritabanı, mimari, sözleşme, ön yüz ve Chromium'da uçtan uca testler. Gece: tüm tarayıcılar, performans testleri.
- **Erişilebilirlik:** Uçtan uca testlerde axe-core taraması; ciddi ve kritik ihlaller testi düşürür.

## Sonuçlar

**Olumlu:**
- Bir kural testsiz uygulanamaz; "bu kuralı hangi test doğruluyor?" sorusunun cevabı her zaman vardır.
- Testler, sahte nesnelerle yazılmış birim testlerinin göremediği hataları (eksik kayıt, işlem sınırı, yetki, veritabanı izni) yakalar.
- Kod yeniden düzenlendiğinde entegrasyon testleri değişmez, çünkü iç yapıya değil davranışa bakar.
- Hesaplama motorlarının sınır durumları rastgele girdilerle aranır.
- Türkçe kültür hataları testte ortaya çıkar.

**Olumsuz / bedeli:**
- Entegrasyon testleri birim testlerinden yavaştır ve Docker gerektirir. Modüllerin testleri paralel çalışarak ve konteyner paylaşılarak süre sınırlandırılır.
- Kaynak koddaki her kural numarasının testi olması, bir kuralın kodu ile testinin aynı PR'da gelmesini zorunlu kılar.
- Performans ve tüm tarayıcılardaki sorunlar PR'da değil, en geç ertesi sabah görünür.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Klasik test piramidi (çoğunlukla sahte nesneli birim testleri) | Gerçek veritabanıyla test yapmanın zor olduğu döneme göre çizildi. Bu uygulamada hataların çoğu parçaların birleştiği yerlerde çıkar ve sahte nesneler bunları gizler. |
| Kapsam oranı eşiği (ör. %80) | Oran hedef olunca bir şey doğrulamayan ama satır çalıştıran testler yazılır. Kural izlenebilirliği ve değişiklik türüne göre test matrisi daha anlamlı kapılardır. |
| İzlenebilirliği yalnızca kod incelemesinde kontrol etmek | Kural sayısı arttıkça güvenilmez; tek geliştiricide gözden kaçma riski yüksek. |
| CI'da başarısız testleri otomatik tekrar çalıştırmak | Kararsızlığı gizler; gerçek hatalar da tekrarla geçebilir. |
| Tüm tarayıcılarda ve performans testleriyle her PR'da çalışmak | CI süresini ve dakikasını katlar; paylaşımlı makinelerde süre ölçümü kararsız olur. |
| Microsoft'un kapsam aracı | Ücretsiz ama kapalı kaynak lisanslı; [ADR-0005](0005-dependency-license-policy.md)'teki listede değil. |
