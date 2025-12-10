import pandas as pd

baraj = pd.read_csv("data/baraj_veri.csv")

# Tarihi datetime formatına çeviriyoruz
baraj["Tarih"] = pd.to_datetime(baraj["Tarih"], dayfirst=True, errors="coerce")
baraj["YIL"] = baraj["Tarih"].dt.year
baraj["AY"] = baraj["Tarih"].dt.month
baraj = baraj.drop(columns=["Tarih"])
baraj.to_csv("data/baraj_yil_ay.csv", index=False)
print("Hazır! baraj_yil_ay.csv oluşturuldu.")
