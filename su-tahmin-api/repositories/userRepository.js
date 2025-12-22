// repositories/userRepository.js

const { pool } = require('../db');   // pool = Promise<ConnectionPool>
const mssql = require('mssql');

/**
 * Kullanıcıyı e-posta adresine göre bulur
 */
const findByEmail = async (email) => {
    try {
        const connection = await pool;

        const result = await connection
            .request()
            .input('email', mssql.NVarChar, email)
            .query(`
                SELECT
                    Id,
                    FirstName,
                    LastName,
                    Email,
                    Role,
                    PasswordHash
                FROM Users
                WHERE Email = @email
            `);

        if (result.recordset.length === 0) {
            return null;
        }

        const user = result.recordset[0];

        return {
            id: user.Id,
            username: user.FirstName,
            email: user.Email,
            password: user.PasswordHash
        };
    } catch (error) {
        console.error('findByEmail DB hatası:', error);
        throw error;
    }
};

/**
 * Yeni kullanıcı oluşturur
 */
const createUser = async (username, email, hashedPassword) => {
    try {
        const connection = await pool;

        const result = await connection
            .request()
            .input('firstName', mssql.NVarChar, username)
            .input('lastName', mssql.NVarChar, 'User')
            .input('email', mssql.NVarChar, email)
            .input('role', mssql.NVarChar, 'Client')
            .input('passwordHash', mssql.NVarChar, hashedPassword)
            .query(`
                INSERT INTO dbo.Users
                (
                    FirstName,
                    LastName,
                    Email,
                    Role,
                    PasswordHash,
                    CreatedAt
                )
                VALUES
                (
                    @firstName,
                    @lastName,
                    @email,
                    @role,
                    @passwordHash,
                    GETDATE()
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT) AS Id;
            `);

        return result.recordset[0].Id;
    } catch (error) {
        console.error('createUser DB hatası:', error);
        throw error;
    }
};

/**
 * Kullanıcıyı ID ile bulur
 */
const findById = async (id) => {
    try {
        const connection = await pool;

        const result = await connection
            .request()
            .input('id', mssql.Int, id)
            .query(`
                SELECT
                    Id,
                    FirstName,
                    LastName,
                    Email,
                    Role
                FROM Users
                WHERE Id = @id
            `);

        if (result.recordset.length === 0) {
            return null;
        }

        return result.recordset[0];
    } catch (error) {
        console.error('findById DB hatası:', error);
        throw error;
    }
};

module.exports = {
    findByEmail,
    createUser,
    findById
};
