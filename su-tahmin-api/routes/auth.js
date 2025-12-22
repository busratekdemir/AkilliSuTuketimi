// routes/auth.js

const express = require('express');
const router = express.Router();
// Controller'ı içe aktar (Henüz fonksiyonları yazmadık, ama tanımlıyoruz)
const authController = require('../controllers/authController');

// POST /api/auth/register
// Yeni kullanıcı kaydı
router.post('/register', authController.registerUser);

// POST /api/auth/login
// Kullanıcı girişi (JWT token döndürür)
router.post('/login', authController.loginUser);

// Not: İleride, body validasyonları (validators.js) bu rotalara middleware olarak eklenecektir.

module.exports = router;