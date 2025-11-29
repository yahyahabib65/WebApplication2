using System.Collections.Generic;

namespace WebApplication2.Models
{
    public class ShopIndexViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<Category> Categories { get; set; }
    }
}
