import type { NextConfig } from "next";

const BACKEND_URL =
  process.env.BACKEND_API_URL ||
  process.env.NEXT_PUBLIC_BACKEND_URL ||
  "http://localhost:5000";

const nextConfig: NextConfig = {
  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: `${BACKEND_URL}/api/:path*`,
      },
      {
        source: "/hubs/:path*",
        destination: `${BACKEND_URL}/hubs/:path*`,
      },
    ];
  },
};

export default nextConfig;
