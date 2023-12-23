/** @type {import('next').NextConfig} */
const nextConfig = {
  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: "https://dineclick-onumz.ondigitalocean.app/api/:path*",
      },
    ];
  },
};

module.exports = nextConfig;
