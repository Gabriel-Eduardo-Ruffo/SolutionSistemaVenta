/*
javascript para HistorialVenta.cshtml de Venta en: carpeta ---> Views ---> Venta ---> HistorialVenta.cshtml

HistorialVenta
HistorialVenta.cshtml es a quien pertenece este script
Historial_Venta es a que vista (View) pertenece

SistemaVenta.AplicacionWeb
wwwroot        ---> js ---> vistas ---> HistorialVenta.js
Views          --->          Venta ---> HistorialVenta.cshtml
Controllers                        ---> VentaController.cs
Models         --->     ViewModels ---> VMVenta.cs

SistemaVenta.BLL
Implementacion                     ---> VentaService.cs

SistemaVenta.Entity
                                   ---> Venta.cs
*/

const VISTA_BUSQUEDA = {
    busquedaFecha: () => {
        $("#txtFechaInicio").val("");
        $("#txtFechaFin").val("");
        $("#txtNumeroVenta").val("");

        $(".busqueda-fecha").show();
        $(".busqueda-venta").hide();
    },
    busquedaVenta: () => {
        $("#txtFechaInicio").val("");
        $("#txtFechaFin").val("");
        $("#txtNumeroVenta").val("");

        $(".busqueda-fecha").hide();
        $(".busqueda-venta").show();
    }
}

$(document).ready(function () {
    VISTA_BUSQUEDA["busquedaFecha"]();//Del objeto VISTA_BUSQUEDA ejecuta la propiedad ["busquedaFecha"] que es en si misma una funcion

    $.datepicker.setDefaults($.datepicker.regional["es"])

    $("#txtFechaInicio").datepicker({dateFormat: "dd/mm/yy"});
    $("#txtFechaFin").datepicker({ dateFormat: "dd/mm/yy" });;
})

$("#cboBuscarPor").change(function () {
    if ($("#cboBuscarPor").val() == "fecha")
    {
        VISTA_BUSQUEDA["busquedaFecha"]();
    }
    else
    {
        VISTA_BUSQUEDA["busquedaVenta"]();
    }
})

$("#btnBuscar").click(function (){
    if ($("#cboBuscarPor").val() == "fecha") {
        if ($("#txtFechaInicio").val().trim() == "" || $("#txtFechaFin").val().trim() == "") {
            toastr.warning("", "Debe ingresar fecha de inicio y de fin");
            return;
        }
    }
    else
    {
        if ($("#txtNumeroVenta").val().trim() == "") {
            toastr.warning("", "Debe ingresar un numero de venta");
            return;
        }
    }

    let numeroVenta = $("#txtNumeroVenta").val();
    let fechaInicio = $("#txtFechaInicio").val();
    let fechaFin = $("#txtFechaFin").val();

    /* loadingoverlay libreria dentro de la carpeta vendor que muestra popup de carga */
    $(".card-body").find("div.row").LoadingOverlay("show");

    fetch(`/Venta/Historial?numeroVenta=${numeroVenta}&fechaInicio=${fechaInicio}&fechaFin=${fechaFin}`)
    .then(response => {
        $(".card-body").find("div.row").LoadingOverlay("hide");
        return response.ok ? response.json() : Promise.reject(response);
    })
    .then(responseJson => {
        $("#tbventa tbody").html("");

        if (responseJson.length > 0)
        {
            responseJson.forEach((venta) => {
                $("#tbventa tbody").append(
                    $("<tr>").append(
                        $("<td>").text(venta.fechaRegistro.replace("T"," ")),
                        $("<td>").text(venta.numeroVenta),
                        $("<td>").text(venta.tipoDocumentoVenta),
                        $("<td>").text(venta.documentoCliente),
                        $("<td>").text(venta.nombreCliente),
                        $("<td>").text(venta.totalVenta),
                        $("<td>").append(
                            $("<button>").addClass("btn btn-info btn-sm").append(
                                $("<i>").addClass("fas fa-eye")
                            ).data("venta",venta)
                        )
                    )
                )
            });
        }
    })
})

/* cada ves que se haga click en el boton que esta dentro del detalle de venta (que este dentro del div contenedor y el tbody y contenga la clase btnInfo)*/
$("#tbventa tbody").on("click", ".btn-info", function() {
    let datosDeLaVenta = $(this).data("venta");

    $("#txtFechaRegistro").val(datosDeLaVenta.fechaRegistro.replace("T", " "));
    $("#txtNumVenta").val(datosDeLaVenta.numeroVenta);
    $("#txtUsuarioRegistro").val(datosDeLaVenta.usuario);
    $("#txtTipoDocumento").val(datosDeLaVenta.tipoDocumentoVenta);
    $("#txtDocumentoCliente").val(datosDeLaVenta.documentoCliente);
    $("#txtNombreCliente").val(datosDeLaVenta.nombreCliente);
    $("#txtSubTotal").val(datosDeLaVenta.subTotal);
    $("#txtIGV").val(datosDeLaVenta.impuestoTotal);
    $("#txtTotal").val(datosDeLaVenta.total);

    $("#tbProductos tbody").html("");

    datosDeLaVenta.detalleVenta.forEach((item) => {
        $("#tbProductos tbody").append(
            $("<tr>").append(
                $("<td>").text(item.descripcionProducto),
                $("<td>").text(item.cantidad),
                $("<td>").text(item.precio),
                $("<td>").text(item.total)
            )
        )
    })

    $("#modalData").modal("show");
})