using Microsoft.AspNetCore.Mvc;

using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.AplicacionWeb.Utilidades.Response;
using SistemaVenta.BLL.Interfaces;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    public class DashBoardController : Controller
    {
        private readonly IDashBoardService _dashBoardServicio;

        public DashBoardController(IDashBoardService dashBoardServicio)
        {
            _dashBoardServicio = dashBoardServicio;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerResumen()
        {
            GenericResponse<VMDashBoard> gResponse = new GenericResponse<VMDashBoard>();
            try
            {
                VMDashBoard vmDashBoard = new VMDashBoard();
                vmDashBoard.TotalVentas = await _dashBoardServicio.TotalVentasUltimaSemana();
                vmDashBoard.TotalIngresos = await _dashBoardServicio.TotalIngresosUltimaSemana();
                vmDashBoard.TotalProductos = await _dashBoardServicio.TotalProductos();
                vmDashBoard.TotalCategorias = await _dashBoardServicio.TotalCategorias();

                List<VMVentasSemana> listaVentaSemana = new List<VMVentasSemana>();
                List<VMProductosSemana> listaProductoSemana = new List<VMProductosSemana>();

                foreach (KeyValuePair<string, int> item in await _dashBoardServicio.VentasUltimaSemana())
                {
                    listaVentaSemana.Add
                    (
                        new VMVentasSemana() 
                        { 
                            Fecha = item.Key, 
                            Total = item.Value
                        }
                    );
                }

                foreach (KeyValuePair<string, int> item in await _dashBoardServicio.ProductosTopUltimaSemana())
                {
                    listaProductoSemana.Add
                    (
                        new VMProductosSemana()
                        {
                            Producto = item.Key,
                            Cantidad = item.Value
                        }
                    );
                }

                vmDashBoard.VentasUltimaSemana = listaVentaSemana;
                vmDashBoard.ProductosTopUltimaSemana = listaProductoSemana;

                gResponse.Estado = true;
                gResponse.Objeto = vmDashBoard;
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message; ;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}