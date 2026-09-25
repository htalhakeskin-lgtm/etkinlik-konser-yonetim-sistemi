# Security Policy · Güvenlik Politikası

## English

### Reporting a vulnerability

Please **do not open a public issue** for a security vulnerability.

Report it privately through GitHub: open the repository's **Security and quality** tab and choose **Report a vulnerability** ([GitHub Docs](https://docs.github.com/en/code-security/security-advisories/guidance-on-reporting-and-writing-information-about-vulnerabilities/privately-reporting-a-security-vulnerability)). Include what you found, how to reproduce it and its possible impact.

### What to expect

- An acknowledgement within 7 days.
- An assessment and, if confirmed, a fix plan. Critical issues are fixed within 2 days and high-severity issues within 7 days of confirmation, as defined in the project's [CI standard](docs/standards/ci.md#10-güvenlik-açığı-denetimi).
- Credit in the advisory, if you wish.

### Scope

- The code and configuration in this repository, on the `main` branch and the latest release.
- The demo environment, once it exists, is in scope for reports but **not for active testing**: do not run automated scanners, load tests or denial-of-service attempts against it, and do not access other users' data.

This is a personal portfolio project; there is no bug bounty.

---

## Türkçe

### Güvenlik açığı bildirimi

Güvenlik açıkları için **herkese açık issue açmayın**.

Açığı GitHub üzerinden gizli olarak bildirin: repodaki **Security and quality** sekmesinde **Report a vulnerability** seçeneğini kullanın ([GitHub belgesi](https://docs.github.com/en/code-security/security-advisories/guidance-on-reporting-and-writing-information-about-vulnerabilities/privately-reporting-a-security-vulnerability)). Ne bulduğunuzu, nasıl yeniden üretileceğini ve olası etkisini yazın.

### Ne beklenmeli

- 7 gün içinde alındı bilgisi.
- Değerlendirme ve açık doğrulanırsa bir düzeltme planı. Kritik açıklar doğrulandıktan sonra 2 gün, yüksek önem dereceli açıklar 7 gün içinde düzeltilir ([sürekli entegrasyon standardı](docs/standards/ci.md#10-güvenlik-açığı-denetimi)).
- İsterseniz güvenlik bildiriminde adınız anılır.

### Kapsam

- Bu repodaki kod ve yapılandırma: `main` dalı ve son sürüm.
- Demo ortamı kurulduğunda bildirimler için kapsamdadır, ama **aktif test için değildir**: otomatik tarayıcı, yük testi ya da hizmet engelleme denemesi yapmayın, başka kullanıcıların verisine erişmeyin.

Bu kişisel bir portfolyo projesidir; ödül programı yoktur.
