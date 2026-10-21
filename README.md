# BatusSales

**AI Business Operating System** — işletmelerin müşteri iletişimini, satış fırsatlarını ve randevularını tek bir SaaS platformunda yönetmesi için geliştirilen proje.

> **Durum (21 Ekim 2026):** Foundation / pre-MVP. Bu repository'deki ilk geliştirme temel uygulama iskeleti ve saf domain kurallarını içerir. Kimlik doğrulama, gerçek tenant izolasyonu, veritabanı işlemleri, AI agent ve ücretlendirme **henüz uygulanmadı**. Üretimde kullanmayın.

## Teknoloji yönü

- Backend: ASP.NET Core **.NET 10 LTS**, modüler monolit; ilerleyen aşamada EF Core + PostgreSQL
- Frontend: **Next.js 16**, React 19, TypeScript, Tailwind CSS 4
- Veritabanı: PostgreSQL (yerel altyapı); Redis/RabbitMQ yalnız ihtiyaç doğunca
- Test: xUnit; frontend TypeScript ve build kontrolü
- AI: model bağımsız katman; RAG/Agent Framework seçimi ölçümlerden sonra

## Hızlı başlangıç

Gereksinimler: .NET SDK 10, Node.js 22+, Docker Compose.

```bash
cp .env.example .env
# .env içindeki POSTGRES_PASSWORD değerini değiştir
docker compose up -d postgres

dotnet run --project backend/BatusSales.Api
# başka terminalde:
cd frontend
npm install
npm run dev
```

Backend: `http://localhost:5000/health/live` (dotnet run'ın terminalde gösterdiği gerçek portu kullanın). Frontend: `http://localhost:3000`.

PostgreSQL şu an uygulamaya bağlı değildir; ORM ve migration'lar Identity/Tenancy diliminde eklenecektir. `/health/live` yalnız prosesin çalıştığını denetler, veritabanı readiness kontrolü değildir.

## Kontroller

```bash
dotnet build backend/BatusSales.Api/BatusSales.Api.csproj
dotnet test backend/BatusSales.Tests/BatusSales.Tests.csproj

cd frontend
npm install
npm run typecheck
npm run build
```

## Çalışma kuralı: önce mevcut durumu kontrol et

1. `main` ve açık PR'ların son commit SHA'larını, dosyaları ve CI durumlarını kontrol et.
2. `docs/PROGRESS.md` kaydını kodla karşılaştır; iş zaten yapıldıysa tekrar commit atma.
3. Tek bir P0 kabul kriteri seç, ayrı branch'te geliştir ve test et.
4. PR'da değişen dosyaları, test sonuçlarını, bilinen riskleri ve yeni teknolojilerin değerlendirmesini kaydet.
5. Birleştirilmiş iş için ilerleme kaydını güncelle; P1/P2'yi kanıtlanmış müşteri ihtiyacına göre planla.

Detaylar: [Yol haritası](docs/PROGRESS.md), [Mimari kararlar](docs/architecture/ADR-0001.md), [Güvenlik kuralları](docs/SECURITY.md), [Teknoloji araştırması](docs/TECH_WATCH.md).

## Lisans

Henüz bir açık kaynak lisansı seçilmedi. Lisans eklenene kadar varsayılan telif hakları geçerlidir.
