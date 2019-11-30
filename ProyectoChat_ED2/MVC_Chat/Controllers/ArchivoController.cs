using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;
using MVC_Chat.Models;
using MVC_Chat.Singleton;

namespace MVC_Chat.Controllers
{
    public class ArchivoController : Controller
    {
        // GET: Archivo
        public ActionResult Index()
        {
            var idReceptor = TempData["receptor"];
            ViewBag.Receptor = idReceptor;

            var userNameReceptor = TempData["receptorUser"];
            ViewBag.userReceptor = userNameReceptor;

            return View();
        }

        // POST: Archivo/Create
        [HttpPost]
        public ActionResult Cargar(HttpPostedFileBase postedFile)
        {
            try
            {
                var idReceptor = TempData["receptor"];
                var filePath = string.Empty;

                var path = Server.MapPath("~/MisArchivos/");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var RutaAbsolutaServer = path;
                var RutaAbsolutaArchivo = "";
                var nombre = postedFile.FileName.Split('.')[0];
                
                filePath = path + Path.GetFileName(postedFile.FileName);
                RutaAbsolutaArchivo = filePath;
                postedFile.SaveAs(filePath);

                var Huffman = new Utilidades.Huffman();
                Huffman.RutaAbosolutaServer = RutaAbsolutaServer;
                Huffman.RutaAbsolutaArchivoOriginal = RutaAbsolutaArchivo;
                Huffman.NombreArchivoOriginal = nombre;
                Huffman.Comprimir();

                var SDES = new Utilidades.SDES(Huffman.NombreArchivoOperado,RutaAbsolutaServer + Huffman.NombreArchivoOperado,RutaAbsolutaServer,250);
                SDES.Operar(1);

                var doc = new Doc();
                doc.DocName = nombre;
                doc.EmisorId = new Jwt().ObtenerId();
                doc.ReceptorId = idReceptor.ToString();

                using (var file = new FileStream(SDES.RutaAbsolutaArchivoOperado,FileMode.Open))
                {
                    using (var reader = new BinaryReader(file,Encoding.UTF8))
                    {
                        var contenido = new byte[reader.BaseStream.Length];
                        contenido = reader.ReadBytes(contenido.Length);

                        var chars = Encoding.UTF8.GetChars(contenido);

                        foreach (var caracter in chars)
                        {
                            doc.Contenido += caracter;
                        }
                    }
                }

                var res = Data.Instancia.GuatChatService.cliente.PostAsJsonAsync("sendoc", doc);
                res.Wait();

                var result = res.Result;

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    System.IO.File.Delete(SDES.RutaAbsolutaArchivoOperado);
                    return RedirectToAction("Conversaciones", "Conversaciones");
                }
                else
                {
                    return RedirectToAction("HomePerfil", "Perfil");
                }

                
            }
            catch
            {
                return RedirectToAction("HomePerfil", "Perfil");
            }
        }


        public ActionResult MisDocumentos()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Obtener()
        {

        }

        public ActionResult Descargar()
        {

        }
    }

}
