/* 
javascript para el index de Negocio en: carpeta ---> Views ---> Negocio ---> index.cshtml

Negocio_Index
Negocio es a quien pertenece este script
Index es a que vista (View) pertenece

SistemaVenta.AplicacionWeb
wwwroot        ---> js ---> vistas ---> Negocio_Index.js
Views          --->        Negocio ---> Index.cshtml
Controllers                        ---> NegocioController.cs
Models         --->     ViewModels ---> VMNegocio.cs

SistemaVenta.BLL
Implementacion                     ---> NegocioService.cs

SistemaVenta.Entity
                                   ---> Negocio.cs
*/

$(document).ready(function () {

    /* loadingoverlay libreria dentro de la carpeta vendor que muestra popup de carga */
    $(".card-body").LoadingOverlay("show");

    fetch("/Negocio/Obtener")
        .then(response => {
            $(".card-body").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {
            if (responseJson.estado)
            {
                const d = responseJson.objeto;
                $("#txtNumeroDocumento").val(d.numeroDocumento);
                $("#txtRazonSocial").val(d.nombre);
                $("#txtCorreo").val(d.correo);
                $("#txtDireccion").val(d.direccion);
                $("#txTelefono").val(d.telefono);
                $("#txtImpuesto").val(d.porcentajeImpuesto);
                $("#txtSimboloMoneda").val(d.simboloMoneda);
                $("#imgLogo").attr("src", d.urlLogo);
            }
            else
            {
                swal("Lo sentimos", responseJson.mensaje, "error");
            }
        })
});

//-----click en botones (llamado a eventos)
/*Boton Guardar Cambios */
$("#btnGuardarCambios").click(function () {
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

    const modelo =
    {
        numeroDocumento: $("#txtNumeroDocumento").val(),
        nombre: $("#txtRazonSocial").val(),
        correo: $("#txtCorreo").val(),
        direccion: $("#txtDireccion").val(),
        telefono: $("#txTelefono").val(),
        porcentajeImpuesto: $("#txtImpuesto").val(),
        simboloMoneda: $("#txtSimboloMoneda").val() 
    }
    const inputLogo = document.getElementById("txtLogo");
    const formData = new FormData();

    formData.append("logo", inputLogo.files[0]);
    formData.append("modelo", JSON.stringify(modelo));

    /* loadingoverlay libreria dentro de la carpeta vendor que muestra popup de carga */
    $(".card-body").LoadingOverlay("show");

    fetch("/Negocio/GuardarCambios",
    {
        method: "POST",
        body: formData
    })   
    .then(response => {
        $(".card-body").LoadingOverlay("hide");
        return response.ok ? response.json() : Promise.reject(response);
    })
    .then(responseJson => {
        console.log(responseJson);
        if (responseJson.estado)
        {
            const d = responseJson.objeto;
            $("#imgLogo").attr("src", d.urlLogo);
        }
        else
        {
            swal("Lo sentimos", responseJson.mensaje, "error");
        }
    })
});