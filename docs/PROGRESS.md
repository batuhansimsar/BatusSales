# BatuSales — Proje ilerleme kaydı

**Son repository incelemesi:** 2026-10-21  
**İncelenen main SHA:** `08fef80b8e14d1627a0fbad5348f9da872548fa7`  
**İnceleme anındaki PR sayısı:** 0  
**Gerçek durum:** Pre-MVP; README-only başlangıcından ilk foundation PR'ına geçiş.

## P0 teslim tablosu

| Dilim | Durum | Kabul kanıtı |
| --- | --- | --- |
| Repo/CI/backend/frontend iskeleti | Bu PR ile teklif ediliyor | CI build/test kontrolü |
| Auth + aktif tenant membership | Yapılmadı | 401/403, cross-tenant testleri |
| Business onboarding ve roller | Yapılmadı | DB migration ve E2E |
| Web chat widget + inbox | Yapılmadı | Gerçek zamanlı insan devri |
| İşletmeye özel tek AI agent | Yapılmadı | Grounded eval, araç izinleri |
| CRM müşteri profili/pipeline | Yapılmadı | Tenant izolasyon testleri |
| Booking personel/hizmet/takvim | Domain interval helper mevcut, P0 yapılmadı | DB collision/concurrency testleri |
| Basit otomasyon ve dashboard | Yapılmadı | Olay ve kullanım metrikleri |
| Billing/trial/kotalar | Yapılmadı | Webhook/idempotency testleri |
| Audit, veri politikası, backup | Yapılmadı | Güvenlik test planı |

## Foundation değişiklikleri

- API'de yalnız health ve genel metadata
- Core'da TenantId/TenantAccess ve BookingInterval saf domain helper'ları
- xUnit testleri; üretim tenant authorization yerine geçmez
- Next.js başlangıç sayfası (demo değil, gerçek plan statüsü)
- Yerel PostgreSQL Compose; API'ye henüz bağlı değil
- CI, mimari ve güvenlik kabul kriterleri

## Sıradaki tek P0 işi

**P0-01: Identity + Tenancy dikey dilimi.** PostgreSQL şeması/migration, kullanıcı işletme üyeliği, auth, sunucu tarafı RBAC, korumalı ilk endpoint, tenant ayrımı e2e testleri. Kimlik çözümü ve üyelik doğrulaması olmadan müşteri API'lerini ekleme.

## Her oturumda tekrar önleme

1. Branches + open PRs + main commit SHA kontrolü.
2. İstenen özellik kodda veya PR'da varsa yeniden yazma.
3. İlerleme tablosunu gerçek kod ve CI ile güncelle.
4. Değişikliği tek, yeni, anlamlı commit/PR altında tut.
5. Release/migration güvenliği ve yeni teknoloji araştırmasını kaydet.
