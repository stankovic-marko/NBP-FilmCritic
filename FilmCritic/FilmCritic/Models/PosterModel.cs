using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace FilmCritic.Models
{
    public class PosterModel
    {
        [BsonId(IdGenerator = typeof(BsonObjectIdGenerator))]
        public ObjectId Id { get; set; }
        [BsonElement("Image")]
        public byte[] Image { get; set; }
    }
}
