﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
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
        public string Usuario { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string Contraseña { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.Int64)]
        public long Telefono { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.Array)]
        public List<Message> Mensajes { get; set; }
    }
}
