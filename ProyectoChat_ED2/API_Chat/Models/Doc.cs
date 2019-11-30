using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API_Chat.Models
{
    public class Doc
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string DocName { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string EmisorId { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string ReceptorId { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string Contenido { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime FechaUp { get; set; }
    }
}
