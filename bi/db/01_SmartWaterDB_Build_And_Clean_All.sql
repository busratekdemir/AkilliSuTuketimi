/* ============================================================
   01_SmartWaterDB_All_In_One.sql
   Amaç:
   - SmartWaterDB oluþtur
   - Hedef tablolarý oluþtur (Stg/Raw/Clean)
   - imp_* import tablolarýndan hedef tablolara aktar
   - Clean_Consumption üret
   Not:
   - Bu scripti çalýþtýrmadan önce SSMS Import Wizard ile þu geçici tablolarý oluþtur:
       dbo.imp_consumption
       dbo.imp_damlevels
       dbo.imp_waterlosses
       dbo.imp_monthlyproduction
   ============================================================ */

IF DB_ID('SmartWaterDB') IS NULL
BEGIN
    CREATE DATABASE SmartWaterDB;
END;
GO

USE SmartWaterDB;
GO

/* ============================================================
   1) Hedef tablolarý oluþtur 
   ============================================================ */

IF OBJECT_ID('dbo.Stg_Consumption','U') IS NULL
BEGIN
    CREATE TABLE dbo.Stg_Consumption (
        YIL INT NULL,
        AY INT NULL,
        ILCE NVARCHAR(100) NULL,
        MAHALLE NVARCHAR(150) NULL,
        ABONELIK_GRUBU NVARCHAR(100) NULL,
        ABONELIK_TURU NVARCHAR(50) NULL,
        ABONE_TIPI NVARCHAR(150) NULL,
        ABONE_ADEDI INT NULL,
        ORTALAMA_TUKETIM FLOAT NULL
    );
END;
GO

IF OBJECT_ID('dbo.Raw_DamLevels','U') IS NULL
BEGIN
    CREATE TABLE dbo.Raw_DamLevels (
        BARAJ_KUYU_ID NVARCHAR(50) NULL,
        BARAJ_KUYU_ADI NVARCHAR(150) NULL,
        DURUM_TARIHI NVARCHAR(50) NULL,
        SU_DURUMU NVARCHAR(50) NULL,
        DOLULUK_ORANI NVARCHAR(50) NULL,
        SU_YUKSEKLIGI NVARCHAR(50) NULL,
        TUKETILEBILIR_SU_KAPASITESI NVARCHAR(50) NULL,
        MAKSIMUM_SU_KAPASITESI NVARCHAR(50) NULL,
        MAKSIMUM_SU_YUKSEKLIGI NVARCHAR(50) NULL,
        MINIMUM_SU_KAPASITESI NVARCHAR(50) NULL,
        MINIMUM_SU_YUKSEKLIGI NVARCHAR(50) NULL,
        BARAJ_SU_DURUMU_GOSTERIMI NVARCHAR(150) NULL,
        SIRA NVARCHAR(50) NULL,
        ENLEM NVARCHAR(50) NULL,
        BOYLAM NVARCHAR(50) NULL,
        KULLANILABILIR_GOL_SU_HACMI NVARCHAR(50) NULL
    );
END;
GO

IF OBJECT_ID('dbo.Raw_WaterLosses','U') IS NULL
BEGIN
    CREATE TABLE dbo.Raw_WaterLosses (
        YIL INT NULL,
        ILCE NVARCHAR(100) NULL,
        TEMIN_SERVIS_BAGLANTISI_KAYIP DECIMAL(18,2) NULL,
        FIZIKI_KAYIPLAR DECIMAL(18,2) NULL,
        IZINLI_TUKETIM DECIMAL(18,2) NULL,
        FATURALANMAYAN_IZINLI_TUKETIM DECIMAL(18,2) NULL,
        GELIR_GETIRMEYEN_SU_MIKTARI DECIMAL(18,2) NULL,
        FATURALANAN_OLCULMEYEN_KULLANIM DECIMAL(18,2) NULL,
        FATURALANMAYAN_OLCULMEYEN_KULLANIM DECIMAL(18,2) NULL,
        SAYACLARDAKI_OLCUM_HATASI DECIMAL(18,2) NULL,
        SISTEME_GIREN_SU_MIKTARI DECIMAL(18,2) NULL,
        FATURALANAN_OLCULEN_KULLANIM DECIMAL(18,2) NULL,
        IDARI_KAYIPLAR DECIMAL(18,2) NULL,
        IZINSIZ_TUKETIM DECIMAL(18,2) NULL,
        FATURALANAN_IZINLI_SU_TUKETIMI DECIMAL(18,2) NULL,
        FATURALANMAYAN_OLCULEN_KULLANIM DECIMAL(18,2) NULL,
        GELIR_GETIREN_SU_MIKTARI DECIMAL(18,2) NULL,
        DEPOLARDA_MEYDANA_GELEN_KACAK DECIMAL(18,2) NULL,
        SU_KAYIPLARI DECIMAL(18,2) NULL
    );
END;
GO

IF OBJECT_ID('dbo.Raw_MonthlyProductionBySource','U') IS NULL
BEGIN
    CREATE TABLE dbo.Raw_MonthlyProductionBySource (
        URETIM_KAYNAGI NVARCHAR(150) NULL,
        URETIM_MIKTARI DECIMAL(18,2) NULL,
        YIL INT NULL,
        AY INT NULL
    );
END;
GO

IF OBJECT_ID('dbo.Clean_Consumption','U') IS NULL
BEGIN
    CREATE TABLE dbo.Clean_Consumption (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        [Year] INT NULL,
        [Month] INT NULL,
        District NVARCHAR(100) NULL,
        Neighborhood NVARCHAR(150) NULL,
        SubscriptionGroup NVARCHAR(100) NULL,
        SubscriptionType NVARCHAR(50) NULL,
        CustomerType NVARCHAR(150) NULL,
        SubscriberCount INT NULL,
        AvgConsumption FLOAT NULL
    );
END;
GO

/* ============================================================
   2) Import tablolarýndan hedef tablolara aktar (imp_* -> Stg/Raw)
   ============================================================ */

TRUNCATE TABLE dbo.Stg_Consumption;

INSERT INTO dbo.Stg_Consumption (
    YIL, AY, ILCE, MAHALLE,
    ABONELIK_GRUBU, ABONELIK_TURU,
    ABONE_TIPI, ABONE_ADEDI, ORTALAMA_TUKETIM
)
SELECT
    YIL, AY, ILCE, MAHALLE,
    ABONELIK_GRUBU, ABONELIK_TURU,
    ABONE_TIPI, ABONE_ADEDI, ORTALAMA_TUKETIM
FROM dbo.imp_consumption;
GO

TRUNCATE TABLE dbo.Raw_DamLevels;

INSERT INTO dbo.Raw_DamLevels
SELECT * FROM dbo.imp_damlevels;
GO

TRUNCATE TABLE dbo.Raw_WaterLosses;

INSERT INTO dbo.Raw_WaterLosses
SELECT * FROM dbo.imp_waterlosses;
GO

TRUNCATE TABLE dbo.Raw_MonthlyProductionBySource;

INSERT INTO dbo.Raw_MonthlyProductionBySource
SELECT * FROM dbo.imp_monthlyproduction;
GO

/* ============================================================
   3) Clean_Consumption üret (Stg -> Clean)
   ============================================================ */

TRUNCATE TABLE dbo.Clean_Consumption;
GO

UPDATE dbo.Stg_Consumption
SET 
    ILCE = LTRIM(RTRIM(ILCE)),
    MAHALLE = LTRIM(RTRIM(MAHALLE)),
    ABONELIK_GRUBU = LTRIM(RTRIM(ABONELIK_GRUBU)),
    ABONELIK_TURU = LTRIM(RTRIM(ABONELIK_TURU)),
    ABONE_TIPI = LTRIM(RTRIM(ABONE_TIPI));
GO

DELETE FROM dbo.Stg_Consumption
WHERE ILCE IS NULL OR MAHALLE IS NULL;
GO

DELETE FROM dbo.Stg_Consumption
WHERE ORTALAMA_TUKETIM < 0;
GO

INSERT INTO dbo.Clean_Consumption (
    [Year], [Month], District, Neighborhood,
    SubscriptionGroup, SubscriptionType, CustomerType,
    SubscriberCount, AvgConsumption
)
SELECT
    YIL, AY, ILCE, MAHALLE,
    ABONELIK_GRUBU, ABONELIK_TURU, ABONE_TIPI,
    ABONE_ADEDI, ORTALAMA_TUKETIM
FROM dbo.Stg_Consumption;
GO

/* ============================================================
   4) Kontroller
   ============================================================ */
SELECT COUNT(*) AS StgCount   FROM dbo.Stg_Consumption;
SELECT COUNT(*) AS CleanCount FROM dbo.Clean_Consumption;
GO

/* ============================================================
   5) Geçici import tablolarýný sil-opsiyonel
   ============================================================ */
IF OBJECT_ID('dbo.imp_consumption','U') IS NOT NULL DROP TABLE dbo.imp_consumption;
IF OBJECT_ID('dbo.imp_damlevels','U') IS NOT NULL DROP TABLE dbo.imp_damlevels;
IF OBJECT_ID('dbo.imp_waterlosses','U') IS NOT NULL DROP TABLE dbo.imp_waterlosses;
IF OBJECT_ID('dbo.imp_monthlyproduction','U') IS NOT NULL DROP TABLE dbo.imp_monthlyproduction;
GO
