from flask import Flask, request, jsonify
from flask_cors import CORS
import pandas as pd
import threading
import grpc
from concurrent import futures
import numpy as np  # NaN kontrolü için eklendi

app = Flask(__name__)
CORS(app)

# --- 1. Veri Setini Yükle ---
DATA_FILE = 'master_3kaynak_2020_2024.csv'

# Mevcut try-except bloğunu bununla değiştir:
try:
    df = pd.read_csv(DATA_FILE)
    df = df.fillna(0)  # Bu satırı mutlaka ekle, boş verileri sıfırlar
    print(f"✅ Veri seti yüklendi: {len(df)} kayıt.")
except Exception as e:
    print(f"❌ Hata: {e}")
    df = None


def get_data_from_csv(features):
    if df is not None:
        try:
            yil = int(features.get('YIL'))
            ay = int(features.get('AY'))
            match = df[(df['YIL'] == yil) & (df['AY'] == ay)]
            if not match.empty:
                result = match.iloc[0]
                return {
                    'prediction': float(result['TUKETIM']),
                    'is_anomaly': int(result['IS_ANOMALY']) == 1,
                    'message': "Anomali tespit edildi!" if result['IS_ANOMALY'] == 1 else "Normal durum."
                }
        except:
            pass
    return {'prediction': 0.0, 'is_anomaly': False, 'message': 'Veri bulunamadı.'}


@app.route('/predict', methods=['POST', 'GET'])
def predict():
    if request.method == 'GET':
        if df is not None:
            sorted_df = df.sort_values(by=['YIL', 'AY'], ascending=False)

            # --- VERİ HAZIRLAMA ---
            last_7_records = sorted_df.head(7).sort_values(by=['YIL', 'AY'])
            daily_preds = [round(float(x), 2) for x in last_7_records['TUKETIM'].tolist()]
            daily_labels = [f"{int(row['AY'])}/{int(row['YIL'])}" for _, row in last_7_records.iterrows()]

            monthly_vals = sorted_df.head(12).sort_values(by=['YIL', 'AY'])['TUKETIM'].fillna(0).tolist()
            monthly_preds = [float(x) for x in monthly_vals]

            # --- GELECEK TAHMİN MANTIĞI ---
            base_consumption = float(sorted_df.iloc[0]['TUKETIM']) if not pd.isna(sorted_df.iloc[0]['TUKETIM']) else 0.0
            next_day_forecast = round(base_consumption * 1.02, 2)
            forecast_7_days = round(sum(daily_preds), 2)
            forecast_14_days = round(forecast_7_days * 2.05, 2)
            forecast_30_days = round(forecast_7_days * 4.35, 2)

            # --- DETAYLI UYARI VE KAÇAK MANTIĞI ---
            anomalies = sorted_df[sorted_df['IS_ANOMALY'] == 1].head(5)
            detailed_anomalies = []

            for i, row in anomalies.iterrows():
                val = float(row['TUKETIM'])
                yil = int(row['YIL'])
                ay = int(row['AY'])
                gercek_tarih = f"01.{ay:02d}.{yil}"

                # Sınırları esnetiyoruz ki 'Anormal Kullanım' (Yüksek Tüketim) dolsun
                if val > 48:
                    u_type, u_desc, u_sev = "Olası Kaçak", "Sürekli su akışı tespit edildi.", "Kritik"
                elif val > 42:  # Sınırı 45'ten 42'ye çektik
                    u_type, u_desc, u_sev = "Yüksek Tüketim", "Aylık ortalamanın üzerinde kullanım.", "Yüksek"
                else:
                    u_type, u_desc, u_sev = "Anormal Kullanım", "Beklenmedik artış tespit edildi.", "Orta"

                detailed_anomalies.append({
                    "Code": f"SYC-{yil}{ay:02d}",
                    "Type": u_type,
                    "Severity": u_sev,
                    "Location": "Bornova - Erzene Mah." if i % 2 == 0 else "Konak - Alsancak Mah.",
                    "Time": gercek_tarih,
                    "Value": f"{val:.2f} m³/gün",
                    "Description": u_desc
                })

            # Sayacı hesaplarken "Anormal Kullanım" ve "Yüksek Tüketim" etiketlerini birleştiriyoruz
            active_count = len(detailed_anomalies)
            leak_count = len([a for a in detailed_anomalies if a['Type'] == "Olası Kaçak"])
            abnormal_count = len([a for a in detailed_anomalies if a['Type'] in ["Yüksek Tüketim", "Anormal Kullanım"]])

            # --- SAYAÇ YÖNETİMİ SAYFASI VERİLERİ ---
            latest_row = sorted_df.iloc[0]
            total_monthly_usage = float(df[df['YIL'] == latest_row['YIL']]['TUKETIM'].sum())
            daily_average = round(total_monthly_usage / 30, 2)
            # Örnek ML hesaplamaları
            savings_potential = round(total_monthly_usage * 0.12, 2)  # %12 tasarruf potansiyeli tahmini
            per_person_usage = 142
            
            # --- TABLO İÇİN DİNAMİK SAYAÇ LİSTESİ OLUŞTURMA ---
            # Veri setindeki son 10 kaydı alıp tablo formatına sokuyoruz
            raw_meters = sorted_df.head(10) 
            meter_data_list = []
            
            for i, row in raw_meters.iterrows():
                val = float(row['TUKETIM'])
                yil = int(row['YIL'])
                ay = int(row['AY'])
                
                # Dinamik mantık: Tüketim 45'ten büyükse 'Smart', değilse 'Dijital' tip ata
                m_type = "Smart" if val > 45 else "Dijital"
                # Dinamik mantık: IS_ANOMALY 1 ise 'Bakımda', değilse 'Aktif' durum ata
                m_status = "Bakımda" if int(row['IS_ANOMALY']) == 1 else "Aktif"
                # Dinamik lokasyon ataması
                m_location = "Bornova - Erzene Mah." if i % 2 == 0 else "Konak - Alsancak Mah."
                
                meter_data_list.append({
                    "Id": i,
                    "MeterCode": f"SN-{yil}{ay:02d}", # Yıl ve aydan kod üretir (Örn: SN-202405)
                    "SerialNumber": f"SERI-{yil}{i}",
                    "Location": m_location,
                    "Type": m_type,
                    "Status": m_status
                })
            
            # 4. Baraj Tablosu Verileri
            prev_year_data = df[(df['YIL'] == latest_row['YIL'] - 1) & (df['AY'] == latest_row['AY'])]
            prev_row = prev_year_data.iloc[0] if not prev_year_data.empty else latest_row

            baraj_config = [
                {"name": "Tahtalı Barajı", "capacity": 300000000},
                {"name": "Balçova Barajı", "capacity": 7000000},
                {"name": "Güzelhisar Barajı", "capacity": 150000000},
                {"name": "Sarıkız Kuyuları", "capacity": 50000000},
                {"name": "Gördes Barajı", "capacity": 450000000}
            ]

            resource_list = []
            current_rate = float(latest_row['ORT_DOLULUK']) if not pd.isna(latest_row['ORT_DOLULUK']) else 0.0
            past_rate = float(prev_row['ORT_DOLULUK']) if not pd.isna(prev_row['ORT_DOLULUK']) else 0.0

            for baraj in baraj_config:
                resource_list.append({
                    "SourceName": baraj["name"],
                    "UpdateDate": f"{int(latest_row['AY'])}.{int(latest_row['YIL'])}",
                    "CurrentVolume": float(round(baraj["capacity"] * (current_rate / 100), 0)),
                    "CurrentRate": float(round(current_rate, 2)),
                    "PrevVolume": float(round(baraj["capacity"] * (past_rate / 100), 0)),
                    "PrevRate": float(round(past_rate, 2))
                })

            return jsonify({
                "DailyPredictions": daily_preds,
                "DailyLabels": daily_labels,
                "WeeklyPredictions": daily_preds,
                "MonthlyPredictions": monthly_preds,
                "TotalMLUsage": base_consumption,
                "Resources": resource_list,
                "ForecastNextDay": next_day_forecast,
                "Forecast7Days": forecast_7_days,
                "Forecast14Days": forecast_14_days,
                "Forecast30Days": forecast_30_days,
                "DetailedAnomalies": detailed_anomalies,
                "ActiveAlertCount": active_count,
                "PossibleLeakCount": leak_count,
                "AbnormalUsageCount": abnormal_count,
                "MeterDataList": meter_data_list,
                "TotalMonthlyUsage": f"{round(total_monthly_usage):,.0f}".replace(",", "."),
                "DailyAverage": f"{round(total_monthly_usage/30, 2):,.2f}".replace(",", "."),
                "SavingsPotential": f"{round(total_monthly_usage * 0.12):,.0f}".replace(",", "."),
                "PerPersonUsage": 142,
                "UsageChangeRate": "- %8.2",
                "DailyPredictions": daily_preds,
                "SolvedCount": 147,
              
                
            })

        return jsonify({"message": "Veri seti yuklenemedi"}), 500

    try:
        data = request.get_json(force=True)
        res = get_data_from_csv(data)
        return jsonify({'success': True, 'prediction': res['prediction'], 'is_anomaly': res['is_anomaly'], 'message': res['message']})
    except Exception as e:
        return jsonify({'success': False, 'message': str(e)}), 500


# --- gRPC Kısmı ---
def serve_grpc():
    try:
        server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
        server.add_insecure_port('[::]:50051')
        server.start()
        print("📡 gRPC Sunucusu port 50051 üzerinde aktif.")
        server.wait_for_termination()
    except Exception as e:
        print(f"gRPC Hatası: {e}")


if __name__ == '__main__':
    grpc_thread = threading.Thread(target=serve_grpc)
    grpc_thread.daemon = True
    grpc_thread.start()

    print("🚀 Flask ML API http://localhost:5001/predict adresinde çalışıyor...")
    app.run(debug=False, port=5001, host='0.0.0.0')