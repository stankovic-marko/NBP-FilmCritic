1. U Startup.cs var client = new MongoClient("mongodb://localhost:27017"); adresu zameniti sa adresom mongoDB servera.
2. Pokrenuti aplikaciju.
3. Registrovati se.
4. Nakon registracije za otkljucavanje svih privilegija korisnika izvrsiti upit: db.users.updateOne({​​Username: "Zameniti sa username-om"}​​, {​​$set: {​​Roles: ["Administratior"]}​​}​​);
5. Prijaviti se na web aplikaciju.