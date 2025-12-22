// services/predictionService.js

const predictionRepository = require('../repositories/predictionRepository');
const mlService = require('./mlService');

// NOT: Kullanıcının giriş yapmadığı özelliklerin (tarih, sıcaklık vb.) sistem tarafından oluşturulduğu varsayılır.

/**
 * Kullanıcı için yeni bir su tüketim tahmini yapar.
 * @param {number} userId - Tahmini isteyen kullanıcının ID'si.
 * @returns {object} Tahmin sonucu ve kullanılan özellikler.
 */
const getNewPrediction = async (userId) => {
    // 1. ML Modelinin Beklediği Giriş Özelliklerini Otomatik Oluşturma
    // Bu kısım, sizin uygulamanızın kalbinde yer alır. 
    // Örnek olarak, mevcut tarih/saat ve sabit bir sıcaklık değeri alalım.
    // Gerçek uygulamada, harici bir Hava Durumu API'si (örn: OpenWeatherMap) kullanılabilir.

    const now = new Date();
    
    // ML modelinin beklediği formatta (Bu, sadece bir örnektir, modelinize göre değişir)
    const featuresForML = {
        current_hour: now.getHours(),
        current_day_of_week: now.getDay(), // 0=Pazar, 1=Pazartesi...
        current_month: now.getMonth() + 1,
        // *** DİKKAT: Bu değerler harici servisten gelmelidir ***
        temperature_celsius: 22.5, // Örnek Sabit Değer
        humidity: 55,             // Örnek Sabit Değer
        is_weekend: now.getDay() === 0 || now.getDay() === 6,
    };
    
    let predictionResult;
    
    try {
        // 2. Özellikleri ML Servisine Gönder ve Tahmini Al
        // mlService.js, bu özellikleri alıp Flask API'ye gönderecek.
        predictionResult = await mlService.getPredictionFromML(featuresForML);
        
        // Tahmin sonucu bir sayı olmalı (örn: 125.75 metreküp)

    } catch (error) {
        console.error('Tahmin yapılırken hata oluştu:', error.message);
        // Hata durumunda bile kaydetmeye çalışmadan önce hatayı Controller'a fırlat
        throw error;
    }

    // 3. Tahmin Sonucunu ve Kullanılan Özellikleri Veritabanına Kaydetme
    try {
        const predictionId = await predictionRepository.createPrediction(
            userId, 
            featuresForML, 
            predictionResult
        );

        return {
            id: predictionId,
            prediction: predictionResult,
            featuresUsed: featuresForML,
            timestamp: now.toISOString()
        };
    } catch (dbError) {
        console.error('Tahmin kaydı veritabanına kaydedilirken hata oluştu:', dbError.message);
        // Kayıt hatası yaşanırsa, yine de tahmini döndürebiliriz veya hata fırlatabiliriz.
        // Güvenlik için, bir hata fırlatmak daha iyidir.
        throw new Error('Tahmin yapıldı, ancak veritabanına kaydedilemedi.');
    }
};

/**
 * Bir kullanıcının tüm geçmiş tahminlerini getirir.
 * @param {number} userId - Kullanıcının ID'si.
 * @returns {Array} Geçmiş tahminler listesi.
 */
const getHistory = async (userId) => {
    return predictionRepository.getPredictionsByUserId(userId);
};


module.exports = {
    getNewPrediction,
    getHistory,
};