# Belge araçları

Faz 0 belgelerinin tutarlılığını korumak için küçük Python betikleri. Python 3.10+ gerekir, ek paket gerekmez.

| Betik | Ne yapar | Ne zaman çalıştırılır |
|---|---|---|
| `check_links.py` | Tüm Markdown belgelerindeki göreli linkleri ve bölüm adreslerini (GitHub'ın başlık adresi kurallarıyla) doğrular. Kırık link varsa hata koduyla çıkar. | Her belge değişikliğinden sonra |
| `sync_rule_refs.py` | İş kuralı (`BR-…`), kullanıcı hikayesi (`US-…`) referanslarını doğrular ve her hikayenin `Kurallar:` satırını iş kuralları kataloğundaki `Hikayeler:` alanlarından yeniden üretir. | `03-business-rules.md` ya da hikayeler değiştiğinde |

```bash
python tools/docs/check_links.py
python tools/docs/sync_rule_refs.py
```

**Not:** Başlığında büyük "İ" bulunan bölümlerin GitHub adresinde, "i" harfinden sonra görünmeyen bir birleşik nokta işareti (U+0307) bulunur. Bu bölümlere link verirken adres elle yazılmamalı, `check_links.py` ile doğrulanmalıdır.
