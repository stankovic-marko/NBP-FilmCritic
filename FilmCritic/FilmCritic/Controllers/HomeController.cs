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

namespace FilmCritic.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMongoDatabase _mongoDB;

        public HomeController(ILogger<HomeController> logger, IMongoDatabase mongoDB)
        {
            _logger = logger;
            _mongoDB = mongoDB;
        }

        public IActionResult Index()
        {
            var collection = _mongoDB.GetCollection<BsonDocument>("people");


            var list = collection.Find(new BsonDocument())
                .ToList();

            foreach (var document in list)
            {
                Console.WriteLine(document["ime"]);
            }
            return View();
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
