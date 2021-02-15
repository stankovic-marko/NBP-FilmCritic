﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FilmCritic.Models;
using Microsoft.AspNetCore.Authorization;
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
        public List<ReviewModel> Reviews;

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
            GetReviews(id);
            return View(this);
        }

        private void GetReviews(string id)
        {
            Reviews = new List<ReviewModel>();
            var reviewsCollection = _mongoDB.GetCollection<BsonDocument>("reviews");
            ObjectId o_id = new ObjectId(id);
            var reviewsDocuments = reviewsCollection.Find($"{{ FilmId: ObjectId('{o_id}'), Comment:{{$ne:null}}  }}").Limit(3).ToList();

            foreach (var reviewDocument in reviewsDocuments)
            {
                Reviews.Add(BsonSerializer.Deserialize<ReviewModel>(reviewDocument));
            }
        }

        private void GetFilm(string id)
        {
            Film = new Film();
            var films = _mongoDB.GetCollection<BsonDocument>("films");
            ObjectId o_id = new ObjectId(id);
            var film = films.Find($"{{ _id: ObjectId('{o_id}')}}").FirstOrDefault();
            Film = BsonSerializer.Deserialize<Film>(film);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize (Roles = "Administrator")]
        public IActionResult Create([FromForm] CreateFilmModel createFilmModel)
        {

            var films = _mongoDB.GetCollection<BsonDocument>("films");
            var posters = _mongoDB.GetCollection<BsonDocument>("posters");

            var toAdd = new Film()
            {
                Director = createFilmModel.Director,
                Storyline = createFilmModel.Storyline,
                Title = createFilmModel.Title,
                Year = createFilmModel.Year,
                Negative = 0,
                Positive = 0
            };

            byte[] posterBytes = new byte[createFilmModel.PosterFile.Length];
            createFilmModel.PosterFile.OpenReadStream().Read(posterBytes, 0, (int) createFilmModel.PosterFile.Length);

            var posterId = ObjectId.GenerateNewId();
            var posterToAdd = new PosterModel()
            {
                Image = posterBytes,
                Id = posterId,
            };

            posters.InsertOneAsync(posterToAdd.ToBsonDocument());

            toAdd.PosterId = posterId;

            films.InsertOneAsync(toAdd.ToBsonDocument());

            return Redirect("/");
        }

        [HttpGet]
        [Route("poster/{posterId}")]
        public IActionResult Poster(string posterId)
        {
            ObjectId posterOId;
            if (!ObjectId.TryParse(posterId, out posterOId))
            {
                return NotFound();
            }

            var posters = _mongoDB.GetCollection<BsonDocument>("posters");
            var posterBson = posters.Find($"{{ _id: ObjectId('{posterId}') }}").FirstOrDefault();

            if (posterBson == null)
            {
                return NotFound();
            }

            PosterModel poster = BsonSerializer.Deserialize<PosterModel>(posterBson);

            return File(poster.Image, "image/jpeg");
        }
    }
}
