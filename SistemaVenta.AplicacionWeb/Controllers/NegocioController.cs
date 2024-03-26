using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using AutoMapper;
using Newtonsoft.Json;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.AplicacionWeb.Utilidades.Response;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.Entity;


namespace SistemaVenta.AplicacionWeb.Controllers
{
    [Authorize]
    public class NegocioController : Controller
    {
        private readonly IMapper _mapper;
        private readonly INegocioService _negocioService;

        public NegocioController(IMapper mapper, INegocioService negocioService)
        {
            _mapper = mapper;
            _negocioService = negocioService;
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Obtener()
        {
            GenericResponse<VMNegocio> gResponse = new GenericResponse<VMNegocio>();

            try
            {
                VMNegocio vmNegocio = _mapper.Map<VMNegocio>(await _negocioService.Obtener());
                gResponse.Estado = true;
                gResponse.Objeto = vmNegocio;
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK,gResponse);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarCambios([FromForm] IFormFile logo, [FromForm] string modelo)
        {
            GenericResponse<VMNegocio> gResponse = new GenericResponse<VMNegocio>();

            try
            {
                VMNegocio vmNegocio = JsonConvert.DeserializeObject<VMNegocio>(modelo);
                string nombreLogo = "";
                Stream logoStream = null;

                if (logo != null)
                {
                    string nombreEnCodigo = Guid.NewGuid().ToString("N");//genera un nombre nuevo unico con numeros y letras
                    string extension = Path.GetExtension(logo.FileName);

                    nombreLogo = string.Concat(nombreEnCodigo, extension);
                    logoStream = logo.OpenReadStream();
                }

                var entidadNegocio = _mapper.Map<Negocio>(vmNegocio);

                Negocio negocioEditado = await _negocioService.GuardarCambios(entidadNegocio, logoStream, nombreLogo);

                vmNegocio = _mapper.Map<VMNegocio>(negocioEditado);

                gResponse.Estado = true;
                gResponse.Objeto = vmNegocio;
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }
    }
}
