using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API_Chat.Models
{
    public class Message
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string Emisor { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string Receptor { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.String)] //Probar DATETIME
        public string Fecha { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string Contenido { get; set; }

    }
}
