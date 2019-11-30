using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_Chat.Models
{
    public class Message
    {
        public string Emisor { get; set; }

        public string Receptor { get; set; }

        public string Fecha { get; set; }

        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "Debe de ingresar su nombre")]
        public string Contenido { get; set; }

        public Message()
        {
            Emisor = "";
            Receptor = "";
            Fecha = "";
        }

        public override string ToString()
        {
            return Contenido + "\nFecha: " + Fecha;
        }
    }
}