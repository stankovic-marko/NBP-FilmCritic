using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FilmCritic.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace FilmCritic.Controllers
{
    public class AuthController : Controller
    {

        private readonly IMongoDatabase _mongoDB;

        public AuthController(IMongoDatabase mongoDB)
        {
            _mongoDB = mongoDB;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginModel loginModel)
        {
            var users = _mongoDB.GetCollection<BsonDocument>("users");

            var toLoginBson = users.Find(
                    "{ Username: \"" + loginModel.Username + "\"}"
                ).FirstOrDefault();

            if (toLoginBson == null)
            {
                ModelState.AddModelError("Password", "Username and Password don't match");
                return View();
            }

            var toLogin = BsonSerializer.Deserialize<ApplicationUser>(toLoginBson);
            
            string password = loginModel.Password;

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: Convert.FromBase64String(toLogin.Salt),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            if (toLogin.PasswordHash != hashed)
            {
                ModelState.AddModelError("Password", "Username and Password don't match");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, toLogin.Id.ToString()),
                new Claim(ClaimTypes.Email, toLogin.Email),
                new Claim(ClaimTypes.NameIdentifier, toLogin.Username),
            };

            if (toLogin.Roles != null)
            {
                foreach (string role in toLogin.Roles)
                    claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
            return Redirect("/");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromForm] RegisterModel registerModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var users = _mongoDB.GetCollection<BsonDocument>("users");

            bool existUsername = users.Find(
                    "{ Username: \"" + registerModel.Username + "\"}"
                ).FirstOrDefault() != null;

            bool existEmail = users.Find(
                    "{ Email: \"" + registerModel.Email + "\" }"
                ).FirstOrDefault() != null;

            if (existEmail || existUsername)
            {
                if (existEmail)
                    ModelState.AddModelError("Email", "Email already in use");
                if (existUsername)
                    ModelState.AddModelError("Username", "Username already in use");
                return View();
            }


            string password = registerModel.Password;

            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            string saltString = Convert.ToBase64String(salt);

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            ApplicationUser toRegister = new ApplicationUser()
            {
                Username = registerModel.Username,
                Email = registerModel.Email,
                PasswordHash = hashed,
                Salt = saltString,
            };

            await users.InsertOneAsync(toRegister.ToBsonDocument());

            return Redirect("/");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Redirect("/");
        }

        [Authorize]
        public IActionResult Closed()
        {
            var user = User;
            return Ok(user.FindFirst(x => x.Type == ClaimTypes.NameIdentifier).Value);
        }

        [Authorize (Roles = "Administrator")]
        public IActionResult Closed2()
        {
            var user = User;
            return Ok(user.FindFirst(x => x.Type == ClaimTypes.NameIdentifier).Value + "admin");
        }
    }
}