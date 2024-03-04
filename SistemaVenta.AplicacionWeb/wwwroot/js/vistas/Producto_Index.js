/* 
javascript para el index de Producto en: carpeta ---> Views ---> Producto ---> index.cshtml

Producto_Index
Producto es a quien pertenece este script
Index es a que vista (View) pertenece

SistemaVenta.AplicacionWeb
wwwroot        ---> js ---> vistas ---> Producto_Index.js
Views          --->       Producto ---> Index.cshtml
Controllers                        ---> ProductoController.cs
Models         --->     ViewModels ---> VMProducto.cs

SistemaVenta.BLL
Implementacion                     ---> ProductoService.cs

SistemaVenta.Entity
                                   ---> Producto.cs
*/

const MODELO_BASE =
{
    idProducto: 0,
    codigoBarra: "",
    marca: "",
    nombre: "",
    idCategoria: 0,
    stock: 0,
    urlImagen: "",
    precio: 0,
    esActivo: "1"
}

let tablaData;

$(document).ready(function ()
{
    /*carga la lista de categorias para mostrarlo dentro del combo box desplegable */
    fetch("/Categoria/Lista")
    .then(response => {
        return response.ok ? response.json() : Promise.reject(response);
    })
    .then(responseJson => {
        if (responseJson.data.length > 0) {
            responseJson.data.forEach((item) => {
                $("#cboCategoria").append(
                    $("<option>").val(item.idCategoria).text(item.descripcion)
                )
            })
        }
    })

    tablaData = $('#tbdata').DataTable({/* se esta trabajando con data tables ( se rellenan las filas en orden con la informacion que viene de Backend y se agregan botones, imagenes e informacion por cada fila */
        responsive: true,
        "ajax": {
            "url": '/Producto/lista',
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "idProducto", "visible": false, "searchable": false },
            {
                "data": "urlImagen", render: function (data) {
                    return `<img style="height:60px" src=${data} class="rounded mx-auto d-block"/>`;
                }
            },
            { "data": "codigoBarra" },
            { "data": "marca" },
            { "data": "descripcion" },
            { "data": "nombreCategoria" },
            { "data": "stock" },
            { "data": "precio" },
            {
                "data": "esActivo", render: function (data) {
                    if (data == 1)
                        return '<span class="badge badge-info">Activo</span>';
                    else
                        return '<span class="badge badge-danger">No Activo</span>';
                }
            },
            {
                "defaultContent": '<button class="btn btn-primary btn-editar btn-sm mr-2"><i class="fas fa-pencil-alt"></i></button>' +
                    '<button class="btn btn-danger btn-eliminar btn-sm"><i class="fas fa-trash-alt"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "80px"
            }
        ],
        order: [[0, "desc"]],
        dom: "Bfrtip",
        buttons: [/* boton para exportar la info a un excel */
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Productos',
                exportOptions: {
                    columns: [2, 3, 4, 5, 6] /*las columnas que vienen en data (idProducto = 0, codigoBarra= 1, marca = 2, descripcion = 3, idCategoria = 4, nombreCategoria = 5, stock = 6, urlImagen = 7, precio = 8 , esActivo = 9 ) */
                }
            }, 'pageLength'
        ],
        language: {/* link para pasar todo a español */
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        },
    });
})


/*
recibe como parametro un modelo
Rellena con los datos recibidos de modelo, cada id correpondiente en index.cshtml y la muestra
 */
function MostrarModal(modelo = MODELO_BASE)
{
    $("#txtId").val(modelo.idProducto);
    $("#txtCodigoBarra").val(modelo.codigoBarra);
    $("#txtMarca").val(modelo.marca);
    $("#txtDescripcion").val(modelo.descripcion);
    $("#cboCategoria").val(modelo.idRol == 0 ? $("#cboCategoria option:first").val() : modelo.idCategoria);
    $("#txtStock").val(modelo.stock);
    $("#txtPrecio").val(modelo.precio);
    $("#cboEstado").val(modelo.esActivo);
    $("#txtImagen").val("");
    $("#imgProducto").attr("src", modelo.urlImagen);

    $("#modalData").modal("show");
}

//-----clicks en botones (llamado a eventos)----
/*Boton nuevo */
$("#btnNuevo").click(function ()
{
    MostrarModal();
})

/*Boton Guardar */
$("#btnGuardar").click(function () {
    const inputs = $("input.input-validar").serializeArray();
    const inputs_sin_valor = inputs.filter((item => item.value.trim() == ""));

    if (inputs_sin_valor.length > 0)
    {
        const mensaje = `Debe completar el campo: "${inputs_sin_valor[0].name}"`;
        /*toastr libreria dentro de la carpeta vendor, para mostrar mensajes en el front*/
        toastr.warning("", mensaje);
        $(`input[name="${inputs_sin_valor[0].name}"]`).focus();
        return;
    }

    const modelo = structuredClone(MODELO_BASE);
    modelo["idProducto"] = parseInt($("#txtId").val());
    modelo["codigoBarra"] = $("#txtCodigoBarra").val();
    modelo["marca"] = $("#txtMarca").val();
    modelo["descripcion"] = $("#txtDescripcion").val();
    modelo["idCategoria"] = $("#cboCategoria").val();
    modelo["stock"] = $("#txtStock").val();
    modelo["precio"] = $("#txtPrecio").val();
    modelo["esActivo"] = $("#cboEstado").val();

    const inputImagen = document.getElementById("txtImagen");

    /* este formData es el que se envia al controller UsuarioController al metodo Crear([FromForm] IFormFile foto, [FromForm] string modelo) */
    const formData = new FormData();
    formData.append("imagen", inputImagen.files[0]);
    formData.append("modelo", JSON.stringify(modelo));

    /* loadingoverlay libreria dentro de la carpeta vendor que muestra popup de carga */
    $("#modalData").find("div.modal-content").LoadingOverlay("show");

    if (modelo.idProducto == 0)/*si es 0 es para crear un usuario nuevo */
    {
        fetch("/Producto/Crear", {
            method: "POST",
            body: formData
        })
        .then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {
            if (responseJson.estado)
            {
                tablaData.row.add(responseJson.objeto).draw(false);
                $("#modalData").modal("hide");
                swal("Listo!", "El producto fue creado", "success");
            }
            else {
                swal("Lo sentimos", responseJson.mensaje, "error");
            }
        })
    }
    else
    {
        fetch("/Producto/Editar", {
            method: "PUT",
            body: formData
        })
        .then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {
            if (responseJson.estado)
            {
                tablaData.row(filaSeleccionada).data(responseJson.objeto).draw(false);
                filaSeleccionada = null;
                $("#modalData").modal("hide");
                swal("Listo!", "El producto fue modificado", "success");
            }
            else
            {
                swal("Lo sentimos", responseJson.mensaje, "error");
            }
        })
    }
})

/*Boton Editar */
let filaSeleccionada;
$("#tbdata tbody").on("click", ".btn-editar", function () {
    /* validamos cuando es responsivo y cuando no por medio de saber si el tag seleccionado tiene tag un child -> (responsivo) o no */
    if ($(this).closest("tr").hasClass("child"))
    {
        filaSeleccionada = $(this).closest("tr").prev();
    }
    else
    {
        filaSeleccionada = $(this).closest("tr");
    }

    const data = tablaData.row(filaSeleccionada).data();
    MostrarModal(data);
})

/*Boton Eliminar */
$("#tbdata tbody").on("click", ".btn-eliminar", function () {
    let fila;

    /* validamos cuando es responsivo y cuando no por medio de saber si el tag seleccionado tiene tag un child -> (responsivo) o no */
    if ($(this).closest("tr").hasClass("child"))
    {
        fila = $(this).closest("tr").prev();
    }
    else
    {
        fila = $(this).closest("tr");
    }

    const data = tablaData.row(fila).data();
    /* modal de confirmacion para eliminar usuario */
    swal({
        title: "Esta seguro?",
        text: `Eliminar el producto "${data.decripcion}"`,
        type: "warning",
        showCancelButton: true,
        confirmButtonClass: "btn-danger",
        confirmButtonText: "Si, eliminar",
        cancelButtonText: "No, volver",
        closeOnConfirm: false,
        closeOnCancel: true
    },
        function (respuesta) {
            if (respuesta)
            {
                $(".showSweetAlert").LoadingOverlay("show");

                fetch(`/Producto/Eliminar?IdProducto=${data.idProducto}`, {
                    method: "DELETE"
                })
                .then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide");
                    return response.ok ? response.json() : Promise.reject(response);
                })
                .then(responseJson => {
                    if (responseJson.estado) {
                        tablaData.row(fila).remove().draw();
                        swal("Listo!", "El producto fue eliminado", "success");
                    }
                    else {
                        swal("Lo sentimos", responseJson.mensaje, "error");
                    }
                })
            }
        }
    );
})