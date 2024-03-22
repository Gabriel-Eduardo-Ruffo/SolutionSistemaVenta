/*
javascript para el index de DashBoard en: carpeta ---> Views ---> DashBoard ---> index.cshtml

DashBoard_Index
DashBoard es a quien pertenece este script
Index es a que vista (View) pertenece

SistemaVenta.AplicacionWeb
wwwroot        ---> js ---> vistas ---> DashBoard_Index.js
Views          --->      DashBoard ---> Index.cshtml
Controllers                        ---> DashBoardController.cs
Models         --->     ViewModels ---> VMDashBoard.cs

SistemaVenta.BLL
Implementacion                     ---> DashBoardService.cs

*/

$(document).ready(function () {

    /* loadingoverlay libreria dentro de la carpeta vendor que muestra popup de carga */
    $("div.container-fluid").LoadingOverlay("show");

    fetch("/DashBoard/ObtenerResumen")
        .then(response => {
            $("div.container-fluid").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {
            if (responseJson.estado)
            {
                //Datos para las tarjetas
                let d = responseJson.objeto;

                $("#totalVenta").text(d.totalVentas);
                $("#totalIngresos").text(d.totalIngresos);
                $("#totalProductos").text(d.totalProductos);
                $("#totalCategorias").text(d.totalCategorias);

                //textos y valores para grafico de barras
                let barChart_labels;
                let barChart_data;

                if (d.ventasUltimaSemana.length > 0)
                {
                    barChart_labels = d.ventasUltimaSemana.map((item) => { return item.fecha });
                    barChart_data = d.ventasUltimaSemana.map((item) => { return item.total });
                }
                else
                {
                    barChart_labels = ["Sin resultados"];
                    barChart_labels = [0];
                }

                //textos y valores para grafico de torta
                let tortaChart_labels;
                let tortaChart_data;

                if (d.productosTopUltimaSemana.length > 0)
                {
                    tortaChart_labels = d.productosTopUltimaSemana.map((item) => { return item.producto });
                    tortaChart_data = d.productosTopUltimaSemana.map((item) => { return item.cantidad });
                }
                else
                {
                    tortaChart_labels = ["Sin resultados"];
                    tortaChart_data = [0];
                }

                //libreria Chart --> para generar los graficos
                // Bar Chart
                let controlVenta = document.getElementById("chartVentas");
                let barChart = new Chart(controlVenta, {
                    type: 'bar',
                    data: {
                        labels: barChart_labels,
                        datasets: [{
                            label: "Cantidad",
                            backgroundColor: "#4e73df",
                            hoverBackgroundColor: "#2e59d9",
                            borderColor: "#4e73df",
                            data: barChart_data,
                        }],
                    },
                    options: {
                        maintainAspectRatio: false,
                        legend: {
                            display: false
                        },
                        scales: {
                            xAxes: [{
                                gridLines: {
                                    display: false,
                                    drawBorder: false
                                },
                                maxBarThickness: 50,
                            }],
                            yAxes: [{
                                ticks: {
                                    min: 0,
                                    maxTicksLimit: 5
                                }
                            }],
                        },
                    }
                });

                // Torta Chart
                let controlProducto = document.getElementById("chartProductos");
                let tortaChart = new Chart(controlProducto, {
                    type: 'doughnut',
                    data: {
                        labels: tortaChart_labels,
                        datasets: [{
                            data: tortaChart_data,
                            backgroundColor: ['#4e73df', '#1cc88a', '#36b9cc', "#FF785B"],
                            hoverBackgroundColor: ['#2e59d9', '#17a673', '#2c9faf', "#FF5733"],
                            hoverBorderColor: "rgba(234, 236, 244, 1)",
                        }],
                    },
                    options: {
                        maintainAspectRatio: false,
                        tooltips: {
                            backgroundColor: "rgb(255,255,255)",
                            bodyFontColor: "#858796",
                            borderColor: '#dddfeb',
                            borderWidth: 1,
                            xPadding: 15,
                            yPadding: 15,
                            displayColors: false,
                            caretPadding: 10,
                        },
                        legend: {
                            display: true
                        },
                        cutoutPercentage: 80,
                    },
                });
            }
        })
});