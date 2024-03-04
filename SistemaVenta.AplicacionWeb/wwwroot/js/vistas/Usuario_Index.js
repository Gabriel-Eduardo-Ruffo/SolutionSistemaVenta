/*
javascript para el index de Usuario en: carpeta ---> Views ---> Usuario ---> index.cshtml

Usuario_Index
Usuario es a quien pertenece este script
Index es a que vista (View) pertenece

SistemaVenta.AplicacionWeb
wwwroot        ---> js ---> vistas ---> Usuario_Index.js
Views          --->        Usuario ---> Index.cshtml
Controllers                        ---> UsuarioController.cs
Models         --->     ViewModels ---> VMUsuario.cs

SistemaVenta.BLL
Implementacion                     ---> UsuarioService.cs

SistemaVenta.Entity
                                   ---> Usuario.cs
*/

const MODELO_BASE = {
    idUsuario: 0,
    nombre: "",
    correo: "",
    telefono: "",
    idRol: 0,
    esActivo: "1",
    urlFoto: ""
}

let tablaData;

$(document).ready(function () {

    fetch("/Usuario/ListaRoles")
    .then(response => {
        return response.ok ? response.json() : Promise.reject(response);
    })
    .then(responseJson => {
        if (responseJson.length > 0)
        {
            responseJson.forEach((item) => {
                $("#cboRol").append(
                    $("<option>").val(item.idRol).text(item.descripcion)
                )
            })
        }
    })

    tablaData = $('#tbdata').DataTable({/* se esta trabajando con data tables ( se rellenan las filas en orden con la informacion que viene de Backend y se agregan botones, imagenes e informacion por cada fila */
        responsive: true,
        "ajax": {
            "url": '/Usuario/lista',
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "idUsuario", "visible": false, "searchable": false },
            {
                "data": "urlFoto", render: function (data) {
                    return `<img style="height:60px" src=${data} class="rounded mx-auto d-block"/>`;
                }
            },
            { "data": "nombre" },
            { "data": "correo" },
            { "data": "telefono" },
            { "data": "nombreRol" },
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
                filename: 'Reporte Usuarios',
                exportOptions: {
                    columns: [2, 3, 4, 5, 6] /*las columnas que vienen en data (idUsuario = 0, urlFoto= 1, nombre = 2, correo = 3, telefono = 4, nombreRol = 5, esActivo = 6 ) */
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
    $("#txtId").val(modelo.idUsuario);
    $("#txtNombre").val(modelo.nombre);
    $("#txtCorreo").val(modelo.correo);
    $("#txtTelefono").val(modelo.telefono);
    $("#cboRol").val(modelo.idRol == 0 ? $("#cboRol option:first").val() : modelo.idRol);
    $("#cboEstado").val(modelo.esActivo);
    $("#txtFoto").val("");
    $("#imgUsuario").attr("src", modelo.urlFoto);

    $("#modalData").modal("show");
}


//-----clicks en botones (llamado a eventos)----
/*Boton nuevo */
$("#btnNuevo").click(function ()
{
    MostrarModal();
})

/*Boton Guardar */
$("#btnGuardar").click(function ()
{
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
    modelo["idUsuario"] = parseInt($("#txtId").val());
    modelo["nombre"] = $("#txtNombre").val();
    modelo["correo"] = $("#txtCorreo").val();
    modelo["telefono"] = $("#txtTelefono").val();
    modelo["idRol"] = $("#cboRol").val();
    modelo["esActivo"] = $("#cboEstado").val();

    const inputFoto = document.getElementById("txtFoto");

    /* este formData es el que se envia al controller UsuarioController al metodo Crear([FromForm] IFormFile foto, [FromForm] string modelo) */
    const formData = new FormData();
    formData.append("foto", inputFoto.files[0]);
    formData.append("modelo", JSON.stringify(modelo));

    /* loadingoverlay libreria dentro de la carpeta vendor que muestra popup de carga */
    $("#modalData").find("div.modal-content").LoadingOverlay("show");

    if (modelo.idUsuario == 0)/*si es 0 es para crear un usuario nuevo */
    {
        fetch("/Usuario/Crear", {
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
                swal("Listo!", "El usuario fue creado", "success");
            }
            else {
                swal("Lo sentimos", responseJson.mensaje, "error");
            }
        })
    }
    else
    {
        fetch("/Usuario/Editar", {
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
                swal("Listo!", "El usuario fue modificado", "success");
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
    if ($(this).closest("tr").hasClass("child")) {
        fila = $(this).closest("tr").prev();
    }
    else {
        fila = $(this).closest("tr");
    }

    const data = tablaData.row(fila).data();
    /* modal de confirmacion para eliminar usuario */
    swal({
        title: "Esta seguro?",
        text: `Eliminar al usuario "${data.nombre}"`,
        type: "warning",
        showCancelButton: true,
        confirmButtonClass: "btn-danger",
        confirmButtonText: "Si, eliminar",
        cancelButtonText: "No, volver",
        closeOnConfirm: false,
        closeOnCancel: true
    },
        function (respuesta)
        {
            if (respuesta)
            {
                $(".showSweetAlert").LoadingOverlay("show");

                fetch(`/Usuario/Eliminar?IdUsuario=${data.idUsuario}`, {
                    method: "DELETE"
                })
                .then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide");
                    return response.ok ? response.json() : Promise.reject(response);
                })
                .then(responseJson => {
                    if (responseJson.estado) {
                        tablaData.row(fila).remove().draw();
                        swal("Listo!", "El usuario fue eliminado", "success");
                    }
                    else {
                        swal("Lo sentimos", responseJson.mensaje, "error");
                    }
                })
            }
        }
    );
})