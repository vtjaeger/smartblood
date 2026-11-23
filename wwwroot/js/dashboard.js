
const API_URL = "";

let bloodPressureChart = null;
let heartRateChart = null;


// ==========================================
// CARREGAMENTO DO DASHBOARD
// ==========================================

async function loadDashboard() {

    const patientId = document.getElementById("patientId").value;
    const message = document.getElementById("message");

    if (!patientId || Number(patientId) <= 0) {
        message.textContent = "Informe um ID de paciente válido.";
        return;
    }

    message.textContent = "Carregando dados...";

    try {

        // Busca a última medição
        const latestResponse = await fetch(
            `${API_URL}/api/Measurements/patient/${patientId}/latest`
        );

        if (!latestResponse.ok) {

            throw new Error(
                latestResponse.status === 404
                    ? "Nenhuma medição encontrada para este paciente."
                    : "Erro ao consultar a última medição."
            );

        }

        const latest = await latestResponse.json();


        // Atualiza pressão arterial
        document.getElementById("bloodPressure").textContent =
            `${latest.systolic}/${latest.diastolic}`;


        // Atualiza frequência cardíaca
        document.getElementById("heartRate").textContent =
            latest.heartRate;


        // Atualiza classificação da IA
        const analysis = latest.aiAnalysis;

        if (analysis) {

            document.getElementById("classification").textContent =
                analysis.classification;

            renderProbabilities(analysis.probabilities);

            document.getElementById("analysisDate").textContent =
                "Análise disponível";

        } else {

            document.getElementById("classification").textContent =
                "Indisponível";

            document.getElementById("analysisDate").textContent =
                "Análise não disponível";

            document.getElementById("probabilities").textContent =
                "Não existe análise de IA para esta medição.";

        }


        // Busca histórico e atualiza gráficos
        await loadHistory(patientId);

        message.textContent = "Dados carregados com sucesso!";

    } catch (error) {

        console.error(error);
        message.textContent = error.message;

    }

}


// ==========================================
// PROBABILIDADES DA IA
// ==========================================

function renderProbabilities(probabilities) {

    const container = document.getElementById("probabilities");

    container.innerHTML = "";

    if (!probabilities) {

        container.textContent = "Probabilidades indisponíveis.";
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
// HISTÓRICO DE MEDIÇÕES
// ==========================================

async function loadHistory(patientId) {

    const response = await fetch(
        `${API_URL}/api/Measurements/patient/${patientId}`
    );

    if (!response.ok) {

        throw new Error("Erro ao consultar o histórico.");

    }

    const measurements = await response.json();

    const tbody = document.getElementById("history");

    tbody.innerHTML = "";


    if (measurements.length === 0) {

        const row = document.createElement("tr");
        const cell = document.createElement("td");

        cell.colSpan = 5;
        cell.textContent = "Nenhuma medição encontrada.";

        row.appendChild(cell);
        tbody.appendChild(row);

        updateCharts([]);

        return;

    }


    // Ordena da mais recente para a mais antiga
    const orderedMeasurements = [...measurements].sort(
        (a, b) => new Date(b.timestamp) - new Date(a.timestamp)
    );


    // Preenche a tabela
    orderedMeasurements.forEach(item => {

        const date = new Date(item.timestamp).toLocaleString("pt-BR");

        const classification = item.aiAnalysis
            ? item.aiAnalysis.classification
            : "Sem análise";

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


    // Seleciona as 10 medições mais recentes
    // e inverte para exibir em ordem cronológica
    const recentMeasurements = orderedMeasurements
        .slice(0, 10)
        .reverse();


    // Atualiza os gráficos
    updateCharts(recentMeasurements);

}


// ==========================================
// ATUALIZAÇÃO DOS GRÁFICOS
// ==========================================

function updateCharts(measurements) {

    const labels = measurements.map(item => {

        return new Date(item.timestamp).toLocaleString("pt-BR", {
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
// GRÁFICO DE PRESSÃO ARTERIAL
// ==========================================

function updateBloodPressureChart(measurements, labels) {

    const canvas = document.getElementById("bloodPressureChart");

    if (!canvas || typeof Chart === "undefined") {

        console.error("Chart.js ou elemento do gráfico não encontrado.");

        return;

    }


    // Evita duplicar gráficos
    if (bloodPressureChart) {

        bloodPressureChart.destroy();

    }


    bloodPressureChart = new Chart(canvas, {

        type: "line",

        data: {

            labels: labels,

            datasets: [

                {
                    label: "Pressão sistólica",

                    data: measurements.map(item => item.systolic),

                    borderColor: "#2563eb",

                    backgroundColor: "rgba(37, 99, 235, 0.1)",

                    tension: 0.3
                },

                {
                    label: "Pressão diastólica",

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
                        text: "Pressão (mmHg)"
                    }

                },

                x: {

                    title: {
                        display: true,
                        text: "Data e horário"
                    }

                }

            }

        }

    });

}


// ==========================================
// GRÁFICO DE FREQUÊNCIA CARDÍACA
// ==========================================

function updateHeartRateChart(measurements, labels) {

    const canvas = document.getElementById("heartRateChart");

    if (!canvas || typeof Chart === "undefined") {

        console.error("Chart.js ou elemento do gráfico não encontrado.");

        return;

    }


    // Evita duplicar gráficos
    if (heartRateChart) {

        heartRateChart.destroy();

    }


    heartRateChart = new Chart(canvas, {

        type: "line",

        data: {

            labels: labels,

            datasets: [

                {
                    label: "Frequência cardíaca",

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
                        text: "Frequência cardíaca (bpm)"
                    }

                },

                x: {

                    title: {
                        display: true,
                        text: "Data e horário"
                    }

                }

            }

        }

    });

}


// ==========================================
// INICIALIZAÇÃO
// ==========================================

document.addEventListener("DOMContentLoaded", function () {

    loadDashboard();

});
