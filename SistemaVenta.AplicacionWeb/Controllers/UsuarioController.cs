using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using AutoMapper;
using Newtonsoft.Json;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.AplicacionWeb.Utilidades.Response;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.Entity;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    [Authorize]
    public class UsuarioController : Controller
    {
        private readonly IUsuarioService _usuarioServicio;
        private readonly IRolService _rolServicio;
        private readonly IMapper _mapper;

        public UsuarioController(IUsuarioService usuarioServicio, IRolService rolServicio, IMapper mapper)
        {
            _usuarioServicio = usuarioServicio;
            _rolServicio = rolServicio;
            _mapper = mapper;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ListaRoles()
        {
            List<VMRol> vmListaRoles = _mapper.Map<List<VMRol>>(await _rolServicio.Lista());//Se pasa de Rol a VMRol con el metodo de AutoMapper
            return StatusCode(StatusCodes.Status200OK, vmListaRoles);
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            List<VMUsuario> vmUsuarioLista = _mapper.Map<List<VMUsuario>>(await _usuarioServicio.Lista());//Se pasa de Rol a VMRol con el metodo de AutoMapper
            return StatusCode(StatusCodes.Status200OK, new {data = vmUsuarioLista });//Se trabaja con data para dataJSON de JQuery
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromForm] IFormFile foto, [FromForm] string modelo)
        {
            GenericResponse<VMUsuario> genericResponse = new GenericResponse<VMUsuario>();

            try
            {
                VMUsuario vmUsuario = JsonConvert.DeserializeObject<VMUsuario>(modelo);
                string nombreFoto = "";
                Stream fotoStream = null;

                if (foto != null)
                {
                    string nombreEnCodigo = Guid.NewGuid().ToString("N");//"N quiere decir que solo se toma los numeros y letras"
                    string extension = Path.GetExtension(foto.FileName);
                    nombreFoto = string.Concat(nombreEnCodigo, extension);
                    fotoStream = foto.OpenReadStream();
                }

                //[correo] y [clave] se reemplazara en el servicio de la logica de negocio en --> SistemaVenta.BLL.Implementacion --> UsuarioService.CS 
                // public async Task<Usuario> Crear(Usuario entidad, Stream foto = null, string nombreFoto = "", string urlPlantillaCorreo = "")
                //linea 85 --> urlPlantillaCorreo = urlPlantillaCorreo.Replace("[correo]", usuarioCreado.Correo).Replace("[clave]", claveGenerada);
                string urlPlantillaCorreo = $"{this.Request.Scheme}://{this.Request.Host}/Plantilla/EnviarClave?correo=[correo]&clave=[clave]";

                //usamos el servicio y convertimos con mapper los campos y pasamos como parametros las variables dentro del if y urlPlantillaCorreo
                Usuario usuarioCreado = await _usuarioServicio.Crear(_mapper.Map<Usuario>(vmUsuario), fotoStream, nombreFoto, urlPlantillaCorreo);

                vmUsuario = _mapper.Map<VMUsuario>(usuarioCreado);

                genericResponse.Estado = true;
                genericResponse.Objeto = vmUsuario;
            }
            catch (Exception ex)
            {
                genericResponse.Estado = false;
                genericResponse.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, genericResponse);
        }

        [HttpPut]
        public async Task<IActionResult> Editar([FromForm] IFormFile foto, [FromForm] string modelo)
        {
            GenericResponse<VMUsuario> genericResponse = new GenericResponse<VMUsuario>();

            try
            {
                VMUsuario vmUsuario = JsonConvert.DeserializeObject<VMUsuario>(modelo);
                string nombreFoto = "";
                Stream fotoStream = null;

                if (foto != null)
                {
                    string nombreEnCodigo = Guid.NewGuid().ToString("N");//"Nquiere decir que solo se toma los numeros y letras"
                    string extension = Path.GetExtension(foto.FileName);
                    nombreFoto = string.Concat(nombreEnCodigo, extension);
                    fotoStream = foto.OpenReadStream();
                }

                //usamos el servicio y convertimos con mapper los campos y pasamos como parametros las variables dentro del if y urlPlantillaCorreo
                Usuario usuarioEditado = await _usuarioServicio.Editar(_mapper.Map<Usuario>(vmUsuario), fotoStream, nombreFoto);

                vmUsuario = _mapper.Map<VMUsuario>(usuarioEditado);

                genericResponse.Estado = true;
                genericResponse.Objeto = vmUsuario;
            }
            catch (Exception ex)
            {
                genericResponse.Estado = false;
                genericResponse.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, genericResponse);
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int idUsuario)
        {
            GenericResponse<string> genericResponse = new GenericResponse<string>();

            try
            {
                genericResponse.Estado = await _usuarioServicio.Eliminar(idUsuario);
            }
            catch (Exception ex)
            {
                genericResponse.Estado = false;
                genericResponse.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, genericResponse);
        }
    }
}
