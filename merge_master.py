import pandas as pd

anom = pd.read_csv("data/anomolili_bireysel_tuketim.csv")
ilce = pd.read_csv("data/ilce_bazlı_ortalama_tuketim.csv")
baraj = pd.read_csv("data/baraj_yil_ay.csv")

anom = anom.copy()
ilce = ilce.copy()
baraj = baraj.copy()

anom.columns = [c.upper() for c in anom.columns]
ilce.columns = [c.upper() for c in ilce.columns]
baraj.columns = [c.upper() for c in baraj.columns]

anom = anom[(anom["YIL"] >= 2020) & (anom["YIL"] <= 2024)]
ilce = ilce[(ilce["YIL"] >= 2020) & (ilce["YIL"] <= 2024)]
baraj = baraj[(baraj["YIL"] >= 2020) & (baraj["YIL"] <= 2024)]

ilce_agg = ilce.groupby(["YIL", "AY", "ILCE"]).agg(
    ORTALAMA_TUKETIM_MEAN=("ORTALAMA_TUKETIM", "mean"),
    ABONE_ADEDI_SUM=("ABONE_ADEDI", "sum"),
    KAYIT_SAYISI=("ORTALAMA_TUKETIM", "size")
).reset_index()

baraj_aylik = baraj.groupby(["YIL", "AY"]).agg(
    ORT_DOLULUK=("DOLULUKORANI", "mean"),
    TOPLAM_SU=("SUDURUMU", "sum"),
    BARAJ_SAYISI=("DOLULUKORANI", "size")
).reset_index()

df = anom.merge(
    ilce_agg,
    on=["YIL", "AY", "ILCE"],
    how="left"
)

df = df.merge(
    baraj_aylik,
    on=["YIL", "AY"],
    how="left"
)

df.to_csv("data/master_3kaynak_2020_2024.csv", index=False)

print("Satır sayısı:", len(df))
print("İlçe sayısı:", df["ILCE"].nunique())
print(df.head())
