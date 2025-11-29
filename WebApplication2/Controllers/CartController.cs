using Microsoft.AspNetCore.Mvc;
using WebApplication2.Data;
using WebApplication2.Extensions;
using WebApplication2.Models;
using System.Threading.Tasks;

namespace WebApplication2.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.Get<Cart>("Cart") ?? new Cart();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                var cart = HttpContext.Session.Get<Cart>("Cart") ?? new Cart();
                cart.AddItem(product, quantity);
                HttpContext.Session.Set("Cart", cart);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.Get<Cart>("Cart") ?? new Cart();
            cart.RemoveItem(productId);
            HttpContext.Session.Set("Cart", cart);
            return RedirectToAction("Index");
        }
    }
}
