using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FilmCritic.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace FilmCritic.Controllers
{
    public class SearchController : Controller
    {
        private readonly IMongoDatabase _mongoDB;
        public List<Film> Films { get; set; }

        public SearchController(IMongoDatabase mongoDB)
        {
            _mongoDB = mongoDB;
        }
        public IActionResult Index([FromQuery(Name = "type")] string type, [FromQuery(Name = "keyword")] string keyword)
        {
            Films = new List<Film>();
            var films = _mongoDB.GetCollection<BsonDocument>("films");
            List<BsonDocument> filtered = new List<BsonDocument>();
            switch (type)
            {
                case "Director":
                    filtered = films.Find($"{{ '{type}': '{keyword}' }}").ToList();
                    break;
                case "Title":
                    filtered = films.Find($"{{ '{type}': '{keyword}' }}").ToList();
                    break;
                case "Year":
                    filtered = films.Find($"{{ '{type}': {keyword} }}").ToList();
                    break;
                default:
                    filtered = films.Find(_ => true).ToList();
                    break;
            }
            foreach (var filmDocument in filtered)
            {
                Films.Add(BsonSerializer.Deserialize<Film>(filmDocument));
            }
            return View(this);
        }
    }
}
