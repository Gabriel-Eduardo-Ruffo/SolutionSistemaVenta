using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.DAL.Implementacion
{
    public class VentaRepository : GenericRepository<Venta>, IVentaRepository
    {
        private readonly DBVENTAContext _dbContext;

        /// <summary>
        /// Constructor
        /// **con : base(dbContext) estamos enviando el dbContext a GenericRepository, por que heredamos de esa clase y necesita ese parametro
        /// </summary>
        /// <param name="dbContext"></param>
        public VentaRepository(DBVENTAContext dbContext) : base(dbContext)
        {
                _dbContext = dbContext;
        }

        /// <summary>
        /// Registrar una venta con sus correspondientes campos armados a partir de la venta
        /// </summary>
        /// <param name="entidad"></param>
        /// <returns></returns>
        public async Task<Venta> Registrar(Venta entidad)
        {
            Venta ventaGenerada = new Venta();

            //usamos tranasacciones para poder hacer rollback si algo sale mal.
            using (var transaccion = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    //iterar el detalleVenta para actualizar el stock disponible de esa entidad
                    foreach (DetalleVenta detalleVenta in entidad.DetalleVenta)
                    {
                        Producto productoEncontrado = _dbContext.Productos.Where(producto => producto.IdProducto == detalleVenta.IdProducto).First();
                        productoEncontrado.Stock = productoEncontrado.Stock - detalleVenta.Cantidad;
                        _dbContext.Productos.Update(productoEncontrado);
                    }
                    await _dbContext.SaveChangesAsync();

                    NumeroCorrelativo numeroCorrelativo = _dbContext.NumeroCorrelativos.Where(x => x.Gestion == "Venta").First();
                    numeroCorrelativo.UltimoNumero = numeroCorrelativo.UltimoNumero + 1;
                    numeroCorrelativo.FechaActualizacion = DateTime.Now;
                    _dbContext.NumeroCorrelativos.Update(numeroCorrelativo);
                    await _dbContext.SaveChangesAsync();

                    string ceros = string.Concat(Enumerable.Repeat("0", numeroCorrelativo.CantidadDigitos.Value));
                    string numeroVenta = ceros + numeroCorrelativo.UltimoNumero.ToString();
                    numeroVenta = numeroVenta.Substring(numeroVenta.Length - numeroCorrelativo.CantidadDigitos.Value, numeroCorrelativo.CantidadDigitos.Value);

                    entidad.NumeroVenta = numeroVenta;

                    await _dbContext.Venta.AddAsync(entidad);
                    await _dbContext.SaveChangesAsync();

                    ventaGenerada = entidad;

                    //realizamos el commit de transaccion completa si no hubo ningun error en las transacciones anteriores (temporales)
                    transaccion.Commit();
                }
                catch (Exception ex)
                {
                    transaccion.Rollback();
                    throw ex;
                }
            }
            return ventaGenerada;
        }

        public async Task<List<DetalleVenta>> Reporte(DateTime fechaInicio, DateTime FechaFin)
        {
            //detalleVenta (Include y ThenInclude es un joins entre tablas)
            List<DetalleVenta> listaResumen  = await _dbContext.DetalleVenta
                .Include(venta => venta.IdVentaNavigation) // Include hace referencia a tabla Venta (_dbContext.DetalleVenta)
                .ThenInclude(usuario => usuario.IdUsuarioNavigation) //hace referencia al usuario dentro de la tabla Venta (venta.IdVentaNavigation)
                .Include(Venta => Venta.IdVentaNavigation) // Include hace referencia a tabla Venta (_dbContext.DetalleVenta)
                .ThenInclude(tipoDetalleVenta => tipoDetalleVenta.IdTipoDocumentoVentaNavigation) //hace referencia al usuario dentro de la tabla Venta (venta.IdVentaNavigation)
                .Where(detalleVenta =>  detalleVenta.IdVentaNavigation.FechaRegistro.Value.Date >= fechaInicio.Date &&
                                        detalleVenta.IdVentaNavigation.FechaRegistro.Value.Date <= FechaFin.Date).ToListAsync();
            return listaResumen;
        }
    }
}