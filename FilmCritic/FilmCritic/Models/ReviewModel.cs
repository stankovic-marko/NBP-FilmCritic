using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FilmCritic.Models
{
    public class ReviewModel
    {
        [BsonId(IdGenerator = typeof(BsonObjectIdGenerator))]
        public ObjectId Id { get; set; }

        [BsonElement("Comment")]
        public string Comment { get; set; }

        [BsonElement("IsPositive")]
        public bool IsPositive { get; set; }

        [BsonElement("UserId")]
        public ObjectId UserId { get; set; }

        [BsonElement("UserId")]
        public ObjectId UserName { get; set; }

        [BsonElement("FilmId")]
        public ObjectId FilmId { get; set; }

        [BsonElement("Time")]
        public DateTime Time { get; set; }
    }
}
