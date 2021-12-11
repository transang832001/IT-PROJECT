using System;
using System.Collections.Generic;
using System.Text;

namespace MovieRecommendationSystem.Models
{
    public class Rank
    {
        public int STT { get; set; }

        public int MovieId { get; set; }

        public string Title { get; set; }

        public double Rating { get; set; }
    }
}
