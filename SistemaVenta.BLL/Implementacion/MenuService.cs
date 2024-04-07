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
    public class MenuService : IMenuService
    {
        private readonly IGenericRepository<Menu> _repositorioMenu;
        private readonly IGenericRepository<RolMenu> _repositorioRolMenu;
        private readonly IGenericRepository<Usuario> _repositorioUsuario;

        public MenuService(IGenericRepository<Menu> repositorioMenu, IGenericRepository<RolMenu> repositorioRolMenu, IGenericRepository<Usuario> repositorioUsuario)
        {
            _repositorioMenu = repositorioMenu;
            _repositorioRolMenu = repositorioRolMenu;
            _repositorioUsuario = repositorioUsuario;
        }

        public async Task<List<Menu>> ObtenerMenus(int idUsuario)
        {
            IQueryable<Usuario> tablaUsuario = await _repositorioUsuario.Consultar(usuario => usuario.IdUsuario == idUsuario);
            IQueryable<RolMenu> tablaRolMenu = await _repositorioRolMenu.Consultar();
            IQueryable<Menu> tablaMenu = await _repositorioMenu.Consultar();

            IQueryable<Menu> MenuPadre = (from usuario in tablaUsuario
                                          join rolMenu in tablaRolMenu on usuario.IdRol equals rolMenu.IdRol
                                          join menu in tablaMenu on rolMenu.IdMenu equals menu.IdMenu
                                          join menuPadre in tablaMenu on menu.IdMenuPadre equals menuPadre.IdMenu
                                          select menuPadre).Distinct().AsQueryable();

            IQueryable<Menu> MenuHijos = (from usuario in tablaUsuario
                                          join rolMenu in tablaRolMenu on usuario.IdRol equals rolMenu.IdRol
                                          join menu in tablaMenu on rolMenu.IdMenu equals menu.IdMenu
                                          where menu.IdMenu != menu.IdMenuPadre
                                          select menu ).Distinct().AsQueryable();

            List<Menu> listaMenu = (from menuPadre in MenuPadre
                                    select new Menu() { 
                                    Descripcion = menuPadre.Descripcion,
                                    Icono = menuPadre.Icono,
                                    Controlador = menuPadre.Controlador,
                                    PaginaAccion = menuPadre.PaginaAccion,
                                    InverseIdMenuPadreNavigation = (from menuHijo in MenuHijos
                                                                    where menuHijo.IdMenuPadre == menuPadre.IdMenu
                                                                    select menuHijo).ToList()
                                    }).ToList();

            return listaMenu;
        }
    }
}
