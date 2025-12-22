// controllers/predictionController.js (Düzeltilmiş ve Sadeleştirilmiş)

const predictionService = require('../services/predictionService');

// @desc    Yeni bir su tüketim tahmini başlatır
// @route   POST /api/prediction
// @access  Private (Oturum açmış kullanıcı gerektirir)
const startNewPrediction = async (req, res, next) => {
    // KULLANICI GİRDİSİNE İHTİYAÇ YOK.
    // Tüm tahmini tetikleme, sadece oturum açan kullanıcıya (req.user.id) bağlıdır.
    const userId = req.user.id; 

    try {
        // İŞ AKIŞI: Service'e sadece kimlik doğrulanan kullanıcı ID'si gönderilir.
        // Service, bu ID'yi kullanarak sistem özelliklerini (tarih, saat, hava durumu varsayımı vb.)
        // otomatik olarak oluşturur ve ML'e gönderir.
        const result = await predictionService.getNewPrediction(userId);

        // Başarılı yanıt döndür
        res.status(200).json({
            message: 'Sistem, sizin adınıza otomatik özelliklerle su tüketim tahmini yaptı.',
            data: result
        });
    } catch (error) {
        next(error); 
    }
};

// @desc    Kullanıcının geçmiş tahminlerini getirir (Bu kısım aynı kalır)
// @route   GET /api/prediction/history
// @access  Private (Oturum açmış kullanıcı gerektirir)
const getPredictionHistory = async (req, res, next) => {
    const userId = req.user.id;

    try {
        const history = await predictionService.getHistory(userId);
        
        res.status(200).json({
            message: 'Geçmiş tahminler başarıyla getirildi.',
            count: history.length,
            data: history
        });
    } catch (error) {
        next(error);
    }
};

module.exports = {
    startNewPrediction,
    getPredictionHistory,
};