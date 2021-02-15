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

        public int FilmsCount { get; set; }
        public int CurrentPage { get; set; }

        [BindProperty]
        public string SearchType { get; set; }
        [BindProperty]
        public string SearchKeyword { get; set; }

        public List<Film> Films { get; set; }

        public HomeController(ILogger<HomeController> logger, IMongoDatabase mongoDB)
        {
            _logger = logger;
            _mongoDB = mongoDB;
        }

        public IActionResult Index([FromQuery(Name = "page")] int page = 1)
        {
            GetFilmsPage(page - 1);
            return View(this);
        }

        private void GetFilmsPage(int v)
        {
            CurrentPage = v;
            FilmsCount = (int)_mongoDB.GetCollection<BsonDocument>("films").Count(_ => true);
            Films = new List<Film>();
            var films = _mongoDB.GetCollection<BsonDocument>("films").Find(_ => true).Skip(v*8).Limit(8).ToList();
            foreach (var filmDocument in films)
            {
                Films.Add(BsonSerializer.Deserialize<Film>(filmDocument));
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
