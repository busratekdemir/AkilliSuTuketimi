// middleware/authMiddleware.js

const { verifyToken } = require('../utils/jwtHelper');
const userRepository = require('../repositories/userRepository');

// Korumalı rotalardan önce çalışacak asenkron middleware fonksiyonu
const protect = async (req, res, next) => {
    let token;

    // 1. Authorization başlığını kontrol et (Bearer <TOKEN> formatında mı?)
    if (req.headers.authorization && req.headers.authorization.startsWith('Bearer')) {
        try {
            // Token'ı başlık değerinden al (Bearer ve boşluktan sonraki kısım)
            token = req.headers.authorization.split(' ')[1];

            // 2. Token'ı doğrula
            const decoded = verifyToken(token);
            
            // 3. Token'daki ID ile kullanıcıyı veritabanında bul
            // NOT: Şifre hariç diğer kullanıcı bilgilerini req.user'a ekliyoruz.
            // Bu, diğer controller'larda kullanıcının ID'sine erişimi sağlar.
            const user = await userRepository.findById(decoded.id);

            if (user) {
                // Şifre alanını güvenlik için çıkartıp req objesine kullanıcıyı ekle
                req.user = { 
                    id: user.id, 
                    username: user.username, 
                    email: user.email 
                };
                next(); // Bir sonraki middleware veya controller'a geç
            } else {
                res.status(401); // 401: Unauthorized (Yetkisiz)
                throw new Error('Geçersiz token. Kullanıcı bulunamadı.');
            }

        } catch (error) {
            // Token doğrulama (süresi dolmuş, imzası hatalı vb.) başarısız olduysa
            console.error('Yetkilendirme Hatası:', error.message);
            res.status(401);
            next(new Error('Yetkisiz erişim. Token geçerli değil veya süresi dolmuş.'));
        }
    }

    if (!token) {
        // Authorization başlığı yoksa veya format yanlışsa
        res.status(401);
        next(new Error('Yetkisiz erişim. Token bulunamadı.'));
    }
};

module.exports = { protect };