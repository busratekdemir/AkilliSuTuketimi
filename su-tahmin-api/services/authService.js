// services/authService.js

// Repository'leri ve Yardımcı Fonksiyonları içe aktar
const userRepository = require('../repositories/userRepository');
const { generateToken } = require('../utils/jwtHelper'); 
const bcrypt = require('bcryptjs'); // Şifre hash'leme için

// Kayıt Olma İşlemi
const register = async (username, email, password) => {
    // 1. Kullanıcının zaten var olup olmadığını kontrol et
    const existingUser = await userRepository.findByEmail(email);
    if (existingUser) {
        // HTTP durum kodu 400 ile özel hata fırlat
        const error = new Error('Bu e-posta adresi zaten kullanımda.');
        error.status = 400; 
        throw error;
    }

    // 2. Şifreyi hash'le
    const salt = await bcrypt.genSalt(10);
    const hashedPassword = await bcrypt.hash(password, salt);

    // 3. Kullanıcıyı veritabanına kaydet
    const userId = await userRepository.createUser(username, email, hashedPassword);
    
    // 4. Token oluştur ve kullanıcı verilerini döndür
    const token = generateToken(userId);

    return { id: userId, username, email, token };
};

// Giriş İşlemi
const login = async (email, password) => {
    // 1. Kullanıcıyı e-posta ile bul
    const user = await userRepository.findByEmail(email);

    if (user && (await bcrypt.compare(password, user.password))) {
        // 2. Şifre doğruysa Token oluştur
        const token = generateToken(user.id);
        
        return { id: user.id, username: user.username, email: user.email, token };
    } else {
        // Kullanıcı bulunamazsa veya şifre yanlışsa hata fırlat
        const error = new Error('Geçersiz e-posta veya şifre.');
        error.status = 401; // Yetkilendirme hatası
        throw error;
    }
};

module.exports = {
    register,
    login,
};