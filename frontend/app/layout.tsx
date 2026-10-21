import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "BatusSales | AI Business Operating System",
  description: "Müşteri iletişimi, satış fırsatları ve randevular için tek iş platformu.",
};

export default function RootLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="tr">
      <body>{children}</body>
    </html>
  );
}
