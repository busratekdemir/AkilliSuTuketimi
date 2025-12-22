// controllers/authController.js

const authService = require('../services/authService');

// @desc    Yeni kullanıcı kaydı
// @route   POST /api/auth/register
// @access  Public
const registerUser = async (req, res, next) => {
    try {
        const { username, email, password } = req.body;
        
        // 1. İş mantığını Service katmanına devret
        const userData = await authService.register(username, email, password);

        // 2. Başarılı yanıt döndür
        res.status(201).json({
            message: 'Kullanıcı başarıyla kaydedildi.',
            user: {
                id: userData.id,
                username: userData.username,
                email: userData.email,
                token: userData.token // JWT token'ı
            }
        });
    } catch (error) {
        // Hata durumunda Express hata işleyicisine (errorHandler) gönder
        next(error); 
    }
};


// @desc    Kullanıcı girişi
// @route   POST /api/auth/login
// @access  Public
const loginUser = async (req, res, next) => {
    try {
        const { email, password } = req.body;
        
        // 1. İş mantığını Service katmanına devret
        const userData = await authService.login(email, password);

        // 2. Başarılı yanıt döndür
        res.status(200).json({
            message: 'Giriş başarılı.',
            user: {
                id: userData.id,
                username: userData.username,
                email: userData.email,
                token: userData.token // JWT token'ı
            }
        });
    } catch (error) {
        // Hata durumunda Express hata işleyicisine gönder
        next(error);
    }
};

module.exports = {
    registerUser,
    loginUser,
};