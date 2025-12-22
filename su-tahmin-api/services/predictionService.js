const axios = require('axios');

/**
 * SOAP veya REST üzerinden gelen veriyi ML servisine iletir.
 * @param {number} userId - İşlemi yapan kullanıcı ID'si.
 * @param {any} inputFeatures - Gelen ham veri (String veya Array).
 */
const getNewPrediction = async (userId, inputFeatures) => {
    // 1. Veri Ön İşleme: Gelen veri "25,30,10" şeklindeyse diziye çevir, değilse olduğu gibi kullan
    const featuresArray = typeof inputFeatures === 'string' 
        ? inputFeatures.split(',').map(num => Number(num.trim())) 
        : inputFeatures;

    // Log: ML'e ne gittiğini görmek için (Hata ayıklama kolaylaşır)
    console.log(`ML Servisine Gönderilen Veri (User: ${userId}):`, featuresArray);

    try {
        // 2. ML Servisine (Flask - Port 5001) İstek At
        const response = await axios.post('http://localhost:5001/predict', {
            features: featuresArray // Temizlenmiş veriyi gönderiyoruz
        }, {
            timeout: 5000 // ML servisi yanıt vermezse 5 saniye sonra zaman aşımına uğrar
        });

        // ML servisinden dönen sonucu al
        const predictionValue = response.data.prediction;

        // 3. Veritabanı Kaydı (İleride burayı aktif edebilirsiniz)
        // const predictionId = await predictionRepository.savePrediction(userId, predictionValue, JSON.stringify(featuresArray));

        return {
            id: Date.now(), 
            prediction: predictionValue,
            status: "Success"
        };

    } catch (error) {
        // Hata yönetimi
        if (error.response) {
            // Flask servisi hata kodu döndürdü (örn: 400, 500)
            console.error("ML Servis Hatası (HTTP):", error.response.data);
            throw new Error(`ML Modeli Hatası: ${error.response.data.error || "Geçersiz giriş"}`);
        } else if (error.request) {
            // Flask servisine hiç ulaşılamadı (Port 5001 kapalı olabilir)
            console.error("ML Servisine Ulaşılamıyor (Port 5001 kapalı mı?)");
            throw new Error("Tahmin servisi şu an çevrimdışı.");
        } else {
            console.error("Tahmin İşlemi sırasında genel hata:", error.message);
            throw new Error("Tahmin yapılırken beklenmedik bir sorun oluştu.");
        }
    }
};

module.exports = { getNewPrediction };