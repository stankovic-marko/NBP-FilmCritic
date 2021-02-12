using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FilmCritic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace FilmCritic.Controllers
{
    public class FilmController : Controller
    {
        private readonly ILogger<FilmController> _logger;
        private readonly IMongoDatabase _mongoDB;

        public Film Film;

        public FilmController(ILogger<FilmController> logger, IMongoDatabase mongoDB)
        {
            _logger = logger;
            _mongoDB = mongoDB;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Detail([FromQuery(Name = "id")] string id)
        {
            GetFilm(id);
            return View(this);
        }

        private void GetFilm(string id)
        {
            Film = new Film();
            var films = _mongoDB.GetCollection<BsonDocument>("films");
            ObjectId o_id = new ObjectId(id);
            var film = films.Find($"{{ _id: ObjectId('{o_id}') }}").FirstOrDefault();
            Film = BsonSerializer.Deserialize<Film>(film);
        }
    }
}
