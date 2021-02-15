using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FilmCritic.Models
{
    public class CreateReviewModel
    {
        public string Comment { get; set; }

        [Required]
        public bool IsPositive { get; set; }

        [Required]
        public string FilmId { get; set; }
    }
}
