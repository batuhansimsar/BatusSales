const priorities = [
  { title: "AI Müşteri Temsilcisi", description: "İşletme bilgisinden yanıt üretimi ve güvenli insan devri." },
  { title: "Birleşik Inbox", description: "Önce web chat; sonrasında resmi mesajlaşma kanalları." },
  { title: "CRM ve Randevu", description: "Müşteri geçmişi, satış aşamaları ve çakışmasız takvim." },
];

export default function HomePage() {
  return (
    <main className="shell">
      <header className="topbar">
        <a className="brand" href="/" aria-label="BatusSales ana sayfa">
          <span className="brandMark">B</span>
          BatusSales
        </a>
        <span className="environment">MVP geliştirme aşamasında</span>
      </header>

      <section className="hero" aria-labelledby="hero-title">
        <p className="eyebrow">AI BUSINESS OPERATING SYSTEM</p>
        <h1 id="hero-title">Müşteri konuşmalarından <em>iş sonuçlarına.</em></h1>
        <p className="lead">
          İşletmelerin mesajlarını, satış fırsatlarını ve randevularını tek noktadan
          yönetmesi için tasarlanan yapay zekâ destekli çalışma alanı.
        </p>
        <a
          className="cta"
          href="https://github.com/batuhansimsar/BatusSales"
          target="_blank"
          rel="noreferrer"
        >
          Geliştirmeyi GitHub'da takip et <span aria-hidden="true">↗</span>
        </a>
        <p className="disclaimer">
          Bu sayfa proje başlangıç ekranıdır; aşağıdaki özellikler henüz müşterilere açık değildir.
        </p>
      </section>

      <section className="modules" aria-labelledby="modules-title">
        <div className="sectionHeader">
          <p className="eyebrow">V1 ÖNCELİKLERİ</p>
          <h2 id="modules-title">İlk sürümün çekirdeği</h2>
        </div>
        <div className="cards">
          {priorities.map((item) => (
            <article className="moduleCard" key={item.title}>
              <div className="status">P0 · Planlandı</div>
              <h3>{item.title}</h3>
              <p>{item.description}</p>
            </article>
          ))}
        </div>
      </section>
      <footer>© {new Date().getFullYear()} BatusSales · Pre-MVP</footer>
    </main>
  );
}
