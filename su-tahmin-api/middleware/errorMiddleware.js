// middleware/errorMiddleware.js

// Tanımlanmamış rotalar için 404 hatası middleware'i
const notFound = (req, res, next) => {
    const error = new Error(`Bu rota bulunamadı - ${req.originalUrl}`);
    res.status(404);
    next(error); // Express'in genel hata işleyicisine gönder
};

// Genel hata işleyici middleware'i
const errorHandler = (err, req, res, next) => {
    // Eğer durum kodu 200 ise (başarılı varsayılır), onu 500 (sunucu hatası) yap
    const statusCode = res.statusCode === 200 ? 500 : res.statusCode;
    res.status(statusCode);

    res.json({
        message: err.message,
        // Geliştirme aşamasında Stack Trace'i göster, üretimde gösterme
        stack: process.env.NODE_ENV === 'production' ? null : err.stack, 
    });
};

module.exports = {
    notFound,
    errorHandler,
};