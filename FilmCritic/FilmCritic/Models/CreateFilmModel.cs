﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FilmCritic.Models
{
    public class CreateFilmModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Director { get; set; }
        
        [Required]
        public string Storyline { get; set; }
        
        [Required]
        public int Year { get; set; }
        
        [Required]
        public IFormFile PosterFile { get; set; }
    }
}
