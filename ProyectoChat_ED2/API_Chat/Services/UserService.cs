using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Driver.GridFS;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using API_Chat.Models;

namespace API_Chat.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _Users;
        private readonly IMongoCollection<Doc> _documents;
        private readonly IMongoDatabase _db;
        private readonly AppSettings _appSettings;
        private GridFSBucket gfs;

        public UserService(IConfiguration config, IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;

            var client = new MongoClient(config.GetConnectionString("GuatChatDB"));

            var dataBase = client.GetDatabase("GuatChatDB");            
            _Users = dataBase.GetCollection<User>("UsuariosGuatChat");
            _documents = dataBase.GetCollection<Doc>("DocumentosGuatChat");

            _db = dataBase;
            gfs = new GridFSBucket(dataBase);
        }

        public List<User> GetAllUsers(string id)
        {
            return _Users.Find(users => users.Id != id).ToList();
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

            if (listaUsuarios.Find(_usuario => _usuario.Username == usuario.Username) == null)
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
        public string UpdateMessageEmisor(string id, Message objMensaje)
        {
            var emisorFiltrer = Builders<User>.Filter.Eq("Id", id);
            var update = Builders<User>.Update.Push("Mensajes", objMensaje);

            _Users.UpdateOne(emisorFiltrer, update);

            var receptorFiltrer = Builders<User>.Filter.Eq("Id", objMensaje.Receptor);

            if (_Users.Find(receptorFiltrer).ToList().Count != 0) //Si es distinto de cero es porque si encontró al usuario en la collecion
            {
                var _receptor = _Users.Find(receptorFiltrer).ToList()[0];
                var infoReceptor = _receptor.Username + "." + _receptor.Id; //username.12lkji2912ojad21G

                var conversacionesFiltrer = Builders<User>.Filter.Eq("Id", objMensaje.Emisor) & Builders<User>.Filter.Eq("Conversaciones", infoReceptor);
                
                if (_Users.Find(conversacionesFiltrer).ToList().Count == 0)
                {
                    
                    var updateConversacion = Builders<User>.Update.Push("Conversaciones", infoReceptor);

                    _Users.UpdateOne(emisorFiltrer, updateConversacion);
                }
            }
            
            var us = _Users.Find(emisorFiltrer).ToList();

            return us[0].Username +"." +us[0].Id;
        }

        /// <summary>
        /// Método que manda y crea una conversación, actualiza mensajes de receptor y emisor
        /// </summary>
        /// <param name="idReceptor"> Id del receptor de la conversacion</param>
        /// <param name="usernameCompuestoEmisor">username del emisor del mensaje, este caso compuesto por username y id</param>
        /// <param name="objMensaje">Objeto mensaje que será creado en la vista del chat</param>
        public bool UpdateMessageReceptor(string idReceptor, string usernameCompuestoEmisor, Message objMensaje)
        {
            var userFiltrer = Builders<User>.Filter.Eq("Id", idReceptor);

            if (_Users.Find(userFiltrer).ToList().Count == 1) //Si ya se elimino retornará false
            {
                var conversacionesFiltrer = Builders<User>.Filter.Eq("Id", idReceptor) & Builders<User>.Filter.Eq("Conversaciones", usernameCompuestoEmisor);

                if (_Users.Find(conversacionesFiltrer).ToList().Count == 0)
                {
                    var updateConversacion = Builders<User>.Update.Push("Conversaciones", usernameCompuestoEmisor);

                    _Users.UpdateOne(userFiltrer, updateConversacion);
                }

                var update = Builders<User>.Update.Push("Mensajes", objMensaje);
                _Users.UpdateOne(userFiltrer, update);

                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Obtiene los mensajes de la conversación entre emisor y receptor
        /// </summary>
        /// <param name="idEmisor">Viene de la ruta</param>
        /// <param name="idReceptor">Viene de la ruta</param>
        /// <returns></returns>
        public List<Message> GetMessages(string idEmisor,string idReceptor)
        {
            var Usuario = _Users.Find(user => user.Id == idEmisor).FirstOrDefault();
            var enviados = Usuario.Mensajes.FindAll(mensaje => (mensaje.Emisor == idEmisor && mensaje.Receptor == idReceptor) || (mensaje.Emisor == idReceptor && mensaje.Receptor == idEmisor));
            
            return enviados;
        }

        public List<string> GetConversations(string idEmisor)
        {
            var Usuario = _Users.Find(user => user.Id == idEmisor).FirstOrDefault();
            return Usuario.Conversaciones;
        }

        //***************************************************************AUTENTICACION E INICIO SE SESION*********************************************************************************************************************************************************
        
        public Jwt Authenticate(string userName, string password)
        {
            var user = _Users.Find(userX => userX.Username == userName && userX.Contraseña == password).FirstOrDefault();

            if(user == null)
            {
                return null;
            }

            var jwt = new Jwt();
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            jwt.Token = tokenHandler.WriteToken(token);
            //jwt.Token = token.ToString();

            return jwt;
        }

        //************************************************************************ARCHIVOS***************************************************************************************
        //FALTA PROBARLO
        public bool SendDocument(string fileName, byte[] fileBytes)
        {
            
            GridFSBucket gfs = new GridFSBucket(_db);            
            var objectId = gfs.UploadFromBytesAsync(fileName, fileBytes);

            return true;
        }

        public Doc SendDocuments(Doc document)
        {
            _documents.InsertOne(document);

            return document;
        }

        public List<Doc> GetDocuments(string usId)
        {
            var listaDocs = _documents.Find(doc => doc.EmisorId == usId || doc.ReceptorId == usId).ToList();

            return listaDocs;
        }

        public Doc DownloadDocument(string fileName)
        {
            var doc = _documents.Find(docX => docX.DocName == fileName).FirstOrDefault();

            return doc;
        }
    }
}