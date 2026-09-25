# ADR-0011: Kimlik doğrulama: sunucu tarafı oturumlu çerez

- **Durum:** Kabul edildi
- **Tarih:** 2026-09-25
- **İlgili:** BR-SYS-002, BR-SYS-005…BR-SYS-008, [ADR-0008](0008-frontend-architecture.md), [ADR-0012](0012-realtime-signalr.md)

## Bağlam

- Kullanıcılar e-posta ve şifreyle girer (US-SYS-010).
- Pasifleştirilen kullanıcının oturumları hemen sona ermelidir (BR-SYS-008).
- Şifre değişince diğer oturumlar kapanmalıdır (BR-SYS-007).
- Hatalı girişlerde hesap kilitlenmelidir (BR-SYS-005).
- Ön yüz API ile aynı adresten sunulur ([ADR-0008](0008-frontend-architecture.md)). SignalR bağlantısının da kimliği doğrulanmalıdır.

## Karar

- **Oturum çerezi:** Giriş başarılı olunca tarayıcıya yalnızca oturum kimliğini taşıyan bir çerez verilir. Çerez JavaScript'ten okunamaz (`HttpOnly`), yalnızca HTTPS'te gönderilir (`Secure`) ve başka sitelerden gelen isteklerde gönderilmez (`SameSite`).
- **Sunucu tarafı oturum:** Oturumun kendisi Identity modülünün `Session` tablosunda tutulur. Her istekte doğrulanır; sık erişim için kısa süreli önbellek kullanılır. Oturum sunucuda sonlandırılınca kullanıcı bir sonraki istekte düşer.
- **Şifre:** ASP.NET Core Identity'nin parola özetleme bileşeni kullanılır. Kilit kuralı Identity modülünde uygulanır.
- **İstek sahteciliğine karşı koruma:** Çerezle kimlik doğrulamada, veri değiştiren isteklerde ASP.NET Core'un antiforgery belirteci istenir.
- **Yetki:** Her uç nokta, modülün tanımladığı bir yetkiyi ister. Kullanıcının rollerinden gelen yetkiler oturum açılırken yüklenir ve oturumla birlikte tutulur (BR-SYS-002).
- **SignalR:** Aynı çerezle kimlik doğrular.

## Sonuçlar

**Olumlu:**
- Belirteç tarayıcı depolamasında tutulmadığı için, sayfaya zararlı kod enjekte edilse bile çalınamaz.
- Oturumlar anında sonlandırılabilir; pasifleştirme ve şifre değişikliği kuralları doğrudan karşılanır.
- Tek adres ve tek çerezle yapılandırma basittir.

**Olumsuz / bedeli:**
- Her istekte oturum doğrulanır; bunun maliyeti önbellekle düşürülür.
- Mobil uygulama ya da başka sistemler API'yi kullanmak isterse ayrıca belirteç tabanlı bir yöntem eklenmesi gerekir (S1 kapsamında yok).

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Tarayıcıda saklanan JWT | Sayfaya enjekte edilen kodla çalınabilir. Süresi dolmadan geri alınması ek bir kara liste mekanizması gerektirir. |
| Harici kimlik sağlayıcı (Keycloak, Entra ID) | SSO kapsam dışı; ayrı bir sunucu işletmek tek kişilik proje için ağır. |
| ASP.NET Core Identity'nin tam hazır arayüzü | Hazır sayfalar ve tablo yapısı bizim kullanıcı modelimize ve ön yüzümüze uymaz; yalnızca parola bileşeni yeterli. |
