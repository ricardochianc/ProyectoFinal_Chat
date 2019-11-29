using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Hanssens.Net;
using MVC_Chat.Models;
using MVC_Chat.Singleton;

namespace MVC_Chat.Controllers
{
    public class LoginController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(FormCollection collection)
        {
            var user = new User();

            user.Username = collection["Username"];
            user.Contraseña = collection["Contraseña"];

            var result = Data.Instancia.GuatChatService.GetToken(user);
            

            if (result.Result != null)
            {
                var jwt = result.Result;

                using (var storage = new LocalStorage())
                {
                    storage.Store("Token", jwt.Token);
                    storage.Store("id", user.Username);
                }

                TempData["token"] = jwt.Token;

                return RedirectToAction("Details", "Perfil");
            }

            return RedirectToAction("Index");
        }
    }
}