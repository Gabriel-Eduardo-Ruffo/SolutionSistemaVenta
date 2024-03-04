using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.Entity;
using System.Globalization;
using AutoMapper;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;

namespace SistemaVenta.AplicacionWeb.Utilidades.Automapper
{
    public class AutoMapperProfile : Profile
    {
        //Definir como va a ser la conversion de Models a View Models y de View Models a Models
        public AutoMapperProfile()
        {
            //CreateMap Hace la conversion de Origen hacia destino
            //ReverseMap, hace la conversion de origen a destino o de destino a origen
            //ForMember, se configura en el tipo o formato de property segun se necesite
            #region Rol
            CreateMap<Rol, VMRol>().ReverseMap();
            #endregion Rol

            #region Usuario
            CreateMap<Usuario, VMUsuario>()
                .ForMember(destino => destino.EsActivo, opciones => opciones.MapFrom(origen => origen.EsActivo == true? 1: 0))
                .ForMember(destino => destino.NombreRol, opciones => opciones.MapFrom(origen => origen.IdRolNavigation.Descripcion));

            CreateMap<VMUsuario, Usuario>()
                .ForMember(destino => destino.EsActivo, opciones => opciones.MapFrom(origen => origen.EsActivo == 1? true : false))
                .ForMember(destino => destino.IdRolNavigation, opciones => opciones.Ignore());
            #endregion Usuario

            #region Negocio
            CreateMap<Negocio, VMNegocio>()
                .ForMember(destino => destino.PorcentajeImpuesto, opciones => opciones.MapFrom(origen => Convert.ToString(origen.PorcentajeImpuesto.Value, new CultureInfo("es-AR"))));

            CreateMap<VMNegocio, Negocio>()
                .ForMember(destino => destino.PorcentajeImpuesto, opciones => opciones.MapFrom(origen => Convert.ToDecimal(origen.PorcentajeImpuesto, new CultureInfo("es-AR"))));
            #endregion Negocio

            #region Categoria
            CreateMap<Categoria, VMCategoria>()
                .ForMember(destino => destino.EsActivo, opciones => opciones.MapFrom(origen => origen.EsActivo == true? 1: 0));

            CreateMap<VMCategoria, Categoria>()
                .ForMember(destino => destino.EsActivo, opciones => opciones.MapFrom(origen => origen.EsActivo == 1? true : false));
            #endregion Categoria

            #region Producto
            CreateMap<Producto, VMProducto>()
                .ForMember(destino => destino.EsActivo, opciones => opciones.MapFrom(origen => origen.EsActivo == true? 1: 0))
                .ForMember(destino => destino.NombreCategoria, opciones => opciones.MapFrom(origen => origen.IdCategoriaNavigation.Descripcion))
                .ForMember(destino => destino.Precio, opciones => opciones.MapFrom(origen => Convert.ToString(origen.Precio.Value, new CultureInfo("es-AR"))));

            CreateMap<VMProducto, Producto>()
                .ForMember(destino => destino.EsActivo, opciones => opciones.MapFrom(origen => origen.EsActivo == 1 ? true : false))
                .ForMember(destino => destino.IdCategoriaNavigation, opciones => opciones.Ignore())
                .ForMember(destino => destino.Precio, opciones => opciones.MapFrom(origen => Convert.ToDecimal(origen.Precio, new CultureInfo("es-AR"))));
            #endregion Producto

            #region TipoDocumentoVenta
            CreateMap<TipoDocumentoVenta, VMTipoDocumentoVenta>().ReverseMap();
            #endregion TipoDocumentoVenta

            #region Venta
            CreateMap<Venta, VMVenta>()
                .ForMember(destino => destino.TipoDocumentoVenta, opciones => opciones.MapFrom(origen => origen.IdTipoDocumentoVentaNavigation.Descripcion))
                .ForMember(destino => destino.Usuario, opciones => opciones.MapFrom(origen => origen.IdUsuarioNavigation.Nombre))
                .ForMember(destino => destino.SubTotal, opciones => opciones.MapFrom(origen => Convert.ToString(origen.SubTotal.Value, new CultureInfo("es-AR"))))
                .ForMember(destino => destino.ImpuestoTotal, opciones => opciones.MapFrom(origen => Convert.ToString(origen.ImpuestoTotal.Value, new CultureInfo("es-AR"))))
                .ForMember(destino => destino.Total, opciones => opciones.MapFrom(origen => Convert.ToString(origen.Total.Value, new CultureInfo("es-AR"))))
                .ForMember(destino => destino.FechaRegistro, opciones => opciones.MapFrom(origen => origen.FechaRegistro.Value.ToString("dd/MM/yyyy HH:mm:ss")));

            CreateMap<VMVenta, Venta>()
                .ForMember(destino => destino.SubTotal, opciones => opciones.MapFrom(origen => Convert.ToDecimal(origen.SubTotal, new CultureInfo("es-AR"))))
                .ForMember(destino => destino.ImpuestoTotal, opciones => opciones.MapFrom(origen => Convert.ToDecimal(origen.ImpuestoTotal, new CultureInfo("es-AR"))))
                .ForMember(destino => destino.Total, opciones => opciones.MapFrom(origen => Convert.ToDecimal(origen.Total, new CultureInfo("es-AR"))));
            #endregion Venta

            #region DetalleVenta
            CreateMap<DetalleVenta, VMDetalleVenta>()
                .ForMember(destino => destino.Precio, opciones => opciones.MapFrom(origen => Convert.ToString(origen.Precio.Value, new CultureInfo("es-AR"))))
                .ForMember(destino => destino.Total, opciones => opciones.MapFrom(origen => Convert.ToString(origen.Total.Value, new CultureInfo("es-AR"))));

            CreateMap<VMDetalleVenta, DetalleVenta>()
                .ForMember(destino => destino.Precio, opciones => opciones.MapFrom(origen => Convert.ToDecimal(origen.Precio, new CultureInfo("es-AR"))))
                .ForMember(destino => destino.Total, opciones => opciones.MapFrom(origen => Convert.ToDecimal(origen.Total, new CultureInfo("es-AR"))));

            CreateMap<DetalleVenta, VMReporteVenta>()
                .ForMember(destino => destino.FechaRegistro, opcion => opcion.MapFrom(origen => origen.IdVentaNavigation.FechaRegistro.Value.ToString("dd/MM/yyyy")))
                .ForMember(destino => destino.NumeroVenta, opcion => opcion.MapFrom(origen => origen.IdVentaNavigation.NumeroVenta))
                .ForMember(destino => destino.TipoDocumento, opcion => opcion.MapFrom(origen => origen.IdVentaNavigation.IdTipoDocumentoVentaNavigation.Descripcion))
                .ForMember(destino => destino.DocumentoCliente, opcion => opcion.MapFrom(origen => origen.IdVentaNavigation.DocumentoCliente))
                .ForMember(destino => destino.NombreCliente, opcion => opcion.MapFrom(origen => origen.IdVentaNavigation.NombreCliente))
                .ForMember(destino => destino.SubTotalVenta, opcion => opcion.MapFrom(origen => Convert.ToString(origen.IdVentaNavigation.SubTotal.Value, new CultureInfo("es-AR"))))
                .ForMember(destino => destino.ImpuestoTotalVenta, opcion => opcion.MapFrom(origen => Convert.ToString(origen.IdVentaNavigation.ImpuestoTotal.Value, new CultureInfo("es-AR"))))
                .ForMember(destino => destino.TotalVenta, opcion => opcion.MapFrom(origen => Convert.ToString(origen.IdVentaNavigation.Total.Value, new CultureInfo("es-AR"))))
                .ForMember(destino => destino.Producto, opcion => opcion.MapFrom(origen => origen.DescripcionProducto))
                .ForMember(destino => destino.Precio, opcion => opcion.MapFrom(origen => Convert.ToString(origen.Precio.Value, new CultureInfo("es-AR"))))
                .ForMember(destino => destino.Total, opcion => opcion.MapFrom(origen => Convert.ToString(origen.Total.Value, new CultureInfo("es-AR"))));
            #endregion DetalleVenta

            #region Menu
            CreateMap<Menu, VMMenu>()
                .ForMember(destino => destino.SubMenus, opcion => opcion.MapFrom(origen => origen.InverseIdMenuPadreNavigation));
            #endregion Menu
        }
    }
}
