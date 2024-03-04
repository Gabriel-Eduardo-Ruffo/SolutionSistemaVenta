using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaVenta.BLL.Interfaces;
using System.Security.Cryptography;

namespace SistemaVenta.BLL.Implementacion
{
    public class UtilidadesService : IUtilidadesService
    {
        /// <summary>
        /// Genera una clave aleatoria en formato string
        /// </summary>
        /// <returns></returns>
        public string GenerarClave()
        {
            //crea una cadena de texto aleatorio (formato "N" de numeros y letras)
            string clave = Guid.NewGuid().ToString("N").Substring(0, 6);
            return clave;
        }

        /// <summary>
        /// metodo para convertir una clave en formato string a clave encriptada en formato string
        /// </summary>
        /// <param name="texto"></param>
        /// <returns></returns>
        public string ConvertirSha256(string texto)
        {
            StringBuilder sb = new StringBuilder();

            //creamos el objeto hash a partir del SHA256 (encriptador)
            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding encoding = Encoding.UTF8;
                byte[] resultado = hash.ComputeHash(encoding.GetBytes(texto));

                foreach (byte b in resultado)
                {
                    sb.Append(b.ToString("x2"));
                }
            }
            return sb.ToString();
        }
    }
}
