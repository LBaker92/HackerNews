const { env } = require('process');

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
  env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'http://localhost:11954';

console.log(`Api URL: ${target}`);

const PROXY_CONFIG =
{
  "/api": {
    target: target,
    secure: false
  }
}

// const PROXY_CONFIG = [
//   {
//     context: [
//       "/stories"
//     ],
//     target: target,
//     secure: false
//   }
// ]

module.exports = PROXY_CONFIG;
