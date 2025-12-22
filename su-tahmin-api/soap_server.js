// soap_server.js
const bodyParser = require('body-parser');
const fs = require('fs');
const path = require('path');
const soap = require('soap');
const predictionService = require('./services/predictionService');

// WSDL dosyasının içeriğini oku
const xml = fs.readFileSync(path.join(__dirname, 'tahmin_servisi.wsdl'), 'utf8');
const SOAP_SERVICE_PATH = '/soap/prediction';

// SOAP metotlarının implementasyonu
const serviceImplementation = {
    PredictionService: {
        PredictionServicePort: {
            // GetPrediction Metodunun Implementasyonu
            GetPrediction: async (args, callback, headers, req) => {
                try {
                    // NOT: SOAP'ta Kimlik Doğrulama
                    // REST/gRPC'deki gibi HTTP başlıklarından veya SOAP Header'ından
                    // JWT/API Key alınır ve burada doğrulanır.
                    
                    // Şimdilik varsayımsal kullanıcı ID'si kullanıyoruz.
                    const userId = 1; 
                    const inputData = args.InputData || args.features;

        if (!inputData) {
            throw new Error("Girdi verisi (InputData) bulunamadı.");
        }
                    
                    // İş mantığını Service katmanına devret
                    const result = await predictionService.getNewPrediction(userIdşinputData);

                    // SOAP yanıtını XML yapısına uygun olarak geri döndür
                    return {
                        PredictionResult: result.prediction,
                        Message: 'SOAP üzerinden su tüketim tahmini başarıyla yapıldı.',
                        PredictionID: result.id
                    };
                } catch (error) {
                    console.error("SOAP GetPrediction hatası:", error.message);
                    // Hata durumunda (SOAP için özelleştirilmiş bir hata yapısı gereklidir)
                    // Basitlik için sadece hata mesajını döndürelim
                    return {
                        Message: `Hata: ${error.message}`,
                        PredictionResult: -1
                    };
                }
            },
            
            // GetPredictionHistory Metodunun Implementasyonu
            GetPredictionHistory: async (args, callback, headers, req) => {
                try {
                    // Varsayımsal kullanıcı ID'si
                    const userId = 1; 

                    // İş mantığını Service katmanına devret
                    const history = await predictionService.getHistory(userId);
                    
                    // SOAP yanıtı için HistoricalPrediction listesi oluştur
                    const predictions = history.map(item => ({
                        ID: item.id,
                        PredictionDate: item.prediction_date.toISOString(),
                        FeaturesJSON: JSON.stringify(item.features),
                        PredictionResult: item.prediction
                    }));

                    // Yanıtı geri döndür
                    return {
                        Predictions: predictions, // WSDL'deki repeated element
                        Message: 'SOAP üzerinden geçmiş tahminler başarıyla getirildi.'
                    };
                } catch (error) {
                    console.error("SOAP GetPredictionHistory hatası:", error.message);
                    return {
                        Message: `Hata: ${error.message}`,
                        Predictions: []
                    };
                }
            }
        }
    }
};

/**
 * SOAP sunucusunu Express uygulamasına bağlar.
 * @param {object} app - Express uygulaması örneği.
 */
function integrateSoapServer(app) {
    // Express'te body-parser'ı kullanma zorunluluğu
    app.use(bodyParser.raw({type: function(){return true;}, limit: '5mb'}));

    // SOAP servisini Express'e bağla
    soap.listen(app, SOAP_SERVICE_PATH, serviceImplementation, xml, (wsdl_path) => {
       // soap_server.js sonundaki log satırı
console.log(`📡 SOAP sunucusu çalışıyor. WSDL: http://localhost:${process.env.PORT || 5000}${SOAP_SERVICE_PATH}?wsdl`);    });
}

module.exports = { integrateSoapServer };