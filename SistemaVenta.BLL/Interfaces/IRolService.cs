using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Interfaces
{
    public interface IRolService
    {
        //Rol es una clase del tipo Identity que representa los campos de la tabla Rol en la DB
        Task<List<Rol>> Lista();
    }
}
