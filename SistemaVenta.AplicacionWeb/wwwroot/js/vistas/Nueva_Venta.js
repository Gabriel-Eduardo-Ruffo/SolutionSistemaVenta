/*
javascript para NuevaVenta.cshtml de Venta en: carpeta ---> Views ---> Venta ---> NuevaVenta.cshtml

NuevaVenta
NuevaVenta.cshtml es a quien pertenece este script
Nueva_Venta es a que vista (View) pertenece

SistemaVenta.AplicacionWeb
wwwroot        ---> js ---> vistas ---> NuevaVenta.js
Views          --->          Venta ---> NuevaVenta.cshtml
Controllers                        ---> VentaController.cs
Models         --->     ViewModels ---> VMVenta.cs

SistemaVenta.BLL
Implementacion                     ---> VentaService.cs

SistemaVenta.Entity
                                   ---> Venta.cs
*/

let valorImpuesto = 0;
$(document).ready(function ()
{
    /*consulta para rellenar el desplegable de tipos de documentos de ventas hay */
    fetch("/Venta/ListaTipoDocumentoVenta")
    .then(response => {
        return response.ok ? response.json() : Promise.reject(response);
    })
    .then(responseJson => {
        if (responseJson.length > 0) {
            responseJson.forEach((item) => {
                $("#cboTipoDocumentoVenta").append(
                    $("<option>").val(item.idTipoDocumentoVenta).text(item.descripcion)
                )
            })
        }
    })

    /*consulta para rellenar los span de sub total, impuesto y total */
    fetch("/Negocio/Obtener")
    .then(response => {
        return response.ok ? response.json() : Promise.reject(response);
    })
    .then(responseJson => {
        if (responseJson.estado)
        {
            const descripcion = responseJson.objeto;
            //console.log(descripcion);
            $("#inputGroupSubTotal").text(`SubTotal-${descripcion.simboloMoneda}`)
            $("#inputGroupIGV").text(`IGV(${descripcion.porcentajeImpuesto}%)-${descripcion.simboloMoneda}`)
            $("#inputGroupTotal").text(`Total-${descripcion.simboloMoneda}`)

            valorImpuesto = parseFloat(descripcion.porcentajeImpuesto);
        }
    })

    /* Desplegable de busqueda de productos en la DB que muestra posible producto a medida que se escribe algo en el input*/
    $("#cboBuscarProducto").select2({
        ajax: {
            url: "/Venta/ObtenerProductos",
            dataType: 'json',
            contentType:"application/json; charset=utf-8",
            delay: 250,
            data: function (params) { /* params es el parametro de busqueda => la palabra que ingresamos en el inputbox de la pagina*/
                return {
                    busqueda: params.term /* la palabra busqueda es la del parametro de entrada del metodo -> public async Task<IActionResult> ObtenerProducto(string busqueda) */
                };
            },
            processResults: function (data) {

                return {
                    results: data.map((elemento) => (
                        {
                            id: elemento.idProducto,
                            text: elemento.descripcion,

                            marca: elemento.marca,
                            categoria: elemento.nombreCategoria,
                            urlImagen: elemento.urlImagen,
                            precio: parseFloat(elemento.precio)
                        }
                    ))
                };
            },
        },
        language: "es",
        placeholder: 'Buscar producto',
        minimumInputLength: 1,
        templateResult: FormatoResultados /* usa la funcion -> function formatoResultados(data)*/
    });
});

//funcion que devuelve el formato en que se va a mostrar la informacion que se encuentre con Select2 (desplegable para buscar productos a vender)
function FormatoResultados(data)
{
    if (data.loading)
    {
        return data.text;
    }

    var contenedor = $(
        `<table width="100%">
            <tr>
                <td style="width:60px">
                    <img style="height:60px;margin-right:10px" src="${data.urlImagen}"/>
                </td>
                <td>
                    <p style="font-weight: bolder;margin:2px">${data.marca}</p>
                    <p style="margin:2px">${data.text}</p>
                </td>
            </tr>
        </table>`
    );
    return contenedor;
}

$(document).on("select2:open", function ()
{
    document.querySelector(".select2-search__field").focus();
});

let productosParaVenta = [];
$("#cboBuscarProducto").on("select2:select", function (e) {
    const data = e.params.data;/* Se obtiene toda la informacion del producto seleccionado en el input Select2 (busqueda de productos */

    let productoEncontrado = productosParaVenta.filter(producto => producto.idProducto == data.id);

    if (productoEncontrado.length > 0)
    {
        $("#cboBuscarProducto").val("").trigger("change");
        toastr.warning("", "El producto ya fue agregado");
        return false;
    }

    /* modal para escribir la cantidad de productos vendidos */
    swal({
        title: data.marca,
        text: data.text,
        imageUrl: data.urlImagen,
        type:"input",
        showCancelButton: true,
        closeOnConfirm: false,
        inputPlaceHolder: "Ingrese la cantidad"
    },
        function (valor)
        {
            if (valor === false)
            {
                return false;
            }

            if (valor === "")
            {
                toastr.warning("", "Se necesita ingresar la cantidad");
                return false;
            }

            if (isNaN(parseInt(valor)))
            {
                toastr.warning("", "Debe ingresar un valor numerico");
                return false;
            }

            let producto =
            {
                idProducto: data.id,
                marcaProducto: data.marca,
                descripcionProducto: data.text,
                categoriaProducto: data.categoria,
                cantidad: parseInt(valor),
                precio: data.precio.toString(),
                total: (parseFloat(valor) * data.precio).toString()
            }

            productosParaVenta.push(producto);

            MostrarProductosPrecios();
            $("#cboBuscarProducto").val("").trigger("change");
            swal.close();
        }
    );
})

/* Agrega un producto con sus caracteristicas de la venta en el listado de productos a vender */
function MostrarProductosPrecios()
{
    let total = 0;
    let igv = 0;
    let subtotal = 0;
    let porcentaje = valorImpuesto / 100;

    $("#tbProducto tbody").html("");
    productosParaVenta.forEach((item) => {
        total = total + parseFloat(item.total);
        $("#tbProducto tbody").append(
            $("<tr>").append(
                $("<td>").append(
                    $("<button>").addClass("btn btn-danger btn-eliminar btn-sm").append(
                        $("<i>").addClass("fas fa-trash-alt")
                    ).data("idProducto",item.idProducto)
                ),
                $("<td>").text(item.descripcionProducto),
                $("<td>").text(item.cantidad),
                $("<td>").text(item.precio),
                $("<td>").text(item.total),
            )
        );
    })

    subtotal = total / (1 + porcentaje);
    igv = total - subtotal;

    $("#txtSubTotal").val(subtotal.toFixed(2));
    $("#txtIGV").val(igv.toFixed(2));
    $("#txtTotal").val(total.toFixed(2));
}

$(document).on("click", "button.btn-eliminar", function ()
{
    const idProductoSeleccionado = $(this).data("idProducto");/*saber que producto se selecciono por medio de su id */

    productosParaVenta = productosParaVenta.filter(producto => producto.idProducto != idProductoSeleccionado);

    MostrarProductosPrecios();
})

$("#btnTerminarVenta").click(function()
{
    if(productosParaVenta.length < 1)
    {
        toastr.warning("", "Debe ingresar uno o mas productos");
        return;
    }

    const vmDetalleVenta = productosParaVenta; /* tiene la misma estructura el ViewModel vmDetalleVenta que la lista de objetos productosParaVenta */
    const venta = {
        idTipoDocumentoVenta: $("#cboTipoDocumentoVenta").val(),
        documentoCliente: $("#txtDocumentoCliente").val(),
        nombreCliente: $("#txtNombreCliente").val(),
        subTotal: $("#txtSubTotal").val(),
        impuestoTotal: $("#txtIGV").val(),
        totalVenta: $("#txtTotal").val(),
        detalleVenta: vmDetalleVenta
    }


    /* loadingoverlay libreria dentro de la carpeta vendor que muestra popup de carga */
    $("#btnTerminarVenta").LoadingOverlay("show");

    /*envio de informacion para registrar la venta en la DB */
    fetch("/Venta/RegistrarVenta", {
        method: "POST",
        headers: { "Content-Type": "application/json; charset=utf-8" },
        body: JSON.stringify(venta)
    })
    .then(response => {
        $("#btnTerminarVenta").LoadingOverlay("hide");
        return response.ok ? response.json() : Promise.reject(response);
    })
    .then(responseJson => {
        if (responseJson.estado) {
            productosParaVenta = [];
            MostrarProductosPrecios();

            $("#txtDocumentoCliente").val("");
            $("#txtNombreCliente").val("");
            $("#cboTipoDocumentoVenta").val($("#cboTipoDocumentoVenta option:first").val());

            swal("Registrado!", `Numero de venta: ${responseJson.objeto.numeroVenta}`, "success");
        }
        else
        {
            swal("Lo sentimos", "No se pudo registrar la venta", "error");
        }
    })
})