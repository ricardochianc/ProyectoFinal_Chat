using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Chat.Models
{
    public class Message
    {
        public string Id { get; set; }
                
        public string Emisor { get; set; }
                
        public string Receptor { get; set; }
                
        public string Fecha { get; set; }
                
        public string Contenido { get; set; }
    }
}