# Teknoloji incelemesi — 2026-10-21

Her geliştirme öncesi yeniden kontrol edilecek; burada kayıtlı sürüm numaraları otomatik güncel kabul edilmez.

| Teknoloji | Kanıt | BatuSales kararı |
| --- | --- | --- |
| .NET 10 LTS | Microsoft 13 Ekim 2026 destek tablosu: .NET 10 aktif LTS, 2028'e kadar | Yeni backend için seç |
| Next.js 16 | Next.js resmi destek politikası: 16.x Active LTS | Frontend için seç; güvenlik yamalarını izle |
| Microsoft Agent Framework 1.0 | Nisan 2026 üretime hazır .NET/Python sürümü; tool approval/orchestration | AI P0 başladığında proof-of-concept, gereksiz multi-agent yok |
| EF Core 10 named query filters | Tenant ve soft-delete filtreleri ayrı uygulanabilir | Tenancy veri modeli gelince değerlendir, auth yerine kullanma |
| ASP.NET Core rate limiter | Yerleşik partitioned rate limit middleware | API'de geçici IP bazlı limit; auth sonrası tenant kotası |
| PostgreSQL exclusion constraint | Bookings için DB düzeyi overlap önleme ihtiyacı | Booking migration sırasında zorunlu |

Kaynaklar:
- https://dotnet.microsoft.com/en-us/platform/support/policy
- https://nextjs.org/support-policy
- https://devblogs.microsoft.com/agent-framework/microsoft-agent-framework-version-1-0/
- https://learn.microsoft.com/en-us/ef/core/querying/filters
- https://learn.microsoft.com/aspnet/core/performance/rate-limit?view=aspnetcore-10.0

**Yeni teknoloji ekleme ilkesi:** Üretime hazır mı, güvenlik/sürüm politikası net mi, P0 sonucunu ölçülebilir biçimde iyileştiriyor mu? Aksi halde P1/P2 backlog'a.
