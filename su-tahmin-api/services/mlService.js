const axios = require('axios');

// ML_API_URL: .env dosyasında http://localhost:8000/predict gibi tanımlı olmalı
const ML_API_URL = process.env.ML_API_URL;

/**
 * Flask ML API'sine veri gönderir ve tahmin sonucunu alır.
 * SOA mimarisi gereği dış servislerle iletişimi yönetir.
 */
const getPredictionFromML = async (features) => {
    // 1. Çevresel Değişken Kontrolü
    if (!ML_API_URL) {
        console.error("HATA: ML_API_URL tanımlı değil!");
        throw new Error('ML servis adresi bulunamadı. Lütfen .env dosyasını kontrol edin.');
    }
    
    try {
        // 2. Flask API'sine İstek Gönderimi (REST/HTTP)
        // Not: Eğer gRPC kullanılacaksa buraya gRPC client kodu eklenmelidir.
        const response = await axios.post(ML_API_URL, features, {
            timeout: 10000, // 10 saniye zaman aşımı
            headers: { 'Content-Type': 'application/json' }
        });

        // 3. Yanıt Yapısı Kontrolü
        // Flask genellikle { prediction: ... } döndürür
        const predictionResult = response.data.prediction;

        if (predictionResult === undefined || predictionResult === null) {
            throw new Error('ML servisi boş bir tahmin sonucu döndürdü.');
        }

        return predictionResult;

    } catch (error) {
        // 4. Detaylı Hata Yönetimi (Error Middleware ile uyumlu)
        console.error('ML Servis Hatası Detayı:', error.message);
        
        let message = 'ML servisine şu an erişilemiyor.';
        let statusCode = 503; // Service Unavailable

        if (error.response) {
            // Sunucu yanıt verdi ama hata kodu döndürdü (400, 500 vb.)
            message = `ML Sunucu Hatası: ${error.response.status}`;
            statusCode = error.response.status;
        } else if (error.code === 'ECONNREFUSED') {
            message = 'Flask sunucusu kapalı veya adresi yanlış.';
        }

        const customError = new Error(message);
        customError.status = statusCode;
        throw customError;
    }
};

module.exports = {
    getPredictionFromML,
};