from flask import Flask, request, jsonify
import joblib
import pandas as pd
from pathlib import Path


# ==========================================================
# CONFIGURAÇÃO
# ==========================================================

app = Flask(__name__)

BASE_DIR = Path(__file__).resolve().parent

MODEL_PATH = BASE_DIR / "model" / "random_forest_model.joblib"


# ==========================================================
# CARREGAR MODELO
# ==========================================================

print("Carregando modelo...")

model = joblib.load(MODEL_PATH)

print("Modelo carregado com sucesso!")


# ==========================================================
# ROTA DE TESTE
# ==========================================================

@app.route("/", methods=["GET"])
def home():

    return jsonify({
        "message": "SmartBlood AI API funcionando"
    })


# ==========================================================
# PREDIÇÃO
# ==========================================================

@app.route("/predict", methods=["POST"])
def predict():

    try:

        data = request.get_json()

        print("\nNova solicitação recebida:")
        print(data)

        # Criar DataFrame com os dados recebidos
        input_data = pd.DataFrame([{
            "Age": data["age"],
            "Gender": data["gender"],
            "Height_cm": data["height_cm"],
            "Weight_kg": data["weight_kg"],
            "BMI": data["bmi"],
            "Systolic_BP": data["systolic_bp"],
            "Diastolic_BP": data["diastolic_bp"],
            "Cholesterol_mg_dL": data["cholesterol_mg_dL"],
            "Blood_Glucose_mg_dL": data["blood_glucose_mg_dL"],
            "Smoking": data["smoking"],
            "Alcohol_Intake": data["alcohol_intake"],
            "Physical_Activity": data["physical_activity"],
            "Family_History": data["family_history"]
        }])

        # Fazer previsão
        prediction = model.predict(input_data)[0]

        # Probabilidades
        probabilities = model.predict_proba(input_data)[0]

        classes = model.classes_

        probability_dict = {
            class_name: round(float(probability), 4)
            for class_name, probability
            in zip(classes, probabilities)
        }

        response = {
            "classification": prediction,
            "probabilities": probability_dict
        }

        print("\nResultado:")
        print(response)

        return jsonify(response)

    except Exception as e:

        print(f"\nErro: {e}")

        return jsonify({
            "error": str(e)
        }), 400


# ==========================================================
# INICIAR SERVIDOR
# ==========================================================

if __name__ == "__main__":

    print("\n======================================")
    print("SMARTBLOOD AI API")
    print("======================================")

    print("\nServidor iniciado em:")
    print("http://localhost:5000")

    app.run(
        host="0.0.0.0",
        port=5000,
        debug=False
    )