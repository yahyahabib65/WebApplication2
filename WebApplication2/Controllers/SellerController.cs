using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Seller")]
    public class SellerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SellerController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);

            if (store == null)
            {
                return View("NoStore");
            }

            if (store.Status != Models.StoreStatus.Approved)
            {
                return RedirectToAction("StoreStatus");
            }

            var products = await _context.Products
                .Where(p => p.StoreId == store.Id)
                .ToListAsync();

            ViewBag.StoreName = store.Name;
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            var userId = _userManager.GetUserId(User);
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);

            if (store == null || store.Status != Models.StoreStatus.Approved)
            {
                return RedirectToAction("StoreStatus");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(Product product)
        {
            var userId = _userManager.GetUserId(User);
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);

            if (store == null || store.Status != Models.StoreStatus.Approved)
            {
                return RedirectToAction("StoreStatus");
            }

            product.StoreId = store.Id;
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> StoreStatus()
        {
            var userId = _userManager.GetUserId(User);
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);

            if (store == null)
            {
                return View("NoStore");
            }

            return View(store);
        }
    }
}
