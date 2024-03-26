using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace SistemaVenta.AplicacionWeb.Utilidades.ViewComponents
{
    /// <summary>
    /// Clase que Hereda de ViewComponent
    /// La vista que devuelve esta en Shared --> Components --> MenuUsuario --> Default.cshtml
    /// Se tiene que respetar el orden de las carpetas y el nombre del archivo 
    /// por que esta clase se relaciona automaticamente con la vista por medio del ViewComponent Heredado
    /// </summary>
    public class MenuUsuarioViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            ClaimsPrincipal claimUsuario = HttpContext.User;

            string nombreUsuario = "";
            string urlFotoUsuario = "";

            if (claimUsuario.Identity.IsAuthenticated)
            {
                //la informacion del claim es la info que esta guardada en las cookies del navegador, esa info se guarda cuando uno se loguea en la pagina
                nombreUsuario = claimUsuario.Claims.Where(claim => claim.Type == ClaimTypes.Name). Select(claim => claim.Value).SingleOrDefault();
                urlFotoUsuario = ((ClaimsIdentity)claimUsuario.Identity).FindFirst("UrlFoto").Value;
            }

            //ViewData comparte informacion con la vista
            ViewData["nombreUsuario"] = nombreUsuario;
            ViewData["urlFotoUsuario"] = urlFotoUsuario;

            return View();
        }
    }
}
