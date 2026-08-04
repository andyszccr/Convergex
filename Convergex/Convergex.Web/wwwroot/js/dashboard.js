(() => {
    if (typeof Chart === "undefined") {
        return;
    }

    const lineCanvas = document.getElementById("conversionsChart");
    if (lineCanvas) {
        const labels = JSON.parse(lineCanvas.dataset.labels || "[]");
        const values = JSON.parse(lineCanvas.dataset.values || "[]");

        new Chart(lineCanvas, {
            type: "line",
            data: {
                labels,
                datasets: [
                    {
                        label: "Conversiones",
                        data: values,
                        borderColor: "#2563EB",
                        backgroundColor: "rgba(37, 99, 235, 0.10)",
                        fill: true,
                        tension: 0.35,
                        pointRadius: 4,
                        pointBackgroundColor: "#2563EB",
                        borderWidth: 2.5
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: { precision: 0, color: "#94A3B8" },
                        grid: { color: "rgba(226, 232, 240, 0.9)" },
                        border: { display: false }
                    },
                    x: {
                        ticks: { color: "#94A3B8" },
                        grid: { display: false },
                        border: { display: false }
                    }
                }
            }
        });
    }

    const donutCanvas = document.getElementById("typesChart");
    if (donutCanvas) {
        const labels = JSON.parse(donutCanvas.dataset.labels || "[]");
        const values = JSON.parse(donutCanvas.dataset.values || "[]");

        new Chart(donutCanvas, {
            type: "doughnut",
            data: {
                labels,
                datasets: [
                    {
                        data: values,
                        backgroundColor: ["#2563EB", "#10B981"],
                        borderWidth: 0,
                        hoverOffset: 4
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: "72%",
                plugins: { legend: { display: false } }
            }
        });
    }
})();
