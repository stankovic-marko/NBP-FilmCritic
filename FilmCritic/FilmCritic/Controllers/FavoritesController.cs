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
    
    public class FavoritesController : Controller
    {
        private readonly IMongoDatabase _mongoDB;

        public FavoritesController(IMongoDatabase mongoDB)
        {
            _mongoDB = mongoDB;
        }

        //[HttpPost]
        [Authorize]
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
        [Authorize]
        [Route("/Favorites/Remove/{id}")]
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

            var favorites = _mongoDB.GetCollection<BsonDocument>("favorites");

            var exist = favorites.DeleteOne($"{{FilmId: ObjectId('{filmId}'), UserId: ObjectId('{userId}')}}");

            return Redirect("/Favorites/User/" + userId);
        }

        [Route("Favorites/User/{id}")]
        public IActionResult UserFavorites(string id)
        {

            var users = _mongoDB.GetCollection<BsonDocument>("users");
            if (!ObjectId.TryParse(id, out ObjectId userId))
            {
                return NotFound();
            }
            var userBson = users.Find($"{{_id: ObjectId('{userId}')}}").FirstOrDefault();

            if (userBson == null)
                return NotFound();
            var user = BsonSerializer.Deserialize<ApplicationUser>(userBson);

            var favorites = _mongoDB.GetCollection<BsonDocument>("favorites");
            var userFavoritesBson = favorites.Find($"{{UserId: ObjectId('{userId}')}}").ToList();

            var userFavorites = new List<FavoriteModel>();

            foreach (var fav in userFavoritesBson)
                userFavorites.Add(BsonSerializer.Deserialize<FavoriteModel>(fav));


            var films = _mongoDB.GetCollection<BsonDocument>("films");

            var userFilmsBson = films.Find(
                $"{{_id: {{$in: [{String.Join(" ", userFavorites.Select(x => "ObjectId('" + x.FilmId.ToString() + "')" ))}]}}}}").ToList();
            
            var userFilms = new List<Film>();
            foreach (var f in userFilmsBson)
                userFilms.Add(BsonSerializer.Deserialize<Film>(f));

            ViewData["User"] = user.Username;
            ViewData["CurrentUser"] = id == User.Identity.Name ? "true": "false";
            return View(userFilms); ;
        }
    }
}