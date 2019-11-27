using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_Chat.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using Newtonsoft;
using JsonConvert = Newtonsoft.Json.JsonConvert;

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

        /// <summary>
        /// Obtiene un usuario en específico partiendo de un id generado por Mongo
        /// </summary>
        /// <param name="id">Identificador de usuario almacenado en el Json</param>
        /// <returns></returns>
        public User Get(string id)
        {
            return _Users.Find(usuario => usuario.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// Crea un usuario en la base de datos
        /// </summary>
        /// <param name="usuario">Un objeto de tipo usuario creado desde el front</param>
        /// <returns></returns>
        public User Create(User usuario)
        {
            var listaUsuarios = _Users.Find(user => true).ToList();

            if (listaUsuarios.Find(_usuario => _usuario.Usuario == usuario.Usuario) == null)
            {
                _Users.InsertOne(usuario);
            }
            else
            {
                usuario = null;
            }

            return usuario;
        }

        /// <summary>
        /// Se actualiza el usuario, sirve para la edición de usuario
        /// </summary>
        /// <param name="id"></param>
        /// <param name="usuario"></param>
        public void UpdateUser(string id, User usuario)
        {
            _Users.ReplaceOne(user => user.Id == id, usuario);
        }

        /// <summary>
        /// Elimina un usuario en específico
        /// </summary>
        /// <param name="id">Identificador único generado por MongoDB</param>
        public void Remove(string id)
        {
            _Users.DeleteOne(usario => usario.Id == id);
        }

        /// <summary>
        /// Agrega un mensaje a la lista de mensajes del usuario
        /// </summary>
        /// <param name="id">Identificador del usario que actúa como emisor</param>
        /// <param name="objMensaje">Objeto de tipo mensaje generado por front</param>
        public void UpdateMessage(string id, object objMensaje)
        {
            var newSubDocument = BsonDocument.Parse(JsonConvert.SerializeObject(objMensaje));
            newSubDocument.Remove("Id");

            string ID = ObjectId.GenerateNewId().ToString();
            newSubDocument.Add(new BsonElement("Id", ID));
            var filtrer = Builders<User>.Filter.Eq("Id", ObjectId.Parse(id));
            var update = Builders<User>.Update.Push("Mensajes", newSubDocument);

            _Users.UpdateOne(filtrer, update);

        }

        /// <summary>
        /// Obtiene los mensajes del receptor, quiere decir que devulve los mensajes con el usuario que se está hablando
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<User> GetMessages(string id) //Este id sería el del receptor
        {
            var messagefiltrer = Builders<User>.Filter.ElemMatch<BsonElement>(
                "Mensajes", new BsonDocument {{"Receptor", id}});

            return  _Users.Find(messagefiltrer).ToList();

        }
    }
}