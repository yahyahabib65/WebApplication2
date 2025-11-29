using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebApplication2.Data;
using WebApplication2.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;

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

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(Product product, IFormFile productImage)
        {
            var userId = _userManager.GetUserId(User);
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);

            if (store == null || store.Status != Models.StoreStatus.Approved)
            {
                return RedirectToAction("StoreStatus");
            }

            product.StoreId = store.Id;

            if (productImage != null && productImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + productImage.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await productImage.CopyToAsync(fileStream);
                }
                product.ImageUrl = "/images/products/" + uniqueFileName;
            }

            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Seller/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Store)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Seller/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Seller/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,ImageUrl,StoreId,CategoryId")] Product product, IFormFile productImage)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            // Ensure the product belongs to the current seller's store
            var userId = _userManager.GetUserId(User);
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
            if (store == null || product.StoreId != store.Id)
            {
                return Forbid();
            }

            if (productImage != null && productImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + productImage.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await productImage.CopyToAsync(fileStream);
                }
                product.ImageUrl = "/images/products/" + uniqueFileName;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Seller/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Store)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            // Ensure the product belongs to the current seller's store
            var userId = _userManager.GetUserId(User);
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
            if (store == null || product.StoreId != store.Id)
            {
                return Forbid();
            }

            return View(product);
        }

        // POST: Seller/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            // Ensure the product belongs to the current seller's store
            var userId = _userManager.GetUserId(User);
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
            if (store == null || product.StoreId != store.Id)
            {
                return Forbid();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        public IActionResult Reporting()
        {
            return View();
        }

        public async Task<IActionResult> ProductReport()
        {
            var userId = _userManager.GetUserId(User);
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
            if (store == null)
            {
                return RedirectToAction("NoStore");
            }

            var products = await _context.Products
                .Where(p => p.StoreId == store.Id)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> SalesReport()
        {
            var userId = _userManager.GetUserId(User);
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
            if (store == null)
            {
                return RedirectToAction("NoStore");
            }

            var salesData = await _context.OrderDetails
                .Where(od => od.Order.StoreId == store.Id)
                .Include(od => od.Order)
                .Include(od => od.Product)
                .Select(od => new SalesReportViewModel
                {
                    Date = od.Order.OrderDate,
                    Name = od.Product.Name,
                    Payment = od.UnitPrice * od.Quantity
                })
                .ToListAsync();

            return View(salesData);
        }

        public async Task<IActionResult> ReturnReport()
        {
            var userId = _userManager.GetUserId(User);
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
            if (store == null)
            {
                return RedirectToAction("NoStore");
            }

            var returns = await _context.Returns
                .Where(r => r.Order.StoreId == store.Id)
                .Include(r => r.Order)
                    .ThenInclude(o => o.Customer)
                .ToListAsync();

            return View(returns);
        }
    }
}
