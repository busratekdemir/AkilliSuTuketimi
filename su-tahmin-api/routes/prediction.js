// routes/prediction.js

const express = require('express');
const router = express.Router();
const predictionController = require('../controllers/predictionController');
const { protect } = require('../middleware/authMiddleware');

// POST /api/prediction
// Yeni tahmin başlatma rotası (Korunmuş)
// Kullanıcı ID'si token'dan alınır, POST gövdesine gerek yoktur (özellikler sistemde oluşturulur)
router.post('/', protect, predictionController.startNewPrediction);

// GET /api/prediction/history
// Kullanıcının geçmiş tahminlerini getirme rotası (Korunmuş)
router.get('/history', protect, predictionController.getPredictionHistory);
// Şimdilik sadece root endpoint'i dışa aktar
router.get('/', (req, res) => {
    res.send('Prediction rotası çalışıyor');
});

// Router nesnesini dışa aktar
module.exports = router;