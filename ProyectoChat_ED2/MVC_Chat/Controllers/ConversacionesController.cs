using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC_Chat.Models;

namespace MVC_Chat.Controllers
{
    public class ConversacionesController : Controller
    {
        // GET: Conversaciones
        public ActionResult Conversaciones()
        {
            var us = new User();
            us.Conversaciones = new List<string>();
            us.Conversaciones.Add("Chian");
            us.Conversaciones.Add("Andrita tu amor");
            us.Conversaciones.Add("Diana tu love");
            return View(us);
        }
    }
}