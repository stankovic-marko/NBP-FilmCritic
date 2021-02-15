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
        public IActionResult Index([FromQuery(Name = "keyword")] string keyword)
        {
            Films = new List<Film>();
            var films = _mongoDB.GetCollection<BsonDocument>("films");
            List<BsonDocument> filtered = new List<BsonDocument>();

            filtered = films.Find($"{{ 'Title': /{keyword}/ }}").ToList();
            foreach (var filmDocument in filtered)
            {
                Films.Add(BsonSerializer.Deserialize<Film>(filmDocument));
            }
            return View(this);
        }
    }
}
