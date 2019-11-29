using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;
using MVC_Chat.Models;
using MVC_Chat.Singleton;
using Newtonsoft.Json;

namespace MVC_Chat.Controllers
{
    public class CrearUsuarioController : Controller
    {
        // GET: CrearUsuario
        public ActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Crear(FormCollection collection)
        {
            var user = new User();

            user.Nombre = collection["Nombre"];
            user.Apellido = collection["Apellido"];
            user.Username = collection["Username"];
            user.Contraseña = collection["Contraseña"];
            user.Telefono = int.Parse(collection["Telefono"]);

            var respuesta = Data.Instancia.GuatChatService.cliente.PostAsJsonAsync("Create", user);
            respuesta.Wait();

            var result = respuesta.Result;

            if (result.StatusCode == HttpStatusCode.Created)
            {
                var readTask = result.Content.ReadAsStringAsync();
                readTask.Wait();

                return RedirectToAction("Index", "Login");
            }

            return RedirectToAction("Crear");
        }
    }
}