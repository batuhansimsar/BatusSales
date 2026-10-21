# Güvenlik eşikleri (P0)

Bu sayfa **tamamlanmış kontrollerin kanıtı değildir**; canlı sisteme geçmeden karşılanacak kriterlerdir.

- [ ] OIDC/session veya doğrulanmış JWT kimlik doğrulama; rotasyon, logout, session iptali
- [ ] Owner/Admin/Agent sunucu tarafı authorization ve tenant membership doğrulaması
- [ ] Cross-tenant read/write/export ve background job e2e negatif testleri
- [ ] API anahtarları/secrets secret store'da; .env commit dışı
- [ ] AI prompt injection test corpus ve tool permission allowlist
- [ ] Kişisel veri, saklama/silme, KVKK/GDPR değerlendirmesi
- [ ] Audit log, hassas verilerin masking'i, rate limits ve maliyet bütçeleri
- [ ] Randevu için DB düzeyinde çakışma koruması
- [ ] Webhook imza doğrulama, retry, idempotency ve mesaj kuyruğunda izolasyon
- [ ] Backup ve gerçek restore denemesi
- [ ] CI + backend unit/integration + frontend E2E + agent evaluation

Not: `/health/live` ve `/api/v1/meta` herkese açık olduğundan yalnız genel metadata içerir. Geliştirilen sonraki iş uçları bu modele kopyalanmamalıdır.
