using System.Collections.Generic;
using WebApplication2.Models;

namespace WebApplication2.Models
{
    public class VendorRatingViewModel
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public double AverageRating { get; set; }
        public List<Rating> Ratings { get; set; }
        public bool IsBlocked { get; set; }
    }
}
