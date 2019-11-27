using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace API_Chat.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string Nombre { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string Apellido { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string Username { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string Contraseña { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.Int32)]
        public int Telefono { get; set; }

        [BsonElement]
        public List<Message> Mensajes { get; set; }

        [BsonElement]
        public List<string> Conversaciones { get; set; } //Tendrá los id de los usuarios con los que se ha hablado?
    }
}
