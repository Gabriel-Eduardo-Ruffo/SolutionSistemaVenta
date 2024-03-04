using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    public class ProductoService : IProductoService
    {
        private readonly IGenericRepository<Producto> _repositorio;
        private readonly IFireBaseService _fireBaseService;

        /// <summary>
        /// Constructor del servicio de Producto
        /// </summary>
        /// <param name="producto"></param>
        /// <param name="fireBaseService"></param>
        /// <param name="utilidadesServicio"></param>
        public ProductoService(IGenericRepository<Producto> producto, IFireBaseService fireBaseService)
        {
            _repositorio = producto;
            _fireBaseService = fireBaseService;
        }

        /// <summary>
        /// Lista los productos existentes en la base de datos
        /// </summary>
        /// <returns></returns>
        public async Task<List<Producto>> Lista()
        {
            IQueryable<Producto> query = await _repositorio.Consultar();
            return query.Include(categoria => categoria.IdCategoriaNavigation).ToList();
        }

        /// <summary>
        /// Crea un producto nueva, validando que no exista anteriormente y que el codigo de barras no se repita y no exista anteriormente
        /// </summary>
        /// <param name="entidad"></param>
        /// <param name="imagen"></param>
        /// <param name="nombreImagen"></param>
        /// <returns></returns>
        /// <exception cref="TaskCanceledException"></exception>
        public async Task<Producto> Crear(Producto entidad, Stream imagen = null, string nombreImagen = "")
        {
            Producto productoExiste = await _repositorio.Obtener(producto => producto.CodigoBarra == entidad.CodigoBarra);

            if (productoExiste != null)
                throw new TaskCanceledException("El codigo de barra ya existe");

            try
            {
                entidad.NombreImagen = nombreImagen;
                if(imagen != null)
                {
                    string urlImagen = await _fireBaseService.SubirStorage(imagen, "carpeta_producto", nombreImagen);
                    entidad.UrlImagen = urlImagen;
                }

                Producto productoCreado = await _repositorio.Crear(entidad);

                if(productoCreado.IdProducto == 0)
                    throw new TaskCanceledException("No se pudo crear el producto");

                IQueryable<Producto> query = await _repositorio.Consultar(producto => producto.IdProducto == productoCreado.IdProducto);

                productoCreado = query.Include(categoria => categoria.IdCategoriaNavigation).First();

                return productoCreado;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Edita un producto existente en DB
        /// </summary>
        /// <param name="entidad"></param>
        /// <param name="imagen"></param>
        /// <returns></returns>
        /// <exception cref="TaskCanceledException"></exception>
        public async Task<Producto> Editar(Producto entidad, Stream imagen = null, string nombreImagen = "")
        {
            //que el codigo de barra sea el mismo y que no exista el id producto en la DB (para que no se repita el codigo de barras en otros productos)
            Producto productoExiste = await _repositorio.Obtener(producto => producto.CodigoBarra == entidad.CodigoBarra && producto.IdProducto != entidad.IdProducto);

            if (productoExiste != null)
                throw new TaskCanceledException("El codigo de barra esta asignado a otro producto");

            try
            {
                IQueryable<Producto> queryProducto = await _repositorio.Consultar(producto => producto.IdProducto == entidad.IdProducto);
                Producto productoParaEditar = queryProducto.First();

                productoParaEditar.CodigoBarra = entidad.CodigoBarra;
                productoParaEditar.Marca = entidad.Marca;
                productoParaEditar.Descripcion = entidad.Descripcion;
                productoParaEditar.IdCategoria = entidad.IdCategoria;
                productoParaEditar.Stock = entidad.Stock;
                productoParaEditar.Precio = entidad.Precio;
                productoParaEditar.EsActivo = entidad.EsActivo;

                if (productoParaEditar.NombreImagen == "")
                {
                    productoParaEditar.NombreImagen = nombreImagen;                       
                }

                if(imagen != null)
                {
                    //se va a mantener el nombre de la imagen que este en firebase por que por cada producto solo hay una imagen, no tiene sentido llenar de imagenes que no se usen en firebase
                    string urlImegen = await _fireBaseService.SubirStorage(imagen, "carpeta_producto", productoParaEditar.NombreImagen);
                    productoParaEditar.UrlImagen = urlImegen;
                }

                bool respuesta = await _repositorio.Editar(productoParaEditar);

                if(!respuesta)
                    throw new TaskCanceledException("No se pudo editar el producto");

                Producto productoEditado = queryProducto.Include(categoria => categoria.IdCategoriaNavigation).First();

                return productoEditado;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Borra un producto de la DB
        /// </summary>
        /// <param name="idProducto"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> Eliminar(int idProducto)
        {
            try
            {
                Producto productoEncontrado = await _repositorio.Obtener(producto => producto.IdProducto == idProducto);
                if(productoEncontrado == null)
                    throw new TaskCanceledException("El producto no existe");

                string nombreImagenBorrar = productoEncontrado.NombreImagen;
                bool respuesta = await _repositorio.Eliminar(productoEncontrado);

                if (respuesta)
                    await _fireBaseService.EliminarStorage("Carpeta_Producto", nombreImagenBorrar);

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}