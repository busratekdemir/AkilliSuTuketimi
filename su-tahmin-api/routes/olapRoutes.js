const express = require("express");
const router = express.Router();

const OLAP_API_BASE_URL = process.env.OLAP_API_BASE_URL;

// 🧪 OLAP API'ye ping at (Postman'da "bağlandı" mesajı için)
router.get("/ping-olap", async (req, res) => {
  try {
    if (!OLAP_API_BASE_URL) {
      return res.status(500).json({
        ok: false,
        error: "OLAP_API_BASE_URL tanımlı değil (.env)"
      });
    }

    const r = await fetch(`${OLAP_API_BASE_URL}/ping`);
    const text = await r.text();

    return res.status(200).json({
      ok: true,
      message: "OLAP API erişildi",
      pong: text
    });
  } catch (e) {
    return res.status(500).json({
      ok: false,
      message: "OLAP API erişilemedi",
      error: e.message
    });
  }
});

// 📊 Dinamik MDX çalıştır (Postman ile gerçek OLAP testi)
router.post("/mdx", async (req, res) => {
  try {
    if (!OLAP_API_BASE_URL) {
      return res.status(500).json({
        ok: false,
        error: "OLAP_API_BASE_URL tanımlı değil (.env)"
      });
    }

    const { Query } = req.body;
    if (!Query) {
      return res.status(400).json({
        ok: false,
        error: "Query boş."
      });
    }

    const r = await fetch(`${OLAP_API_BASE_URL}/mdx`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ Query })
    });

    const text = await r.text();

    let data;
    try {
      data = JSON.parse(text);
    } catch {
      data = { raw: text };
    }

    if (!r.ok) {
      return res.status(r.status).json({
        ok: false,
        message: "OLAP API hata döndürdü",
        fromOlap: data
      });
    }

    return res.status(200).json(data);
  } catch (e) {
    return res.status(500).json({
      ok: false,
      error: e.message
    });
  }
});

module.exports = router;