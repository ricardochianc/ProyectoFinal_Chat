using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC_Chat.Models;

namespace MVC_Chat.Controllers
{
    public class PerfilController : Controller
    {
        // GET: Perfil
        public ActionResult HomePerfil()
        {
            var user = new User();
            user.Id = "1";
            user.Nombre = "pablo";
            user.Apellido = "garcia";
            user.Username = "pjgm14";
            user.Contraseña = "1234";
            user.Telefono = 0;
            return View(user);
        }

        // GET: Perfil/Editar
        public ActionResult Editar()
        {
            var user = new User();
            user.Id = "1";
            user.Nombre = "pablo";
            user.Apellido = "garcia";
            user.Username = "pjgm14";
            user.Contraseña = "1234";
            user.Telefono = 0;

            return View(user);
        }
    }
}
