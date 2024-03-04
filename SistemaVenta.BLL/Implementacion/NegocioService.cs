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
    public class NegocioService : INegocioService
    {
        private readonly IGenericRepository<Negocio> _repositorio;
        private readonly IFireBaseService _firebaseService;

        public NegocioService(IGenericRepository<Negocio> repositorio, IFireBaseService fireBaseService)
        {
            _repositorio = repositorio;
            _firebaseService = fireBaseService;
        }

        public async Task<Negocio> Obtener()
        {
            try
            {
                //el * idNegocio = 1 * es con el cual se trabaja en la app (siempre se trabaja con un negocio nada mas.
                //TODO a futuro ver si se puede trabajar con varios negocios
                Negocio negocioEncontrado = await _repositorio.Obtener(tablaNegocio => tablaNegocio.IdNegocio == 1);
                return negocioEncontrado;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<Negocio> GuardarCambios(Negocio entidad, Stream logo = null, string nombreLogo = "")
        {
            try
            {
                Negocio negocioEncontrado = await _repositorio.Obtener(tablaNegocio => tablaNegocio.IdNegocio == 1);

                negocioEncontrado.NumeroDocumento = entidad.NumeroDocumento;
                negocioEncontrado.Nombre = entidad.Nombre;
                negocioEncontrado.Correo = entidad.Correo;
                negocioEncontrado.Direccion = entidad.Direccion;
                negocioEncontrado.Telefono = entidad.Telefono;
                negocioEncontrado.PorcentajeImpuesto = entidad.PorcentajeImpuesto;
                negocioEncontrado.SimboloMoneda = entidad.SimboloMoneda;
                negocioEncontrado.NombreLogo = nombreLogo == "" ? entidad.NombreLogo : nombreLogo;

                if (logo != null)
                {
                    string urlLogo = await _firebaseService.SubirStorage(logo, "carpeta_logo", nombreLogo);
                    negocioEncontrado.UrlLogo = urlLogo;
                }

                await _repositorio.Editar(negocioEncontrado);
                return negocioEncontrado;

            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
