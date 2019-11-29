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
    public class ConversacionesController : Controller
    {
        // GET: Conversaciones
        public ActionResult Conversaciones()
        {
            string id = new Jwt().ObtenerId();

            if (id != "")
            {
                var direccion = "Conversaciones/" + id;
                var respuesta = Data.Instancia.GuatChatService.cliente.GetAsync(direccion);
                respuesta.Wait();

                var result = respuesta.Result;

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    var ConversacionesUser = JsonConvert.DeserializeObject<List<string>>(readTask.Result);

                    var listaConversaciones = new List<string>();


                    foreach (var conversacion in ConversacionesUser)
                    {
                        listaConversaciones.Add(conversacion.Split('.')[0]);
                    }

                    return View(listaConversaciones);
                }
                else
                {
                    return RedirectToAction("HomePerfil", "Perfil");
                }

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
            

            var us = new User();
            us.Conversaciones = new List<string>();
            us.Conversaciones.Add("Chian");
            us.Conversaciones.Add("Andrita tu amor");
            us.Conversaciones.Add("Diana tu love");
            return View(us);
        }
    }
}