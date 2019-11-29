using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using MVC_Chat.Models;
using MVC_Chat.Singleton;
using Newtonsoft.Json;
using Utilidades;

namespace MVC_Chat.Controllers
{
    public class PerfilController : Controller
    {
        // GET: Perfil
        public ActionResult HomePerfil()
        {
            var id = new Jwt().ObtenerId();

            if (id != "")
            {
                var direccion = "Perfil/" + id;
                var respuesta = Data.Instancia.GuatChatService.cliente.GetAsync(direccion);
                respuesta.Wait();

                var result = respuesta.Result;

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    var user = JsonConvert.DeserializeObject<User>(readTask.Result);
                    return View(user);
                }
                else if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        // GET: Perfil/Editar
        public ActionResult Editar()
        {
            var id = TempData["id"];

            var direccion = "Perfil/" + id;
            var respuesta = Data.Instancia.GuatChatService.cliente.GetAsync(direccion);
            respuesta.Wait();

            var result = respuesta.Result;

            var readTask = result.Content.ReadAsStringAsync();
            readTask.Wait();

            var user = JsonConvert.DeserializeObject<User>(readTask.Result);

            return View(user);
        }

        [HttpPost]
        public ActionResult Editar(FormCollection collection)
        {
            var user = new User();
            user.Id = new Jwt().ObtenerId();
            user.Nombre = collection["Nombre"];
            user.Apellido = collection["Apellido"];
            user.Contraseña = collection["Contraseña"];
            user.Username = collection["Username"];
            user.Telefono = int.Parse(collection["Telefono"]);

            var direccion = "Perfil/" + new Jwt().ObtenerId();
            var respuesta = Data.Instancia.GuatChatService.cliente.PutAsJsonAsync(direccion,user);
            respuesta.Wait();

            var result = respuesta.Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                return RedirectToAction("CerrarSesion");
            }

            TempData["id"] = new Jwt().ObtenerId();
            return RedirectToAction("Editar", "Perfil");

        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult MenuResult(FormCollection collection)
        {
            try
            {
                var id = new Jwt().ObtenerId();
                TempData["id"] = id;

                if (collection["CrearConversacion"] != null)
                {
                    return RedirectToAction("", "");
                }
                else if (collection["Conversaciones"] != null)
                {
                    return RedirectToAction("Conversaciones","Conversaciones");
                }
                else if (collection["EditarPerfil"] != null)
                {
                    return RedirectToAction("Editar");
                }
                else if (collection["EliminarPerfil"] != null)
                {
                    return RedirectToAction("Eliminar");
                }
                else if (collection["CerrarSesion"] != null)
                {
                    return RedirectToAction("CerrarSesion");
                }
            }
            catch (Exception e)
            {
                var error = e.Message;
                return RedirectToAction("HomePerfil");
            }

            return null;
        }

        public ActionResult CerrarSesion()
        {
            Data.Instancia.GuatChatService.cliente.DefaultRequestHeaders.Authorization = null;

            return RedirectToAction("Index", "Login");
        }

        public ActionResult Eliminar()
        {
            var id = TempData["id"];

            var direccion = "Perfil/" + id;
            var respuesta = Data.Instancia.GuatChatService.cliente.DeleteAsync(direccion);
            respuesta.Wait();

            var result = respuesta.Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                return RedirectToAction("CerrarSesion");
            }

            TempData["id"] = new Jwt().ObtenerId();
            return RedirectToAction("HomePerfil", "Perfil");

        }
    }
}
