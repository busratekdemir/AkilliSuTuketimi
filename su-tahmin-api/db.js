require('dotenv').config();
const sql = require('mssql');

const config = {
    user: process.env.DB_USER,
    password: process.env.DB_PASSWORD,
    server: process.env.DB_HOST,
    database: process.env.DB_NAME,
    port: parseInt(process.env.DB_PORT),
    pool: {
        max: 10,
        min: 0,
        idleTimeoutMillis: 30000
    },
    options: {
        encrypt: false,
        enableArithAbort: true
    }
};

// 🔑 Promise export ediyoruz
const poolPromise = sql.connect(config)
    .then(pool => {
        console.log('✅ MSSQL veritabanı bağlantısı başarılı.');
        return pool;
    })
    .catch(err => {
        console.error('❌ MSSQL veritabanı bağlantı hatası:', err.message);
        process.exit(1);
    });

module.exports = {
    pool: poolPromise,
    sql
};
