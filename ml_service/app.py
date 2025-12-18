from flask import Flask, request, jsonify
import joblib, json
import pandas as pd
import numpy as np
from pathlib import Path

app = Flask(__name__)

ART = Path(__file__).resolve().parent / "artifacts"

model = joblib.load(ART / "best_model.joblib")

with open(ART / "feature_cols.json", "r", encoding="utf-8") as f:
    feature_cols = json.load(f)

@app.get("/health")
def health():
    return jsonify({"status": "ok", "features": feature_cols})

@app.post("/predict")
def predict():
    data = request.get_json(silent=True)
    if not data:
        return jsonify({"error": "JSON body gerekli"}), 400

    records = data["records"] if "records" in data else [data]
    X = pd.DataFrame(records)

    for col in feature_cols:
        if col not in X.columns:
            X[col] = np.nan

    X = X[feature_cols]
    for col in feature_cols:
        X[col] = pd.to_numeric(X[col], errors="coerce")

    X = X.fillna(X.median(numeric_only=True))

    preds = model.predict(X.values)
    return jsonify({"predictions": [float(p) for p in preds]})

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000, debug=True)
