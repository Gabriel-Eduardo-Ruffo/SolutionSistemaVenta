using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaVenta.DAL.DBContext;
using SistemaVenta.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace SistemaVenta.DAL.Implementacion
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly DBVENTAContext _dbContext;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"></param>
        public GenericRepository(DBVENTAContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Obtiene un resultado segun el filtro aplicado
        /// </summary>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public async Task<TEntity> Obtener(Expression<Func<TEntity, bool>> filtro)
        {
            try
            {
                TEntity entidad = await _dbContext.Set<TEntity>().FirstOrDefaultAsync(filtro);
                return entidad;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Crea un nuevo registro a partir de los datos que recibe por medio del parametro entidad
        /// </summary>
        /// <param name="entidad"></param>
        /// <returns></returns>
        public async Task<TEntity> Crear(TEntity entidad)
        {
            try
            {
                _dbContext.Set<TEntity>().Add(entidad);//prepara en memoria (en la variable _dbContext) la entidad que va a ser guardada)
                await _dbContext.SaveChangesAsync();//guarda en DB
                return entidad;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Edita un registro a partir de los datos que recibe por medio del parametro entidad
        /// </summary>
        /// <param name="entidad"></param>
        /// <returns></returns>
        public async Task<bool> Editar(TEntity entidad)
        {
            try
            {
                _dbContext.Set<TEntity>().Update(entidad);
                await _dbContext.SaveChangesAsync();//guarda en DB
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Elimina un registro a partir de los datos que recibe por medio del parametro entidad
        /// </summary>
        /// <param name="entidad"></param>
        /// <returns></returns>
        public async Task<bool> Eliminar(TEntity entidad)
        {
            try
            {
                _dbContext.Set<TEntity>().Remove(entidad);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            { 
                throw;
            }
        }

        /// <summary>
        /// Realiza una consulta a partir de los datos que recibe por medio del parametro filtro, si este es null, hace una consulta select all simple
        /// </summary>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public async Task<IQueryable<TEntity>> Consultar(Expression<Func<TEntity, bool>> filtro = null)
        {
            IQueryable<TEntity> queryEntidad = filtro == null? _dbContext.Set<TEntity>() : _dbContext.Set<TEntity>().Where(filtro);
            return queryEntidad;
        }
    }
}