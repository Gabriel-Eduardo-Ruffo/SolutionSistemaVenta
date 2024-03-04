/*
javascript para el index de Usuario en: carpeta ---> Views ---> Categoria ---> index.cshtml

Categoria_Index
Categoria es a quien pertenece este script
Index es a que vista (View) pertenece

SistemaVenta.AplicacionWeb
wwwroot        ---> js ---> vistas ---> Categoria_Index.js
Views          --->      Categoria ---> Index.cshtml
Controllers                        ---> CategoriaController.cs
Models         --->     ViewModels ---> VMCategoria.cs

SistemaVenta.BLL
Implementacion                     ---> CategoriaService.cs

SistemaVenta.Entity
                                   ---> Categoria.cs
*/

const MODELO_BASE = {
    idCategoria: 0,
    descripcion: "",
    esActivo: 1
}


let tablaData;

$(document).ready(function ()
{
    tablaData = $('#tbdata').DataTable({/* se esta trabajando con data tables ( se rellenan las filas en orden con la informacion que viene de Backend y se agregan botones, imagenes e informacion por cada fila */
        responsive: true,
        "ajax": {
            "url": '/Categoria/lista',
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "idCategoria", "visible": false, "searchable": false },
            { "data": "descripcion" },
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
                filename: 'Reporte Categorias',
                exportOptions: {
                    columns: [1, 2] /*las columnas que vienen en data (idCategoria = 0, descripcion= 1, esActivo = 2 ) */
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
function MostrarModal(modelo = MODELO_BASE) {
    $("#txtId").val(modelo.idCategoria);
    $("#txtDescripcion").val(modelo.descripcion);
    $("#cboEstado").val(modelo.esActivo);

    $("#modalData").modal("show");
}

//-----clicks en botones (llamado a eventos)----
/*Boton nuevo */
$("#btnNuevo").click(function () {
    MostrarModal();
})

/*Boton Guardar */
$("#btnGuardar").click(function ()
{
    if ($("#txtDescripcion").val().trim == "")
    {
        /*toastr libreria dentro de la carpeta vendor, para mostrar mensajes en el front*/
        toastr.warning("", "Debe completar el campo descripcion");
        $("#txtDescripcion").focus();
        return;
    }

    const modelo = structuredClone(MODELO_BASE);
    modelo["idCategoria"] = parseInt($("#txtId").val());
    modelo["descripcion"] = $("#txtDescripcion").val();
    modelo["esActivo"] = $("#cboEstado").val();

    /* loadingoverlay libreria dentro de la carpeta vendor que muestra popup de carga */
    $("#modalData").find("div.modal-content").LoadingOverlay("show");

    if (modelo.idCategoria == 0)/*si es 0 es para crear un usuario nuevo */
    {
        fetch("/Categoria/Crear", {
            method: "POST",
            headers: {"Content-Type": "application/json; charset=utf-8"},
            body: JSON.stringify(modelo)
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
                swal("Listo!", "La categoria fue creada", "success");
            }
            else
            {
                swal("Lo sentimos", responseJson.mensaje, "error");
            }
        })
    }
    else
    {
    fetch("/Categoria/Editar", {
        method: "PUT",
        headers: { "Content-Type": "application/json; charset=utf-8" },
        body: JSON.stringify(modelo)
    })
        .then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {
            if (responseJson.estado) {
                tablaData.row(filaSeleccionada).data(responseJson.objeto).draw(false);
                filaSeleccionada = null;
                $("#modalData").modal("hide");
                swal("Listo!", "La categoria fue modificada", "success");
            }
            else {
                swal("Lo sentimos", responseJson.mensaje, "error");
            }
        })
    }
})

/*Boton Editar */
let filaSeleccionada;
$("#tbdata tbody").on("click", ".btn-editar", function () {
    /* validamos cuando es responsivo y cuando no por medio de saber si el tag seleccionado tiene tag un child -> (responsivo) o no */
    if ($(this).closest("tr").hasClass("child")) {
        filaSeleccionada = $(this).closest("tr").prev();
    }
    else {
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
        text: `Eliminar la Categoria "${data.descripcion}"`,
        type: "warning",
        showCancelButton: true,
        confirmButtonClass: "btn-danger",
        confirmButtonText: "Si, eliminar",
        cancelButtonText: "No, volver",
        closeOnConfirm: false,
        closeOnCancel: true
    },
        function (respuesta) {
            if (respuesta) {
                $(".showSweetAlert").LoadingOverlay("show");

                fetch(`/Categoria/Eliminar?IdCategoria=${data.idCategoria}`, {
                    method: "DELETE"
                })
                .then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide");
                    return response.ok ? response.json() : Promise.reject(response);
                })
                .then(responseJson => {
                    if (responseJson.estado) {
                        tablaData.row(fila).remove().draw();
                        swal("Listo!", "La categoria fue eliminada", "success");
                    }
                    else {
                        swal("Lo sentimos", responseJson.mensaje, "error");
                    }
                })
            }
        }
    );
})