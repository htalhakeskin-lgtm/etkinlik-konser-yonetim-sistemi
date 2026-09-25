# ADR-0012: Anlık güncelleme: SignalR

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** BR-SYS-012, US-WHS-005, [05-module-map.md §9.2](../05-module-map.md#92-olayların-teslimi), [ADR-0008](0008-frontend-architecture.md)

## Bağlam

- Stok, rezervasyon, transfer ve çakışma değişiklikleri, açık ekranlara sayfa yenilenmeden yansımalıdır.
- Hedef: işlemlerin %95'inde 300 milisaniye, en geç 1 saniye.
- Bağlantı koptuğunda kullanıcı uyarılmalı; bağlantı geri gelince güncel veri yüklenmelidir.

## Karar

- **Teknoloji:** ASP.NET Core SignalR. Hub, ana uygulamada (Host) bulunur.
- **Gruplar:** İstemciler ilgilendikleri gruplara katılır: `warehouse:{id}`, `event:{id}`, `user:{id}`.
- **Tetikleme:** Host entegrasyon olaylarını dinler ve ilgili gruplara kısa bir **değişiklik bildirimi** gönderir: ne değişti, hangi kayıt.
- **Veri akışı:** İstemci bildirimi alınca ilgili TanStack Query sorgularını geçersiz kılar ve veriyi API'den yeniden alır. Böylece verinin tek kaynağı API olarak kalır; ekranın gösterdiği veri ile API'nin döndürdüğü hiçbir zaman farklılaşmaz.
- **Bağlantı:** Kopunca SignalR istemcisi otomatik yeniden bağlanır. Arayüz bu sırada uyarı gösterir; bağlantı geri gelince açık sorguların hepsi yenilenir.
- **Ölçek:** Birden fazla uygulama örneği gerekirse bir SignalR arka düzlemi (backplane) eklenir.

## Sonuçlar

**Olumlu:**
- .NET'in yerleşik parçasıdır; ek bağımlılık yoktur.
- Gruplar, yeniden bağlanma ve kimlik doğrulama hazır gelir.
- "Bildirim + yeniden sorgu" yaklaşımı basittir ve tutarsız ekran riskini ortadan kaldırır.

**Olumsuz / bedeli:**
- Her bildirim bir API isteğine yol açar. Bu ölçekte (yaklaşık 20 eşzamanlı kullanıcı) sorun değildir. Gerekirse küçük değişiklikler bildirimin içinde de gönderilebilir.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Server-Sent Events | Tek yönlü akış için yeterli; ama gruplar ve yeniden bağlanma elle yazılır. |
| Aralıklı sorgulama (polling) | Gecikme hedefini ancak çok sık sorguyla karşılar; gereksiz yük getirir. |
| Doğrudan WebSocket | Bağlantı yönetimi, gruplar ve yeniden bağlanma elle yazılır. |
