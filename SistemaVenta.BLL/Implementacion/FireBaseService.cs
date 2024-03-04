using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Firebase.Auth;
using Firebase.Storage;

using SistemaVenta.BLL.Interfaces;
using SistemaVenta.Entity;
using SistemaVenta.DAL.Interfaces;

namespace SistemaVenta.BLL.Implementacion
{
    public class FireBaseService : IFireBaseService
    {
        private readonly IGenericRepository<Configuracion> _repositorio;

        public FireBaseService(IGenericRepository<Configuracion> repositorio)
        {
            _repositorio = repositorio;
        }

        /// <summary>
        /// Metodo que sirve para subir imagenes a Firebase.
        /// el metodo contiene un Token de cancelacion
        /// </summary>
        /// <param name="streamArchivo"></param>
        /// <param name="carpetaDestino"></param>
        /// <param name="nombreArchivo"></param>
        /// <returns>
        /// devuelve una URL donde se encuentra la imagen que se subio a Firebase.
        /// </returns>
        public async Task<string> SubirStorage(Stream streamArchivo, string carpetaDestino, string nombreArchivo)
        {
            string URLImagen = "";

            try
            {
                IQueryable<Configuracion> query = await _repositorio.Consultar(consulta => consulta.Recurso.Equals("FireBase_Storage"));
                Dictionary<string, string> configuracion = query.ToDictionary(keySelector: configuracion => configuracion.Propiedad, elementSelector: configuracion => configuracion.Valor);

                var auth = new FirebaseAuthProvider(new FirebaseConfig(configuracion["api_key"]));
                var credenciales = await auth.SignInWithEmailAndPasswordAsync(configuracion["email"], configuracion["clave"]);

                var tokenCancelacion = new CancellationTokenSource();
                var tarea = new FirebaseStorage(
                                                configuracion["ruta"],
                                                new FirebaseStorageOptions
                                                {
                                                    AuthTokenAsyncFactory = () => Task.FromResult(credenciales.FirebaseToken),
                                                    ThrowOnCancel = true
                                                }).
                                                Child(configuracion[carpetaDestino]).
                                                Child(nombreArchivo).
                                                PutAsync(streamArchivo, tokenCancelacion.Token);
                URLImagen = await tarea;
            }
            catch(Exception ex)
            {
                URLImagen = "";
            }

            return URLImagen;
        }

        public async Task<bool> EliminarStorage(string carpetaDestino, string nombreArchivo)
        {
            try
            {
                IQueryable<Configuracion> query = await _repositorio.Consultar(consulta => consulta.Recurso.Equals("FireBase_Storage"));
                Dictionary<string, string> configuracion = query.ToDictionary(keySelector: configuracion => configuracion.Propiedad, elementSelector: configuracion => configuracion.Valor);

                var auth = new FirebaseAuthProvider(new FirebaseConfig(configuracion["api_key"]));
                var credenciales = await auth.SignInWithEmailAndPasswordAsync(configuracion["email"], configuracion["clave"]);

                var tokenCancelacion = new CancellationTokenSource();
                var tarea = new FirebaseStorage(
                                                configuracion["ruta"],
                                                new FirebaseStorageOptions
                                                {
                                                    AuthTokenAsyncFactory = () => Task.FromResult(credenciales.FirebaseToken),
                                                    ThrowOnCancel = true
                                                }).
                                                Child(configuracion[carpetaDestino]).
                                                Child(configuracion[nombreArchivo]).
                                                DeleteAsync();
                await tarea;
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}