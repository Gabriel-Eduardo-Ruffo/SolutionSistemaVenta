using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using System.Net;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;
using System.Diagnostics.CodeAnalysis;

namespace SistemaVenta.BLL.Implementacion
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IGenericRepository<Usuario> _repositorio;
        private readonly IFireBaseService _fireBaseService;
        private readonly IUtilidadesService _utilidadesService;
        private readonly ICorreoService _correreoService;

        /// <summary>
        /// constructor de la clase (servicios para usuario - Alta, Baja, Modificacion, obtencion de usuarios, reestabolecer clave)
        /// </summary>
        /// <param name="repositorio"></param>
        /// <param name="fireBaseService"></param>
        /// <param name="utilidadesService"></param>
        /// <param name="correreoService"></param>
        public UsuarioService(IGenericRepository<Usuario> repositorio, IFireBaseService fireBaseService, IUtilidadesService utilidadesService, ICorreoService correreoService)
        {
            _repositorio = repositorio;
            _fireBaseService = fireBaseService;
            _utilidadesService = utilidadesService;
            _correreoService = correreoService;
        }


        /// <summary>
        /// Devuelve una lista de usuarios que incluye el rol que tiene cada usuario.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Usuario>> Lista()
        {
            IQueryable<Usuario> query = await _repositorio.Consultar();
            return query.Include(rol => rol.IdRolNavigation).ToList();
        }

        /// <summary>
        /// Crea un usuario
        /// </summary>
        /// <param name="entidad"></param>
        /// <param name="foto"></param>
        /// <param name="nombreFoto"></param>
        /// <param name="urlPlantillaCorreo"></param>
        /// <returns></returns>
        public async Task<Usuario> Crear(Usuario entidad, Stream foto = null, string nombreFoto = "", string urlPlantillaCorreo = "")
        {
            Usuario usuarioExiste = await _repositorio.Obtener(usuario => usuario.Correo == entidad.Correo);

            if (usuarioExiste != null)
                throw new TaskCanceledException("El correo ya existe");

            try
            {
                //creamos la clave de usuario de forma aleatoria y encreiptada
                string claveGenerada = _utilidadesService.GenerarClave();
                entidad.Clave = _utilidadesService.ConvertirSha256(claveGenerada);

                //Guardamos la foto en fireBase
                entidad.NombreFoto = nombreFoto;
                if (entidad.NombreFoto != null)
                {
                    //objeto foto y el armado del nombre de la carpeta y la imagen que se va a guardar en firebase 
                    string urlFoto = await _fireBaseService.SubirStorage(foto, "carpeta_usuario", entidad.NombreFoto);
                    entidad.UrlFoto = urlFoto;
                }

                //Creamos el usuario y validamos si se creo correctamente
                Usuario usuarioCreado = await _repositorio.Crear(entidad);
                if (usuarioCreado.IdUsuario == 0)
                    throw new TaskCanceledException("No se puedo crear el usuario");

                if (urlPlantillaCorreo != "")
                    urlPlantillaCorreo = urlPlantillaCorreo.Replace("[correo]", usuarioCreado.Correo).Replace("[clave]", claveGenerada);

                string htmlCorreo = "";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlPlantillaCorreo);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        StreamReader? sr = null;
                        if(response.CharacterSet == null)//seteamos el encodin de ser necesario si viene con caracteres raros.
                            sr = new StreamReader(dataStream);
                        else
                            sr = new StreamReader(dataStream, Encoding.GetEncoding(response.CharacterSet));

                        htmlCorreo = sr.ReadToEnd();
                        response.Close();
                        sr.Close();
                    }
                }

                if (htmlCorreo != "")
                    await _correreoService.EnviarCorreo(usuarioCreado.Correo, "Cuenta creada", htmlCorreo);

                IQueryable<Usuario> query = await _repositorio.Consultar(usuario => usuario.IdUsuario == usuarioCreado.IdUsuario);
                usuarioCreado = query.Include(rol => rol.IdRolNavigation).First();

                return usuarioCreado;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Edita informacion de un usuario
        /// </summary>
        /// <param name="entidad"></param>
        /// <param name="foto"></param>
        /// <param name="nombreFoto"></param>
        /// <returns></returns>
        public async Task<Usuario> Editar(Usuario entidad, Stream foto = null, string nombreFoto = "")
        {
            Usuario usuarioExiste = await _repositorio.Obtener(usuario => usuario.Correo == entidad.Correo && usuario.IdUsuario != entidad.IdUsuario);
            if(usuarioExiste != null)
                throw new TaskCanceledException("El correo ya existe");

            try
            {
                IQueryable<Usuario> queryUsuario = await _repositorio.Consultar(usuario => usuario.IdUsuario == entidad.IdUsuario);
                Usuario usuarioEditar = queryUsuario.First();
                usuarioEditar.Nombre = entidad.Nombre;
                usuarioEditar.Correo = entidad.Correo;
                usuarioEditar.Telefono = entidad.Telefono;
                usuarioEditar.IdRol = entidad.IdRol;
                usuarioEditar.EsActivo = entidad.EsActivo;

                if(usuarioEditar.NombreFoto == "")
                    usuarioEditar.NombreFoto = nombreFoto;

                if(foto != null)
                {
                    string urlFoto = await _fireBaseService.SubirStorage(foto, "carpeta_usuario", usuarioEditar.NombreFoto);
                    usuarioEditar.UrlFoto = urlFoto;
                }

                bool respuesta = await _repositorio.Editar(usuarioEditar);
                if (!respuesta)
                    throw new TaskCanceledException("No se pudo modificar el usuario");

                Usuario usuarioEditado = queryUsuario.Include(rol => rol.IdRolNavigation).First();

                return usuarioEditado;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Elimina un susario por Id
        /// </summary>
        /// <param name="idUsuario"></param>
        /// <returns></returns>
        public async Task<bool> Eliminar(int idUsuario)
        {
            try
            {
                Usuario usuarioEncontrado = await _repositorio.Obtener(usuario => usuario.IdUsuario == idUsuario);
                if (usuarioEncontrado == null) 
                    throw new TaskCanceledException("El usuario no existe");

                string nombreFoto = usuarioEncontrado.NombreFoto;
                bool respuesta = await _repositorio.Eliminar(usuarioEncontrado);

                if (respuesta)
                    await _fireBaseService.EliminarStorage("carpeta_usuario", nombreFoto);

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Obtiene un usuario por credenciales (correo y clave)
        /// </summary>
        /// <param name="correo"></param>
        /// <param name="clave"></param>
        /// <returns></returns>
        public async Task<Usuario> ObtenerPorCredenciales(string correo, string clave)
        {
            string claveEncriptada = _utilidadesService.ConvertirSha256(clave);
            Usuario usuarioEncontrado = await _repositorio.Obtener(usuario => usuario.Equals(correo) && usuario.Equals(claveEncriptada));

            return usuarioEncontrado;
        }

        /// <summary>
        /// Obtiene un usuario por ID
        /// </summary>
        /// <param name="idUsuario"></param>
        /// <returns></returns>
        public async Task<Usuario> ObtenerPorId(int idUsuario)
        {
            IQueryable<Usuario> query = await _repositorio.Consultar(usuario => usuario.IdUsuario == idUsuario);

            Usuario resultado = query.Include(rol => rol.IdRolNavigation).FirstOrDefault();

            return resultado;
        }

        /// <summary>
        /// Guarda la informacion de un usuario
        /// </summary>
        /// <param name="entidad"></param>
        /// <returns></returns>
        public async Task<bool> GuardarPerfil(Usuario entidad)
        {
            try
            {
                Usuario usuarioEncontrado = await _repositorio.Obtener(usuario => usuario.IdUsuario == entidad.IdUsuario);
                if (usuarioEncontrado == null)
                    throw new TaskCanceledException("El usuario no existe");

                usuarioEncontrado.Correo = entidad.Correo;
                usuarioEncontrado.Telefono = entidad.Telefono;

                bool respuesta = await _repositorio.Editar(usuarioEncontrado);

                return respuesta;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Cambia la clave de un usuario
        /// </summary>
        /// <param name="idUsuario"></param>
        /// <param name="claveActual"></param>
        /// <param name="claveNueva"></param>
        /// <returns></returns>
        public async Task<bool> CambiarClave(int idUsuario, string claveActual, string claveNueva)
        {
            try
            {
                Usuario usuarioEncontrado = await _repositorio.Obtener(usuario => usuario.IdUsuario == idUsuario);

                if (usuarioEncontrado == null)
                    throw new TaskCanceledException("El Usaurio no existe");

                if(usuarioEncontrado.Clave != _utilidadesService.ConvertirSha256(claveActual))
                    throw new TaskCanceledException("La contraseña ingresada como actual, no es correcta");

                usuarioEncontrado.Clave = _utilidadesService.ConvertirSha256(claveNueva);

                bool respuesta = await _repositorio.Editar(usuarioEncontrado);

                return respuesta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Reestablece la clave de un usuario
        /// </summary>
        /// <param name="correo"></param>
        /// <param name="urlPlantillaCorreo"></param>
        /// <returns></returns>
        public async Task<bool> ReestablecerClave(string correo, string urlPlantillaCorreo)
        {
            try
            {
                Usuario usuarioEncontrado = await _repositorio.Obtener(usuario => usuario.Correo == correo);

                if (usuarioEncontrado == null)
                    throw new TaskCanceledException("No encontramos nungun usuario asociado al correo");

                string claveGenerada = _utilidadesService.GenerarClave();
                usuarioEncontrado.Clave = _utilidadesService.ConvertirSha256(claveGenerada);

                urlPlantillaCorreo = urlPlantillaCorreo.Replace("[clave]", claveGenerada);

                string htmlCorreo = "";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlPlantillaCorreo);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        StreamReader? sr = null;
                        if (response.CharacterSet == null)//seteamos el encodin de ser necesario si viene con caracteres raros.
                            sr = new StreamReader(dataStream);
                        else
                            sr = new StreamReader(dataStream, Encoding.GetEncoding(response.CharacterSet));

                        htmlCorreo = sr.ReadToEnd();
                        response.Close();
                        sr.Close();
                    }
                }

                bool correoEnviado = false;

                if (htmlCorreo != "")
                    correoEnviado =  await _correreoService.EnviarCorreo(correo, "Contraseña Reestablecida", htmlCorreo);

                if (!correoEnviado)
                    throw new TaskCanceledException("Tenemos problema. Por favor intentalo de nuevo mas tarde");

                bool respuesta = await _repositorio.Editar(usuarioEncontrado);

                return correoEnviado;
            }
            catch (Exception ex)
            { 
                throw;
            }
        }
    }
}