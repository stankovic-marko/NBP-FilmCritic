using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FilmCritic.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace FilmCritic.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMongoDatabase _mongoDB;

        public List<Film> Films { get; set; }

        public HomeController(ILogger<HomeController> logger, IMongoDatabase mongoDB)
        {
            _logger = logger;
            _mongoDB = mongoDB;
        }

        public IActionResult Index()
        {
            GetFirstFilms(12);
            return View(this);
        }

        private void GetFirstFilms(int v)
        {
            Films = new List<Film>();
            var films = _mongoDB.GetCollection<BsonDocument>("films").Find(_ => true).ToList();
            for (int i = 0; i < v; i++)
            {
                if (i == films.Count)
                {
                    break;
                }
                Films.Add(BsonSerializer.Deserialize<Film>(films[i]));
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
