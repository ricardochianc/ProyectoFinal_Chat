using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_Chat.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;

namespace API_Chat.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _Users;

        public UserService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("GuatChatDB"));
            var dataBase = client.GetDatabase("GuatChatDB");

            _Users = dataBase.GetCollection<User>("UsuariosGuatChat");
        }

        public User Get(string id)
        {
            return _Users.Find(usuario => usuario.Id == id).FirstOrDefault();
        }

        public User Create(User usuario)
        {
            _Users.InsertOne(usuario);
            return usuario;
        }

        public void UpdateUser(string id, User usuario)
        {
            _Users.ReplaceOne(user => user.Id == id, usuario);
        }

        public void Remove(string id)
        {
            _Users.DeleteOne(usario => usario.Id == id);
        }

        public void UpdateConversations(string id)
        {
            //_Users.UpdateOne(usuario => )
        }
    }
}
