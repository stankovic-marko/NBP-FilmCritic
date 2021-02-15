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

        public ReviewController(IMongoDatabase mongoDB)
        {
            _mongoDB = mongoDB;
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