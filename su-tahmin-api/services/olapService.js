const http = require("http");

const OLAP_API_BASE_URL = process.env.OLAP_API_BASE_URL;
// .env: OLAP_API_BASE_URL=http://10.116.74.91:5055

const DEFAULT_MDX = `
SELECT
  {[Measures].[Toplam Tuketim]} ON COLUMNS,
  [Dim Date].[Yil].Members ON ROWS
FROM [Smart Water DW]
`;

function postJson(url, bodyObj) {
  return new Promise((resolve, reject) => {
    if (!url) return reject(new Error("OLAP_API_BASE_URL tanımlı değil (.env)."));

    const u = new URL(url);
    const data = JSON.stringify(bodyObj);

    const options = {
      hostname: u.hostname,
      port: u.port || 80,
      path: u.pathname,
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Content-Length": Buffer.byteLength(data)
      }
    };

    const req = http.request(options, (res) => {
      let raw = "";
      res.on("data", (chunk) => (raw += chunk));
      res.on("end", () => {
        let parsed;
        try {
          parsed = JSON.parse(raw);
        } catch {
          parsed = { raw };
        }

        if (res.statusCode < 200 || res.statusCode >= 300) {
          return reject(
            new Error(`OLAP API hata: ${res.statusCode} ${JSON.stringify(parsed)}`)
          );
        }

        resolve(parsed);
      });
    });

    req.on("error", reject);
    req.write(data);
    req.end();
  });
}

function getText(url) {
  return new Promise((resolve, reject) => {
    if (!url) return reject(new Error("OLAP_API_BASE_URL tanımlı değil (.env)."));

    const u = new URL(url);

    const options = {
      hostname: u.hostname,
      port: u.port || 80,
      path: u.pathname,
      method: "GET"
    };

    const req = http.request(options, (res) => {
      let raw = "";
      res.on("data", (chunk) => (raw += chunk));
      res.on("end", () => resolve({ statusCode: res.statusCode, body: raw }));
    });

    req.on("error", reject);
    req.end();
  });
}

const getOlapData = async (req, res) => {
  try {
    const result = await postJson(`${OLAP_API_BASE_URL}/mdx`, { Query: DEFAULT_MDX });

    return res.json({
      success: true,
      source: "SOA -> OLAP API (/mdx) -> SSAS",
      data: result
    });
  } catch (err) {
    console.error("OLAP API çağrı hatası:", err.message);
    return res.status(500).json({
      success: false,
      error: err.message
    });
  }
};

const testOlapConnection = async (req, res) => {
  try {
    const r = await getText(`${OLAP_API_BASE_URL}/ping`);

    if (r.statusCode < 200 || r.statusCode >= 300) {
      return res.status(r.statusCode).json({
        ok: false,
        message: "OLAP API ping başarısız",
        pong: r.body
      });
    }

    return res.status(200).json({
      ok: true,
      message: "OLAP API bağlantısı başarılı",
      pong: r.body
    });
  } catch (err) {
    console.error("OLAP test hatası:", err.message);
    return res.status(500).json({
      ok: false,
      message: "OLAP API'ye erişilemedi",
      error: err.message
    });
  }
};

module.exports = { getOlapData, testOlapConnection };