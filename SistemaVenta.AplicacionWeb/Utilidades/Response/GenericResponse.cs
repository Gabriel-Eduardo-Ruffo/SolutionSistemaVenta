namespace SistemaVenta.AplicacionWeb.Utilidades.Response
{
    /// <summary>
    /// Formato de respuesta que vamos a dar a cada solicitud que se haga al sitio web
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    public class GenericResponse<TObject>
    {
        public bool Estado { get; set; }
        public string? Mensaje { get; set; }
        public TObject? Objeto { get; set; }
        public List<TObject>? ListaObjeto { get; set; }
    }
}
