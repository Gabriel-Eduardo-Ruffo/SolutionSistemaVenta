using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;
using System.Globalization;

namespace SistemaVenta.BLL.Implementacion
{
    public class DasBoardService : IDashBoardService
    {
        private readonly IVentaRepository _repositorioVenta;
        private readonly IGenericRepository<DetalleVenta> _repositorioDetalleVenta;
        private readonly IGenericRepository<Categoria> _repositorioCategoria;
        private readonly IGenericRepository<Producto> _repositorioProducto;
        private DateTime FechaInicio = DateTime.Now;

        public DasBoardService(IVentaRepository repositorioVenta, IGenericRepository<DetalleVenta> repositorioDetalleVenta, IGenericRepository<Categoria> repositorioCategoria, IGenericRepository<Producto> repositorioProducto)
        {
            _repositorioVenta = repositorioVenta;
            _repositorioDetalleVenta = repositorioDetalleVenta;
            _repositorioCategoria = repositorioCategoria;
            _repositorioProducto = repositorioProducto;

            FechaInicio = FechaInicio.AddDays(-7);//se trabaja con el rango de una semana por eso se resta 7 dias a la fecha atual
        }

        public async Task<int> TotalVentasUltimaSemana()
        {
            try
            {
                IQueryable<Venta> query = await _repositorioVenta.Consultar(venta => venta.FechaRegistro.Value.Date >= FechaInicio.Date);
                int total = query.Count();
                return total;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<string> TotalIngresosUltimaSemana()
        {
            try
            {
                IQueryable<Venta> query = await _repositorioVenta.Consultar(venta => venta.FechaRegistro.Value.Date >= FechaInicio.Date);
                decimal resultado = query.Select(venta => venta.Total).Sum(venta => venta.Value);
                return Convert.ToString(resultado, new CultureInfo("es-AR"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> TotalProductos()
        {
            try
            {
                IQueryable<Producto> query = await _repositorioProducto.Consultar();
                int total = query.Count();
                return total;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> TotalCategorias()
        {
            try
            {
                IQueryable<Categoria> query = await _repositorioCategoria.Consultar();
                int total = query.Count();
                return total;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Dictionary<string, int>> VentasUltimaSemana()
        {
            try
            {
                IQueryable<Venta> query = await _repositorioVenta.Consultar(venta => venta.FechaRegistro.Value.Date >= FechaInicio);
                Dictionary<string, int> resultado = query.GroupBy(venta => venta.FechaRegistro.Value.Date)
                                                         .OrderByDescending(grupo => grupo.Key)
                                                         .Select(dv => new { fecha = dv.Key.ToString("dd/MM/yyyy"), total = dv.Count()})
                                                         .ToDictionary(keySelector: r => r.fecha, elementSelector: r => r.total);
                return resultado;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Dictionary<string, int>> ProductosTopUltimaSemana()
        {
            try
            {
                IQueryable<DetalleVenta> query = await _repositorioDetalleVenta.Consultar();
                Dictionary<string, int> resultado = query.Include(venta => venta.IdVentaNavigation)
                                                         .Where(detalleVenta => detalleVenta.IdVentaNavigation.FechaRegistro.Value.Date >= FechaInicio.Date)
                                                         .GroupBy(detalleVenta => detalleVenta.DescripcionProducto)
                                                         .OrderByDescending(grupo => grupo.Count())
                                                         .Select(dv => new { producto = dv.Key, total = dv.Count() })
                                                         .Take(4)
                                                         .ToDictionary(keySelector: r => r.producto, elementSelector: r => r.total);
                return resultado;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}