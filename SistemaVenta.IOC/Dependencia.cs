using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.DAL.Implementacion;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.BLL.Implementacion;
using SistemaVenta.BLL.Interfaces;


namespace SistemaVenta.IOC
{
    public static class Dependencia
    {
        public static void InyectarDependencias(this IServiceCollection services, IConfiguration configuration)
        {
            //Insertar opciones de conexion a DB (el contexto de conexion)
            services.AddDbContext<DBVENTAContext>(options => options.UseSqlServer(configuration.GetConnectionString("CadenaSQL")));
            
            //Insertar repositorio generico (para realizar AMB en base de datos) - (puede ser de cualquier tipo, por eso no se especifica el tipo que maneja)
            services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            //Insertar el repositorio de ventas (es del tipo Venta)
            services.AddScoped<IVentaRepository, VentaRepository>();

            //Insertar el repositorio de correos
            services.AddScoped<ICorreoService, CorreoService>();

            //Insertar el repositorio de Firebase
            services.AddScoped<IFireBaseService, FireBaseService>();

            //Insertar el repositorio de Utilidades
            services.AddScoped<IUtilidadesService, UtilidadesService>();

            //Inserta el repositorio de Roles
            services.AddScoped<IRolService, RolService>();

            //Inserta el repositorio de Usuarios
            services.AddScoped<IUsuarioService, UsuarioService>();

            //Inserta el repositorio de Negocio
            services.AddScoped<INegocioService, NegocioService>();

            //Inserta el repositorio de Categoria
            services.AddScoped<ICategoriaService, CategoriaService>();

            //Inserta el repositorio de Producto
            services.AddScoped<IProductoService, ProductoService>();

            //Inserta el repositorio de Tipo de Documento de Venta
            services.AddScoped<ITipoDocumentoVentaService, TipoDocumentoVentaService>();

            //Inserta el repositorio de Venta
            services.AddScoped<IVentaService, VentaService>();

            //Inserta el repositorio de DashBoard
            services.AddScoped<IDashBoardService, DasBoardService>();
        }
    }
}