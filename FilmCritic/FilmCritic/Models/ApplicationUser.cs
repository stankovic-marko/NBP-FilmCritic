using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FilmCritic.Models
{
    public class ApplicationUser
    {
        [BsonId(IdGenerator = typeof(BsonObjectIdGenerator))]
        public ObjectId Id { get; set; }

        [BsonElement("Username")]
        public string Username { get; set; }

        [BsonElement("Email")]
        public string Email { get; set; }

        [BsonElement("PasswordHash")]
        public string PasswordHash { get; set; }

        [BsonElement("Salt")]
        public string Salt { get; set; }

        [BsonElement("Roles")]
        public IList<string> Roles { get; set; }
    }
}
