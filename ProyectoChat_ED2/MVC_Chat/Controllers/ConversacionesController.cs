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
        }

        public ActionResult CrearConversacion()
        {
            string id = new Jwt().ObtenerId();

            if (id != "")
            {
                var direccion = "AllUsers/" + id;
                var respuesta = Data.Instancia.GuatChatService.cliente.GetAsync(direccion);
                respuesta.Wait();

                var result = respuesta.Result;

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    var usuariosRegistrados = JsonConvert.DeserializeObject<List<User>>(readTask.Result);
                    
                    return View(usuariosRegistrados);
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
        }

        public ActionResult Mensajes(string id)
        {
            //El id recibido será el del receptor
            var idEmisor = new Jwt().ObtenerId();

            if (idEmisor != "")
            {
                var direccion = "Perfil/" + idEmisor;
                var respuesta = Data.Instancia.GuatChatService.cliente.GetAsync(direccion);
                respuesta.Wait();

                var result = respuesta.Result;

                var readTask = result.Content.ReadAsStringAsync();
                readTask.Wait();

                var userEmisor = JsonConvert.DeserializeObject<User>(readTask.Result);

                var UserReceptor = "";

                foreach (var itemConversacion in userEmisor.Conversaciones)
                {
                    if (itemConversacion.Split('.')[1] == id)
                    {
                        UserReceptor = itemConversacion.Split('.')[0];
                    }
                }

                if (UserReceptor != "")
                {
                    TempData["userReceptor"] = UserReceptor;
                }
                else
                {
                    var direccion2 = "Perfil/" + id;
                    var respuesta2 = Data.Instancia.GuatChatService.cliente.GetAsync(direccion2);
                    respuesta2.Wait();

                    var result2 = respuesta2.Result;

                    var readTask2 = result2.Content.ReadAsStringAsync();
                    readTask2.Wait();

                    var userReceptor = JsonConvert.DeserializeObject<User>(readTask2.Result);

                    var Receptor = userReceptor.Username;

                    TempData["userReceptor"] = Receptor;
                }

                ViewBag.userReceptor = TempData["userReceptor"];

                var direccionMensajes = "Chat/" + idEmisor + "/" + id;
                var respuestaMensajes = Data.Instancia.GuatChatService.cliente.GetAsync(direccionMensajes);
                respuestaMensajes.Wait();

                var resultMensaje = respuestaMensajes.Result;

                ViewBag.Emisor = idEmisor;
                ViewBag.Receptor = id;

                if (resultMensaje.StatusCode == HttpStatusCode.OK)
                {
                    var readTaskMessages = resultMensaje.Content.ReadAsStringAsync();
                    readTaskMessages.Wait();

                    var listaMensajes = JsonConvert.DeserializeObject<List<Message>>(readTaskMessages.Result);

                    return View(listaMensajes);
                }
                return RedirectToAction("HomePerfil","Perfil");
            }
            else
            {
                return RedirectToAction("CerrarSesion", "Perfil");
            }
            
        }

        public ActionResult UserNameToId(string usernameReceptor)
        {
            var idEmisor = new Jwt().ObtenerId();

            if (idEmisor != "")
            {
                var direccion = "Perfil/" + idEmisor;
                var respuesta = Data.Instancia.GuatChatService.cliente.GetAsync(direccion);
                respuesta.Wait();

                var result = respuesta.Result;

                var readTask = result.Content.ReadAsStringAsync();
                readTask.Wait();

                var user = JsonConvert.DeserializeObject<User>(readTask.Result);

                var idReceptor = "";

                foreach (var itemConversacion in user.Conversaciones)
                {
                    if (itemConversacion.Split('.')[0] == usernameReceptor)
                    {
                        idReceptor = itemConversacion.Split('.')[1];
                    }
                }
                return RedirectToAction("Mensajes", new { id = idReceptor });
            }

            return RedirectToAction("Conversaciones", "Conversaciones");

        }

        [HttpPost]
        public ActionResult MandarMensajes(FormCollection collection)
        {
            var idReceptor = TempData["receptor"];
            var cuerpoMensaje = collection["Contenido"];
            var idEmisor = new Jwt().ObtenerId();

            var mensaje = new Message();
            mensaje.Contenido = cuerpoMensaje;

            TempData.Remove("receptor");

            if (idEmisor != "")
            {
                var direccion = "Chat/" + idEmisor + "/" + idReceptor;
                var respuesta = Data.Instancia.GuatChatService.cliente.PutAsJsonAsync(direccion,mensaje);
                respuesta.Wait();

                var result = respuesta.Result;

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();
                    return RedirectToAction("Mensajes", new { id = idReceptor });
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
        }
    }
}