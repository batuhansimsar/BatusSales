"use client";

import { useEffect, useState } from "react";

type Status = "checking" | "online" | "offline";

export function ApiStatus() {
  const [status, setStatus] = useState<Status>("checking");

  useEffect(() => {
    const controller = new AbortController();

    fetch("/api/health", { cache: "no-store", signal: controller.signal })
      .then((response) => {
        if (!controller.signal.aborted) {
          setStatus(response.ok ? "online" : "offline");
        }
      })
      .catch(() => {
        if (!controller.signal.aborted) {
          setStatus("offline");
        }
      });

    return () => controller.abort();
  }, []);

  const label =
    status === "online"
      ? "API bağlantısı aktif"
      : status === "offline"
        ? "API bağlantısı yok"
        : "API kontrol ediliyor";

  return (
    <div
      role="status"
      aria-live="polite"
      className="flex items-center gap-2 rounded-full border border-white/10 bg-white/5 px-4 py-2 text-sm text-slate-300"
    >
      <span
        className={
          "h-2.5 w-2.5 rounded-full " +
          (status === "online"
            ? "bg-emerald-400"
            : status === "offline"
              ? "bg-rose-400"
              : "bg-amber-400")
        }
      />
      {label}
    </div>
  );
}
