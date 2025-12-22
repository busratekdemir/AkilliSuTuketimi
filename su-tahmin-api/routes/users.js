// routes/users.js

const express = require('express');
const router = express.Router();
const userController = require('../controllers/userController'); // Henüz yazmadık
const { protect } = require('../middleware/authMiddleware'); // Middleware'i içe aktar

// GET /api/users/profile
// Bu rotaya sadece geçerli bir JWT'ye sahip oturum açmış kullanıcılar erişebilir.
router.get('/profile', protect, userController.getUserProfile); 

// Şimdilik sadece root endpoint'i dışa aktar
router.get('/', (req, res) => {
    res.send('Users rotası çalışıyor');
});

// Router nesnesini dışa aktar
module.exports = router;