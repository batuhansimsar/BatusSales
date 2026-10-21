type ApiStatus = { service: string; version: string; phase: string };

async function readApiStatus(): Promise<ApiStatus | null> {
  const baseUrl = process.env.API_INTERNAL_URL ?? "http://localhost:5080";

  try {
    const response = await fetch(`${baseUrl}/api/v1/status`, {
      cache: "no-store",
      signal: AbortSignal.timeout(3000),
    });
    if (!response.ok) return null;

    const payload: unknown = await response.json();
    if (
      typeof payload === "object" &&
      payload !== null &&
      "phase" in payload &&
      payload.phase === "foundation"
    ) {
      return payload as ApiStatus;
    }
    return null;
  } catch {
    return null;
  }
}

const plannedModules = [
  { number: "01", title: "AI Müşteri Temsilcisi", detail: "İşletmeye özel bilgi tabanı, güvenli araç kullanımı ve insan devri" },
  { number: "02", title: "Birleşik Gelen Kutusu", detail: "Web mesajları, atama, görüşme geçmişi ve çözüm durumları" },
  { number: "03", title: "CRM & Satış", detail: "Müşteri profilleri, fırsatlar ve satış aşamaları" },
  { number: "04", title: "Randevu Yönetimi", detail: "Personel, müsaitlik, rezervasyon ve çakışma önleme" },
];

export default async function Home() {
  const api = await readApiStatus();

  return (
    <main className="shell">
      <header className="topbar">
        <a className="brand" href="/" aria-label="BatuSales ana sayfa">
          <span className="brand-mark">B</span>
          <span>Batu<span className="brand-accent">Sales</span></span>
        </a>
        <span className="environment">GELİŞTİRME ORTAMI · V0.1</span>
      </header>

      <section className="hero">
        <div className="eyebrow"><span className="pulse" /> PRODUCT FOUNDATION / 2026</div>
        <h1>Her müşteri konuşması,<br /><span>daha iyi bir iş fırsatı.</span></h1>
        <p className="lede">BatuSales, işletmelerin mesajlarını, müşteri ilişkilerini ve randevularını tek merkezden yönetmesi için tasarlanıyor.</p>
        <div className="hero-actions">
          <a className="primary-button" href="#roadmap">Ürün yol haritası <span aria-hidden="true">↗</span></a>
          <a className="secondary-button" href="https://github.com/batuhansimsar/BatusSales" target="_blank" rel="noreferrer">GitHub deposu</a>
        </div>
      </section>

      <section className="status-panel" aria-label="Sistem durumu">
        <div className="status-copy">
          <p className="mini-label">SİSTEM DURUMU</p>
          <h2>İlk yapı taşı hazır.</h2>
          <p>Bu ekran gerçek bir yönetim paneli değildir. API ve altyapı geliştiriliyor; müşteri işlemleri henüz aktif değil.</p>
        </div>
        <div className="status-value">
          <span className={api ? "status-dot online" : "status-dot offline"} />
          <span>{api ? "API erişilebilir" : "API bağlantısı yok"}</span>
          <small>{api ? `${api.service} · ${api.version}` : "Bağlantıyı veya Docker servislerini kontrol edin"}</small>
        </div>
      </section>

      <section className="roadmap" id="roadmap">
        <div className="section-heading">
          <div>
            <p className="mini-label">YOL HARİTASI</p>
            <h2>İlk müşteriye giden modüller</h2>
          </div>
          <span className="section-note">P0 · PLANLANIYOR</span>
        </div>
        <div className="modules">
          {plannedModules.map((module) => (
            <article className="module" key={module.number}>
              <div className="module-top"><span>{module.number}</span><span className="not-ready">Henüz geliştirilmedi</span></div>
              <h3>{module.title}</h3>
              <p>{module.detail}</p>
              <span className="module-arrow" aria-hidden="true">↗</span>
            </article>
          ))}
        </div>
      </section>
      <footer><span>BatuSales © 2026</span><span>Build with care. Ship with evidence.</span></footer>
    </main>
  );
}
