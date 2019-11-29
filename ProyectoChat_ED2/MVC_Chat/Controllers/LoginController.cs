using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using Hanssens.Net;
using MVC_Chat.Models;
using MVC_Chat.Singleton;
using Newtonsoft.Json;

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

            var res = Data.Instancia.GuatChatService.cliente.PostAsJsonAsync("authenticate", user);
            res.Wait();

            var result = res.Result;

            if (result.StatusCode == HttpStatusCode.Accepted)
            {
                var readTask = result.Content.ReadAsStringAsync();
                readTask.Wait();

                var jwt = JsonConvert.DeserializeObject<Jwt>(readTask.Result);

                using (var storage = new LocalStorage())
                {
                    storage.Clear();
                    storage.Store("Token", jwt.Token);
                    storage.Store("id", user.Username);

                    storage.Get("Token");
                }

                TempData["token"] = jwt.Token;

                return RedirectToAction("Details", "Perfil");
            }
            //Aqui va algo para decirle al usuario que su usuario y contraseña

            return RedirectToAction("Index");
        }
    }
}