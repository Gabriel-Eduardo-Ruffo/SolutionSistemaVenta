/* 
javascript para el index de Perfil en: carpeta ---> Views ---> Home ---> Perfil.cshtml

Home_Perfil
Perfil es a quien pertenece este script
Home es a que vista (View) pertenece

SistemaVenta.AplicacionWeb
wwwroot        ---> js ---> vistas ---> Home_Perfil.js
Views          --->           Home ---> Perfil.cshtml
Controllers                        ---> HomeController.cs
Models         --->     ViewModels ---> VMPerfil.cs

SistemaVenta.BLL
Implementacion                     ---> UsuarioService.cs

SistemaVenta.Entity
                                   ---> Usuario.cs
*/

$(document).ready(function () {

    /* loadingoverlay libreria dentro de la carpeta vendor que muestra popup de carga */
    $(".container-fluid").LoadingOverlay("show");

    fetch("/Home/ObtenerUsuario")
    .then(response => {
        $(".container-fluid").LoadingOverlay("hide");
        return response.ok ? response.json() : Promise.reject(response);
    })
    .then(responseJson => {
        if (responseJson.estado) {
            const d = responseJson.objeto;
            $("#imgFoto").attr("src", d.urlFoto);
            $("#txtNombre").val(d.nombre);
            $("#txtCorreo").val(d.correo);
            $("#txTelefono").val(d.telefono);
            $("#txtRol").val(d.nombreRol);
        }
        else {
            swal("Lo sentimos", responseJson.mensaje, "error");
        }
    })
});

$("#btnGuardarCambios").click(function(){
    if ($("#txtCorreo").val().trim == "") {
        /*toastr libreria dentro de la carpeta vendor, para mostrar mensajes en el front*/
        toastr.warning("", "Debe completar el campo correo");
        $("#txtCorreo").focus();
        return;
    }

    if ($("#txTelefono").val().trim == "") {
        /*toastr libreria dentro de la carpeta vendor, para mostrar mensajes en el front*/
        toastr.warning("", "Debe completar el campo telefono");
        $("#txTelefono").focus();
        return;
    }

    /* modal de confirmacion para guardar los cambios del perfil de usuario */
    swal({
        title: "Desea guardar los cambios?",
        type: "warning",
        showCancelButton: true,
        confirmButtonClass: "btn-primary",
        confirmButtonText: "Si",
        cancelButtonText: "No",
        closeOnConfirm: false,
        closeOnCancel: true
    },
        function (respuesta) {
            if (respuesta) {
                $(".showSweetAlert").LoadingOverlay("show");

                let modelo = {
                    correo: $("#txtCorreo").val().trim(),
                    telefono: $("#txTelefono").val().trim()
                };

                fetch("/Home/GuardarPerfil", {
                    method: "POST",
                    headers: { "Content-Type": "application/json; charset=utf-8" },
                    body: JSON.stringify(modelo)
                })
                .then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide");
                    return response.ok ? response.json() : Promise.reject(response);
                })
                .then(responseJson => {
                    if (responseJson.estado) {
                        swal("Listo!", "Los cambios fueron guardados", "success");
                    }
                    else {
                        swal("Lo sentimos", responseJson.mensaje, "error");
                    }
                })
            }
        }
    );
});

$("#btnCambiarClave").click(function () {
    const inputs = $("input.input-validar").serializeArray();
    const inputs_sin_valor = inputs.filter((item => item.value.trim() == ""));

    if (inputs_sin_valor.length > 0) {
        const mensaje = `Debe completar el campo: "${inputs_sin_valor[0].name}"`;
        /*toastr libreria dentro de la carpeta vendor, para mostrar mensajes en el front*/
        toastr.warning("", mensaje);
        $(`input[name="${inputs_sin_valor[0].name}"]`).focus();
        return;
    }

    if ($("#txtClaveNueva").val().trim() != $("#txtConfirmarClave").val().trim()) {
        /*toastr libreria dentro de la carpeta vendor, para mostrar mensajes en el front*/
        toastr.warning("", "Las contraseñas nuevas no coinciden");
        $("#txtCorreo").focus();
        return;
    }

    let modelo = {
        claveActual: $("#txtClaveActual").val().trim(),
        claveNueva: $("#txtClaveNueva").val().trim()
    }

    fetch("/Home/CambiarClave", {
        method: "POST",
        headers: { "Content-Type": "application/json; charset=utf-8" },
        body: JSON.stringify(modelo)
    })
    .then(response => {
        $(".showSweetAlert").LoadingOverlay("hide");
        return response.ok ? response.json() : Promise.reject(response);
    })
    .then(responseJson => {
        if (responseJson.estado) {
            swal("Listo!", "Su contraseña fue actualizada", "success");
            $("input.input-validar").val("");
        }
        else {
            swal("Lo sentimos", responseJson.mensaje, "error");
        }
    })
});