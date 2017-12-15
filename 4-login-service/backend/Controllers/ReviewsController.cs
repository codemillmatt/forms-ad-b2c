using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    [Route("/api/[Controller]")]
    public class ReviewsController : Controller
    {
        [HttpGet]
        [Authorize]
        public IEnumerable<Review> GetAllReviews()
        {
            var allReviews = new List<Review> {
                new Review { Text = "Yum" },
                new Review { Text = "Meh" }
            };

            return allReviews;
        }
    }
}