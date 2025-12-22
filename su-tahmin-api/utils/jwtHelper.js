// utils/jwtHelper.js

const jwt = require('jsonwebtoken');

const getJwtSecret = () => {
    if (!process.env.JWT_SECRET) {
        throw new Error('JWT_SECRET tanımlı değil. .env dosyasını kontrol et.');
    }
    return process.env.JWT_SECRET;
};

// Token oluşturma
const generateToken = (id) => {
    return jwt.sign(
        { id },
        getJwtSecret(),
        { expiresIn: '7d' }
    );
};

// Token doğrulama
const verifyToken = (token) => {
    try {
        return jwt.verify(token, getJwtSecret());
    } catch (error) {
        throw new Error('Geçersiz veya süresi dolmuş token.');
    }
};

module.exports = {
    generateToken,
    verifyToken,
};
