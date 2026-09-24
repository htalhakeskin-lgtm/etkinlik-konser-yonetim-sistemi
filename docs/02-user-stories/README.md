# 02 — Kullanıcı Hikayeleri

> **Durum:** v1.2 · **Son güncelleme:** 2026-09-24
> **Kararlar:** [Bölüm 6](#6-kararlar)

## 1. Bu klasör ne işe yarar

Sistemin her rol için ne yapması gerektiğini, test edilebilir kabul kriterleriyle tanımlar. Geliştirmede her issue bir veya birkaç hikayeye bağlanır; bir hikaye, kabul kriterlerinin hepsi karşılandığında biter.

## 2. Ayrıntı seviyesi

Veritabanı tasarımında izlenen iki seviyeli yaklaşım burada da geçerlidir:

- **S1 (MVP):** Her hikaye kabul kriterleriyle birlikte ayrıntılı yazılır.
- **S2–S6:** Yalnızca epic seviyesinde listelenir ([Bölüm 5](#5-s2s6-epic-listesi)). Kavramsal ERD ve modül haritası için bu seviye yeterlidir. Ayrıntılı hikayeler her sürümün başında yazılır, çünkü önceki sürümlerde öğrenilenler onları değiştirecektir.

## 3. Yazım kuralları

**Kimlik:** `US-<MODÜL>-<NNN>`. Kimlik, hikaye başka bir dosyaya taşınsa da değişmez. Silinen hikayenin numarası tekrar kullanılmaz.

| Modül kodu | Modül |
|---|---|
| `SYS` | Çekirdek ve sistem |
| `PTY` | Kişi ve firma |
| `VEN` | Mekan |
| `ART` | Sanatçı ve prodüksiyon |
| `EVT` | Etkinlik |
| `RDR` | Teknik rider |
| `EQP` | Ekipman kataloğu ve stok |
| `WHS` | Depo işlemleri (çıkış, giriş, transfer) |
| `MRP` | İhtiyaç hesabı ve rezervasyon |

**Biçim:**

```markdown
### US-EVT-001 · Kısa başlık
**<Rol> olarak** <ne> **istiyorum**, **çünkü** <neden>.

Öncelik: Must · Demo adımı: 1
Kurallar: BR-EVT-016

**Kabul kriterleri**
1. Test edilebilir tek bir koşul.
```

- **Öncelik (MoSCoW):** `Must` MVP demo senaryosu için gerekli. `Should` S1'de yapılır ama demoyu engellemez. `Could` zaman kalırsa yapılır, yoksa sonraki sürüme kayar.
- **Demo adımı:** Hikayenin karşıladığı [MVP demo senaryosu](../00-scope.md#61-bitti-kriteri-mvp-demo-senaryosu) adımları.
- **Terimler** yalnızca [sözlükteki](../01-glossary.md) adlarla yazılır.
- **Kurallar** satırı, hikayeyi ilgilendiren [iş kurallarını](../03-business-rules.md) listeler. Kabul kriterleri okunabilirlik için kuralı kısaca tekrar eder; ikisi çelişirse iş kuralları kataloğu geçerlidir ve hikaye düzeltilir.

## 4. Roller ve dosyalar

Her hikaye, işi asıl yapan rolün dosyasındadır.

| Dosya | Rol | S1 hikaye sayısı |
|---|---|---|
| [00-all-users.md](00-all-users.md) | Tüm kullanıcılar | 3 |
| [01-system-admin.md](01-system-admin.md) | Sistem yöneticisi | 5 |
| [02-booking-manager.md](02-booking-manager.md) | Booking / prodüksiyon müdürü | 11 |
| [03-technical-manager.md](03-technical-manager.md) | Teknik müdür | 17 |
| [04-warehouse-manager.md](04-warehouse-manager.md) | Depo sorumlusu | 12 |
| [05-general-manager.md](05-general-manager.md) | Genel müdür | 3 |

**S1 yetki özeti:** Genel müdür S1'deki tüm ekranları salt okunur görür. Depo sorumlusu okutma işlemlerini (çıkış, giriş, transfer) yalnızca bağlı olduğu depoda yapar, diğer depoların stoğunu görür. Bir kullanıcının birden fazla rolü olabilir.

### 4.1 MVP demo senaryosu izlenebilirliği

Demo senaryosunun her adımı en az bir `Must` hikayeyle karşılanır:

| Demo adımı | Hikayeler |
|---|---|
| 1. Talep ve 2. opsiyon | US-EVT-001, US-EVT-002 |
| 2. Opsiyon yükselmesi | US-EVT-003 |
| 3. Müzakere ve Onaylı geçişleri | US-EVT-004 |
| 4. Rider versiyonunun bağlanması | US-RDR-001, US-RDR-002, US-RDR-003 |
| 5. Net ihtiyaç hesabı | US-EVT-005, US-VEN-002, US-MRP-001, US-MRP-008 |
| 6. Rezervasyon önerisi ve onayı | US-MRP-002 |
| 7. Depolar arası transfer | US-MRP-003, US-WHS-004 |
| 8. Çakışma paneli | US-MRP-004 |
| 9. Dış kiralama | US-MRP-005 |
| 10. Rider karşılama raporu | US-MRP-006 |
| 11. Toplama listesi ve çıkış | US-EQP-006, US-EQP-007, US-WHS-001, US-WHS-002 |
| 12. Giriş, hasar ve sayım farkı | US-WHS-003, US-WHS-006 |
| 13. Anlık güncelleme ve eşzamanlı çıkış | US-WHS-005, US-EQP-008 |
| 14. İşlem geçmişi | US-SYS-004 |

Demo için gerekli ana veriler (depo, taraf, mekan, sanatçı, katalog, birim, kasa) US-SYS-005, US-PTY-001, US-VEN-001, US-ART-001 ve US-EQP-001…004 ile girilir. S1'de toplam 51 hikaye var.

## 5. S2–S6 epic listesi

| Sürüm | Epic | Ana rol |
|---|---|---|
| S2 | Crew kaydı, yetkinlik ve sertifika yönetimi | Teknik müdür |
| S2 | Crew müsaitliği ve çağrı atama | Teknik müdür |
| S2 | Ekip ve mekan çakışmaları (yetkinlik, dinlenme süresi, opsiyon önceliği) | Teknik müdür |
| S2 | Kaynak takvimi (Gantt) | Teknik müdür |
| S2 | Seanslar ve etkinlik günü zaman noktaları | Booking müdürü |
| S2 | Day sheet, run of show, call sheet belgeleri | Booking müdürü |
| S2 | Input list, hospitality rider, stage plot | Teknik müdür |
| S2 | Güç hesabı ve jeneratör uyarısı | Teknik müdür |
| S2 | Bakım kayıtları ve periyodik bakım hatırlatması | Depo sorumlusu |
| S2 | Periyodik depo sayımı | Depo sorumlusu |
| S2 | Belge yükleme, uygulama içi bildirimler, Excel/CSV dışa aktarım | Tüm kullanıcılar |
| S3 | Anlaşma hunisi ve aktiviteler | Sponsorluk / satış |
| S3 | Sanatçı ücret modelleri | Booking müdürü |
| S3 | Fiyat listesi ve rider'dan teklif | Sponsorluk / satış |
| S3 | Sözleşme ve imza durumu (S1'deki elle onayın yerini alır) | Booking müdürü |
| S3 | Sponsorluk paketleri ve teslimat takibi | Sponsorluk / satış |
| S3 | Dış kiralama sipariş formu (PDF) | Teknik müdür |
| S4 | Etkinlik bütçesi ve bütçe sapması | Muhasebe |
| S4 | Gelir ve gider kaydı | Muhasebe |
| S4 | Bilet satış importu | Muhasebe |
| S4 | Çoklu para birimi ve kur | Muhasebe |
| S4 | Hesaplaşma ve hesaplaşma föyü (S1'deki elle onayın yerini alır) | Muhasebe |
| S4 | Muhasebe yazılımına dışa aktarım | Muhasebe |
| S5 | Araç ve sevkiyat yönetimi | Depo sorumlusu |
| S5 | Yükleme hesabı | Depo sorumlusu |
| S5 | Turne ve turne tarihleri | Booking müdürü |
| S5 | Şehirler arası geçiş kontrolü ve mesafeye göre depo seçimi | Teknik müdür |
| S5 | Turne haritası | Booking müdürü |
| S6 | Yönetim dashboard'u ve kârlılık | Genel müdür |
| S6 | Kullanım oranı ve satın al / kirala analizi | Teknik müdür |
| S6 | Crew mobil görünümü | Ekip üyesi |
| S6 | E-posta bildirimleri, 2FA, kayıt bazlı ince yetki | Sistem yöneticisi |
| S6 | Sarf malzeme takibi, mükerrer taraf birleştirme | Depo sorumlusu, booking müdürü |

## 6. Kararlar

| No | Soru | Karar | Gerekçe |
|---|---|---|---|
| H-01 | Dış opsiyonlar kaydedilsin mi? | Evet (US-EVT-002). | Opsiyon yükselmesi ancak önümüzdeki opsiyonlar bilinirse doğru çalışır. |
| H-02 | Dönen ekipman farklı bir depoya giriş yapabilsin mi? | Evet; giriş, okutan depo sorumlusunun deposuna yapılır (US-WHS-003). | Turne ve çok şehirli işlerde ekipman en yakın depoya döner. |
| H-03 | Dış kiralanan ekipman nasıl takip edilsin? | İki yöntem birlikte: varsayılan olarak sipariş kaydıyla, depoya teslim alınan satırlar istenirse QR ile (US-MRP-005, US-WHS-007). | Tek kasalık basit kiralamada QR gereksiz yük. Çok kasalı kiralamada eksiksiz iadeyi ancak okutma garanti eder. |
| H-04 | Rider'ı kim girer? | Teknik müdür girer ve değiştirir; booking müdürü görür (US-RDR-001). | Rider satırları katalogla eşleştirme ve muadil seçimi gerektirir; bu teknik bilgi ister. Booking müdürü rider'ı ajanstan alıp teknik müdüre iletir. |

## 7. Değişiklik kaydı

| Tarih | Versiyon | Değişiklik |
|---|---|---|
| 2026-09-24 | v0.1 | İlk taslak |
| 2026-09-24 | v1.0 | Açık sorular karara bağlandı; dış kiralamanın QR ile takibi için US-WHS-007 eklendi. |
| 2026-09-24 | v1.1 | Her hikayeye ilgili iş kuralları eklendi. US-MRP-001: hesap Onaylı durumunda da çalıştırılabilir. US-WHS-002: her okutma anında kaydedilir. |
| 2026-09-24 | v1.2 | İş kuralı kararları: US-SYS-007 ve US-MRP-008 eklendi (saat hassasiyetinde hazırlık ve dönüş payı); etkinlik zamanı saatli oldu; süresi geçmiş opsiyon işaretleri; teknik hizmette isteğe bağlı opsiyon. |
