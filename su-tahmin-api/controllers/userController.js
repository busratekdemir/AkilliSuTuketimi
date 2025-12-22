// controllers/userController.js

// @desc    Oturum açmış kullanıcının profilini getirir
// @route   GET /api/users/profile
// @access  Private (Korunmuş rota, authMiddleware.js gerektirir)
const getUserProfile = async (req, res, next) => {
    // authMiddleware (protect) başarılı olduğu zaman, 
    // kullanıcı bilgileri req.user objesine eklenir.
    const user = req.user;

    // req.user objesi zaten authMiddleware tarafından kontrol edildiği için,
    // burada sadece veriyi döndürmemiz yeterlidir.
    if (user) {
        res.status(200).json({
            id: user.id,
            username: user.username,
            email: user.email,
            message: "Kullanıcı profili başarıyla getirildi."
        });
    } else {
        // Bu durum teorik olarak oluşmamalıdır çünkü middleware zaten çalışmıştır.
        res.status(404);
        next(new Error('Kullanıcı bulunamadı.'));
    }
};

// Bu controller'da sadece bir metot olduğu için onu dışa aktarıyoruz.
module.exports = {
    getUserProfile,
};