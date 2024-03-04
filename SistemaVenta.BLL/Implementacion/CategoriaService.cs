using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    public class CategoriaService : ICategoriaService
    {
        private readonly IGenericRepository<Categoria> _repositorio;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="repositorio"></param>
        public CategoriaService(IGenericRepository<Categoria> repositorio)
        {
            _repositorio = repositorio;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<Categoria>> Lista()
        {
            IQueryable<Categoria> query = await _repositorio.Consultar();
            return query.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entidad"></param>
        /// <returns></returns>
        public async Task<Categoria> Crear(Categoria entidad)
        {
            try
            {
                Categoria categoriaCreada = await _repositorio.Crear(entidad);
                if (categoriaCreada.IdCategoria == 0)
                    throw new TaskCanceledException("No se pudo crear la categoria");

                return categoriaCreada;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entidad"></param>
        /// <returns></returns>
        public async Task<Categoria> Editar(Categoria entidad)
        {
            try
            {
                Categoria categoriaEncontrada = await _repositorio.Obtener(categoria => categoria.IdCategoria == entidad.IdCategoria);
                categoriaEncontrada.Descripcion = entidad.Descripcion;
                categoriaEncontrada.EsActivo = entidad.EsActivo;

                bool respuesta = await _repositorio.Editar(categoriaEncontrada);

                if (!respuesta)
                    throw new TaskCanceledException("No se pudo modificar la categoria");

                return categoriaEncontrada;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idCategoria"></param>
        /// <returns></returns>
        public async Task<bool> Eliminar(int idCategoria)
        {
            try
            {
                Categoria categoriaEncontrada = await _repositorio.Obtener(categoria => categoria.IdCategoria == idCategoria);

                if (categoriaEncontrada == null)
                    throw new TaskCanceledException("La categoria no existe");

                bool respuesta = await _repositorio.Eliminar(categoriaEncontrada);

                return respuesta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}