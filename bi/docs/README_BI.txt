# BI / OLAP Katmanı

Bu klasör, Akıllı Su Tüketimi projesinin
veri ambarı (DW), ETL (SSIS) ve OLAP (SSAS) katmanlarını içerir.

## SQL Scriptleri
- `01_SmartWaterDB_Build_And_Clean_All.sql` : Operasyonel veritabanı
- `02_SmartWaterDW_Build_And_Load_All.sql` : Veri ambarı (DW)

## SSIS
ETL süreçleri SSIS paketleri ile gerçekleştirilmiştir.
Kaynak → Staging → DW adımları izlenmiştir.

## SSAS (OLAP Cube)
SSAS Multidimensional proje ile OLAP küpü oluşturulmuştur.
Küp içerisinde çoklu measure ve dimension bulunmaktadır.

## Notlar
Connection string ve server isimleri geliştirici ortamına göre düzenlenmelidir.
