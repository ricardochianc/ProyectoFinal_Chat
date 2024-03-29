﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Chat.Models
{
    public class Doc
    {
        public string Id { get; set; }

        public string DocName { get; set; }

        public string EmisorId { get; set; }

        public string ReceptorId { get; set; }

        public string Contenido { get; set; }

        public DateTime FechaUp { get; set; }

        public Doc()
        {
            Id = "";
            DocName = "";
            EmisorId = "";
            ReceptorId = "";
            Contenido = "";
            FechaUp = DateTime.Now;
        }
    }
}