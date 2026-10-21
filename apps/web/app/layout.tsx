import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "BatusSales | AI Business OS",
  description: "An AI-first operating system for customer conversations, bookings and sales.",
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
