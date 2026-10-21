import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "BatuSales — Business OS",
  description: "AI-assisted operations for ambitious local businesses.",
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
