import { NextResponse } from "next/server";

export const dynamic = "force-dynamic";

export async function GET() {
  // Only a server-managed configuration value controls the destination.
  const origin = (process.env.BATUSSALES_API_URL ?? "http://localhost:5080").replace(/\/$/, "");
  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), 3000);

  try {
    const response = await fetch(origin + "/health/live", {
      cache: "no-store",
      signal: controller.signal,
    });

    if (!response.ok) {
      return NextResponse.json({ status: "offline" }, { status: 503 });
    }

    return NextResponse.json(
      { status: "online" },
      { headers: { "Cache-Control": "no-store" } },
    );
  } catch {
    return NextResponse.json({ status: "offline" }, { status: 503 });
  } finally {
    clearTimeout(timeout);
  }
}
