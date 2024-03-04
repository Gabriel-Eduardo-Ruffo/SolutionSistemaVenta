using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Mail;

using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;
using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.BLL.Implementacion
{
    public class CorreoService : ICorreoService
    {
        private readonly IGenericRepository<Configuracion> _repositorio;

        public CorreoService(IGenericRepository<Configuracion> repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<bool> EnviarCorreo(string CorreoDestino, string Asunto, string Mensaje)
        {
            try
            {
                IQueryable<Configuracion> query = await _repositorio.Consultar(consulta => consulta.Recurso.Equals("Servicio_Correo"));
                Dictionary<string, string> configuracion = query.ToDictionary(keySelector: consulta => consulta.Propiedad, elementSelector: consulta => consulta.Valor);
                var credenciales = new NetworkCredential(configuracion["correo"], configuracion["clave"]);

                var correo = new MailMessage()
                { 
                    From = new MailAddress(configuracion["correo"], configuracion["alias"]),
                    Subject = Asunto,
                    Body = Mensaje,
                    IsBodyHtml = true
                };
                correo.To.Add(new MailAddress(CorreoDestino));

                var clienteServidor = new SmtpClient()
                {
                    Host = configuracion["host"],
                    Port = int.Parse(configuracion["puerto"]),
                    Credentials = credenciales,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    EnableSsl = true,
                };

                clienteServidor.Send(correo);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
