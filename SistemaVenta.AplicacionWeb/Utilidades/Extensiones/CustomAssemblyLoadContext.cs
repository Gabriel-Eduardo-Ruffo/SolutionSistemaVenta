using System.Reflection;
using System.Runtime.Loader;

namespace SistemaVenta.AplicacionWeb.Utilidades.Extensiones
{
    /// <summary>
    /// Esta clase permite trabajar con extensiones externas en nuestro proyecto.
    /// Se utiliza para usar la extension de la libreria de PDF (dll en la carpeta LibreriaPDF)
    /// </summary>
    public class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        public IntPtr LoadUnmanagedLibrary(string absolutePath)
        {
            return LoadUnmanagedDll(absolutePath);
        }
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            return LoadUnmanagedDllFromPath(unmanagedDllName);
        }
        protected override Assembly Load(AssemblyName assemblyName)
        {
            throw new NotImplementedException();
        }
    }
}
