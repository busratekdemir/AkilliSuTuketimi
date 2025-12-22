// server.js (REST API Giriş Noktası)

// 1. Ortam değişkenlerini yükle (.env)
require("dotenv").config();

const express = require("express");
const cors = require("cors");

// ✅ OLAP Routes
const olapRoutes = require("./routes/olapRoutes");

// 2. Servisleri ve Route'ları İçe Aktar
const { startGrpcServer } = require("./grpc_server");
const { integrateSoapServer } = require("./soap_server");
const { notFound, errorHandler } = require("./middleware/errorMiddleware");

const authRoutes = require("./routes/auth");
const userRoutes = require("./routes/users");
const predictionRoutes = require("./routes/prediction");
const meterRoutes = require("./routes/meters");

// 3. Express uygulamasını başlatma
const app = express();

// 4. Global Middleware'ler
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: false }));

// ✅ OLAP route bağlama (middleware'lerden sonra, hatalardan önce)
app.use("/api/olap", olapRoutes);

// 5. SOAP Sunucusunu Express'e Entegre Et
integrateSoapServer(app);

// 6. Ana Endpoint
app.get("/", (req, res) => {
  res.send("Su Tahmin API Servisi çalışıyor... REST, gRPC, SOAP ve OLAP için hazır!");
});

// 7. Route (Yönlendirme) Tanımlamaları
app.use("/api/auth", authRoutes);
app.use("/api/users", userRoutes);
app.use("/api/prediction", predictionRoutes);
app.use("/api/meters", meterRoutes);

// 8. Hata Yönetimi Middleware'leri (Rotalardan sonra gelmeli)
app.use(notFound);
app.use(errorHandler);

// 9. Sunucuları Başlat
startGrpcServer();

const PORT = process.env.PORT || 5000;

app.listen(PORT, () => {
  console.log(`🚀 REST Sunucusu http://localhost:${PORT} adresinde çalışıyor...`);
});