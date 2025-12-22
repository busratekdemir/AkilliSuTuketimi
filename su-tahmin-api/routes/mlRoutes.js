const express = require('express');
const router = express.Router();
const mlService = require('../services/mlService');
const { protect } = require('../middleware/authMiddleware'); // Güvenlik kontrolü

/**
 * @route   POST /api/ml/predict
 * @desc    Su tüketim tahmini yapar
 * @access  Private (Bearer Token gerekir)
 */
router.post('/predict', protect, async (req, res, next) => {
    try {
        const features = req.body; // Postman'den gelen veriler (sıcaklık, nem vb.)
        
        // ML servisine SOA üzerinden istek atılır
        const prediction = await mlService.getPredictionFromML(features);
        
        res.status(200).json({
            success: true,
            prediction: prediction,
            unit: "m3"
        });
    } catch (error) {
        next(error); // Hataları merkezi hata yönetimine iletir
    }
});

module.exports = router;