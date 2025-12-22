/* ============================================================
   02_SmartWaterDW_Build_And_Load_All.sql
   Amaç:
   - SmartWaterDW oluþtur
   - Boyut (Dim) ve Fakt (Fact) tablolarýný kur
   - SmartWaterDB'den ETL ile doldur
   ============================================================ */

IF DB_ID('SmartWaterDW') IS NULL
BEGIN
    CREATE DATABASE SmartWaterDW;
END;
GO

USE SmartWaterDW;
GO

/* ============================================================
   0) VARSA TABLOLARI FK SIRASIYLA SÝL (tekrar çalýþtýrýlabilir)
   ============================================================ */
IF OBJECT_ID('dbo.FactWaterProductionBySource','U') IS NOT NULL DROP TABLE dbo.FactWaterProductionBySource;
IF OBJECT_ID('dbo.FactDamLevel','U') IS NOT NULL DROP TABLE dbo.FactDamLevel;
IF OBJECT_ID('dbo.FactWaterLosses','U') IS NOT NULL DROP TABLE dbo.FactWaterLosses;
IF OBJECT_ID('dbo.FactConsumption','U') IS NOT NULL DROP TABLE dbo.FactConsumption;

IF OBJECT_ID('dbo.DimWaterSource','U') IS NOT NULL DROP TABLE dbo.DimWaterSource;
IF OBJECT_ID('dbo.DimDam','U') IS NOT NULL DROP TABLE dbo.DimDam;
IF OBJECT_ID('dbo.DimDistrict','U') IS NOT NULL DROP TABLE dbo.DimDistrict;
IF OBJECT_ID('dbo.DimSubscriptionType','U') IS NOT NULL DROP TABLE dbo.DimSubscriptionType;
IF OBJECT_ID('dbo.DimLocation','U') IS NOT NULL DROP TABLE dbo.DimLocation;
IF OBJECT_ID('dbo.DimDate','U') IS NOT NULL DROP TABLE dbo.DimDate;
GO

/* ============================================================
   1) DIM TABLOLARI
   ============================================================ */

CREATE TABLE dbo.DimDate (
    DateKey INT NOT NULL PRIMARY KEY,   -- YYYYMM
    Yil INT NOT NULL,
    Ay  INT NOT NULL
);
GO

CREATE TABLE dbo.DimLocation (
    LocationKey INT IDENTITY(1,1) PRIMARY KEY,
    Ilce NVARCHAR(100) NOT NULL,
    Mahalle NVARCHAR(150) NOT NULL
);
GO

CREATE TABLE dbo.DimSubscriptionType (
    SubscriptionTypeKey INT IDENTITY(1,1) PRIMARY KEY,
    AbonelikGrubu NVARCHAR(100) NOT NULL,
    AbonelikTuru  NVARCHAR(50)  NOT NULL,
    AboneTipi     NVARCHAR(150) NOT NULL
);
GO

CREATE TABLE dbo.DimDistrict (
    DistrictKey INT IDENTITY(1,1) PRIMARY KEY,
    DistrictName NVARCHAR(100) NOT NULL
);
GO

CREATE TABLE dbo.DimDam (
    DamKey INT IDENTITY(1,1) PRIMARY KEY,
    DamName NVARCHAR(150) NOT NULL
);
GO

CREATE TABLE dbo.DimWaterSource (
    WaterSourceKey INT IDENTITY(1,1) PRIMARY KEY,
    WaterSourceName NVARCHAR(150) NOT NULL
);
GO

/* ============================================================
   2) FACT TABLOLARI
   ============================================================ */

CREATE TABLE dbo.FactConsumption (
    FactConsumptionId INT IDENTITY(1,1) PRIMARY KEY,
    DateKey INT NOT NULL,
    LocationKey INT NOT NULL,
    SubscriptionTypeKey INT NOT NULL,
    AboneAdedi INT NULL,
    OrtalamaTuketim DECIMAL(18,2) NULL,
    ToplamTuketim  DECIMAL(18,2) NULL,
    CONSTRAINT FK_FC_Date FOREIGN KEY (DateKey) REFERENCES dbo.DimDate(DateKey),
    CONSTRAINT FK_FC_Loc  FOREIGN KEY (LocationKey) REFERENCES dbo.DimLocation(LocationKey),
    CONSTRAINT FK_FC_Sub  FOREIGN KEY (SubscriptionTypeKey) REFERENCES dbo.DimSubscriptionType(SubscriptionTypeKey)
);
GO

CREATE TABLE dbo.FactWaterLosses (
    FactWaterLossId INT IDENTITY(1,1) PRIMARY KEY,
    DateKey INT NOT NULL,
    DistrictKey INT NOT NULL,

    TeminServisBaglantisiKayip       DECIMAL(18,2) NULL,
    FizikiKayiplar                   DECIMAL(18,2) NULL,
    IzinliTuketim                    DECIMAL(18,2) NULL,
    FaturalanmayanIzinliTuketim      DECIMAL(18,2) NULL,
    GelirGetirmeyenSuMiktari         DECIMAL(18,2) NULL,
    FaturalananOlculmeyenKullanim    DECIMAL(18,2) NULL,
    FaturalanmayanOlculmeyenKullanim DECIMAL(18,2) NULL,
    SayaclardakiOlcumHatasi          DECIMAL(18,2) NULL,
    SuMiktari                        DECIMAL(18,2) NULL,
    FaturalananOlculenKullanim       DECIMAL(18,2) NULL,
    IdariKayiplar                    DECIMAL(18,2) NULL,
    IzinsizTuketim                   DECIMAL(18,2) NULL,
    FaturalananIzinliSuTuketimi      DECIMAL(18,2) NULL,
    FaturalanmayanOlculenKullanim    DECIMAL(18,2) NULL,
    GelirGetirenSuMiktari            DECIMAL(18,2) NULL,
    DepolardaKacak                   DECIMAL(18,2) NULL,
    SuKayiplari                      DECIMAL(18,2) NULL,

    CONSTRAINT FK_FWL_Date FOREIGN KEY (DateKey) REFERENCES dbo.DimDate(DateKey),
    CONSTRAINT FK_FWL_Dist FOREIGN KEY (DistrictKey) REFERENCES dbo.DimDistrict(DistrictKey)
);
GO

CREATE TABLE dbo.FactDamLevel (
    FactDamLevelId INT IDENTITY(1,1) PRIMARY KEY,
    DateKey INT NOT NULL,
    DamKey  INT NOT NULL,
    DolulukOrani DECIMAL(18,2) NULL,
    SuMiktari    DECIMAL(18,2) NULL,
    CONSTRAINT FK_FDL_Date FOREIGN KEY (DateKey) REFERENCES dbo.DimDate(DateKey),
    CONSTRAINT FK_FDL_Dam  FOREIGN KEY (DamKey)  REFERENCES dbo.DimDam(DamKey)
);
GO

CREATE TABLE dbo.FactWaterProductionBySource (
    FactWaterProductionId INT IDENTITY(1,1) PRIMARY KEY,
    DateKey INT NOT NULL,
    WaterSourceKey INT NOT NULL,
    UretilenSuMiktari DECIMAL(18,2) NULL,
    CONSTRAINT FK_FWP_Date FOREIGN KEY (DateKey) REFERENCES dbo.DimDate(DateKey),
    CONSTRAINT FK_FWP_WS   FOREIGN KEY (WaterSourceKey) REFERENCES dbo.DimWaterSource(WaterSourceKey)
);
GO

/* ============================================================
   3) DIM DOLDURMA (SmartWaterDB kaynak)
   ============================================================ */

-- DimDate: Clean_Consumption + üretim + baraj (hepsini kapsasýn)
INSERT INTO dbo.DimDate (DateKey, Yil, Ay)
SELECT DISTINCT (c.[Year]*100)+c.[Month], c.[Year], c.[Month]
FROM SmartWaterDB.dbo.Clean_Consumption c;

INSERT INTO dbo.DimDate (DateKey, Yil, Ay)
SELECT DISTINCT (p.[YIL]*100)+p.[AY], p.[YIL], p.[AY]
FROM SmartWaterDB.dbo.Raw_MonthlyProductionBySource p
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.DimDate d WHERE d.DateKey = (p.[YIL]*100)+p.[AY]
);

INSERT INTO dbo.DimDate (DateKey, Yil, Ay)
SELECT DISTINCT (YEAR(TRY_CONVERT(date, r.DURUM_TARIHI))*100)+MONTH(TRY_CONVERT(date, r.DURUM_TARIHI)),
       YEAR(TRY_CONVERT(date, r.DURUM_TARIHI)),
       MONTH(TRY_CONVERT(date, r.DURUM_TARIHI))
FROM SmartWaterDB.dbo.Raw_DamLevels r
WHERE TRY_CONVERT(date, r.DURUM_TARIHI) IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM dbo.DimDate d
      WHERE d.DateKey = (YEAR(TRY_CONVERT(date, r.DURUM_TARIHI))*100)+MONTH(TRY_CONVERT(date, r.DURUM_TARIHI))
  );

-- DimLocation
INSERT INTO dbo.DimLocation (Ilce, Mahalle)
SELECT DISTINCT c.District, c.Neighborhood
FROM SmartWaterDB.dbo.Clean_Consumption c;

-- DimSubscriptionType
INSERT INTO dbo.DimSubscriptionType (AbonelikGrubu, AbonelikTuru, AboneTipi)
SELECT DISTINCT c.SubscriptionGroup, c.SubscriptionType, c.CustomerType
FROM SmartWaterDB.dbo.Clean_Consumption c
WHERE c.SubscriptionGroup IS NOT NULL
  AND c.SubscriptionType IS NOT NULL
  AND c.CustomerType IS NOT NULL;

-- DimDistrict
INSERT INTO dbo.DimDistrict (DistrictName)
SELECT DISTINCT r.ILCE
FROM SmartWaterDB.dbo.Raw_WaterLosses r
WHERE r.ILCE IS NOT NULL;

-- DimDam
INSERT INTO dbo.DimDam (DamName)
SELECT DISTINCT r.BARAJ_KUYU_ADI
FROM SmartWaterDB.dbo.Raw_DamLevels r
WHERE r.BARAJ_KUYU_ADI IS NOT NULL;

-- DimWaterSource
INSERT INTO dbo.DimWaterSource (WaterSourceName)
SELECT DISTINCT p.URETIM_KAYNAGI
FROM SmartWaterDB.dbo.Raw_MonthlyProductionBySource p
WHERE p.URETIM_KAYNAGI IS NOT NULL;
GO

/* ============================================================
   4) FACT DOLDURMA
   ============================================================ */

-- FactConsumption
INSERT INTO dbo.FactConsumption (
    DateKey, LocationKey, SubscriptionTypeKey,
    AboneAdedi, OrtalamaTuketim, ToplamTuketim
)
SELECT
    d.DateKey,
    l.LocationKey,
    s.SubscriptionTypeKey,
    CAST(c.SubscriberCount AS INT),
    CAST(c.AvgConsumption AS DECIMAL(18,2)),
    CAST(c.SubscriberCount AS DECIMAL(18,2)) * CAST(c.AvgConsumption AS DECIMAL(18,2))
FROM SmartWaterDB.dbo.Clean_Consumption c
JOIN dbo.DimDate d
  ON d.Yil = c.[Year] AND d.Ay = c.[Month]
JOIN dbo.DimLocation l
  ON l.Ilce = c.District AND l.Mahalle = c.Neighborhood
JOIN dbo.DimSubscriptionType s
  ON s.AbonelikGrubu = c.SubscriptionGroup
 AND s.AbonelikTuru  = c.SubscriptionType
 AND s.AboneTipi     = c.CustomerType;
GO

-- FactWaterLosses (yýllýk ise Ay=1 temsil)
INSERT INTO dbo.FactWaterLosses (
    DateKey, DistrictKey,
    TeminServisBaglantisiKayip, FizikiKayiplar, IzinliTuketim,
    FaturalanmayanIzinliTuketim, GelirGetirmeyenSuMiktari,
    FaturalananOlculmeyenKullanim, FaturalanmayanOlculmeyenKullanim,
    SayaclardakiOlcumHatasi, SuMiktari, FaturalananOlculenKullanim,
    IdariKayiplar, IzinsizTuketim, FaturalananIzinliSuTuketimi,
    FaturalanmayanOlculenKullanim, GelirGetirenSuMiktari,
    DepolardaKacak, SuKayiplari
)
SELECT
    d.DateKey,
    dist.DistrictKey,
    r.TEMIN_SERVIS_BAGLANTISI_KAYIP,
    r.FIZIKI_KAYIPLAR,
    r.IZINLI_TUKETIM,
    r.FATURALANMAYAN_IZINLI_TUKETIM,
    r.GELIR_GETIRMEYEN_SU_MIKTARI,
    r.FATURALANAN_OLCULMEYEN_KULLANIM,
    r.FATURALANMAYAN_OLCULMEYEN_KULLANIM,
    r.SAYACLARDAKI_OLCUM_HATASI,
    r.SISTEME_GIREN_SU_MIKTARI,
    r.FATURALANAN_OLCULEN_KULLANIM,
    r.IDARI_KAYIPLAR,
    r.IZINSIZ_TUKETIM,
    r.FATURALANAN_IZINLI_SU_TUKETIMI,
    r.FATURALANMAYAN_OLCULEN_KULLANIM,
    r.GELIR_GETIREN_SU_MIKTARI,
    r.DEPOLARDA_MEYDANA_GELEN_KACAK,
    r.SU_KAYIPLARI
FROM SmartWaterDB.dbo.Raw_WaterLosses r
JOIN dbo.DimDistrict dist
  ON dist.DistrictName = r.ILCE
JOIN dbo.DimDate d
  ON d.Yil = r.YIL AND d.Ay = 1;
GO

-- FactDamLevel
INSERT INTO dbo.FactDamLevel (DateKey, DamKey, DolulukOrani, SuMiktari)
SELECT
    d.DateKey,
    dam.DamKey,
    TRY_CONVERT(DECIMAL(18,2), REPLACE(r.DOLULUK_ORANI, ',', '.')),
    TRY_CONVERT(DECIMAL(18,2), REPLACE(r.SU_DURUMU, ',', '.'))
FROM SmartWaterDB.dbo.Raw_DamLevels r
JOIN dbo.DimDate d
  ON d.Yil = YEAR(TRY_CONVERT(date, r.DURUM_TARIHI))
 AND d.Ay  = MONTH(TRY_CONVERT(date, r.DURUM_TARIHI))
JOIN dbo.DimDam dam
  ON dam.DamName = r.BARAJ_KUYU_ADI
WHERE TRY_CONVERT(date, r.DURUM_TARIHI) IS NOT NULL;
GO

-- FactWaterProductionBySource
INSERT INTO dbo.FactWaterProductionBySource (DateKey, WaterSourceKey, UretilenSuMiktari)
SELECT
    d.DateKey,
    ws.WaterSourceKey,
    p.URETIM_MIKTARI
FROM SmartWaterDB.dbo.Raw_MonthlyProductionBySource p
JOIN dbo.DimDate d
  ON d.Yil = p.YIL AND d.Ay = p.AY
JOIN dbo.DimWaterSource ws
  ON ws.WaterSourceName = p.URETIM_KAYNAGI;
GO

/* ============================================================
   5) ÝNDEXLER (sorgu performansý)
   ============================================================ */
CREATE NONCLUSTERED INDEX IX_FactConsumption_Date ON dbo.FactConsumption(DateKey);
CREATE NONCLUSTERED INDEX IX_FactConsumption_Location ON dbo.FactConsumption(LocationKey);
CREATE NONCLUSTERED INDEX IX_FactConsumption_Sub ON dbo.FactConsumption(SubscriptionTypeKey);

CREATE NONCLUSTERED INDEX IX_FactWaterLosses_Date ON dbo.FactWaterLosses(DateKey);
CREATE NONCLUSTERED INDEX IX_FactWaterLosses_District ON dbo.FactWaterLosses(DistrictKey);

CREATE NONCLUSTERED INDEX IX_FactDamLevel_Date ON dbo.FactDamLevel(DateKey);
CREATE NONCLUSTERED INDEX IX_FactDamLevel_Dam ON dbo.FactDamLevel(DamKey);

CREATE NONCLUSTERED INDEX IX_FactWaterProd_Date ON dbo.FactWaterProductionBySource(DateKey);
CREATE NONCLUSTERED INDEX IX_FactWaterProd_Source ON dbo.FactWaterProductionBySource(WaterSourceKey);
GO

/* ============================================================
   6) Hýzlý Kontroller
   ============================================================ */
SELECT 'DimDate' AS T, COUNT(*) AS C FROM dbo.DimDate
UNION ALL SELECT 'DimLocation', COUNT(*) FROM dbo.DimLocation
UNION ALL SELECT 'DimSubscriptionType', COUNT(*) FROM dbo.DimSubscriptionType
UNION ALL SELECT 'DimDistrict', COUNT(*) FROM dbo.DimDistrict
UNION ALL SELECT 'DimDam', COUNT(*) FROM dbo.DimDam
UNION ALL SELECT 'DimWaterSource', COUNT(*) FROM dbo.DimWaterSource
UNION ALL SELECT 'FactConsumption', COUNT(*) FROM dbo.FactConsumption
UNION ALL SELECT 'FactWaterLosses', COUNT(*) FROM dbo.FactWaterLosses
UNION ALL SELECT 'FactDamLevel', COUNT(*) FROM dbo.FactDamLevel
UNION ALL SELECT 'FactWaterProductionBySource', COUNT(*) FROM dbo.FactWaterProductionBySource;
GO
