using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MVC_Chat.Singleton;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace MVC_Chat.Models
{
    public class Jwt
    {
        public string Token { get; set; }

        public string ObtenerId()
        {
            var id = "";

            if (Data.Instancia.GuatChatService.cliente.DefaultRequestHeaders.Authorization != null)
            {


                var tokenHeader = Data.Instancia.GuatChatService.cliente.DefaultRequestHeaders.Authorization.Parameter;

                var tokenHandler = new JwtSecurityTokenHandler();

                var jwtToken = tokenHandler.ReadJwtToken(tokenHeader);

                var listaClaims = jwtToken.Claims.ToList();

                id = listaClaims.Find(x => x.Type == "unique_name").Value.ToString();
            }

            return id;
        }
    }
}