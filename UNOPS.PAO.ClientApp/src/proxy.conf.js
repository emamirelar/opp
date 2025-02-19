const { env } = require("process");

const target = env.ASPNETCORE_HTTPS_PORT
  ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}`
  : env.ASPNETCORE_URLS
    ? env.ASPNETCORE_URLS.split(";")[0]
    : "https://localhost:7123";

const PROXY_CONFIG = [
  {
    context: ["/user/", "/api/"],
    target: target,
    secure: false,
    changeOrigin: true,
    headers: {
      Connection: "keep-alive",
    },
  },
];

module.exports = PROXY_CONFIG;
