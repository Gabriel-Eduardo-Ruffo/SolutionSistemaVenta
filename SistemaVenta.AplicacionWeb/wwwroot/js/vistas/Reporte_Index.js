/*
javascript para Index.cshtml de Venta en: carpeta ---> Views ---> Reporte ---> Index.cshtml

Reporte
Index.cshtml es a quien pertenece este script
Reporte_Index es a que vista (View) pertenece

SistemaVenta.AplicacionWeb
wwwroot        ---> js ---> vistas ---> Reporte_Index.js
Views          --->          Venta ---> Index.cshtml
Controllers                        ---> ReporteController.cs
Models         --->     ViewModels ---> VMReporteVenta.cs

SistemaVenta.BLL
Implementacion                     ---> VentaService.cs

SistemaVenta.Entity
                                   ---> Venta.cs
*/

let tablaData;

$(document).ready(function () {
    $.datepicker.setDefaults($.datepicker.regional["es"])

    $("#txtFechaInicio").datepicker({ dateFormat: "dd/mm/yy" });
    $("#txtFechaFin").datepicker({ dateFormat: "dd/mm/yy" });

    tablaData = $('#tbdata').DataTable({/* se esta trabajando con data tables ( se rellenan las filas en orden con la informacion que viene de Backend y se agregan botones, imagenes e informacion por cada fila */
        responsive: true,
        "ajax": {
            "url": '/reporte/reporteventa?fechaInicio=01/01/1991&fechaFin=01/01/1991',
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "fechaRegistro" },
            { "data": "numeroVenta" },
            { "data": "tipoDocumento" },
            { "data": "documentoCliente" },
            { "data": "nombreCliente" },
            { "data": "subTotalVenta" },
            { "data": "impuestoTotalVenta" },
            { "data": "totalVenta" },
            { "data": "producto" },
            { "data": "cantidad" },
            { "data": "precio" },
            { "data": "total" }
        ],
        order: [[0, "desc"]],
        dom: "Bfrtip",
        buttons: [/* boton para exportar la info a un excel */
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Ventas',
            }, 'pageLength'
        ],
        language: {/* link para pasar todo a español */
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        },
    });
})

//Logica para el boton buscar
$("#btnBuscar").click(function () {
    if ($("#txtFechaInicio").val().trim() == "" || $("#txtFechaFin").val().trim() == "") {
        toastr.warning("", "Debe ingresar fecha de inicio y de fin");
        return;
    }
    let fechaInicio = $("#txtFechaInicio").val().trim();
    let fechaFin = $("#txtFechaFin").val().trim();

    let nuevaURL = `/reporte/reporteventa?fechaInicio=${fechaInicio}&fechaFin=${fechaFin}`;

    tablaData.ajax.url(nuevaURL).load();//actualizamos la url del ajax con las fechas que seleccionamos. (originalmente la fecha de inicio y fin era 01/01/1991)
});