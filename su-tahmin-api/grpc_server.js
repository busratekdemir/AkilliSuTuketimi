// grpc_server.js

const path = require('path');
const grpc = require('@grpc/grpc-js');
const protoLoader = require('@grpc/proto-loader');
const predictionService = require('./services/predictionService');

// .proto dosyasının konumu
const PROTO_PATH = path.join(__dirname, 'tahmin.proto');
const GRPC_PORT = process.env.GRPC_PORT || '50051';

// proto dosyasını yükleme seçenekleri
const packageDefinition = protoLoader.loadSync(PROTO_PATH, {
    keepCase: true,
    longs: String,
    enums: String,
    defaults: true,
    oneofs: true
});

// Yüklenen paketi al
const tahmin_proto = grpc.loadPackageDefinition(packageDefinition).tahmin_servisi;

// ========================================================
// 1. SERVİS METOTLARINI (HANDLER) TANIMLAMA
// ========================================================

/**
 * Yeni bir tahmin başlatma gRPC metodu.
 * @param {object} call - Gelen isteği ve metadata'yı içerir.
 * @param {function} callback - Yanıtı göndermek için kullanılır.
 */
const getPrediction = async (call, callback) => {
    try {
        // NOT: gRPC'de Kimlik Doğrulama
        // REST API'deki gibi token'ı burada doğrulamamız gerekir.
        // Basitlik için, şimdilik kullanıcı ID'sini metadata'dan aldığımızı varsayalım.
        // Gerçek uygulamada, bu metadata'da bir JWT olur ve doğrulanır.

        // call.metadata'dan (varsayımsal) kullanıcı ID'sini al
        // const userId = call.metadata.get('user_id')[0]; 
        
        // Şimdilik sabit bir kullanıcı ID'si veya varsayımsal bir ID kullanalım.
        // Normalde bu ID, doğrulanan JWT'den gelir.
        const userId = 1; 

        // İş mantığını REST API'den paylaştığımız Service katmanına devret
        const result = await predictionService.getNewPrediction(userId);

        // gRPC yanıtını oluştur ve geri gönder
        callback(null, {
            prediction_result: result.prediction,
            message: 'gRPC üzerinden su tüketim tahmini başarıyla yapıldı.',
            prediction_id: result.id
        });
    } catch (error) {
        console.error("gRPC GetPrediction hatası:", error.message);
        
        // Hata durumunda gRPC Status kodları ile hata mesajı gönder
        callback({
            code: grpc.status.INTERNAL,
            details: error.message || 'Tahmin servisinde iç hata oluştu.'
        });
    }
};

/**
 * Geçmiş tahminleri getirme gRPC metodu.
 */
const getPredictionHistory = async (call, callback) => {
    try {
        // Varsayımsal kullanıcı ID'si
        const userId = 1; 

        // İş mantığını Service katmanına devret
        const history = await predictionService.getHistory(userId);

        // gRPC yanıtı için HistoricalPrediction listesi oluştur
        const predictions = history.map(item => ({
            id: item.id,
            prediction_date: item.prediction_date.toISOString(), // Tarihi string'e çevir
            features_json: JSON.stringify(item.features),      // JSON objesini string'e çevir
            prediction_result: item.prediction
        }));

        // Yanıtı geri gönder
        callback(null, {
            predictions: predictions
        });
    } catch (error) {
        console.error("gRPC GetPredictionHistory hatası:", error.message);
        
        callback({
            code: grpc.status.INTERNAL,
            details: error.message || 'Geçmiş tahminler getirilirken iç hata oluştu.'
        });
    }
};

// ========================================================
// 2. GRPC SUNUCUSUNU OLUŞTURMA VE BAŞLATMA
// ========================================================

/**
 * gRPC sunucusunu oluşturur ve başlatır.
 */
function startGrpcServer() {
    // Yeni bir gRPC sunucusu oluştur
    const server = new grpc.Server();

    // Servis tanımını ve uygulayacağımız metotları sunucuya ekle
    server.addService(tahmin_proto.PredictionService.service, {
        GetPrediction: getPrediction,
        GetPredictionHistory: getPredictionHistory
    });

    // Belirtilen adreste ve portta sunucuyu bağla
    server.bindAsync(
        `0.0.0.0:${GRPC_PORT}`, 
        grpc.ServerCredentials.createInsecure(), // Güvenli olmayan (insecure) kimlik bilgisi kullan
        (error, port) => {
            if (error) {
                console.error("gRPC sunucu hatası:", error);
                return;
            }
            console.log(`gRPC sunucusu ${port} portunda çalışıyor.`);
            server.start();
        }
    );
}

// Ana Express sunucusunda kullanmak için dışa aktar
module.exports = { startGrpcServer };