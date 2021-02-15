using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FilmCritic.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace FilmCritic.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly IMongoDatabase _mongoDB;

        public List<ReviewModel> Reviews;
        public Film Film;
        public int ReviewsCount { get; set; }
        public int CurrentPage { get; set; }

        public ReviewController(IMongoDatabase mongoDB)
        {
            _mongoDB = mongoDB;
        }

        public IActionResult AllReviews([FromQuery(Name = "id")] string filmId, [FromQuery(Name = "page")] int page = 1)
        {
            GetReviews(filmId, page - 1);
            GetFilm(filmId);
            return View(this);
        }

        private void GetFilm(string id)
        {
            Film = new Film();
            var films = _mongoDB.GetCollection<BsonDocument>("films");
            ObjectId o_id = new ObjectId(id);
            var film = films.Find($"{{ _id: ObjectId('{o_id}')}}").FirstOrDefault();
            Film = BsonSerializer.Deserialize<Film>(film);
        }

        private void GetReviews(string filmId, int page)
        {

            Reviews = new List<ReviewModel>();
            var reviewsCollection = _mongoDB.GetCollection<BsonDocument>("reviews");
            ObjectId o_id = new ObjectId(filmId);
            ReviewsCount = (int) reviewsCollection.Find($"{{ FilmId: ObjectId('{o_id}'), Comment:{{$ne:null}}  }}").Count();
            CurrentPage = page;
            var reviewsDocuments = reviewsCollection.Find($"{{ FilmId: ObjectId('{o_id}'), Comment:{{$ne:null}}  }}").Skip(page*5).Limit(5).ToList();

            foreach (var reviewDocument in reviewsDocuments)
            {
                Reviews.Add(BsonSerializer.Deserialize<ReviewModel>(reviewDocument));
            }
        }

        public IActionResult Create([FromQuery(Name = "id")] string filmId)
        {
            var films = _mongoDB.GetCollection<BsonDocument>("films");
            ObjectId o_id = new ObjectId(filmId);
            var filmBson = films.Find($"{{ _id: ObjectId('{o_id}') }}").FirstOrDefault();

            if (filmBson == null)
            {
                return NotFound();
            }

            var film = BsonSerializer.Deserialize<Film>(filmBson);

            ViewData["filmName"] = film.Title;

            return View(new CreateReviewModel() { FilmId = filmId });
        }

        [HttpPost]
        public IActionResult Create([FromForm] CreateReviewModel review)
        {
            string userId = User.Identity.Name;
            // does film exist
            var films = _mongoDB.GetCollection<BsonDocument>("films");
            ObjectId filmId;

            if (!ObjectId.TryParse(review.FilmId, out filmId))
            {
                return NotFound();
            }

            var filmBson = films.Find($"{{ _id: ObjectId('{filmId}') }}").FirstOrDefault();

            if (filmBson == null)
            {
                return NotFound();
            }

            var film = BsonSerializer.Deserialize<Film>(filmBson);

            //does review for film from user already exist
            var reviews = _mongoDB.GetCollection<BsonDocument>("reviews");

            var exist = reviews.FindOneAndDelete($"{{FilmId: ObjectId('{filmId}'), UserId: ObjectId('{userId}')}}");

            if (exist != null)
            {
                var oldReview = BsonSerializer.Deserialize<ReviewModel>(exist);

                if (oldReview.IsPositive)
                    films.UpdateOne($"{{_id: ObjectId('{film.Id}')}}", "{$inc: { Positive: -1}}");
                else
                    films.UpdateOne($"{{_id: ObjectId('{film.Id}')}}", "{$inc: { Negative: -1}}");
            }

            ReviewModel toAdd = new ReviewModel()
            {
                Comment = review.Comment,
                FilmId = filmId,
                IsPositive = review.IsPositive,
                UserId = ObjectId.Parse(userId),
                Time = DateTime.Now
            };

            if (toAdd.IsPositive)
                films.UpdateOne($"{{_id: ObjectId('{film.Id}')}}", "{$inc: { Positive: 1}}");
            else
                films.UpdateOne($"{{_id: ObjectId('{film.Id}')}}", "{$inc: { Negative: 1}}");

            reviews.InsertOne(toAdd.ToBsonDocument());
            return Redirect("/film/detail/?id=" + review.FilmId);
        }
    }
}