using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MVC_Chat.Service;

namespace MVC_Chat.Singleton
{
    public class Data
    {
        private static Data _instancia = null;

        public static Data Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = new Data();
                }

                return _instancia;
            }
        }

        public ApiService GuatChatService = new ApiService("http://localhost:51372/GuatChat/user/");
    }
}