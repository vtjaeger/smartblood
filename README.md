# SmartBlood 🩸

**Intelligent Health Monitoring System**

SmartBlood is a health monitoring system that integrates IoT devices, Artificial Intelligence, and clinical data interoperability to monitor patient health information, such as blood pressure and heart rate.

The project uses a backend application developed in C# with ASP.NET Core, an Artificial Intelligence API developed in Python, and a MySQL database.

## 🎯 Project Goals

- Monitor patient health measurements.
- Receive data from IoT devices or simulators.
- Apply Machine Learning to classify blood pressure measurements.
- Store measurement history and AI analysis results.
- Display health information through an interactive dashboard.
- Provide clinical data using the HL7 FHIR standard.

## 🏗️ Project Architecture

SmartBlood consists of the following main components:

| Component | Technology | Responsibility |
|---|---|---|
| Backend | C# / ASP.NET Core | Main API, business logic, and data persistence |
| Artificial Intelligence | Python / Flask / Scikit-learn | Machine Learning predictions |
| Database | MySQL | Patient, measurement, and analysis storage |
| Frontend | HTML / CSS / JavaScript | Health monitoring dashboard |
| Interoperability | HL7 FHIR | Standardized representation of clinical information |

### System Workflow

1. An IoT device or simulator sends a health measurement.
2. The backend receives and validates the data.
3. The measurement is stored in the MySQL database.
4. The backend sends the required patient data to the Python API.
5. The Random Forest model performs a prediction.
6. The AI result is returned to the backend.
7. The analysis is stored in the database.
8. The dashboard displays the information to the user.

## 🧠 Artificial Intelligence

SmartBlood uses Machine Learning with the **Random Forest** algorithm, implemented using the Scikit-learn library.

The model receives patient characteristics and health information to generate a classification among the following categories:

- Normal
- Stage 1
- Stage 2

In addition to the predicted classification, the model returns estimated probabilities for each category.

### AI Project Structure

```text
IA/
├── app.py
├── train_model.py
├── requirements.txt
├── .gitignore
├── data/
└── model/
    └── random_forest_model.joblib
```

| File | Description |
|---|---|
| `app.py` | Flask API responsible for receiving data and executing predictions |
| `train_model.py` | Model training script |
| `requirements.txt` | Python dependencies |
| `data/` | Training datasets |
| `model/` | Trained model storage |

### Python API

The Python API provides the following endpoints:

| Method | Endpoint | Description |
|---|---|---|
| GET | `/` | Checks whether the API is running |
| POST | `/predict` | Generates a prediction using the trained model |

To start the API:

```powershell
cd IA

python -m venv venv

.\venv\Scripts\Activate.ps1

pip install -r requirements.txt

python app.py
```

The API will be available at:

```text
http://localhost:5000
```

**Important:** The Python virtual environment (`venv`) should not be committed to GitHub. Dependencies can be installed using `requirements.txt`.

The current model generates predictions using its previously trained parameters. Automatic periodic retraining is planned as a future project enhancement and will require reliable reference data, performance evaluation, and model version control.

## ⚙️ ASP.NET Core Backend

The backend centralizes the system operations and integrates the database, Python API, and frontend.

Main responsibilities:

- Patient registration and retrieval.
- Health measurement reception.
- Measurement history retrieval.
- Artificial Intelligence integration.
- AI analysis persistence.
- FHIR resource generation.

### Configuration

Configure the MySQL connection and Python API address in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=smartblood;user=root;password=YOUR_PASSWORD"
  },
  "AI": {
    "BaseUrl": "http://localhost:5000/"
  },
  "WearableSimulator": {
    "Enabled": true,
    "ApiBaseUrl": "http://localhost:5109/",
    "PatientId": 1,
    "DeviceId": "wrist-04a7",
    "IntervalSeconds": 5
  }
}
```

Adjust the connection settings according to your environment.

Do not commit real passwords or credentials to the repository. For shared or production environments, use local configuration files or environment variables.

### Running the Backend

Open the SmartBlood solution in Visual Studio and run the ASP.NET Core project.

Swagger documentation will be available at:

```text
https://localhost:7062/swagger
```

The ports may vary depending on the project configuration.

## 🗄️ Database

The project uses MySQL with the following database:

```text
smartblood
```

Main tables:

| Table | Description |
|---|---|
| `Patients` | Patient registration information |
| `Measurements` | Measurements received from devices |
| `AIAnalyses` | Results generated by the AI model |
| `__EFMigrationsHistory` | Entity Framework Core migration history |

The `AIAnalyses` table is related to `Measurements`, allowing each AI analysis to be associated with its corresponding measurement.

Configure the database connection in `appsettings.json` before running the backend.

## ⌚ IoT Simulator

The project includes a simulator that automatically generates measurements to test the system.

Configuration:

```json
"WearableSimulator": {
  "Enabled": true,
  "ApiBaseUrl": "http://localhost:5109/",
  "PatientId": 1,
  "DeviceId": "wrist-04a7",
  "IntervalSeconds": 5
}
```

Parameters:

- `Enabled`: enables or disables the simulator.
- `ApiBaseUrl`: backend address.
- `PatientId`: patient associated with the measurements.
- `DeviceId`: device identifier.
- `IntervalSeconds`: interval between measurements.

The simulator allows testing the integration between data collection, storage, Artificial Intelligence, and the dashboard without requiring a physical device.

## 📊 Dashboard

The frontend uses HTML, CSS, and JavaScript and is served by the ASP.NET Core application.

Project structure:

```text
wwwroot/
├── index.html
├── css/
│   └── style.css
└── js/
    └── dashboard.js
```

The dashboard provides:

- Patient lookup.
- Blood pressure monitoring.
- Heart rate monitoring.
- AI classification results.
- Estimated probabilities for each category.
- Measurement history.
- Blood pressure trend charts.
- Heart rate trend charts.

Chart.js is used to generate the charts.

Access the dashboard at:

```text
https://localhost:7062/
```

## 🏥 HL7 FHIR Integration

SmartBlood uses the HL7 FHIR standard to represent clinical information in a structured and interoperable format.

The following resources are supported:

- `Patient`: patient information.
- `Observation`: blood pressure measurements.
- `Observation`: heart rate measurements.
- `Bundle`: collection of related FHIR resources.

### FHIR Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/Patients/{id}/fhir` | Returns a patient as a FHIR resource |
| GET | `/api/Patients/{id}/fhir/bundle` | Returns the Patient and observations from the latest measurement |
| GET | `/api/Measurements/{id}/fhir` | Returns a blood pressure observation |
| GET | `/api/Measurements/{id}/fhir/heart-rate` | Returns a heart rate observation |

FHIR resources are serialized using the Firely SDK.

The project exposes FHIR resources but does not currently push data to an external FHIR server or electronic health record system.

## 🔌 Main Backend Endpoints

### Patients

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/Patients` | Registers a patient |
| GET | `/api/Patients` | Retrieves all patients |
| GET | `/api/Patients/{id}` | Retrieves a specific patient |

### Measurements

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/Measurements` | Registers a measurement and requests AI analysis |
| GET | `/api/Measurements` | Retrieves measurements |
| GET | `/api/Measurements/{id}` | Retrieves a specific measurement |
| GET | `/api/Measurements/patient/{id}` | Retrieves a patient's measurement history |
| GET | `/api/Measurements/patient/{id}/latest` | Retrieves the patient's latest measurement |

Use Swagger to inspect the complete request and response contracts.

## 🚀 Getting Started

Follow these steps to run SmartBlood locally.

### 1. Database Setup

- Install and start MySQL.
- Create or configure the `smartblood` database.
- Configure the connection in `appsettings.json`.
- Apply Entity Framework Core migrations if necessary.

### 2. Python API

Open a terminal in the `IA` directory:

```powershell
python -m venv venv

.\venv\Scripts\Activate.ps1

pip install -r requirements.txt

python app.py
```

Check the API:

```text
http://localhost:5000/
```

### 3. C# Backend

- Open the solution in Visual Studio.
- Restore NuGet packages.
- Check the MySQL and Python API configuration.
- Run the ASP.NET Core project.

### 4. Dashboard

Access:

```text
https://localhost:7062/
```

### 5. Testing the Complete Workflow

1. Make sure a patient is registered.
2. Confirm that the Python API is running.
3. Start the backend.
4. Enable the IoT simulator if desired.
5. Look up the patient in the dashboard.
6. Check measurements, classifications, and probabilities.

## 🔒 Security and Data Protection

SmartBlood handles health-related information. Therefore:

- Do not publish personal or clinical data on GitHub.
- Do not commit passwords, tokens, or credentials.
- Use anonymized or appropriate test datasets.
- Control access to patient information.
- Validate the quality of training data.
- Do not use AI-generated predictions as ground-truth labels without independent validation.

The model's classifications are computational estimates intended to support health monitoring. They are not medical diagnoses and do not replace professional medical evaluation.

## 🛠️ Technologies

- C#
- ASP.NET Core
- Entity Framework Core
- Python
- Flask
- Scikit-learn
- Pandas
- Joblib
- MySQL
- HTML5
- CSS3
- JavaScript
- Chart.js
- HL7 FHIR
- Firely SDK
- Visual Studio

## 📌 Future Improvements

- Implement automatic periodic model retraining.
- Add AI model version management.
- Evaluate model performance before deployment.
- Implement training history and monitoring.
- Improve data quality and model performance monitoring.
- Expand health monitoring alerts.
- Extend interoperability with external systems using HL7 FHIR.

## 👨‍💻 Project

**SmartBlood — Intelligent Health Monitoring System**

An integrated health monitoring solution combining Internet of Things, Artificial Intelligence, and clinical data interoperability.
