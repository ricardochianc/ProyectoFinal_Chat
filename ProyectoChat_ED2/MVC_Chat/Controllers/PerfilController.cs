using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MVC_Chat.Models;
using MVC_Chat.Singleton;
using Newtonsoft.Json;

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
            var user = new User();
            user.Id = "1";
            user.Nombre = "pablo";
            user.Apellido = "garcia";
            user.Username = "pjgm14";
            user.Contraseña = "1234";
            user.Telefono = 0;

            return View(user);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult MenuResult(FormCollection collection)
        {
            try
            {
                if (collection["CrearConversacion"] != null)
                {
                    return RedirectToAction("", "");
                }
                else if (collection["Conversaciones"] != null)
                {
                    return RedirectToAction("", "");
                }
                else if (collection["EditarPerfil"] != null)
                {
                    return RedirectToAction("", "");
                }
                else if (collection["EliminarPerfil"] != null)
                {
                    return RedirectToAction("", "");
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
    }
}
