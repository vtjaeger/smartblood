const API_URL = "";

let bloodPressureChart = null;
let heartRateChart = null;


// ==========================================
// DASHBOARD LOADING
// ==========================================

async function loadDashboard() {

    const patientId = document.getElementById("patientId").value;
    const message = document.getElementById("message");

    if (!patientId || Number(patientId) <= 0) {
        message.textContent = "Please enter a valid patient ID.";
        return;
    }

    message.textContent = "Loading data...";

    try {

        // Fetch the latest measurement
        const latestResponse = await fetch(
            `${API_URL}/api/Measurements/patient/${patientId}/latest`
        );

        if (!latestResponse.ok) {

            throw new Error(
                latestResponse.status === 404
                    ? "No measurements found for this patient."
                    : "Error retrieving the latest measurement."
            );

        }

        const latest = await latestResponse.json();


        // Update blood pressure
        document.getElementById("bloodPressure").textContent =
            `${latest.systolic}/${latest.diastolic}`;


        // Update heart rate
        document.getElementById("heartRate").textContent =
            latest.heartRate;


        // Update AI classification
        const analysis = latest.aiAnalysis;

        if (analysis) {

            document.getElementById("classification").textContent =
                analysis.classification;

            renderProbabilities(analysis.probabilities);

            document.getElementById("analysisDate").textContent =
                "Analysis available";

        } else {

            document.getElementById("classification").textContent =
                "Unavailable";

            document.getElementById("analysisDate").textContent =
                "Analysis not available";

            document.getElementById("probabilities").textContent =
                "No AI analysis is available for this measurement.";

        }


        // Fetch history and update charts
        await loadHistory(patientId);

        message.textContent = "Data loaded successfully!";

    } catch (error) {

        console.error(error);
        message.textContent = error.message;

    }

}


// ==========================================
// AI PROBABILITIES
// ==========================================

function renderProbabilities(probabilities) {

    const container = document.getElementById("probabilities");

    container.innerHTML = "";

    if (!probabilities) {

        container.textContent = "Probabilities unavailable.";
        return;

    }

    for (const [name, value] of Object.entries(probabilities)) {

        const percentage = (value * 100).toFixed(1);

        const probability = document.createElement("div");
        probability.className = "probability";

        const label = document.createElement("div");
        label.className = "probability-label";

        const nameElement = document.createElement("strong");
        nameElement.textContent = name;

        const percentageElement = document.createElement("span");
        percentageElement.textContent = `${percentage}%`;

        label.appendChild(nameElement);
        label.appendChild(percentageElement);

        const progress = document.createElement("div");
        progress.className = "progress";

        const progressBar = document.createElement("div");
        progressBar.className = "progress-bar";
        progressBar.style.width = `${percentage}%`;

        progress.appendChild(progressBar);

        probability.appendChild(label);
        probability.appendChild(progress);

        container.appendChild(probability);

    }

}


// ==========================================
// MEASUREMENT HISTORY
// ==========================================

async function loadHistory(patientId) {

    const response = await fetch(
        `${API_URL}/api/Measurements/patient/${patientId}`
    );

    if (!response.ok) {

        throw new Error("Error retrieving measurement history.");

    }

    const measurements = await response.json();

    const tbody = document.getElementById("history");

    tbody.innerHTML = "";


    if (measurements.length === 0) {

        const row = document.createElement("tr");
        const cell = document.createElement("td");

        cell.colSpan = 5;
        cell.textContent = "No measurements found.";

        row.appendChild(cell);
        tbody.appendChild(row);

        updateCharts([]);

        return;

    }


    // Sort measurements from newest to oldest
    const orderedMeasurements = [...measurements].sort(
        (a, b) => new Date(b.timestamp) - new Date(a.timestamp)
    );


    // Populate the table
    orderedMeasurements.forEach(item => {

        const date = new Date(item.timestamp).toLocaleString("en-US");

        const classification = item.aiAnalysis
            ? item.aiAnalysis.classification
            : "No analysis";

        const row = document.createElement("tr");

        const values = [
            date,
            item.systolic,
            item.diastolic,
            item.heartRate,
            classification
        ];

        values.forEach(value => {

            const cell = document.createElement("td");

            cell.textContent = value;

            row.appendChild(cell);

        });

        tbody.appendChild(row);

    });


    // Select the 10 most recent measurements
    // and reverse them to display in chronological order
    const recentMeasurements = orderedMeasurements
        .slice(0, 10)
        .reverse();


    // Update charts
    updateCharts(recentMeasurements);

}


// ==========================================
// CHART UPDATES
// ==========================================

function updateCharts(measurements) {

    const labels = measurements.map(item => {

        return new Date(item.timestamp).toLocaleString("en-US", {
            day: "2-digit",
            month: "2-digit",
            hour: "2-digit",
            minute: "2-digit"
        });

    });

    updateBloodPressureChart(measurements, labels);

    updateHeartRateChart(measurements, labels);

}


// ==========================================
// BLOOD PRESSURE CHART
// ==========================================

function updateBloodPressureChart(measurements, labels) {

    const canvas = document.getElementById("bloodPressureChart");

    if (!canvas || typeof Chart === "undefined") {

        console.error("Chart.js or chart element not found.");

        return;

    }


    // Prevent duplicate charts
    if (bloodPressureChart) {

        bloodPressureChart.destroy();

    }


    bloodPressureChart = new Chart(canvas, {

        type: "line",

        data: {

            labels: labels,

            datasets: [

                {
                    label: "Systolic Blood Pressure",

                    data: measurements.map(item => item.systolic),

                    borderColor: "#2563eb",

                    backgroundColor: "rgba(37, 99, 235, 0.1)",

                    tension: 0.3
                },

                {
                    label: "Diastolic Blood Pressure",

                    data: measurements.map(item => item.diastolic),

                    borderColor: "#16a34a",

                    backgroundColor: "rgba(22, 163, 74, 0.1)",

                    tension: 0.3
                }

            ]

        },

        options: {

            responsive: true,

            maintainAspectRatio: false,

            plugins: {

                legend: {
                    position: "top"
                }

            },

            scales: {

                y: {

                    beginAtZero: false,

                    title: {
                        display: true,
                        text: "Blood Pressure (mmHg)"
                    }

                },

                x: {

                    title: {
                        display: true,
                        text: "Date and Time"
                    }

                }

            }

        }

    });

}


// ==========================================
// HEART RATE CHART
// ==========================================

function updateHeartRateChart(measurements, labels) {

    const canvas = document.getElementById("heartRateChart");

    if (!canvas || typeof Chart === "undefined") {

        console.error("Chart.js or chart element not found.");

        return;

    }


    // Prevent duplicate charts
    if (heartRateChart) {

        heartRateChart.destroy();

    }


    heartRateChart = new Chart(canvas, {

        type: "line",

        data: {

            labels: labels,

            datasets: [

                {
                    label: "Heart Rate",

                    data: measurements.map(item => item.heartRate),

                    borderColor: "#dc2626",

                    backgroundColor: "rgba(220, 38, 38, 0.1)",

                    tension: 0.3,

                    fill: true
                }

            ]

        },

        options: {

            responsive: true,

            maintainAspectRatio: false,

            plugins: {

                legend: {
                    position: "top"
                }

            },

            scales: {

                y: {

                    beginAtZero: false,

                    title: {
                        display: true,
                        text: "Heart Rate (bpm)"
                    }

                },

                x: {

                    title: {
                        display: true,
                        text: "Date and Time"
                    }

                }

            }

        }

    });

}


// ==========================================
// INITIALIZATION
// ==========================================

document.addEventListener("DOMContentLoaded", function () {

    loadDashboard();

});
