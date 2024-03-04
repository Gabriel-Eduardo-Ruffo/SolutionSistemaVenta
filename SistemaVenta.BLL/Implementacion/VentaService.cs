using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;
    
namespace SistemaVenta.BLL.Implementacion
{
    public class VentaService : IVentaService
    {
        private readonly IGenericRepository<Producto> _repositorioProducto;
        private readonly IVentaRepository _repositorioVenta;


        /// <summary>
        /// Constructor de Venta Servicio
        /// </summary>
        /// <param name="repositorioProducto"></param>
        /// <param name="repositorioVenta"></param>
        public VentaService(IGenericRepository<Producto> repositorioProducto, IVentaRepository repositorioVenta)
        {
            _repositorioProducto = repositorioProducto;
            _repositorioVenta = repositorioVenta;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="busqueda"></param>
        /// <returns></returns>
        public async Task<List<Producto>> ObtenerProductos(string busqueda)
        {
            IQueryable<Producto> query = await _repositorioProducto.Consultar(productos => 
                                                                        productos.EsActivo == true && 
                                                                        productos.Stock > 0 &&
                                                                        string.Concat(productos.CodigoBarra, productos.Marca, productos.Descripcion).Contains(busqueda)
                                                                    );

            return query.Include(Categoria => Categoria.IdCategoriaNavigation).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entidad"></param>
        /// <returns></returns>
        public async Task<Venta> Registrar(Venta entidad)
        {
            try
            {
                return await _repositorioVenta.Registrar(entidad);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numeroVenta"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        /// <returns></returns>
        public async Task<List<Venta>> Historial(string numeroVenta, string fechaInicio, string fechaFin)
        {
            IQueryable<Venta> query = await _repositorioVenta.Consultar();

            fechaInicio = fechaInicio is null ? "" : fechaInicio;
            fechaFin = fechaFin is null ? "" : fechaFin;

            //Si trabajamos con rango de fechas en la consulta
            if (fechaInicio != "" && fechaFin != "")
            {
                DateTime fechaInicioTemp = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", new CultureInfo("es-AR"));
                DateTime fechaFinTemp = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", new CultureInfo("es-AR"));

                var resultado = query.Where(ventas => 
                                    ventas.FechaRegistro.Value.Date >= fechaInicioTemp && ventas.FechaRegistro.Value.Date <= fechaFinTemp)
                                    .Include(tipoDocumento => tipoDocumento.IdTipoDocumentoVentaNavigation)
                                    .Include(usuario => usuario.IdUsuarioNavigation)
                                    .Include(detalleVenta => detalleVenta.DetalleVenta)
                                    .ToList();
                return resultado;
            }
            else//si trabajamos con numeros de venta
            {

                var resultado = query.Where(ventas =>
                                   ventas.NumeroVenta == numeroVenta)
                                    .Include(tipoDocumento => tipoDocumento.IdTipoDocumentoVentaNavigation)
                                    .Include(usuario => usuario.IdUsuarioNavigation)
                                    .Include(detalleVenta => detalleVenta.DetalleVenta)
                                    .ToList();
                return resultado;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numeroVenta"></param>
        /// <returns></returns>
        public async Task<Venta> Detalle(string numeroVenta)
        {
            IQueryable<Venta> query = await _repositorioVenta.Consultar(venta => venta.NumeroVenta == numeroVenta);

            return query.Include(tipoDocumento => tipoDocumento.IdTipoDocumentoVentaNavigation)
                        .Include(usuario => usuario.IdUsuarioNavigation)
                        .Include(detalleVenta => detalleVenta.DetalleVenta)
                        .First();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        /// <returns></returns>
        public async Task<List<DetalleVenta>> Reporte(string fechaInicio, string fechaFin)
        {
            DateTime fechaInicioTemp = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", new CultureInfo("es-AR"));
            DateTime fechaFinTemp = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", new CultureInfo("es-AR"));

            List<DetalleVenta> lista = await _repositorioVenta.Reporte(fechaInicioTemp, fechaFinTemp);
            return lista;
        }
    }
}