import pandas as pd
import joblib

from pathlib import Path

from sklearn.model_selection import train_test_split
from sklearn.preprocessing import OneHotEncoder, StandardScaler
from sklearn.compose import ColumnTransformer
from sklearn.pipeline import Pipeline

from sklearn.linear_model import LogisticRegression
from sklearn.ensemble import RandomForestClassifier

from sklearn.metrics import (
    accuracy_score,
    classification_report,
    confusion_matrix
)


# ==========================================================
# CONFIGURAÇÃO
# ==========================================================

BASE_DIR = Path(__file__).resolve().parent

DATA_PATH = BASE_DIR / "data" / "hypertension.xlsx"
MODEL_DIR = BASE_DIR / "model"

MODEL_DIR.mkdir(exist_ok=True)


# ==========================================================
# CARREGAR DATASET
# ==========================================================

print("======================================")
print("SMARTBLOOD - TREINAMENTO DA IA")
print("======================================")

print("\nCarregando dataset...")

df = pd.read_excel(DATA_PATH)

print(f"Dataset carregado: {len(df)} registros")


# ==========================================================
# FEATURES E TARGET
# ==========================================================

features = [
    "Age",
    "Gender",
    "Height_cm",
    "Weight_kg",
    "BMI",
    "Systolic_BP",
    "Diastolic_BP",
    "Cholesterol_mg_dL",
    "Blood_Glucose_mg_dL",
    "Smoking",
    "Alcohol_Intake",
    "Physical_Activity",
    "Family_History"
]

target = "Hypertension_Status"


X = df[features]
y = df[target]


print("\n--- Features utilizadas ---")

for feature in features:
    print(f"- {feature}")


print("\n--- Classes ---")
print(y.value_counts())


# ==========================================================
# SEPARAR TREINO E TESTE
# ==========================================================

X_train, X_test, y_train, y_test = train_test_split(
    X,
    y,
    test_size=0.20,
    random_state=42,
    stratify=y
)


print("\n--- Divisão dos dados ---")

print(f"Treinamento: {len(X_train)} registros")
print(f"Teste: {len(X_test)} registros")


# ==========================================================
# FEATURES NUMÉRICAS E CATEGÓRICAS
# ==========================================================

numeric_features = [
    "Age",
    "Height_cm",
    "Weight_kg",
    "BMI",
    "Systolic_BP",
    "Diastolic_BP",
    "Cholesterol_mg_dL",
    "Blood_Glucose_mg_dL"
]

categorical_features = [
    "Gender",
    "Smoking",
    "Alcohol_Intake",
    "Physical_Activity",
    "Family_History"
]


# ==========================================================
# PRÉ-PROCESSAMENTO
# ==========================================================

preprocessor = ColumnTransformer(
    transformers=[
        (
            "num",
            StandardScaler(),
            numeric_features
        ),
        (
            "cat",
            OneHotEncoder(handle_unknown="ignore"),
            categorical_features
        )
    ]
)


# ==========================================================
# MODELO 1 - LOGISTIC REGRESSION
# ==========================================================

print("\n======================================")
print("TREINANDO LOGISTIC REGRESSION")
print("======================================")

logistic_model = Pipeline(
    steps=[
        (
            "preprocessor",
            preprocessor
        ),
        (
            "classifier",
            LogisticRegression(
                max_iter=1000,
                class_weight="balanced"
            )
        )
    ]
)


logistic_model.fit(
    X_train,
    y_train
)


logistic_predictions = logistic_model.predict(X_test)


logistic_accuracy = accuracy_score(
    y_test,
    logistic_predictions
)


print(
    f"\nAccuracy: {logistic_accuracy:.4f}"
)


print("\n--- Classification Report ---")

print(
    classification_report(
        y_test,
        logistic_predictions
    )
)


print("--- Confusion Matrix ---")

print(
    confusion_matrix(
        y_test,
        logistic_predictions
    )
)


# ==========================================================
# MODELO 2 - RANDOM FOREST
# ==========================================================

print("\n======================================")
print("TREINANDO RANDOM FOREST")
print("======================================")

random_forest_model = Pipeline(
    steps=[
        (
            "preprocessor",
            preprocessor
        ),
        (
            "classifier",
            RandomForestClassifier(
                n_estimators=200,
                random_state=42,
                class_weight="balanced"
            )
        )
    ]
)


random_forest_model.fit(
    X_train,
    y_train
)


random_forest_predictions = random_forest_model.predict(
    X_test
)


random_forest_accuracy = accuracy_score(
    y_test,
    random_forest_predictions
)


print(
    f"\nAccuracy: {random_forest_accuracy:.4f}"
)


print("\n--- Classification Report ---")

print(
    classification_report(
        y_test,
        random_forest_predictions
    )
)


print("--- Confusion Matrix ---")

print(
    confusion_matrix(
        y_test,
        random_forest_predictions
    )
)


# ==========================================================
# SALVAR MODELOS
# ==========================================================

logistic_path = MODEL_DIR / "logistic_model.joblib"

random_forest_path = MODEL_DIR / "random_forest_model.joblib"


joblib.dump(
    logistic_model,
    logistic_path
)


joblib.dump(
    random_forest_model,
    random_forest_path
)


print("\n======================================")
print("MODELOS SALVOS")
print("======================================")

print(
    f"\nLogistic Regression:"
    f"\n{logistic_path}"
)

print(
    f"\nRandom Forest:"
    f"\n{random_forest_path}"
)


print("\n======================================")
print("TREINAMENTO FINALIZADO")
print("======================================")


# ==========================================================
# ANÁLISE DA IMPORTÂNCIA DAS FEATURES
# ==========================================================

print("\n======================================")
print("IMPORTÂNCIA DAS FEATURES")
print("======================================")

rf_classifier = random_forest_model.named_steps["classifier"]

feature_names = (
    numeric_features
    + list(
        random_forest_model
        .named_steps["preprocessor"]
        .named_transformers_["cat"]
        .get_feature_names_out(categorical_features)
    )
)

importances = rf_classifier.feature_importances_

feature_importance = pd.DataFrame({
    "Feature": feature_names,
    "Importance": importances
})

feature_importance = feature_importance.sort_values(
    by="Importance",
    ascending=False
)

print("\n--- Features mais importantes ---")

print(
    feature_importance.to_string(index=False)
)