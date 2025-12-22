const express = require('express');
const router = express.Router();

// GET /api/meters - ML Tahminlerini Dönen Uç
router.get('/', (req, res) => {
    try {
        // Buradaki veriler normalde veritabanından veya ML modelinden akar
        const mlData = [
            { 
                Id: 1, 
                MeterCode: "ML-TAHMIN-001", 
                SerialNumber: "SN-101", 
                Location: "Merkez", 
                Status: "Aktif", 
                PredictedUsage: 450.75, // ML'den gelen tahmin
                ConfidenceScore: 0.98 
            }
        ];
        res.status(200).json(mlData);
    } catch (error) {
        res.status(500).json({ message: "ML Bağlantı Hatası" });
    }
});
module.exports = router;