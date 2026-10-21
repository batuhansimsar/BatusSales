import { ApiStatus } from "../components/api-status";

const principles = [
  {
    number: "01",
    title: "Tek gelen kutusu",
    text: "Web sohbetiyle başlayıp tüm kanalları zamanla aynı iş akışında birleştirme.",
  },
  {
    number: "02",
    title: "Güvenilir AI",
    text: "İşletmenin bilgilerinden yanıt verme, belirsizliği insana devretme ve her aksiyonu denetleme.",
  },
  {
    number: "03",
    title: "Randevu & CRM",
    text: "İşletmeye özel müşteri, teklif ve rezervasyon süreçlerini tek yerde toplama.",
  },
];

export default function Home() {
  return (
    <main className="relative min-h-screen overflow-hidden bg-[#090d19]">
      <div aria-hidden="true" className="pointer-events-none absolute -right-48 -top-64 h-[700px] w-[700px] rounded-full bg-violet-800/20 blur-[130px]" />
      <div className="relative mx-auto max-w-6xl px-6 pb-28 pt-7 md:px-10">
        <header className="flex flex-wrap items-center justify-between gap-4 border-b border-white/10 pb-7">
          <a href="/" className="flex items-center gap-3 text-xl font-bold tracking-tight">
            <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-violet-500 text-white">B</span>
            BatusSales
          </a>
          <ApiStatus />
        </header>

        <section className="grid items-center gap-14 py-24 lg:grid-cols-[1.25fr_1fr]">
          <div>
            <span className="inline-flex rounded-full border border-violet-400/30 bg-violet-400/10 px-4 py-2 text-xs font-semibold uppercase tracking-[0.2em] text-violet-200">
              Foundation · Henüz MVP değil
            </span>
            <h1 className="mt-9 max-w-3xl text-5xl font-bold leading-[1.1] tracking-tight text-white md:text-7xl">
              İşletmenizin yeni
              <span className="block bg-gradient-to-r from-violet-300 via-indigo-300 to-cyan-200 bg-clip-text text-transparent">yapay zekâ işletim sistemi.</span>
            </h1>
            <p className="mt-7 max-w-xl text-lg leading-8 text-slate-400">
              Mesajlar, müşteri ilişkileri, randevular ve otomasyonlar.
              Tek SaaS platformunda bir araya gelecek şekilde tasarlanıyor.
            </p>
            <div className="mt-9 flex flex-wrap gap-3">
              <a href="#roadmap" className="rounded-xl bg-violet-500 px-6 py-3 font-semibold text-white transition hover:bg-violet-400">
                Yol haritasını gör
              </a>
              <a href="#architecture" className="rounded-xl border border-white/15 px-6 py-3 font-semibold text-white transition hover:border-violet-400/50">
                Mimari ilkeler
              </a>
            </div>
          </div>

          <div className="rounded-3xl border border-white/10 bg-white/[0.045] p-6 shadow-2xl shadow-violet-950/30">
            <div className="mb-7 flex items-center justify-between">
              <h2 className="font-semibold text-white">Ürün geliştirme durumu</h2>
              <span className="rounded-full bg-amber-400/10 px-3 py-1 text-xs font-medium text-amber-200">Başlangıç</span>
            </div>
            <div className="space-y-4">
              {[
                ["Mimari ve API temeli", "Bu aşamada"],
                ["Hesap & tenant yönetimi", "Sonraki adım"],
                ["Web sohbeti & inbox", "Planlandı"],
                ["AI agent & bilgi bankası", "Planlandı"],
                ["CRM & randevu", "Planlandı"],
                ["Abonelik & güvenlik testleri", "Planlandı"],
              ].map(([title, status], index) => (
                <div key={title} className="flex items-center justify-between gap-4 rounded-xl border border-white/10 bg-[#0f1425] px-4 py-4">
                  <span className="flex items-center gap-3 text-sm text-slate-200">
                    <span className="text-slate-500">{String(index + 1).padStart(2, "0")}</span>
                    {title}
                  </span>
                  <span className="shrink-0 text-xs text-slate-400">{status}</span>
                </div>
              ))}
            </div>
            <p className="mt-5 text-xs leading-5 text-slate-500">
              Bu görünüm gerçek müşteri verisi içermez. Canlı özellikler tamamlandıkça yenilenecek.
            </p>
          </div>
        </section>

        <section id="roadmap" className="border-t border-white/10 py-16">
          <div className="mb-9">
            <p className="text-sm font-semibold uppercase tracking-[0.2em] text-violet-300">Öncelik P0</p>
            <h2 className="mt-3 text-3xl font-bold text-white">İlk ücretli müşteriye giden yol</h2>
          </div>
          <div className="grid gap-4 md:grid-cols-3">
            {principles.map((principle) => (
              <article key={principle.number} className="rounded-2xl border border-white/10 bg-white/[0.035] p-7">
                <span className="text-sm font-semibold text-violet-300">{principle.number}</span>
                <h3 className="mt-6 text-xl font-semibold text-white">{principle.title}</h3>
                <p className="mt-3 leading-7 text-slate-400">{principle.text}</p>
              </article>
            ))}
          </div>
        </section>

        <section id="architecture" className="rounded-3xl border border-white/10 bg-[#101528] px-8 py-10">
          <h2 className="text-2xl font-semibold text-white">Önce güvenlik ve sadelik</h2>
          <p className="mt-4 max-w-4xl leading-8 text-slate-400">
            Modüler monolit yaklaşımı; .NET 10 API, Next.js 16, TypeScript ve Tailwind.
            Kimlik doğrulama ve tenant bazlı veri yalıtımı tamamlanmadan müşteri verisi alan modüller açılmayacak.
          </p>
        </section>
      </div>
    </main>
  );
}
