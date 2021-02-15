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
    public class FavoritesController : Controller
    {
        private readonly IMongoDatabase _mongoDB;

        public FavoritesController(IMongoDatabase mongoDB)
        {
            _mongoDB = mongoDB;
        }

        [HttpPost]
        [Route("/Favorites/Add/{id}")]
        public IActionResult Add(string id)
        {
            string userId = User.Identity.Name;

            // does film exist
            var films = _mongoDB.GetCollection<BsonDocument>("films");
            ObjectId filmId;

            if (!ObjectId.TryParse(id, out filmId))
            {
                return NotFound();
            }

            var filmBson = films.Find($"{{ _id: ObjectId('{filmId}') }}").FirstOrDefault();

            if (filmBson == null)
            {
                return NotFound();
            }

            var film = BsonSerializer.Deserialize<Film>(filmBson);

            //is already in favorites
            var favorites = _mongoDB.GetCollection<BsonDocument>("favorites");

            var exist = favorites.Find($"{{FilmId: ObjectId('{filmId}'), UserId: ObjectId('{userId}')}}").FirstOrDefault();

            if (exist != null)
            {
                return Redirect("/film/detail/?id=" + id);
            }

            FavoriteModel toAdd = new FavoriteModel()
            {
                FilmId = filmId,
                UserId = ObjectId.Parse(userId),
                Time = DateTime.Now
            };

            favorites.InsertOne(toAdd.ToBsonDocument());
            return Redirect("/film/detail/?id=" + id);
        }

        [HttpPost]
        [Route("/Favorites/Add/{id}")]
        public IActionResult Remove(string id)
        {
            string userId = User.Identity.Name;

            // does film exist
            var films = _mongoDB.GetCollection<BsonDocument>("films");
            ObjectId filmId;

            if (!ObjectId.TryParse(id, out filmId))
            {
                return NotFound();
            }

            var filmBson = films.Find($"{{ _id: ObjectId('{filmId}') }}").FirstOrDefault();

            if (filmBson == null)
            {
                return NotFound();
            }

            var film = BsonSerializer.Deserialize<Film>(filmBson);

            //is already in favorites
            var favorites = _mongoDB.GetCollection<BsonDocument>("favorites");

            var exist = favorites.DeleteOne($"{{FilmId: ObjectId('{filmId}'), UserId: ObjectId('{userId}')}}");

            return Redirect("/film/detail/?id=" + id);
        }
    }
}