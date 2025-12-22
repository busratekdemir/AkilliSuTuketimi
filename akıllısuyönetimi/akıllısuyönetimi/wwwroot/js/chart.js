// wwwroot/js/chart-setup.js

(function () { // Kendiliğinden çalışan anonim fonksiyon (IIFE) ile kodu hemen başlatıyoruz

    // 1. Grafiğin çizileceği HTML Canvas elementini bulun.
    // Index.cshtml dosyasında bu ID'yi kullanmıştık.
    const ctx = document.getElementById('consumptionChart');

    if (ctx) {

        // 2. Mock (Örnek) Veri Setini Tanımlayın
        const consumptionData = {
            labels: ['01.03', '02.03', '03.03', '04.03', '05.03', '06.03', '07.03'],
            datasets: [
                {
                    label: 'Gerçek Tüketim',
                    data: [45000, 44000, 46000, 45500, 44500, 46500, 47000],
                    borderColor: 'rgb(75, 192, 192)', // Mavi/Turkuaz çizgi
                    backgroundColor: 'rgba(75, 192, 192, 0.2)', // Alan dolgusu
                    tension: 0.4, // Çizgiyi yumuşatır
                    fill: true, // Çizginin altını doldurur
                    pointRadius: 3, // Noktaları biraz görünür yaptık
                    pointHoverRadius: 6,
                },
                {
                    label: 'Tahmin Edilen',
                    data: [43500, 44500, 45500, 46000, 45000, 46000, 47500],
                    borderColor: 'rgb(255, 159, 64)', // Turuncu çizgi
                    backgroundColor: 'transparent',
                    borderDash: [5, 5], // Kesikli çizgi
                    tension: 0.4,
                    pointRadius: 3,
                    pointHoverRadius: 6,
                }
            ]
        };

        // 3. Grafik Seçeneklerini ve Yapısını Tanımlayın
        new Chart(ctx, {
            type: 'line', // Çizgi grafik tipi
            data: consumptionData,
            options: {
                responsive: true,
                maintainAspectRatio: false, // Yüksekliği kontrol etmeyi sağlar
                plugins: {
                    legend: {
                        display: true,
                        position: 'top',
                    },
                    tooltip: {
                        mode: 'index',
                        intersect: false,
                    }
                },
                scales: {
                    x: {
                        display: true,
                        // X ekseni başlığını kapalı tuttuk
                        title: {
                            display: false,
                        }
                    },
                    y: {
                        display: true,
                        title: {
                            display: true,
                            text: 'Tüketim (m³)'
                        },
                        beginAtZero: false,
                        // Y ekseni limitleri
                        min: 30000,
                        max: 60000,
                        ticks: {
                            stepSize: 15000
                        }
                    }
                }
            }
        });
    }

})(); // Kodu hemen çalıştırır.