using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FilmCritic.Models
{
    public class Film
    {
        [BsonId(IdGenerator = typeof(BsonObjectIdGenerator))]
        public ObjectId Id { get; set; }
        [BsonElement("Title")]
        public string Title { get; set; }
        [BsonElement("Director")]
        public string Director { get; set; }
        [BsonElement("Storyline")]
        public string Storyline { get; set; }
        [BsonElement("Year")]
        public int Year { get; set; }
        [BsonElement("Poster")]
        public string Poster { get; set; }
    }
}
