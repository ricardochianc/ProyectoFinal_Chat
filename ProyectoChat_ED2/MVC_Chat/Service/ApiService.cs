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
        private const string baseAdress = "http://localhost:51372/";
        public static HttpClient cliente = new HttpClient();

        public ApiService()
        {
            cliente.BaseAddress = new Uri(baseAdress);
        }

        public async Task<Jwt> GetToken(User usuario)
        {
            //HttpResponseMessage search =  await cliente.GetAsync("/Perfil/" + usuario.Id);



            //if (search.StatusCode == HttpStatusCode.OK)
            //{

            var resultado = cliente.PostAsJsonAsync("GuatChat/User/authenticate", usuario);
            resultado.Wait();

            if (resultado.Result.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<Jwt>(resultado.ToString());
            }
            return null;
        }
    }
}