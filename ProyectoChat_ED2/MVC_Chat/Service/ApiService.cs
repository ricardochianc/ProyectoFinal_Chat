using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using MVC_Chat.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MVC_Chat.Service
{
    public class ApiService
    {
        public HttpClient cliente = new HttpClient();

        public ApiService(string baseAdress)
        {
            cliente.BaseAddress = new Uri(baseAdress);
        }
        
    }
}